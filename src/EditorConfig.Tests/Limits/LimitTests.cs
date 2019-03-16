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

			file.Properties.Should().ContainKey("00000000000000000000000000000000000000000000000001");
			file.Properties.Should().ContainKey("000000000000000000000000000000000000000000000000002");
		}
		
		[Test] public void ValueLengths()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "test2", ".limits.editorconfig");
			file.Properties.Should().NotBeEmpty();

			file.Properties.Should().ContainKey("k255");
			file.Properties.Should().ContainKey("k256");
			
			file.Properties["k255"].Should().HaveLength(255);
			file.Properties["k256"].Should().HaveLength(256);
		}
		
		[Test] public void Alternatives()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), "hello.vsct", ".limits.editorconfig");
			file.Properties.Should().NotBeEmpty().And.HaveCount(1);

			file.Properties.Should().ContainKey("x");
			file.Properties.Should().ContainValue("y");
		}
		


	}
}
