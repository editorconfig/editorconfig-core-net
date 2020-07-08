using System.Reflection;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.IndentSizes
{
	[TestFixture]
	class IndentSizesTests : EditorConfigTestBase
	{
		[Test]
		public void PositiveNumber()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".positive.editorconfig");
			file.IndentSize.Should().NotBeNull();
			file.IndentSize.NumberOfColumns.Should().Be(2);
			file.IndentSize.UseTabWidth.Should().BeFalse();
			
			file.TabWidth.Should().Be(file.IndentSize.NumberOfColumns);
			
			//tab_width is unspecified and indent_size is a positive integer, editorconfig dictates 
			//that tabwidth should thus default to indent_size
			file.Properties.Should().HaveCount(2);
		}

		[Test]
		public void NegativeNumber()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".negative.editorconfig");
			file.IndentSize.Should().BeNull();
		}

		[Test]
		public void Tab()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".tab.editorconfig");
			file.IndentSize.Should().NotBeNull();
			file.IndentSize.NumberOfColumns.Should().NotHaveValue();
			file.IndentSize.UseTabWidth.Should().BeTrue();
		}

		[Test]
		public void Bogus()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.IndentSize.Should().BeNull();
			this.HasBogusKey(file,"indent_size");

		}

	}
}
