using System;
using System.IO;
using System.Reflection;
using AwesomeAssertions;
using EditorConfig.Core;

namespace EditorConfig.Tests
{
	public class EditorConfigTestBase
	{
		protected void HasBogusKey(FileConfiguration file, string key)
		{
			file.Properties.Should().NotBeEmpty().And.HaveCount(1);
			AssertHasProperty(key, file);
			var bogusCharset = file.Properties[key];
			bogusCharset.Should().Be("bogus");
		}

		protected FileConfiguration GetConfig(MethodBase method, string fileName, string configurationFile = ".editorconfig")
		{
			var file = GetFileFromMethod(method, fileName);
			var parser = new EditorConfigParser(configurationFile);
			var fileConfigs = parser.Parse(file);
			fileConfigs.Should().NotBeNull();
			return fileConfigs;
		}

		protected string GetFileFromMethod(MethodBase method, string fileName)
		{
			var type = method.DeclaringType;
			var @namespace = type.Namespace;
			var folderSep = Path.DirectorySeparatorChar.ToString();
			var folder = @namespace.Replace("EditorConfig.Tests.", "").Replace(".", folderSep);
			var file = Path.Combine(folder, fileName);

			var cwd = Environment.CurrentDirectory;
			file = Path.Combine(cwd.Replace(OutputPath("Release"), "").Replace(OutputPath("Debug"), ""), file);
			return file;

			string OutputPath(string configuration) => $"bin{folderSep}net10.0{folderSep}{configuration}";
		}

		protected void AssertHasProperty(string property, FileConfiguration file) =>
			file.Properties.ContainsKey(property).Should().BeTrue($"Expected to find {property} in Properties for file: {file.FileName}");

		protected void AssertHasNotProperty(string property, FileConfiguration file) =>
			file.Properties.ContainsKey(property).Should().BeFalse($"Did not expect to find {property} in Properties for file: {file.FileName}");
	}
}
