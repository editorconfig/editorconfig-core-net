using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.InsertFinal
{
	[TestFixture]
	class InsertFinalNewlineTests : EditorConfigTestBase
	{
		[Test]
		public void True()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".true.editorconfig");
			file.InsertFinalNewline.Should().BeTrue();
		}

		[Test]
		public void False()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".false.editorconfig");
			file.InsertFinalNewline.Should().BeFalse();
		}

		[Test]
		public void Bogus()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.InsertFinalNewline.Should().NotHaveValue();
			this.HasBogusKey(file, "insert_final_newline");
		}

	}
}
