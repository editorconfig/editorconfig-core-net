﻿using System;
using System.Collections.Concurrent;
using System.IO;

namespace EditorConfig.Core;

/// <summary>
/// Cache unchanged parsed EditorConfigFiles.
/// </summary>
public static class EditorConfigFileCache
{
	private static string GetFileHash(string filename)
	{
		using var sha256 = System.Security.Cryptography.SHA256.Create();
		using var stream = File.OpenRead(filename);
		var hash = sha256.ComputeHash(stream);
		return BitConverter.ToString(hash).Replace("-", "");
	}

	private static readonly ConcurrentDictionary<string, EditorConfigFile> FileCache = new();

	/// <summary>
	/// Retrieves a cached EditorConfigFile based on the file name and file hash.
	/// The cache will be populated when the file was not present.
	/// </summary>
	/// <remarks>This function is thread safe. The cache will not be hit when the file does not exist.</remarks>
	/// <param name="file"></param>
	/// <returns></returns>
	public static EditorConfigFile GetOrCreate(string file)
	{
		if (!File.Exists(file)) return EditorConfigFile.Parse(file);

		var key = $"{file}_{GetFileHash(file)}";
		return FileCache.GetOrAdd(key, _ => EditorConfigFile.Parse(file, key));
	}
}
