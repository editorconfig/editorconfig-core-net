using System.Reflection;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.IndentStyles
{
	[TestFixture]
	internal class IndentStyleTests : EditorConfigTestBase
	{
		[Test]
		public void Space()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".space.editorconfig");
			file.IndentStyle.Should().Be(IndentStyle.Space);
		}

		[Test]
		public void Tab()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".tab.editorconfig");
			file.IndentStyle.Should().Be(IndentStyle.Tab);

			file.IndentSize.Should().NotBeNull();
			file.IndentSize.UseTabWidth.Should().BeTrue();
			
			// Set indent_size to "tab" if indent_size is unspecified and indent_style is set to "tab".
			file.Properties.Should().HaveCount(2);
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.IndentStyle.Should().BeNull();
			HasBogusKey(file, "indent_style");
		}

	}
}
