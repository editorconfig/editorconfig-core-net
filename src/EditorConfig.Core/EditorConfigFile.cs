using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EditorConfig.Core
{
	/// <summary> Information about the originating editor config file</summary>
	public interface IEditorConfigFile
	{
		/// <summary> The directory of the EditorConfig file </summary>
		public string Directory { get; }

		/// <summary> The name of the EditorConfig file </summary>
		public string FileName { get; }

		/// <summary> A hint this instance of <see cref="EditorConfigFile"/> was cached</summary>
		public string CacheKey { get; }

		/// <summary> Indicates wheter the loaded editorconfig represents the root of the chain </summary>
		public bool IsRoot { get;  }
	}


	/// <summary>
	/// Represents the raw config file as INI, please use <see cref="EditorConfigParser.GetConfigurationFilesTillRoot"/>
	/// </summary>
	public class EditorConfigFile : IEditorConfigFile
	{
		private static readonly Regex SectionRe = new Regex(@"^\s*\[(([^#;]|\\#|\\;)+)\]\s*([#;].*)?$");
		private static readonly Regex CommentRe = new Regex(@"^\s*[#;]");
		private static readonly Regex PropertyRe = new Regex(@"^\s*([\w\.\-_]+)\s*[=:]\s*(.*?)\s*([#;].*)?$");

		private static readonly string[] KnownProperties =
		{
			"indent_style",
			"indent_size",
			"tab_width",
			"end_of_line",
			"charset",
			"trim_trailing_whitespace",
			"insert_final_newline",
			"max_line_length",
			"root",
		};

		private readonly Dictionary<string, string> _globalDict = new Dictionary<string, string>();
		/// <summary> All discovered sections </summary>
		public List<ConfigSection> Sections { get; } = new List<ConfigSection>();

		/// <inheritdoc cref="IEditorConfigFile.Directory"/>
		public string Directory { get; }

		/// <inheritdoc cref="IEditorConfigFile.FileName"/>
		public string FileName { get; }

		/// <inheritdoc cref="IEditorConfigFile.CacheKey"/>
		public string CacheKey { get; }

		private readonly bool _isRoot;
		/// <inheritdoc cref="IEditorConfigFile.IsRoot"/>
		public bool IsRoot => _isRoot;

		private EditorConfigFile(
			string fileName,
			string directory,
			TextReader reader,
			string cacheKey = null)
		{
			Directory = directory;
			FileName = fileName;
			CacheKey = cacheKey;
			ReadAndParse(reader);

			if (_globalDict.TryGetValue("root", out var value))
				bool.TryParse(value, out _isRoot);
		}

		/// <summary> Parses EditorConfig file from the file path. </summary>
		/// <param name="path"> File path in a physical file system. </param>
		/// <returns> Parsed EditorConfig file. </returns>
		public static EditorConfigFile Parse(string path) => Parse(path, cacheKey: null);

		/// <summary> Parses EditorConfig file from the text reader. </summary>
		/// <param name="reader"> Text reader. </param>
		/// <param name="directory"> EditorConfig directory files to be matched to. </param>
		/// <param name="fileName"> EditorConfig file name. </param>
		/// <returns> Parsed EditorConfig file. </returns>
		public static EditorConfigFile Parse(TextReader reader, string directory, string fileName = ".editorconfig") =>
			new(fileName, directory, reader);

		internal static EditorConfigFile Parse(string path, string cacheKey)
		{
			using var file = File.OpenRead(path);
			using var reader = new StreamReader(file);
			return new EditorConfigFile(
				Path.GetFileName(path), Path.GetDirectoryName(path),
				reader, cacheKey);
		}

		private void ReadAndParse(TextReader reader)
		{
			var activeDict = _globalDict;
			var sectionName = string.Empty;
			var reset = false;
			while (reader.ReadLine() is { } line)
			{
				if (string.IsNullOrWhiteSpace(line)) continue;

				if (CommentRe.IsMatch(line)) continue;
				var matches = PropertyRe.Matches(line);
				if (matches.Count > 0)
				{
					var key = matches[0].Groups[1].Value.Trim();
					var value = matches[0].Groups[2].Value.Trim();

					key = key.ToLowerInvariant();
					if (KnownProperties.Contains(key, StringComparer.OrdinalIgnoreCase))
						value = value.ToLowerInvariant();

					//! do not Add(), avoid exceptions on duplicate keys
					activeDict[key] = value;
					reset = false;
					continue;
				}
				matches = SectionRe.Matches(line);
				if (matches.Count <= 0) continue;

				if (!string.IsNullOrEmpty(sectionName))
				{
					var section = new ConfigSection(sectionName, this, activeDict);
					Sections.Add(section);
					reset = true;
				}

				sectionName = matches[0].Groups[1].Value;
				activeDict = new Dictionary<string, string>();
			}

			if (!reset)
			{
				var section = new ConfigSection(sectionName, this, activeDict);
				Sections.Add(section);
			}

		}
	}
}
