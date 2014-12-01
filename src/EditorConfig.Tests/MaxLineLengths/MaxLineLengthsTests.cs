using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.MaxLineLengths
{
	[TestFixture]
	class MaxLineLengthsTests : EditorConfigTestBase
	{
		[Test]
		public void PositiveNumber()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".positive.editorconfig");
			file.MaxLineLength.Should().HaveValue();
			file.MaxLineLength.Value.Should().Be(120);
		}

		[Test]
		public void NegativeNumber()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".negative.editorconfig");
			file.MaxLineLength.Should().NotHaveValue();
		}

		[Test]
		public void Bogus()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.MaxLineLength.Should().NotHaveValue();
			this.HasBogusKey(file,"max_line_length");

		}

	}
}
