using System.Reflection;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.TrimTrailing
{
	[TestFixture]
	class TrimTrailingWhitespaceTests : EditorConfigTestBase
	{
		[Test]
		public void True()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".true.editorconfig");
			file.TrimTrailingWhitespace.Should().BeTrue();
		}

		[Test]
		public void False()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".false.editorconfig");
			file.TrimTrailingWhitespace.Should().BeFalse();
		}

		[Test]
		public void Bogus()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.TrimTrailingWhitespace.Should().NotHaveValue();
			this.HasBogusKey(file, "trim_trailing_whitespace");
		}

	}
}
