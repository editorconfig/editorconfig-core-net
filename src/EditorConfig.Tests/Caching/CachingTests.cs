using System.Reflection;
using EditorConfig.Core;
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
			var config2 = parser.Parse(fileName);
			// Not sure how to assert this...
		}
	}
}
