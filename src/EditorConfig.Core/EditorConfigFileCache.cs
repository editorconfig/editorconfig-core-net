using System;
using System.Collections.Concurrent;
using System.IO.Abstractions;

namespace EditorConfig.Core;

/// <summary>
/// Cache unchanged parsed EditorConfigFiles.
/// </summary>
public static class EditorConfigFileCache
{
	private static readonly ConcurrentDictionary<string, EditorConfigFile> FileCache = new();

	/// <summary>
	/// Retrieves a cached EditorConfigFile based on the file path, last-write time, and file size.
	/// The cache is invalidated when either the size or modification timestamp changes.
	/// </summary>
	/// <remarks>
	/// Thread safe. Falls back to a fresh parse when the file does not exist.
	/// Cache lookup requires only a single cheap metadata stat — the file content is never
	/// read on a cache hit.
	/// </remarks>
	public static EditorConfigFile GetOrCreate(string file, IFileSystem fileSystem = null)
	{
		fileSystem ??= new FileSystem();
		if (!fileSystem.File.Exists(file)) return EditorConfigFile.Parse(file, fileSystem);

		var info = fileSystem.FileInfo.New(file);
		var key = $"{file}|{info.LastWriteTimeUtc.Ticks}|{info.Length}";

		return FileCache.GetOrAdd(key, _ => EditorConfigFile.Parse(file, key, fileSystem));
	}
}
