using System;
using System.Collections.Generic;
using System.IO;
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

		/// <summary> Indicates whether the loaded editorconfig represents the root of the chain </summary>
		public bool IsRoot { get;  }
	}


	/// <summary>
	/// Represents the raw config file as INI, please use <see cref="EditorConfigParser.GetConfigurationFilesTillRoot"/>
	/// </summary>
	public partial class EditorConfigFile : IEditorConfigFile
	{
#if NET7_0_OR_GREATER
		[GeneratedRegex(@"^\s*\[(([^#;]|\\#|\\;)+)\]\s*([#;].*)?$")]
		private static partial Regex SectionRe();

		[GeneratedRegex(@"^\s*[#;]")]
		private static partial Regex CommentRe();

		[GeneratedRegex(@"^\s*([\w\.\-_]+)\s*[=:]\s*(.*?)\s*([#;].*)?$")]
		private static partial Regex PropertyRe();
#else
		private static readonly Regex _sectionRe  = new Regex(@"^\s*\[(([^#;]|\\#|\\;)+)\]\s*([#;].*)?$", RegexOptions.Compiled);
		private static readonly Regex _commentRe  = new Regex(@"^\s*[#;]", RegexOptions.Compiled);
		private static readonly Regex _propertyRe = new Regex(@"^\s*([\w\.\-_]+)\s*[=:]\s*(.*?)\s*([#;].*)?$", RegexOptions.Compiled);

		private static Regex SectionRe()  => _sectionRe;
		private static Regex CommentRe()  => _commentRe;
		private static Regex PropertyRe() => _propertyRe;
#endif

		private static readonly HashSet<string> KnownProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
		/// <param name="directory"> EditorConfig base directory to match file sections to. Default is null. </param>
		/// <param name="fileName"> EditorConfig file name. Default is '.editorconfig'. </param>
		/// <returns> Parsed EditorConfig file. </returns>
		public static EditorConfigFile Parse(TextReader reader, string directory = null, string fileName = ".editorconfig") =>
			new(fileName, directory, reader);

		/// <summary> Parses EditorConfig file from a text reader with an explicit cache key (used by EditorConfigFileCache). </summary>
		internal static EditorConfigFile Parse(TextReader reader, string directory, string fileName, string cacheKey) =>
			new(fileName, directory, reader, cacheKey);

		internal static EditorConfigFile Parse(string path, string cacheKey)
		{
			using var file = File.OpenRead(path);
			using var reader = new StreamReader(file);
			return new EditorConfigFile(
				System.IO.Path.GetFileName(path), System.IO.Path.GetDirectoryName(path),
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

				if (CommentRe().IsMatch(line)) continue;

				// Use Match (not Matches) — we only need the first match, avoid MatchCollection allocation
				var propMatch = PropertyRe().Match(line);
				if (propMatch.Success)
				{
					var key   = propMatch.Groups[1].Value.Trim();
					var value = propMatch.Groups[2].Value.Trim();

					key = key.ToLowerInvariant();
					if (KnownProperties.Contains(key))
						value = value.ToLowerInvariant();

					//! do not Add(), avoid exceptions on duplicate keys
					activeDict[key] = value;
					reset = false;
					continue;
				}

				var sectionMatch = SectionRe().Match(line);
				if (!sectionMatch.Success) continue;

				if (!string.IsNullOrEmpty(sectionName))
				{
					var section = new ConfigSection(sectionName, this, activeDict);
					Sections.Add(section);
					reset = true;
				}

				sectionName = sectionMatch.Groups[1].Value;
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
