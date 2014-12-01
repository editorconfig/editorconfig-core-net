using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using IniParser.Parser;
using Minimatch;

namespace EditorConfig.Core
{


	public class EditorConfigParser
	{
		public static readonly string VersionString = "0.11.4";
		public static readonly Version Version = new Version(VersionString);

		private readonly Options _globOptions = new Options { MatchBase = true, Dot = true, NoExt = true };

		public string ConfigFileName { get; private set; }

		public Version ParseVersion { get; private set; }

		public EditorConfigParser(string configFileName = ".editorconfig", Version developmentVersion = null)
		{
			ConfigFileName = configFileName ?? ".editorconfig";
			ParseVersion = developmentVersion ?? Version;
		}


		public IEnumerable<FileConfiguration> Parse(params string[] fileNames)
		{
			return fileNames
				.Select(f => f
					.Trim()
					.Trim(new[] { '\r', '\n' })
					.Replace(@"\", "/")
				)
				.Select(this.ParseFile)
				.ToList();
		}

		private FileConfiguration ParseFile(string fileName)
		{
			Debug.WriteLine(":: {0} :: {1}", this.ConfigFileName, fileName);
			var fullPath = Path.GetFullPath(fileName);
			var configFiles = this.AllParentConfigFiles(fullPath);

			//All the .editorconfig files going from root =>.fileName
			var editorConfigFiles = this.ParseConfigFilesTillRoot(configFiles).Reverse();

			var sections =
				from configFile in editorConfigFiles
				from section in configFile.Sections
				let glob = this.FixGlob(section.Name, configFile.Directory)
				where this.IsMatch(glob, fileName, configFile.Directory)
				select section;

			var allProperties =
				from section in sections
				from kv in section
				select FileConfiguration.Sanitize(kv.Key, kv.Value);

			var properties = new Dictionary<string, string>();
			foreach (var kv in allProperties)
				properties[kv.Key] = kv.Value;

			return new FileConfiguration(ParseVersion, fileName, properties);
		}

		private bool IsMatch(string glob, string fileName, string directory)
		{
			var matcher = new Minimatcher(glob, _globOptions);
			var isMatch = matcher.IsMatch(fileName);
			Debug.WriteLine("{0} :: {1} \t\t:: {2}", isMatch ? "✓" : "✘", glob, fileName);
			return isMatch;
		}

		private string FixGlob(string glob, string directory)
		{
			//glob = glob.Trim();

			switch (glob.IndexOf('/'))
			{
				case -1: glob = "**/" + glob; break;
				case 0: glob = glob.Substring(1); break;
			}
			glob = Regex.Replace(glob, @"\*\*", "{*,**/**/**}");

			directory = directory.Replace(@"\", "/");
			if (!directory.EndsWith("/")) directory += "/";

			return directory + glob;
		}

		private IEnumerable<EditorConfigFile> ParseConfigFilesTillRoot(IEnumerable<string> configFiles)
		{
			foreach (var configFile in configFiles.Select(f=> new EditorConfigFile(f)))
			{
				yield return configFile;
				if (configFile.IsRoot) yield break;
			}
		}

		private IEnumerable<string> AllParentConfigFiles(string fullPath)
		{
			return from parent in this.AllParentDirectories(fullPath)
				   let configFile = Path.Combine(parent, this.ConfigFileName)
				   where File.Exists(configFile)
				   select configFile;
		}

		private IEnumerable<string> AllParentDirectories(string fullPath)
		{
			var root = new DirectoryInfo(fullPath).Root.FullName;
			var dir = Path.GetDirectoryName(fullPath);
			do
			{
				if (dir == null) yield break;
				yield return dir;
				dir = Directory.GetParent(dir).FullName;
			} while (dir != root);
		}
	}
}