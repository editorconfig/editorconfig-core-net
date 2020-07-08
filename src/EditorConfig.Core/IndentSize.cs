using System.Diagnostics;

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

		public static IndentSize Columns(int numberOfColumns) => numberOfColumns switch
		{
			1 => Column1, 2 => Column2, 3 => Column3, 4 => Column4,
			5 => Column5, 6 => Column6, 7 => Column7, 8 => Column8,
			_ => new IndentSize(numberOfColumns)
		};
		
		public bool IsUnset { get; }
		public bool UseTabWidth { get; }
		public int? NumberOfColumns { get; }

		private IndentSize() => IsUnset = true;

		private IndentSize(bool useTabs) => UseTabWidth = useTabs;

		private IndentSize(int numberOfColumns)
		{
			this.NumberOfColumns = numberOfColumns;
		}
	}
}