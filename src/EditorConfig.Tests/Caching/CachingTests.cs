using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
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

			var parser = new EditorConfigParser();
			var config1 = parser.Parse(fileName);
			config1.EditorConfigFiles.Should().NotBeNullOrEmpty();
			config1.EditorConfigFiles.Should().OnlyContain(f => !string.IsNullOrEmpty(f.CacheKey));
			var config2 = parser.Parse(fileName);
			config2.EditorConfigFiles.Should().NotBeNullOrEmpty();
			config2.EditorConfigFiles.Should().OnlyContain(f => !string.IsNullOrEmpty(f.CacheKey));
		}

		// Regression test for https://github.com/editorconfig/editorconfig-core-net/issues/64
		// Two unrelated MockFileSystem instances that happen to place a same-length
		// .editorconfig at the same path with the same LastWriteTime must never share a
		// cached parse — each EditorConfigParser gets its own private EditorConfigFileCache
		// by default, so this is impossible by construction.
		[Test]
		public void TwoDistinctFileSystems_DoNotCollide_WithDefaultPrivateCache()
		{
			var configA = "root = true\n\n[*.cs]\nindent_size = 2\n";
			var configB = "root = true\n\n[*.cs]\nindent_size = 4\n";
			var sharedTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

			var fsA = new MockFileSystem();
			fsA.AddFile("/repo/.editorconfig", new MockFileData(configA) { LastWriteTime = sharedTime });
			fsA.AddFile("/repo/Foo.cs", new MockFileData(""));

			var fsB = new MockFileSystem();
			fsB.AddFile("/repo/.editorconfig", new MockFileData(configB) { LastWriteTime = sharedTime });
			fsB.AddFile("/repo/Foo.cs", new MockFileData(""));

			var resultA = new EditorConfigParser(fsA).Parse("/repo/Foo.cs");
			var resultB = new EditorConfigParser(fsB).Parse("/repo/Foo.cs");

			resultA.Properties["indent_size"].Should().Be("2");
			resultB.Properties["indent_size"].Should().Be("4");
		}

		// Same collision scenario as above, but explicitly sharing one EditorConfigFileCache
		// across the two parsers. Verifies the filesystem-identity-aware key in
		// EditorConfigFileCache itself, independent of per-parser instance scoping.
		[Test]
		public void TwoDistinctFileSystems_DoNotCollide_WhenSharingOneFileCache()
		{
			var configA = "root = true\n\n[*.cs]\nindent_size = 2\n";
			var configB = "root = true\n\n[*.cs]\nindent_size = 4\n";
			var sharedTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

			var fsA = new MockFileSystem();
			fsA.AddFile("/repo/.editorconfig", new MockFileData(configA) { LastWriteTime = sharedTime });
			fsA.AddFile("/repo/Foo.cs", new MockFileData(""));

			var fsB = new MockFileSystem();
			fsB.AddFile("/repo/.editorconfig", new MockFileData(configB) { LastWriteTime = sharedTime });
			fsB.AddFile("/repo/Foo.cs", new MockFileData(""));

			var sharedCache = new EditorConfigFileCache();
			var resultA = new EditorConfigParser(fsA, sharedCache).Parse("/repo/Foo.cs");
			var resultB = new EditorConfigParser(fsB, sharedCache).Parse("/repo/Foo.cs");

			resultA.Properties["indent_size"].Should().Be("2");
			resultB.Properties["indent_size"].Should().Be("4");
		}

		// Opting into a shared EditorConfigFileCache across parsers on the *same* filesystem
		// should actually reuse the cached EditorConfigFile instance.
		[Test]
		public void SharedFileCache_ReusesSameEditorConfigFile_AcrossParsersOnSameFileSystem()
		{
			var fs = new MockFileSystem();
			fs.AddFile("/repo/.editorconfig", new MockFileData("root = true\n\n[*.cs]\nindent_size = 2\n"));
			fs.AddFile("/repo/Foo.cs", new MockFileData(""));
			fs.AddFile("/repo/Bar.cs", new MockFileData(""));

			var sharedCache = new EditorConfigFileCache();
			var parser1 = new EditorConfigParser(fs, sharedCache);
			var parser2 = new EditorConfigParser(fs, sharedCache);

			var config1 = parser1.Parse("/repo/Foo.cs");
			var config2 = parser2.Parse("/repo/Bar.cs");

			var file1 = config1.EditorConfigFiles.Single();
			var file2 = config2.EditorConfigFiles.Single();
			ReferenceEquals(file1, file2).Should().BeTrue();
		}

		// A missing config file is not an error (matches editorconfig-core-c, which ignores
		// I/O errors from non-existent files while walking the chain) — GetOrCreate should
		// hand back an empty, non-root result rather than throwing.
		[Test]
		public void GetOrCreate_MissingFile_DoesNotThrowAndIsNotRoot()
		{
			var fs = new MockFileSystem();
			var cache = new EditorConfigFileCache();

			var result = cache.GetOrCreate("/repo/.editorconfig", fs);

			result.Should().NotBeNull();
			result.IsRoot.Should().BeFalse();
		}

		// The target file passed to Parse never needs to exist on disk — only the ancestor
		// .editorconfig files do. This mirrors editorconfig-core-c's editorconfig_parse(),
		// which builds the candidate config paths purely from the path string.
		[Test]
		public void Parse_VirtualFile_ResolvesAncestorChainEvenWhenFileDoesNotExistOnDisk()
		{
			var fs = new MockFileSystem();
			fs.AddFile("/repo/.editorconfig", new MockFileData("root = true\n\n[*.cs]\nindent_size = 2\n"));
			// note: /repo/Ghost.cs is intentionally never added — it does not exist on disk

			var config = new EditorConfigParser(fs).Parse("/repo/Ghost.cs");

			config.Properties["indent_size"].Should().Be("2");
		}
	}
}
