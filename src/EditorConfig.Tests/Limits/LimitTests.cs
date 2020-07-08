using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.Limits
{
	[TestFixture]
	class LimitTests : EditorConfigTestBase
	{
		[Test] public void KeyLengths()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "test1", ".limits.editorconfig");
			file.Properties.Should().NotBeEmpty();

			AssertHasProperty("00000000000000000000000000000000000000000000000001", file);
			AssertHasProperty("000000000000000000000000000000000000000000000000002", file);
		}
		
		[Test] public void ValueLengths()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "test2", ".limits.editorconfig");
			file.Properties.Should().NotBeEmpty();

			AssertHasProperty("k255", file);
			AssertHasProperty("k256", file);
			
			file.Properties["k255"].Should().HaveLength(255);
			file.Properties["k256"].Should().HaveLength(256);
		}
		
		[Test] public void Alternatives()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "hello.vsct", ".limits.editorconfig");
			file.Properties.Should().NotBeEmpty().And.HaveCount(1);
			
			AssertHasProperty("x", file);

			file.Properties["x"].Should().Be("y");
		}
		


	}
}
