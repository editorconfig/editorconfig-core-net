using System.IO.Abstractions;
using System.Reflection;
using AwesomeAssertions;
using EditorConfig.Core;
using TUnit;

namespace EditorConfig.Tests.Caching
{
	public class CachingTests : EditorConfigTestBase
	{
		[Test]
		public void FileShouldCached()
		{
			var fileName = GetFileFromMethod(MethodBase.GetCurrentMethod(), ".editorconfig");

			var fileSystem = new FileSystem();
			var parser = new EditorConfigParser(
				f => EditorConfigFileCache.GetOrCreate(f, fileSystem),
				fileSystem: fileSystem);
			var config1 = parser.Parse(fileName);
			config1.EditorConfigFiles.Should().NotBeNullOrEmpty();
			config1.EditorConfigFiles.Should().OnlyContain(f => !string.IsNullOrEmpty(f.CacheKey));
			var config2 = parser.Parse(fileName);
			config2.EditorConfigFiles.Should().NotBeNullOrEmpty();
			config2.EditorConfigFiles.Should().OnlyContain(f => !string.IsNullOrEmpty(f.CacheKey));
		}
	}
}
