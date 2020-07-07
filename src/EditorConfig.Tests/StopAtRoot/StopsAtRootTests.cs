using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.StopAtRoot
{
	[TestFixture]
	class StopsAtRootTests : EditorConfigTestBase
	{
		[Test]
		public void SomeProp_HasValueFromClosestConfig()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), @"X\Y\Z\f.x");
			file.Properties.Should().NotBeEmpty().And.ContainKey("some_prop");
			var someProp = file.Properties["some_prop"];
			someProp.Should().Be("z");
		}

		[Test]
		public void ParentsFromRootDoNotLeakIntoScope()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), @"X\Y\Z\f.x");
			file.Properties.Should().NotBeEmpty().And.NotContainKey("outer_prop");
		}
		
		[Test]
		public void ChildrenDoNotInterfere()
		{
			var file = this.GetConfig(MethodBase.GetCurrentMethod(), @"X\f.x");
			file.Properties.Should().NotBeEmpty().And.ContainKey("outer_prop");
			file.Properties.Should().NotBeEmpty().And.ContainKey("some_prop");
			var someProp = file.Properties["some_prop"];
			someProp.Should().Be("x");
		}
	}
}
