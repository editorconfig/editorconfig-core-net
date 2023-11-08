using System.IO;
using EditorConfig.Core;
using FluentAssertions;
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
			var configContent = @"""
			                    root = true

			                    [*.cs]
			                    end_of_line = lf
			                    """;
			var stringReader = new StringReader(configContent);
			var editorConfigFile = EditorConfigFile.Parse(stringReader, "C://VirtualPath");

			var parser = new EditorConfigParser();

			var config1 = parser.Parse("C://VirtualPath/myfile.cs", new[] { editorConfigFile });
			config1.EditorConfigFiles.Should().ContainSingle(f => f.IsRoot);
			config1.EndOfLine.Should().Be(EndOfLine.LF);

			var config2 = parser.Parse("C://DifferentFolder/myfile.cs", new[] { editorConfigFile });
			config2.EditorConfigFiles.Should().BeEmpty();
		}
	}
}
