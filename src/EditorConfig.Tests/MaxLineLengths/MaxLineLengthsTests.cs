using System.Reflection;
using AwesomeAssertions;
using TUnit;

namespace EditorConfig.Tests.MaxLineLengths
{
	internal class MaxLineLengthsTests : EditorConfigTestBase
	{
		[Test]
		public void PositiveNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".positive.editorconfig");
			file.MaxLineLength.Should().HaveValue();
			file.MaxLineLength.Value.Should().Be(120);
		}

		[Test]
		public void NegativeNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".negative.editorconfig");
			file.MaxLineLength.Should().NotHaveValue();
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.MaxLineLength.Should().NotHaveValue();
			HasBogusKey(file, "max_line_length");
		}
	}
}
