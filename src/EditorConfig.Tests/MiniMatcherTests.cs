using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Minimatch;
using NUnit.Framework;

namespace EditorConfig.Tests
{
	[TestFixture]
	public class MiniMatcherTests
	{
		private readonly Options _globOptions = new Options { MatchBase = true, Dot = true, NoExt = true };

		[Test]
		public void MatcherTest()
		{
			var glob = "C:/Projects/editorconfig-core-net/tests/filetree/top/of/path";
			var file = "C:/Projects/editorconfig-core-net/tests/filetree/top/of/path";
			var m = new Minimatcher(glob, _globOptions);
			var match = m.IsMatch(file);
			match.Should().BeTrue();
		}
	}
}
