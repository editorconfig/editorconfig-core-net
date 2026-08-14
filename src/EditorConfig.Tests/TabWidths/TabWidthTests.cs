using System.Reflection;
using AwesomeAssertions;
using TUnit;

namespace EditorConfig.Tests.TabWidths
{
	internal class TabWidthTests : EditorConfigTestBase
	{
		[Test]
		public void PositiveNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".positive.editorconfig");
			file.TabWidth.Should().HaveValue();
			file.TabWidth.Value.Should().Be(4);
		}

		[Test]
		public void NegativeNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".negative.editorconfig");
			file.TabWidth.Should().NotHaveValue();
		}

		[Test]
		public void TabIndentSizeAndSpecifiedTabWidth()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".tab.editorconfig");
			file.TabWidth.Should().HaveValue();
			file.TabWidth.Value.Should().Be(4);

			// Set indent_size to tab_width if indent_size is "tab"
			file.IndentSize.Should().NotBeNull();
			file.IndentSize.NumberOfColumns.Should().Be(file.TabWidth.Value);
		}

		[Test]
		public void ExplicitTabWidthNotOverwrittenByIndentSize()
		{
			// indent_size=1 and tab_width=3: the explicit tab_width must win.
			// Previously the first normalization block unconditionally set TabWidth = IndentSize,
			// overwriting the explicit value.
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".explicit-tab-width.editorconfig");
			file.TabWidth.Should().HaveValue();
			file.TabWidth.Value.Should().Be(3);
			file.IndentSize.Should().NotBeNull();
			file.IndentSize.NumberOfColumns.Should().Be(1);
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.IndentSize.Should().BeNull();
			HasBogusKey(file, "tab_width");
		}
	}
}
