﻿namespace EditorConfig.Core
{
	/// <summary>
	/// set to latin1, utf-8, utf-8-bom, utf-16be or utf-16le to control the character set. Use of utf-8-bom is discouraged.
	/// </summary>
	public enum Charset
	{
#pragma warning disable CS1591
		Latin1,
		UTF8,
		/// <summary>
		/// Usage of UFT8BOM is discouraged
		/// </summary>
		UTF8BOM,
		UTF16BE,
		UTF16LE,
#pragma warning restore CS1591

	}
}
