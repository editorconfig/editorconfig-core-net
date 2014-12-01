using System.Reflection;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.EndOfLines
{
	[TestFixture]
	class EndOfLineTests : EditorConfigTestBase
	{
		[Test]
		public void Lf()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".lf.editorconfig");
			file.EndOfLine.Should().Be(EndOfLine.LF);
		}

		[Test]
		public void Cr()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".cr.editorconfig");
			file.EndOfLine.Should().Be(EndOfLine.CR);
		}

		[Test]
		public void Crlf()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".crlf.editorconfig");
			file.EndOfLine.Should().Be(EndOfLine.CRLF);
		}

		[Test]
		public void Bogus()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.EndOfLine.Should().BeNull();
			this.HasBogusKey(file, "end_of_line");
		}

	}
}
