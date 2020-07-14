using System.Reflection;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.TrimTrailing
{
	[TestFixture]
	internal class TrimTrailingWhitespaceTests : EditorConfigTestBase
	{
		[Test]
		public void True()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".true.editorconfig");
			file.TrimTrailingWhitespace.Should().BeTrue();
		}

		[Test]
		public void False()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".false.editorconfig");
			file.TrimTrailingWhitespace.Should().BeFalse();
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.TrimTrailingWhitespace.Should().NotHaveValue();
			HasBogusKey(file, "trim_trailing_whitespace");
		}

	}
}
