using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EditorConfig.Core
{

	public class FileConfiguration 
	{
		private List<ConfigSection> Sections { get; }

		/// <summary>
		/// set to tab or space to use hard tabs or soft tabs respectively.
		/// </summary>
		public IndentStyle? IndentStyle => Sections.FirstOrDefault(s => s.IndentStyle.HasValue)?.IndentStyle;

		private readonly IndentSize _indentSizeOverride = null;
		/// <summary>
		/// a whole number defining the number of columns used for each indentation level and the width of soft tabs (when supported). 
		/// When set to tab, the value of tab_width (if specified) will be used.
		/// </summary>
		public IndentSize IndentSize=> _indentSizeOverride ?? Sections.LastOrDefault(s => s.IndentSize != null)?.IndentSize;

		private readonly int? _tabWidthOverride;
		/// <summary>
		/// a whole number defining the number of columns used to represent a tab character. 
		/// This defaults to the value of indent_size and doesn't usually need to be specified.
		/// </summary>
		public int? TabWidth => _tabWidthOverride ?? Sections.FirstOrDefault(s => s.TabWidth.HasValue)?.TabWidth; 
		
		/// <summary>
		/// set to lf, cr, or crlf to control how line breaks are represented.
		/// </summary>
		public EndOfLine? EndOfLine=> Sections.FirstOrDefault(s => s.EndOfLine.HasValue)?.EndOfLine; 
		
		/// <summary>
		/// set to latin1, utf-8, utf-8-bom, utf-16be or utf-16le to control the character set. Use of utf-8-bom is discouraged.
		/// </summary>
		public Charset? Charset=> Sections.FirstOrDefault(s => s.Charset.HasValue)?.Charset; 
		
		/// <summary>
		/// set to true to remove any whitespace characters preceding newline characters and false to ensure it doesn't.
		/// </summary>
		public bool? TrimTrailingWhitespace=> Sections.FirstOrDefault(s => s.TrimTrailingWhitespace.HasValue)?.TrimTrailingWhitespace; 
		
		/// <summary>
		/// set to true ensure file ends with a newline when saving and false to ensure it doesn't.
		/// </summary>
		public bool? InsertFinalNewline=> Sections.FirstOrDefault(s => s.InsertFinalNewline.HasValue)?.InsertFinalNewline; 
		
		/// <summary>
		/// Forces hard line wrapping after the amount of characters specified
		/// </summary>
		public int? MaxLineLength=> Sections.FirstOrDefault(s => s.MaxLineLength.HasValue)?.MaxLineLength;

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

			//default tab_width to indent_size when indent size is a number
			if (this.IndentSize != null && this.IndentSize.NumberOfColumns.HasValue)
			{
				this._tabWidthOverride = this.IndentSize.NumberOfColumns.Value;
				properties["tab_width"] = this._tabWidthOverride.ToString();
			}

			// Set indent_size to "tab" if indent_size is unspecified and indent_style is set to "tab".
			if (this.IndentStyle.HasValue && this.IndentStyle == Core.IndentStyle.Tab && this.IndentSize == null
			    && Version >= new Version(0, 10))
			{
				this._indentSizeOverride = IndentSize.Tab;
				properties["indent_size"] = "tab";
			}

			// Set tab_width to indent_size if indent_size is specified and tab_width is unspecified
			if (this.IndentSize != null && !this.TabWidth.HasValue && !this.IndentSize.UseTabWidth)
			{
				//only set tab_width to indent_size if indent size holds a positive integer
				if (this.IndentSize.NumberOfColumns.HasValue && this.IndentSize.NumberOfColumns.Value >= 0)
				{
					this._tabWidthOverride = this.IndentSize.NumberOfColumns.Value;
					properties["tab_width"] = this._tabWidthOverride.ToString();
				}
				
				// unset carries over see:
				//  ctest . -R "unset_indent_size"
				else if (this.IndentSize.IsUnset)
				{
					this._tabWidthOverride =  new int?();
					properties["tab_width"] = "unset";
				}
			}

			// Set indent_size to tab_width if indent_size is "tab"
			if (this.IndentSize != null && this.TabWidth != null && this.IndentSize.UseTabWidth)
			{
				this._indentSizeOverride = IndentSize.Columns(this.TabWidth.Value);
				properties["indent_size"] = this.TabWidth.Value.ToString();
			}
			
			Properties = new ReadOnlyDictionary<string, string>(properties);
		}
	}
}
