using System;
using System.Collections.Generic;
using System.IO.Abstractions;
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

		internal EditorConfigFile(string path, IFileSystem fileSystem, string cacheKey = null)
		{
			Directory = fileSystem.Path.GetDirectoryName(path);
			FileName = fileSystem.Path.GetFileName(path);
			CacheKey = cacheKey;
			Parse(path, fileSystem);

			if (_globalDict.TryGetValue("root", out var value))
				bool.TryParse(value, out _isRoot);
		}

		private void Parse(string file, IFileSystem fileSystem)
		{
			var lines = fileSystem.File.ReadLines(file);

			var activeDict = _globalDict;
			var sectionName = string.Empty;
			var reset = false;
			foreach (var line in lines)
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
