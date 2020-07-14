using System;
using System.Collections.Generic;
using System.Linq;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

/*
 * (c) 2018 JetBrains s.r.o., SLaks, EditorConfig Team
 * Under MIT License
 * From https://github.com/editorconfig/editorconfig-core-net
 * From https://github.com/SLaks/Minimatch
 */

namespace EditorConfig.Tests
{
	[TestFixture]
	public class MiniMatcherTests
	{
		private readonly GlobMatcherOptions _globOptions = new GlobMatcherOptions { MatchBase = true, Dot = true, AllowWindowsPaths = true };

		[Test]
		public void MatcherTest()
		{
			var glob = "C:/Projects/editorconfig-core-net/tests/filetree/top/of/path";
			var file = "C:/Projects/editorconfig-core-net/tests/filetree/top/of/path";
			var m = GlobMatcher.Create(glob, _globOptions);
			var match = m.IsMatch(file);
			match.Should().BeTrue();
		}

		private static void TestCase(string pattern, IList<string> expected, GlobMatcherOptions options = null, IEnumerable<string> input = null)
		{
			input = input ?? files;

			var filtered = input;
			var mm = GlobMatcher.Create(pattern, options);
			filtered = filtered.Where(mm.IsMatch);
			if (options != null && options.NoNull)
				filtered = filtered.DefaultIfEmpty(pattern);

			filtered = filtered.OrderBy(s => s);

			Assert.AreEqual(
				string.Join(Environment.NewLine, expected.OrderBy(s => s)),
				string.Join(Environment.NewLine, filtered),
				"Failure from `" + pattern + "`"
			);
		}

		private static void AddFiles(params string[] entries) => files.AddRange(entries);

		private static void ReplaceFiles(params string[] entries)
		{
			files.Clear();
			files.AddRange(entries);
		}

		private static readonly List<string> files = new List<string>();

		[SetUp]
		public void DefaultFiles() =>
			ReplaceFiles(
				"a", "b", "c", "d", "abc"
				, "abd", "abe", "bb", "bcd"
				, "ca", "cb", "dd", "de"
				, "bdir/", "bdir/cfile"
			);

		[Test]
		public void BashCookBook()
		{
			//"http://www.bashcookbook.com/bashinfo/source/bash-1.14.7/tests/glob-test"
			TestCase("a*", new[] { "a", "abc", "abd", "abe" });
			TestCase("X*", new[] { "X*" }, new GlobMatcherOptions { NoNull = true });

			// allow null glob expansion
			TestCase("X*", new string[0]);

			// isaacs: Slightly different than bash/sh/ksh
			// \\* is not un-escaped to literal "*" in a failed match,
			// but it does make it get treated as a literal star
			TestCase("\\*", new[] { "\\*" }, new GlobMatcherOptions { NoNull = true });
			TestCase("\\**", new[] { "\\**" }, new GlobMatcherOptions { NoNull = true });
			TestCase("\\*\\*", new[] { "\\*\\*" }, new GlobMatcherOptions { NoNull = true });

			TestCase("b*/", new[] { "bdir/" });
			TestCase("c*", new[] { "c", "ca", "cb" });
			TestCase("**", files);

			TestCase("\\.\\./*/", new[] { "\\.\\./*/" }, new GlobMatcherOptions { NoNull = true });
			TestCase("s/\\..*//", new[] { "s/\\..*//" }, new GlobMatcherOptions { NoNull = true });
		}

		[Test]
		public void LegendaryLarryCrashesBashes()
		{
			TestCase(
				"/^root:/{s/^[^:]*:[^:]*:([^:]*).*$/\\1/"
				, new[] { "/^root:/{s/^[^:]*:[^:]*:([^:]*).*$/\\1/" }, new GlobMatcherOptions { NoNull = true });
			TestCase(
				"/^root:/{s/^[^:]*:[^:]*:([^:]*).*$/\u0001/"
				, new[] { "/^root:/{s/^[^:]*:[^:]*:([^:]*).*$/\u0001/" }, new GlobMatcherOptions { NoNull = true });
		}

