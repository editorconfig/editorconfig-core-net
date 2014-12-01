using System.Reflection;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.Charsets
{
	[TestFixture]
	class CharsetTests : EditorConfigTestBase
	{
		[Test]
		public void Utf8()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".utf8.editorconfig");
			file.Charset.Should().Be(Charset.UTF8);
		}

		[Test]
		public void Utf8Bom()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".utf8-bom.editorconfig");
			file.Charset.Should().Be(Charset.UTF8BOM);
		}

		[Test]
		public void Utf16le()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".utf16le.editorconfig");
			file.Charset.Should().Be(Charset.UTF16LE);
		}

		[Test]
		public void Utf16be()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".utf16be.editorconfig");
			file.Charset.Should().Be(Charset.UTF16BE);
		}

		[Test]
		public void Latin1()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".latin1.editorconfig");
			file.Charset.Should().Be(Charset.Latin1);
		}

		[Test]
		public void Bogus()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.Charset.Should().BeNull();
			this.HasBogusKey(file, "charset");
		}

	}
}
