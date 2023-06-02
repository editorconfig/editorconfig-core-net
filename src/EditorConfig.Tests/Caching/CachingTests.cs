using System.Reflection;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.Caching
{
	[TestFixture]
	public class CachingTests : EditorConfigTestBase
	{
		[Test]
		public void FileShouldCached()
		{
			var fileName = GetFileFromMethod(MethodBase.GetCurrentMethod(),  ".editorconfig");

			var parser = new EditorConfigParser(EditorConfigFileCache.GetOrCreate);
			var config1 = parser.Parse(fileName);
			config1.EditorConfigFiles.Should().NotBeNullOrEmpty();
			config1.EditorConfigFiles.Should().OnlyContain(f => !string.IsNullOrEmpty(f.CacheKey));
			var config2 = parser.Parse(fileName);
			config2.EditorConfigFiles.Should().NotBeNullOrEmpty();
			config2.EditorConfigFiles.Should().OnlyContain(f => !string.IsNullOrEmpty(f.CacheKey));
		}
	}
}
