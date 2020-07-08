using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.Defaults
{
	[TestFixture]
	class DefaultsTests : EditorConfigTestBase
	{
		[Test]
		public void CanFindFile()
		{
			//We only place an editorconfig in this folder to force root.
			//An editorconfig file is not necessary for defaults but we do not want any parent 
			//config files leaking into our test
			var file = this.GetFileFromMethod(MethodBase.GetCurrentMethod(), ".editorconfig");
			File.Exists(file).Should().BeTrue();
		}

		[Test]
		public void DefaultsInitializeToExpectedValues()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "somefile.randomextension");
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
