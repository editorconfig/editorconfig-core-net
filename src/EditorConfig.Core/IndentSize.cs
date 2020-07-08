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
		
		public bool IsUnset { get; }
		public bool UseTabWidth { get; }
		public int? NumberOfColumns { get; }

		internal IndentSize() => IsUnset = true;

		internal IndentSize(bool useTabs) => UseTabWidth = useTabs;

		public IndentSize(int numberOfColumns)
		{
			this.NumberOfColumns = numberOfColumns;
		}
	}
}