		[Test]
		public void CharacterClasses()
		{
			TestCase("[a-c]b*", new[] { "abc", "abd", "abe", "bb", "cb" });
			TestCase(
				"[a-y]*[^c]", new[]
				{
					"abd", "abe", "bb", "bcd",
					"bdir/", "ca", "cb", "dd", "de"
				});
			TestCase("a*[^c]", new[] { "abd", "abe" });
			AddFiles("a-b", "aXb");
			TestCase("a[X-]b", new[] { "a-b", "aXb" });
			AddFiles(".x", ".y");
			TestCase("[^a-c]*", new[] { "d", "dd", "de" });
			AddFiles("a*b/", "a*b/ooo");
			TestCase("a\\*b/*", new[] { "a*b/ooo" });
			TestCase("a\\*?/*", new[] { "a*b/ooo" });
			TestCase(
				"*\\\\!*", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "echo !7" });
			TestCase("*\\!*", new[] { "echo !7" }, null, new[] { "echo !7" });
			TestCase("*.\\*", new[] { "r.*" }, null, new[] { "r.*" });
			TestCase("a[b]c", new[] { "abc" });
			TestCase("a[\\b]c", new[] { "abc" });
			TestCase("a?c", new[] { "abc" });
			TestCase(
				"a\\*c", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "abc" });
			TestCase(
				"", new[] { "" }, new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "" });
		}

		[Test]
		public void AppleBash()
		{
			AddFiles("a-b", "aXb", ".x", ".y", "a*b/", "a*b/ooo");

			//http://www.opensource.apple.com/source/bash/bash-23/bash/tests/glob-test"
			AddFiles("man/", "man/man1/", "man/man1/bash.1");
			TestCase("*/man*/bash.*", new[] { "man/man1/bash.1" });
			TestCase("man/man1/bash.1", new[] { "man/man1/bash.1" });
			TestCase("a***c", new[] { "abc" }, null, new[] { "abc" });
			TestCase("a*****?c", new[] { "abc" }, null, new[] { "abc" });
			TestCase("?*****??", new[] { "abc" }, null, new[] { "abc" });
			TestCase("*****??", new[] { "abc" }, null, new[] { "abc" });
			TestCase("?*****?c", new[] { "abc" }, null, new[] { "abc" });
			TestCase("?***?****c", new[] { "abc" }, null, new[] { "abc" });
			TestCase("?***?****?", new[] { "abc" }, null, new[] { "abc" });
			TestCase("?***?****", new[] { "abc" }, null, new[] { "abc" });
			TestCase("*******c", new[] { "abc" }, null, new[] { "abc" });
			TestCase("*******?", new[] { "abc" }, null, new[] { "abc" });
			TestCase("a*cd**?**??k", new[] { "abcdecdhjk" }, null, new[] { "abcdecdhjk" });
			TestCase("a**?**cd**?**??k", new[] { "abcdecdhjk" }, null, new[] { "abcdecdhjk" });
			TestCase("a**?**cd**?**??k***", new[] { "abcdecdhjk" }, null, new[] { "abcdecdhjk" });
			TestCase("a**?**cd**?**??***k", new[] { "abcdecdhjk" }, null, new[] { "abcdecdhjk" });
			TestCase("a**?**cd**?**??***k**", new[] { "abcdecdhjk" }, null, new[] { "abcdecdhjk" });
			TestCase("a****c**?**??*****", new[] { "abcdecdhjk" }, null, new[] { "abcdecdhjk" });
			TestCase("[-abc]", new[] { "-" }, null, new[] { "-" });
			TestCase("[abc-]", new[] { "-" }, null, new[] { "-" });
			TestCase("\\", new[] { "\\" }, null, new[] { "\\" });
			TestCase("[\\\\]", new[] { "\\" }, null, new[] { "\\" });
			TestCase("[[]", new[] { "[" }, null, new[] { "[" });
			TestCase("[", new[] { "[" }, null, new[] { "[" });


			// a right bracket shall lose its special meaning and
			// represent itself in a bracket expression if it occurs
			// first in the list.  -- POSIX.2 2.8.3.2
			TestCase("[*", new[] { "[abc" }, null, new[] { "[abc" });


			TestCase("[]]", new[] { "]" }, null, new[] { "]" });
			TestCase("[]-]", new[] { "]" }, null, new[] { "]" });
			TestCase(@"[a-\z]", new[] { "p" }, null, new[] { "p" });
			TestCase(
				"??**********?****?", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "abc" });
			TestCase(
				"??**********?****c", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "abc" });
			TestCase(
				"?************c****?****", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "abc" });
			TestCase(
				"*c*?**", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "abc" });
			TestCase(
				"a*****c*?**", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "abc" });
			TestCase(
				"a********???*******", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "abc" });
			TestCase(
				"[]", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "a" });
			TestCase(
				"[abc", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "[" });
		}

		[Test]
		public void NoCase()
		{
			AddFiles("a-b", "aXb", ".x", ".y", "a*b/", "a*b/ooo", "man/", "man/man1/", "man/man1/bash.1");


			TestCase(
				"XYZ", new[] { "xYz" }, new GlobMatcherOptions { IgnoreCase = true, /*null = true*/ }
				, new[] { "xYz", "ABC", "IjK" });
			TestCase(
				"ab*", new[] { "ABC" }, new GlobMatcherOptions { IgnoreCase = true, /*null = true*/ }
				, new[] { "xYz", "ABC", "IjK" });
			TestCase(
				"[ia]?[ck]", new[] { "ABC", "IjK" }, new GlobMatcherOptions { IgnoreCase = true, /*null = true*/ }
				, new[] { "xYz", "ABC", "IjK" });
		}

		[Test]
		public void OneStar_TwoStar()
		{
			AddFiles("a-b", "aXb", ".x", ".y", "a*b/", "a*b/ooo", "man/", "man/man1/", "man/man1/bash.1");

			// [ pattern, new [] { matches }, MM opts, files, TAP opts]
			TestCase(
				"{/*,*}", new string[0], new GlobMatcherOptions
				{
					/*null = true*/
				}, new[] { "/asdf/asdf/asdf" });
			TestCase(
				"{/?,*}", new[] { "/a", "bb" }, new GlobMatcherOptions
				{
					/*null = true*/
				}
				, new[] { "/a", "/b/b", "/a/b/c", "bb" });
		}

		[Test]
		public void DotMatching()
		{
			AddFiles("a-b", "aXb", ".x", ".y", "a*b/", "a*b/ooo", "man/", "man/man1/", "man/man1/bash.1");

			//"Dots should not match unless requested"
			TestCase("**", new[] { "a/b" }, new GlobMatcherOptions { }, new[] { "a/b", "a/.d", ".a/.d" });

			// .. and . can only match patterns starting with .,
			// even when options.Dot is set.
			ReplaceFiles("a/./b", "a/../b", "a/c/b", "a/.d/b");
			TestCase("a/*/b", new[] { "a/c/b", "a/.d/b" }, new GlobMatcherOptions { Dot = true });
			TestCase("a/.*/b", new[] { "a/./b", "a/../b", "a/.d/b" }, new GlobMatcherOptions { Dot = true });
			TestCase("a/*/b", new[] { "a/c/b" }, new GlobMatcherOptions { Dot = false });
			TestCase("a/.*/b", new[] { "a/./b", "a/../b", "a/.d/b" }, new GlobMatcherOptions { Dot = false });


			// this also tests that changing the options needs
			// to change the cache key, even if the pattern is
			// the same!
			TestCase(
				"**", new[] { "a/b", "a/.d", ".a/.d" }, new GlobMatcherOptions { Dot = true }
				, new[] { ".a/.d", "a/.d", "a/b" });
		}

		[Test]
		public void ParenSlashes()
		{
			//AddFiles("a-b", "aXb", ".x", ".y", "a*b/", "a*b/ooo", "man/", "man/man1/", "man/man1/bash.1");

			//"paren sets cannot contain slashes"
			TestCase("*(a/b)", new[] { "*(a/b)" }, new GlobMatcherOptions { NoNull = true }, new[] { "a/b" });

			// brace sets trump all else.
			//
			// invalid glob pattern.  fails on bash4 and bsdglob.
			// however, in this implementation, it's easier just
			// to do the intuitive thing, and let brace-expansion
			// actually come before parsing any extglob patterns,
			// like the documentation seems to say.
			//
			// XXX: if anyone complains about this, either fix it
			// or tell them to grow up and stop complaining.
			//
			// bash/bsdglob says this:
			// , new [] { "*(a|{b),c)}", ["*(a|{b),c)}" }, new Options {}, new [] { "a", "ab", "ac", "ad" });
			// but we do this instead:
			//TestCase("*(a|{b),c)}", new[] { "a", "ab", "ac" }, new EditorConfigMinimatcherOptions { }, new[] { "a", "ab", "ac", "ad" });

			// test partial parsing in the presence of comment/negation chars
			TestCase("[!a*", new[] { "[!ab" }, new GlobMatcherOptions { }, new[] { "[!ab", "[ab" });
			TestCase("[#a*", new[] { "[#ab" }, new GlobMatcherOptions { }, new[] { "[#ab", "[ab" });

			// like: {a,b|c\\,d\\\|e} except it's unclosed, so it has to be escaped.
			/*TestCase(
			  "+(a|*\\|c\\\\|d\\\\\\|e\\\\\\\\|f\\\\\\\\\\|g"
			  , new[] { "+(a|b\\|c\\\\|d\\\\|e\\\\\\\\|f\\\\\\\\|g" }
			  , new EditorConfigMinimatcherOptions { }
			  , new[] { "+(a|b\\|c\\\\|d\\\\|e\\\\\\\\|f\\\\\\\\|g", "a", "b\\c" });
			*/

			// crazy nested {,,} and *(||) tests.
			ReplaceFiles(
				"a", "b", "c", "d"
				, "ab", "ac", "ad"
				, "bc", "cb"
				, "bc,d", "c,db", "c,d"
				, "d)", "(b|c", "*(b|c"
				, "b|c", "b|cc", "cb|c"
				, "x(a|b|c)", "x(a|c)"
				, "(a|b|c)", "(a|c)");
			//TestCase("*(a|{b,c})", new[] { "a", "b", "c", "ab", "ac" });
			//TestCase("{a,*(b|c,d)}", new[] { "a", "(b|c", "*(b|c", "d)" });
			// a
			// *(b|c)
			// *(b|d)
			//TestCase("{a,*(b|{c,d})}", new[] { "a", "b", "bc", "cb", "c", "d" });
			//TestCase("*(a|{b|c,c})", new[] { "a", "b", "c", "ab", "ac", "bc", "cb" });


			// test various flag settings.
			TestCase(
				"*(a|{b|c,c})", new[] { "x(a|b|c)", "x(a|c)", "(a|b|c)", "(a|c)" }
				, new GlobMatcherOptions { });
			TestCase(
				"a?b", new[] { "x/y/acb", "acb/" }, new GlobMatcherOptions { MatchBase = true }
				, new[] { "x/y/acb", "acb/", "acb/d/e", "x/y/acb/d" });
			TestCase("#*", new[] { "#a", "#b" }, new GlobMatcherOptions { NoComment = true }, new[] { "#a", "#b", "c#d" });
		}

		[Test]
		public void NegationTests()
		{
			TestCase("**/.x/**", new[] { ".x/" }, null, new[] { ".x/" });
			// begin channelling Boole and deMorgan...

			ReplaceFiles("d", "e", "!ab", "!abc", "a!b", "\\!a");
			// anything that is NOT a* matches.
			TestCase("!a*", new[] { "\\!a", "d", "e", "!ab", "!abc" });

			// anything that IS !a* matches.
			TestCase("!a*", new[] { "!ab", "!abc" }, new GlobMatcherOptions { NoNegate = true });

			// anything that IS a* matches
			TestCase("!!a*", new[] { "a!b" });

			// anything that is NOT !a* matches
			TestCase("!\\!a*", new[] { "a!b", "d", "e", "\\!a" });

			// negation nestled within a pattern
			ReplaceFiles(
				"foo.js"
				, "foo.bar"
				// can't match this one without negative lookbehind.
				, "foo.js.js"
				, "blar.js"
				, "foo."
				, "boo.js.boo");
			//TestCase("*.!(js)", new[] { "foo.bar", "foo.", "boo.js.boo" });

			// https://github.com/isaacs/minimatch/issues/5
			ReplaceFiles(
				"a/b/.x/c"
				, "a/b/.x/c/d"
				, "a/b/.x/c/d/e"
				, "a/b/.x"
				, "a/b/.x/"
				, "a/.x/b"
				, ".x"
				, ".x/"
				, ".x/a"
				, ".x/a/b"
				, "a/.x/b/.x/c"
				, ".x/.x");

			TestCase("**/.x/**", new[] { ".x/", ".x/a", ".x/a/b", "a/.x/b", "a/b/.x/", "a/b/.x/c", "a/b/.x/c/d", "a/b/.x/c/d/e" });
		}

		[TestCase("a*/**b**", "abc/ss/aa/vv/ggbbhh/qq", true)]
		[TestCase("a*/**b**", "abc/ss/aa/vv/gghh/qq", false)]
		[TestCase("**/bin/**.exe", "My/BiN/Debug/program.exe", true)]
		[TestCase("**/bin/**.exe", "My/BiN/Debug/program.dll", false)]
		[TestCase("**/bin/**.exe", "bin/program.exe", true)]
		[TestCase("**aaa**ddd***ccc**", "aaa/aaa/ddd/ddd/ccc/ccc", true)]
		[TestCase("**aaa**ddd***ccc**", "aaa/aaa/ccc/ccc", false)]
		//[TestCase(".\\s.slN", "S.sln", true)]
		//[TestCase("s.slN", ".\\S.sln", true)]
		[TestCase(".", "", false)]
		[TestCase("/a/b/*", "/a/b/c", true)]

		[TestCase("*.txt", "/a/b/c/d/e/x.txt", false)]
		[TestCase("**.txt", "/a/b/c/d/e/x.txt", true)]
		[TestCase("**/*.txt", "/a/b/c/d/e/x.txt", true)]

		[TestCase("**/*.resx", @"C:\Repositories\ReSharperUp\ReSharperAutomationTools\test\data\CleanupCode\functional\Smoke\SampleSolution_2008\Properties\Resources.resx", true)]
		[TestCase("**/*.resx", @"SampleSolution_2008\Properties\Resources.resx", true)]
		[TestCase("**/*.resx", @"SampleSolution_2008\\Properties\\Resources.resx", true)]

		[TestCase("**.resx", @"C:\Repositories\ReSharperUp\ReSharperAutomationTools\test\data\CleanupCode\functional\Smoke\SampleSolution_2008\Properties\Resources.resx", true)]
		[TestCase("**.resx", @"SampleSolution_2008\Properties\Resources.resx", true)]
		[TestCase("**.resx", @"SampleSolution_2008\\Properties\\Resources.resx", true)]

		[TestCase("*.resx", @"C:\Repositories\ReSharperUp\ReSharperAutomationTools\test\data\CleanupCode\functional\Smoke\SampleSolution_2008\Properties\Resources.resx", false)]
		[TestCase("*.resx", @"SampleSolution_2008\Properties\Resources.resx", false)]
		[TestCase("*.resx", @"SampleSolution_2008\\Properties\\Resources.resx", false)]
		public void MatchesTest(string pattern, string fileName, bool expectsMatch)
		{
			var wildcard = GlobMatcher.Create(pattern, new GlobMatcherOptions { IgnoreCase = true, AllowWindowsPaths = true });
			var match = wildcard.IsMatch(fileName);
			Assert.AreEqual(expectsMatch, match, $"Failure with pattern {pattern}, fileName {fileName}, should match {expectsMatch}");
		}
	}
}
