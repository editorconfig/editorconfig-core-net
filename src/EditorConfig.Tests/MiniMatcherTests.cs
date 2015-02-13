using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EditorConfig.Core;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests
{
	[TestFixture]
	public class MiniMatcherTests
	{
		private readonly EditorConfigMinimatcherOptions _globOptions = new EditorConfigMinimatcherOptions { MatchBase = true, Dot = true, NoExt = true };

		[Test]
		public void MatcherTest()
		{
			var glob = "C:/Projects/editorconfig-core-net/tests/filetree/top/of/path";
			var file = "C:/Projects/editorconfig-core-net/tests/filetree/top/of/path";
			var m = new EditorConfigMinimatcher(glob, _globOptions);
			var match = m.IsMatch(file);
			match.Should().BeTrue();
		}
	}
}
