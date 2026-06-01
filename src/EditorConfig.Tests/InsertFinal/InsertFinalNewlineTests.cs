using System.Reflection;
using AwesomeAssertions;
using TUnit;

namespace EditorConfig.Tests.InsertFinal
{
	internal class InsertFinalNewlineTests : EditorConfigTestBase
	{
		[Test]
		public void True()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".true.editorconfig");
			file.InsertFinalNewline.Should().BeTrue();
		}

		[Test]
		public void False()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".false.editorconfig");
			file.InsertFinalNewline.Should().BeFalse();
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.InsertFinalNewline.Should().NotHaveValue();
			HasBogusKey(file, "insert_final_newline");
		}
	}
}
