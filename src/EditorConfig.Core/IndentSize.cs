﻿using System.Diagnostics;

#pragma warning disable CS1591
namespace EditorConfig.Core
{
	/// <summary>
	/// a whole number defining the number of columns used for each indentation level and the width of soft tabs (when supported).
	/// When set to tab, the value of tab_width (if specified) will be used.
	/// </summary>
	public class IndentSize
	{
		public static IndentSize Tab { get; } = new IndentSize(useTabs: true);
		public static IndentSize Unset { get; } = new IndentSize();

		private static IndentSize Column1 { get; } = new IndentSize(1);
		private static IndentSize Column2 { get; } = new IndentSize(2);
		private static IndentSize Column3 { get; } = new IndentSize(3);
		private static IndentSize Column4 { get; } = new IndentSize(4);
		private static IndentSize Column5 { get; } = new IndentSize(5);
		private static IndentSize Column6 { get; } = new IndentSize(6);
		private static IndentSize Column7 { get; } = new IndentSize(7);
		private static IndentSize Column8 { get; } = new IndentSize(8);

		public static IndentSize Columns(int numberOfColumns)
		{
			switch (numberOfColumns)
			{
				case 1: return Column1;
				case 2: return Column2;
				case 3: return Column3;
				case 4: return Column4;
				case 5: return Column5;
				case 6: return Column6;
				case 7: return Column7;
				case 8: return Column8;
				default:
					return new IndentSize(numberOfColumns);
			}
		}

		public bool IsUnset { get; }
		public bool UseTabWidth { get; }
		public int? NumberOfColumns { get; }

		private IndentSize() => IsUnset = true;

		private IndentSize(bool useTabs) => UseTabWidth = useTabs;

		private IndentSize(int numberOfColumns) => NumberOfColumns = numberOfColumns;
	}
#pragma warning restore CS1591
}
