using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EditorConfig.Core
{
	/*
	 * 
indent_style: set to tab or space to use hard tabs or soft tabs respectively.
indent_size: a whole number defining the number of columns used for each indentation level and the width of soft tabs (when supported). When set to tab, the value of tab_width (if specified) will be used.
tab_width: a whole number defining the number of columns used to represent a tab character. This defaults to the value of indent_size and doesn't usually need to be specified.
end_of_line: set to lf, cr, or crlf to control how line breaks are represented.
charset: set to latin1, utf-8, utf-8-bom, utf-16be or utf-16le to control the character set. Use of utf-8-bom is discouraged.
trim_trailing_whitespace: set to true to remove any whitespace characters preceding newline characters and false to ensure it doesn't.
insert_final_newline: set to true ensure file ends with a newline when saving and false to ensure it doesn't.
root: special property that should be specified at the top of the file outside of any sections. Set to true to stop .editorconfig files search on current file.
*/

	public enum IndentStyle
	{
		Tab,
		Space
	}

	public enum EndOfLine
	{
// ReSharper disable InconsistentNaming
		LF,
		CR,
		CRLF
// ReSharper restore InconsistentNaming
	}

	public enum Charset
	{
		Latin1,
		UTF8,
		/// <summary>
		/// Usage of UFT8BOM is discouraged
		/// </summary>
		UTF8BOM,
		UTF16BE,
		UTF16LE,

	}

	public class IndentSize
	{
		public bool UseTabWidth { get; private set; }
		public int? NumberOfColumns { get; private set; }

		public IndentSize(bool useTabs)
		{
			this.UseTabWidth = useTabs;
		}

		public IndentSize(int numberOfColumns)
		{
			this.NumberOfColumns = numberOfColumns;
		}
	}

	public class FileConfiguration
	{
		public IndentStyle? IndentStyle { get; private set; }
		public IndentSize IndentSize { get; private set; }
		public int? TabWidth { get; private set; }
		public EndOfLine? EndOfLine { get; private set; }
		public Charset? Charset { get; private set; }
		public bool? TrimTrailingWhitespace { get; private set; }
		public bool? InsertFinalNewline { get; private set; }

		private static readonly string[] KnownProperties =
		{
			"indent_style",
			"indent_size",
			"tab_width",
			"end_of_line",
			"charset",
			"trim_trailing_whitespace",
			"insert_final_newline",
			"root",
		};

		private readonly Dictionary<string, string> _properties;

		public IDictionary<string, string> Properties { get { return _properties; } }

		public string FileName { get; private set; }

		public Version Version { get; private set; }

		internal FileConfiguration(Version version, string fileName, Dictionary<string, string> properties)
		{
			FileName = fileName;
			Version = version;
			_properties = this.SanitizeProperties(properties ?? new Dictionary<string, string>());
			this.ParseKnownProperties();
		}

		internal static KeyValuePair<string, string> Sanitize(string key, string value)
		{
			key = key.ToLowerInvariant();
			if (KnownProperties.Contains(key, StringComparer.OrdinalIgnoreCase))
				value = value.ToLowerInvariant();
			return new KeyValuePair<string, string>(key, value);
		}

		private Dictionary<string, string> SanitizeProperties(Dictionary<string, string> properties)
		{
			// Set indent_size to "tab" if indent_size is unspecified and indent_style is set to "tab".
			if (properties.ContainsKey("indent_style") && properties["indent_style"] == "tab" && !properties.ContainsKey("indent_size")
				&& Version >= new Version(0, 10))
				properties["indent_size"] = "tab";

			// Set tab_width to indent_size if indent_size is specified and tab_width is unspecified
			if (properties.ContainsKey("indent_size") && !properties.ContainsKey("tab_width") && properties["indent_size"] != "tab")
				properties["tab_width"] = properties["indent_size"];

			// Set indent_size to tab_width if indent_size is "tab"
			if (properties.ContainsKey("indent_size") && properties.ContainsKey("tab_width") && properties["indent_size"] == "tab")
				properties["indent_size"] = properties["tab_width"];


			return properties;
		}

		private void ParseKnownProperties()
		{
			this.ParseIndentStyle();
			this.ParseIndentSize();
			this.ParseTabWidth();
			this.ParseEndOfLine();
			this.ParseCharset();
			this.ParseTrimTrailingWhitespace();
			this.ParseInsertFinalNewline();
			
		}

		private void ParseIndentStyle()
		{
			string indentStyle;
			if (!_properties.TryGetValue("indent_style", out indentStyle)) return;

			switch (indentStyle)
			{
				case "space":
					this.IndentStyle = EditorConfig.Core.IndentStyle.Space;
					return;
				case "tab":
					this.IndentStyle = EditorConfig.Core.IndentStyle.Tab;
					return;
			}
		}

		private void ParseIndentSize()
		{
			string indentSize;
			if (!_properties.TryGetValue("indent_size", out indentSize)) return;

			switch (indentSize)
			{
				case "tab":
					this.IndentSize = new IndentSize(useTabs: true);
					return;
				default:
					int size;
					if (int.TryParse(indentSize, out size) && size > 0)
						this.IndentSize = new IndentSize(size);
					return;
			}
		}

		private void ParseTabWidth()
		{
			//default to indent_size when indent size is a number
			if (this.IndentSize != null && this.IndentSize.NumberOfColumns.HasValue)
				this.TabWidth = this.IndentSize.NumberOfColumns.Value;

			string tabWidth;
			if (!_properties.TryGetValue("tab_width", out tabWidth)) return;
			
			int width;
			if (int.TryParse(tabWidth, out width) && width > 0)
				this.TabWidth = width;
		}

		private void ParseEndOfLine()
		{
			string endOfLine;
			if (!_properties.TryGetValue("end_of_line", out endOfLine)) return;

			switch (endOfLine)
			{
				case "lf":
					this.EndOfLine = EditorConfig.Core.EndOfLine.LF;
					return;
				case "cr":
					this.EndOfLine = EditorConfig.Core.EndOfLine.CR;
					return;
				case "crlf":
					this.EndOfLine = EditorConfig.Core.EndOfLine.CRLF;
					return;
			}
		}

		private void ParseCharset()
		{
			string charset;
			if (!_properties.TryGetValue("charset", out charset)) return;

			switch (charset)
			{
				case "latin1":
					this.Charset = EditorConfig.Core.Charset.Latin1;
					return;
				case "utf-16-be":
					this.Charset = EditorConfig.Core.Charset.UTF16BE;
					return;
				case "utf-16le":
					this.Charset = EditorConfig.Core.Charset.UTF16LE;
					return;
				case "utf-8":
					this.Charset = EditorConfig.Core.Charset.UTF8;
					return;
				case "utf-8-bom":
					this.Charset = EditorConfig.Core.Charset.UTF8BOM;
					return;
			}
		}

		private void ParseTrimTrailingWhitespace()
		{
			string trimTrailingWhitespace;
			if (!_properties.TryGetValue("trim_trailing_whitespace", out trimTrailingWhitespace)) return;

			bool trim;
			if (bool.TryParse(trimTrailingWhitespace, out trim))
				this.TrimTrailingWhitespace = trim;
		}

		private void ParseInsertFinalNewline()
		{
			string insertFinalNewline;
			if (!_properties.TryGetValue("insert_final_newline", out insertFinalNewline)) return;

			bool insert;
			if (bool.TryParse(insertFinalNewline, out insert))
				this.InsertFinalNewline = insert;
		}
	}
}
