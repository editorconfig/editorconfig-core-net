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

			                    [*]
			                    end_of_line = lf
			                    """;
			var configDirectory = "C://VirtualPath/";
			var stringReader = new StringReader(configContent);
			var editorConfigFile = EditorConfigFile.Parse(stringReader, configDirectory);

			var parser = new EditorConfigParser();
			var config = parser.Parse("C://VirtualPath/myfile.cs", new[] { editorConfigFile });

			config.EditorConfigFiles.Should().ContainSingle(f => f.IsRoot);
		}
	}
}
