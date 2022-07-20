﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace EditorConfig.Core
{

	public class FileConfiguration
	{
		private List<ConfigSection> Sections { get; }

		/// <summary>
		/// set to tab or space to use hard tabs or soft tabs respectively.
		/// </summary>
		public IndentStyle? IndentStyle { get; }

		/// <summary>
		/// a whole number defining the number of columns used for each indentation level and the width of soft tabs (when supported).
		/// When set to tab, the value of tab_width (if specified) will be used.
		/// </summary>
		public IndentSize IndentSize { get; }

		/// <summary>
		/// a whole number defining the number of columns used to represent a tab character.
		/// This defaults to the value of indent_size and doesn't usually need to be specified.
		/// </summary>
		public int? TabWidth { get; }

		/// <summary>
		/// set to lf, cr, or crlf to control how line breaks are represented.
		/// </summary>
		public EndOfLine? EndOfLine { get; }

		/// <summary>
		/// set to latin1, utf-8, utf-8-bom, utf-16be or utf-16le to control the character set. Use of utf-8-bom is discouraged.
		/// </summary>
		public Charset? Charset { get; }

		/// <summary>
		/// set to true to remove any whitespace characters preceding newline characters and false to ensure it doesn't.
		/// </summary>
		public bool? TrimTrailingWhitespace { get; }

		/// <summary>
		/// set to true ensure file ends with a newline when saving and false to ensure it doesn't.
		/// </summary>
		public bool? InsertFinalNewline { get; }

		/// <summary>
		/// Forces hard line wrapping after the amount of characters specified
		/// </summary>
		public int? MaxLineLength { get; }

		/// <summary>
		/// All the editorconfig properties relevant for this file
		/// </summary>
		public IReadOnlyDictionary<string, string> Properties { get; }

		/// <summary>
		/// The filename we asked the configuration for
		/// </summary>
		public string FileName { get; }

		/// <summary>
		/// A reference to the version number of the parser
		/// </summary>
		public Version Version { get; }

		/// <summary>
		/// Holds the editor configuration for a file, please use <see cref="EditorConfigParser.Parse"/> to get an instance
		/// </summary>
		internal FileConfiguration(Version version, string fileName, List<ConfigSection> sections)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("file should not be null or whitespace", nameof(fileName));

			FileName = fileName;
			Version = version;
			Sections = sections;

			var allProperties = sections.SelectMany(section => section);
			var properties = new Dictionary<string, string>();
			foreach (var kv in allProperties)
				properties[kv.Key] = kv.Value;

			IndentStyle = Sections.LastOrDefault(s => s.IndentStyle.HasValue)?.IndentStyle;
			IndentSize = Sections.LastOrDefault(s => s.IndentSize != null)?.IndentSize;
			TabWidth = Sections.LastOrDefault(s => s.TabWidth.HasValue)?.TabWidth;
			EndOfLine = Sections.LastOrDefault(s => s.EndOfLine.HasValue)?.EndOfLine;
			Charset = Sections.LastOrDefault(s => s.Charset.HasValue)?.Charset;
			TrimTrailingWhitespace = Sections.LastOrDefault(s => s.TrimTrailingWhitespace.HasValue)?.TrimTrailingWhitespace;
			InsertFinalNewline = Sections.LastOrDefault(s => s.InsertFinalNewline.HasValue)?.InsertFinalNewline;
			MaxLineLength = Sections.LastOrDefault(s => s.MaxLineLength.HasValue)?.MaxLineLength;

			//default tab_width to indent_size when indent size is a number
			if (IndentSize != null && IndentSize.NumberOfColumns.HasValue)
			{
				TabWidth = IndentSize.NumberOfColumns.Value;
				properties["tab_width"] = TabWidth.Value.ToString();
			}

			// Set indent_size to "tab" if indent_size is unspecified and indent_style is set to "tab".
			if (IndentStyle.HasValue && IndentStyle == Core.IndentStyle.Tab && IndentSize == null
			    && Version >= new Version(0, 10))
			{
				IndentSize = IndentSize.Tab;
				properties["indent_size"] = "tab";
			}

			// Set tab_width to indent_size if indent_size is specified and tab_width is unspecified
			if (IndentSize != null && !TabWidth.HasValue && !IndentSize.UseTabWidth)
			{
				//only set tab_width to indent_size if indent size holds a positive integer
				if (IndentSize.NumberOfColumns.HasValue && IndentSize.NumberOfColumns.Value >= 0)
				{
					TabWidth = IndentSize.NumberOfColumns.Value;
					properties["tab_width"] = TabWidth.Value.ToString();
				}

				// unset carries over see:
				//  ctest . -R "unset_indent_size"
				else if (IndentSize.IsUnset)
				{
					TabWidth = new int?();
					properties["tab_width"] = "unset";
				}
			}

			// Set indent_size to tab_width if indent_size is "tab"
			if (IndentSize != null && TabWidth != null && IndentSize.UseTabWidth)
			{
				IndentSize = IndentSize.Columns(TabWidth.Value);
				properties["indent_size"] = TabWidth.Value.ToString();
			}

			Properties = new ReadOnlyDictionary<string, string>(properties);
		}
	}
}
