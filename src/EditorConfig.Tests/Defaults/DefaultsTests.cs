using System.IO;
using System.Reflection;
using AwesomeAssertions;
using EditorConfig.Core;
using TUnit;

namespace EditorConfig.Tests.Defaults
{
	internal class DefaultsTests : EditorConfigTestBase
	{
		[Test]
		public void CanFindFile()
		{
			//We only place an editorconfig in this folder to force root.
			//An editorconfig file is not necessary for defaults but we do not want any parent
			//config files leaking into our test
			var file = GetFileFromMethod(MethodBase.GetCurrentMethod(), ".editorconfig");
			File.Exists(file).Should().BeTrue();
		}

		[Test]
		public void DefaultsInitializeToExpectedValues()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "somefile.randomextension");
			file.Should().NotBeNull();
			file.FileName.Should().EndWith("somefile.randomextension");
			file.Charset.Should().BeNull();
			file.EndOfLine.Should().BeNull();
			file.IndentSize.Should().BeNull();
			file.IndentStyle.Should().BeNull();
			file.InsertFinalNewline.Should().NotHaveValue();
			file.Properties.Should().NotBeNull().And.BeEmpty();
			file.TabWidth.Should().NotHaveValue();
			file.TrimTrailingWhitespace.Should().NotHaveValue();
			file.Version.Should().Be(EditorConfigParser.Version);
		}
	}
}
