using System.IO;
using AwesomeAssertions;
using EditorConfig.Core;
using TUnit;

namespace EditorConfig.Tests.ResolvedChain
{
	public class ResolvedChainDirectoryTests
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
		public void GetResolvedChainForDirectory_ReturnsEquivalentChain()
		{
			var dir = GetTestDirectory("TabWidths");
			var probeFile = Path.Combine(dir, "probe.cs");

			var parser = new EditorConfigParser(".positive.editorconfig");

			var chainFromFile = parser.GetResolvedChain(probeFile);
			var chainFromDir  = parser.GetResolvedChainForDirectory(dir);

			chainFromDir.Sections.Length.Should().Be(chainFromFile.Sections.Length);
			for (var i = 0; i < chainFromFile.Sections.Length; i++)
				chainFromDir.Sections[i].Glob.Should().Be(chainFromFile.Sections[i].Glob);
		}

		[Test]
		public void GetConfigurationFilesTillRootFromDirectory_ReturnsEquivalentList()
		{
			var dir = GetTestDirectory("TabWidths");
			var probeFile = Path.Combine(dir, "probe.cs");

			var parser = new EditorConfigParser(".positive.editorconfig");

			var fromFile = parser.GetConfigurationFilesTillRoot(probeFile);
			var fromDir  = parser.GetConfigurationFilesTillRootFromDirectory(dir);

			fromDir.Count.Should().Be(fromFile.Count);
			for (var i = 0; i < fromFile.Count; i++)
				fromDir[i].Directory.Should().Be(fromFile[i].Directory);
		}

		[Test]
		public void GetResolvedChainForDirectory_ParsesFileCorrectly()
		{
			var dir = GetTestDirectory("TabWidths");
			var parser = new EditorConfigParser(".positive.editorconfig");

			var chain = parser.GetResolvedChainForDirectory(dir);
			var config = parser.Parse(Path.Combine(dir, "f.x"), chain);

			config.TabWidth.Should().HaveValue();
			config.TabWidth!.Value.Should().Be(4);
		}
	}
}
