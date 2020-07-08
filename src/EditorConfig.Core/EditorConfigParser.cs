using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EditorConfig.Core
{
	/// <summary>
	/// The EditorConfigParser locates all relevant editorconfig files and makes sure they are merged correctly.
	/// </summary>
	public class EditorConfigParser
	{
		/// <summary>
		/// The current (and latest parser supported) version as string
		/// </summary>
		public static readonly string VersionString = "0.12.1";

		/// <summary>
		/// The current editorconfig version
		/// </summary>
		public static readonly Version Version = new Version(VersionString);

		private readonly GlobMatcherOptions _globOptions = new GlobMatcherOptions { MatchBase = true, Dot = true, AllowWindowsPaths = true };
		
		/// <summary>
		/// The configured name of the files holding editorconfig values, defaults to ".editorconfig"
		/// </summary>
		public string ConfigFileName { get; private set; }
		
		/// <summary>
		/// The editor config parser version in use, defaults to latest <see cref="EditorConfigParser.Version"/>
		/// </summary>
		public Version ParseVersion { get; private set; }
		
		/// <summary>
		/// The EditorConfigParser locates all relevant editorconfig files and makes sure they are merged correctly.
		/// </summary>
		/// <param name="configFileName">The name of the file(s) holding the editorconfiguration values</param>
		/// <param name="developmentVersion">Only used in testing, development to pass an older version to the parsing routine</param>
		public EditorConfigParser(string configFileName = ".editorconfig", Version developmentVersion = null)
		{
			ConfigFileName = configFileName ?? ".editorconfig";
			ParseVersion = developmentVersion ?? Version;
		}

		/// <summary>
		/// Gets the FileConfiguration for each of the passed fileName by resolving their relevant editorconfig files.
		/// </summary>
		public IEnumerable<FileConfiguration> Parse(params string[] fileNames) => fileNames.Select(f => Parse(f)).ToList();


		/// <summary>
		/// Get the editorconfig configuration (see <see cref="FileConfiguration"/> for the provided <paramref name="fileName"/>
		/// </summary>
		/// <param name="fileName">The path to the file we want to know it's editorconfig settings for</param>
		/// <param name="editorConfigFiles">
		/// If null will traverse the file path to find all relevant editorconfig files. <para/>
		/// This can be costly if repeated multiple times, if you call this for the same file multiple times look in to
		/// using <see cref="GetConfigurationFilesTillRoot"/> and passing that explicitly to <paramref name="editorConfigFiles"/>
		/// </param>
		/// <returns></returns>
		public FileConfiguration Parse(string fileName, IEnumerable<EditorConfigFile> editorConfigFiles = null)
		{
			var file = fileName.Trim('\r', '\n', ' ');
			Debug.WriteLine(":: {0} :: {1}", this.ConfigFileName, file);

			var fullPath = Path.GetFullPath(file);

			//All the .editorconfig files going from root =>.fileName
			editorConfigFiles = editorConfigFiles ?? this.GetConfigurationFilesTillRoot(file);

			var sections =
				from configFile in editorConfigFiles
				from section in configFile.Sections
				where this.IsMatch(section.Glob, fullPath, configFile.Directory)
				select section;

			return new FileConfiguration(ParseVersion, file, sections.ToList());
		}

		private bool IsMatch(string glob, string fileName, string directory)
		{
			var matcher = GlobMatcher.Create(glob, _globOptions);
			var isMatch = matcher.IsMatch(fileName);
			Debug.WriteLine("{0} :: {1} \t\t:: {2}", isMatch ? "?" : "?", glob, fileName);
			return isMatch;
		}

		/// <summary>
		/// Gets all relevant <see cref="EditorConfigFile"/> for <see cref="file"/> until the first config file upwards
		/// marked as root.
		/// </summary>
		public IList<EditorConfigFile> GetConfigurationFilesTillRoot(string file)
		{
			var fullPath = Path.GetFullPath(file);
			var configFiles = this.AllParentConfigFiles(fullPath);

			return this.ParseConfigFilesTillRoot(configFiles).Reverse().ToList();
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
				var dirInfo = new DirectoryInfo(dir);
				if (dirInfo.Parent == null) yield break;
				dir = dirInfo.Parent.FullName;
			} while (dir != root);
		}
	}
}