using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EditorConfig.Core;
using FluentAssertions;

namespace EditorConfig.Tests
{
	class EditorConfigTestBase
	{
		protected void HasBogusKey(FileConfiguration file, string key)
		{
			file.Properties.Should().NotBeEmpty().And.HaveCount(1).And.ContainKey(key);
			var bogusCharset = file.Properties[key];
			bogusCharset.Should().Be("bogus");
		}

		protected FileConfiguration GetConfig(MethodBase method, string fileName, string configurationFile = ".editorconfig")
		{
			var file = this.GetFileFromMethod(method, fileName);
			var parser = new EditorConfigParser(configurationFile);
			var fileConfigs = parser.Parse(file);
			fileConfigs.Should().NotBeEmpty();
			return fileConfigs.First();
		}

		protected string GetFileFromMethod(MethodBase method, string fileName)
		{
			var type = method.DeclaringType;
			var @namespace = type.Namespace;
			var folderSep = Path.DirectorySeparatorChar.ToString();
			var folder = @namespace.Replace("EditorConfig.Tests.", "").Replace(".", folderSep);
			var file = Path.Combine(folder, fileName.Replace(@"\", folderSep));
			file = Path.Combine(Environment.CurrentDirectory.Replace("bin" + folderSep + "Debug", "").Replace("bin" + folderSep + "Release", ""), file);
			return file;
		}
	}
}
