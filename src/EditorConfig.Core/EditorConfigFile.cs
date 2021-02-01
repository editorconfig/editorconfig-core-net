using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EditorConfig.Core
{
	/// <summary>
	/// Represents the raw config file as INI
	/// </summary>
	public class EditorConfigFile
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
		public List<ConfigSection> Sections { get; } = new List<ConfigSection>(); 

		public string Directory { get; }
		
		private readonly bool _isRoot;
		public bool IsRoot => _isRoot;

		public EditorConfigFile(string file)
		{
			Directory = Path.GetDirectoryName(file);
			Parse(file);

			if (_globalDict.ContainsKey("root"))
				bool.TryParse(_globalDict["root"], out _isRoot);

		}

		private void Parse(string file)
		{
			var lines = File.ReadLines(file);

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
					var section = new ConfigSection(sectionName, Directory, activeDict);
					Sections.Add(section);
					reset = true;
				}

				sectionName = matches[0].Groups[1].Value;
				activeDict = new Dictionary<string, string>();
			}

			if (!reset)
			{
				var section = new ConfigSection(sectionName, Directory, activeDict);
				Sections.Add(section);
			}
			
		}
	}
}
