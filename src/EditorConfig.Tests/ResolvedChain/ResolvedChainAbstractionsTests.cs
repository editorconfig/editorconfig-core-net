using System.IO;
using System.IO.Abstractions;
using AwesomeAssertions;
using EditorConfig.Core;
using TUnit;

namespace EditorConfig.Tests.ResolvedChain
{
	public class ResolvedChainAbstractionsTests
	{
		private static string GetTestDirectory(string subfolder)
		{
			var cwd = System.Environment.CurrentDirectory;
			var sep = Path.DirectorySeparatorChar.ToString();
			cwd = cwd
				.Replace($"bin{sep}net10.0{sep}Release", "")
				.Replace($"bin{sep}net10.0{sep}Debug", "");
			return Path.GetFullPath(Path.Combine(cwd, subfolder));
		}

		[Test]
		public void Parse_IFileInfo_ReturnsSameAsStringOverload()
		{
			var dir = GetTestDirectory("TabWidths");
			var filePath = Path.Combine(dir, "f.x");
			var fs = new FileSystem();
			var parser = new EditorConfigParser(".positive.editorconfig");

			var fromString = parser.Parse(filePath);
			var fromFileInfo = parser.Parse(fs.FileInfo.New(filePath));

			fromFileInfo.TabWidth.Should().Be(fromString.TabWidth);
			fromFileInfo.FileName.Should().Be(fromString.FileName);
		}

		[Test]
		public void GetResolvedChain_IFileInfo_ReturnsSameChainAsStringOverload()
		{
			var dir = GetTestDirectory("TabWidths");
			var filePath = Path.Combine(dir, "f.x");
			var fs = new FileSystem();
			var parser = new EditorConfigParser(".positive.editorconfig");

			var fromString   = parser.GetResolvedChain(filePath);
			var fromFileInfo = parser.GetResolvedChain(fs.FileInfo.New(filePath));

			fromFileInfo.Sections.Length.Should().Be(fromString.Sections.Length);
		}

		[Test]
		public void GetResolvedChainForDirectory_IDirectoryInfo_ReturnsSameChainAsStringOverload()
		{
			var dir = GetTestDirectory("TabWidths");
			var fs = new FileSystem();
			var parser = new EditorConfigParser(".positive.editorconfig");

			var fromString    = parser.GetResolvedChainForDirectory(dir);
			var fromDirInfo   = parser.GetResolvedChainForDirectory(fs.DirectoryInfo.New(dir));

			fromDirInfo.Sections.Length.Should().Be(fromString.Sections.Length);
		}
	}
}
