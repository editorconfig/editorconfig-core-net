using System.Reflection;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.IndentStyles
{
	[TestFixture]
	class IndentStyleTests : EditorConfigTestBase
	{
		[Test]
		public void Space()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".space.editorconfig");
			file.IndentStyle.Should().Be(IndentStyle.Space);
		}

		[Test]
		public void Tab()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".tab.editorconfig");
			file.IndentStyle.Should().Be(IndentStyle.Tab);

			// Set indent_size to "tab" if indent_size is unspecified and indent_style is set to "tab".
			file.Properties.Should().HaveCount(2);
			file.IndentSize.Should().NotBeNull();
			file.IndentSize.UseTabWidth.Should().BeTrue();
		}

		[Test]
		public void Bogus()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.IndentStyle.Should().BeNull();
			this.HasBogusKey(file, "indent_style");
		}

	}
}
