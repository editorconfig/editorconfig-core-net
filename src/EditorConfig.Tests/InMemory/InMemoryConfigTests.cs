using System.IO;
using EditorConfig.Core;
using FluentAssertions;
using Microsoft.VisualBasic;
using NUnit.Framework;

namespace EditorConfig.Tests.InMemory
{
	[TestFixture]
	public class CachingTests : EditorConfigTestBase
	{
		[Test]
		public void InMemoryConfigIsUsable()
		{
			var configContent = @"""
			                    root = true

			                    [*.cs]
			                    end_of_line = lf
			                    """;
			var stringReader = new StringReader(configContent);
			var editorConfigFile = EditorConfigFile.Parse(stringReader);

			var parser = new EditorConfigParser();
			var config = parser.Parse("myfile.cs", new[] { editorConfigFile });

			config.EditorConfigFiles.Should().ContainSingle(f => f.IsRoot);
			config.EndOfLine.Should().Be(EndOfLine.LF);
		}

		[Test]
		public void InMemoryConfigIsUsableWithVirtualPath()
		{
			var virtualDirectory = Path.Combine(Directory.GetDirectoryRoot("."), "VirtualPath");

			var configContent = @"""
			                    root = true

			                    [*.cs]
			                    end_of_line = lf
			                    """;
			var stringReader = new StringReader(configContent);
			var editorConfigFile = EditorConfigFile.Parse(stringReader, virtualDirectory);

			var parser = new EditorConfigParser();

			var file = Path.Combine(virtualDirectory, "myfile.cs");
			var config1 = parser.Parse(file, new[] { editorConfigFile });
			config1.EditorConfigFiles.Should().ContainSingle(f => f.IsRoot);
			config1.EndOfLine.Should().Be(EndOfLine.LF);

			var directoryOutOfScope = Path.Combine(Directory.GetDirectoryRoot("."), "DifferentDirectory");
			var fileOutOfScope = Path.Combine(directoryOutOfScope, "myfile.cs");
			var config2 = parser.Parse(fileOutOfScope, new[] { editorConfigFile });
			config2.EditorConfigFiles.Should().BeEmpty();
		}
	}
}
