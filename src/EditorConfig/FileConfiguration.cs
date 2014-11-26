using System;
using System.Collections.Generic;
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
	public class FileConfiguration
	{
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

		//TODO strongly type KnownProperties

		private readonly Dictionary<string, string> _properties;

		public IDictionary<string, string> Properties { get { return _properties; } }

		public string FileName { get; private set; }

		public Version Version { get; private set; }

		internal FileConfiguration(Version version, string fileName, Dictionary<string, string> properties)
		{
			FileName = fileName;
			Version = version;
			_properties = this.SanitizeProperties(properties ?? new Dictionary<string, string>());
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
	}
}
