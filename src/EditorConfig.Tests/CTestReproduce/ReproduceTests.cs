using System.Reflection;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.CTestReproduce
{
	[TestFixture]
	public class ReproduceTests : EditorConfigTestBase
	{
		[Test]
		public void Backslash2()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), @"\\.txt", "backslash2.editorconfig");

			AssertHasProperty("backslash", file);
		}
		
		//indent_size_default_with_tab_width_ML
		[Test]
		public void IndentSizeDefaultWithTabWidthML()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), @"test3.c", "indent_size_ml.editorconfig");

			file.Properties.Should().HaveCount(3);
		}
		
		//unset_indent_size_ML
		[Test]
		public void UnsetIndentSize()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), @"unset/indent_size.txt", "unset.editorconfig");

			file.Properties.Should().HaveCount(2);
			AssertHasProperty("indent_size", file);
			AssertHasProperty("tab_width", file);
			file.Properties["indent_size"].Should().Be("unset");
			file.Properties["tab_width"].Should().Be("unset");
		}

	}
}
