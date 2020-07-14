using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.TabWidths
{
	[TestFixture]
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
		public void TabIndenSizeAndSpecifiedTabWidth()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".tab.editorconfig");
			file.TabWidth.Should().HaveValue();
			file.TabWidth.Value.Should().Be(4);

			// Set indent_size to tab_width if indent_size is "tab"
			file.IndentSize.Should().NotBeNull();
			file.IndentSize.NumberOfColumns.Should().Be(file.TabWidth.Value);
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.IndentSize.Should().BeNull();
			HasBogusKey(file,"tab_width");

		}

	}
}
