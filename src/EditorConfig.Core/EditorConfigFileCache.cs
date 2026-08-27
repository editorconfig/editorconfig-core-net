using System.Collections.Concurrent;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EditorConfig.Core;

/// <summary>
/// Caches unchanged parsed <see cref="EditorConfigFile"/>s.
/// </summary>
/// <remarks>
/// Not static — each <see cref="EditorConfigParser"/> owns a private instance of this cache
/// by default, so cache entries can never be observed across parsers/filesystems unless a
/// cache instance is deliberately shared. Pass an instance to multiple
/// <see cref="EditorConfigParser"/> constructors (e.g. via
/// <see cref="EditorConfigParser(IFileSystem, EditorConfigFileCache, string, System.Version)"/>)
/// to opt into sharing.
/// </remarks>
public sealed class EditorConfigFileCache
{
	private readonly ConcurrentDictionary<string, EditorConfigFile> _cache = new();

	private static readonly ConditionalWeakTable<IFileSystem, object> FileSystemIds = new();
	private static long _nextFileSystemId;

	/// <summary>
	/// A missing config file contributes no properties and is not an error — this matches
	/// editorconfig-core-c's editorconfig_parse(), which explicitly ignores I/O errors
	/// (including non-existent files) while walking the chain rather than failing. There is
	/// no stat data for a missing file, so there's nothing meaningful to key or cache; we
	/// hand back this shared, stateless, zero-section instance instead. Every call still
	/// re-checks <see cref="IFile.Exists"/> fresh, so a file created later is picked up
	/// correctly on the next call.
	/// </summary>
	private static readonly EditorConfigFile Empty = EditorConfigFile.Parse(TextReader.Null);

	/// <summary>
	/// Retrieves a cached EditorConfigFile based on the file path, last-write time, and file size.
	/// The cache is invalidated when either the size or modification timestamp changes.
	/// </summary>
	/// <remarks>
	/// Thread safe. Returns a shared empty (non-root, zero-section) instance when the file does
	/// not exist, rather than throwing — a missing config file contributes nothing to the chain.
	/// Cache lookup requires only a single cheap metadata stat — the file content is never
	/// read on a cache hit.
	/// </remarks>
	public EditorConfigFile GetOrCreate(string file, IFileSystem fileSystem = null)
	{
		fileSystem ??= new FileSystem();
		if (!fileSystem.File.Exists(file)) return Empty;

		var info = fileSystem.FileInfo.New(file);
		var key = $"{FileSystemKeyPart(fileSystem)}{file}|{info.LastWriteTimeUtc.Ticks}|{info.Length}";

		return _cache.GetOrAdd(key, _ => EditorConfigFile.Parse(file, key, fileSystem));
	}

	/// <summary>
	/// Real physical <see cref="FileSystem"/> instances are interchangeable — they all reflect
	/// the same on-disk state, so path+mtime+length is already a sufficient key and any number
	/// of them can safely share cache entries. Any other <see cref="IFileSystem"/> (mocks or
	/// other custom implementations, e.g. MockFileSystem) is not guaranteed to agree on content
	/// for the same path+mtime+length across instances, so its entries are qualified by
	/// filesystem identity to prevent cross-instance collisions.
	/// </summary>
	private static string FileSystemKeyPart(IFileSystem fileSystem)
	{
		if (fileSystem.GetType() == typeof(FileSystem)) return string.Empty;

		var id = FileSystemIds.GetValue(fileSystem, _ => Interlocked.Increment(ref _nextFileSystemId));
		return $"{id}|";
	}
}
