using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;

namespace EditorConfig.Core
{
	/// <summary>
	/// The EditorConfigParser locates all relevant editorconfig files and makes sure they are merged correctly.
	/// </summary>
	public class EditorConfigParser
	{
		private Func<string, EditorConfigFile> Factory { get; }

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
		/// The IFileSystem in use, defaults to <see cref="FileSystem"/>
		/// </summary>
		public IFileSystem FileSystem { get; private set; }

		/// <summary>
		/// The EditorConfigParser locates all relevant editorconfig files and makes sure they are merged correctly.
		/// </summary>
		/// <param name="configFileName">The name of the file(s) holding the editorconfiguration values</param>
		/// <param name="developmentVersion">Only used in testing, development to pass an older version to the parsing routine</param>
		/// <param name="fileSystem">The <see cref="IFileSystem"/> to use. Can be used for testing.</param>
		public EditorConfigParser(string configFileName = ".editorconfig", Version developmentVersion = null, IFileSystem fileSystem = null)
			: this(f => new EditorConfigFile(f, fileSystem ?? new FileSystem()), configFileName, developmentVersion, fileSystem)
		{
		}

		/// <summary>
		/// The EditorConfigParser locates all relevant editorconfig files and makes sure they are merged correctly.
		/// </summary>
		/// <param name="factory">
		/// Function that take the file name and constructs a new EditorConfigFile instance.
		/// Pass `EditorConfigFileCache.GetOrCreate` to apply caching.
		/// </param>
		/// <param name="configFileName"></param>
		/// <param name="developmentVersion"></param>
		/// <param name="fileSystem"></param>
		public EditorConfigParser(Func<string, EditorConfigFile> factory, string configFileName = ".editorconfig", Version developmentVersion = null, IFileSystem fileSystem = null)
		{
			Factory = factory;
			ConfigFileName = configFileName ?? ".editorconfig";
			ParseVersion = developmentVersion ?? Version;
			FileSystem = fileSystem ?? new FileSystem();
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
			Debug.WriteLine(":: {0} :: {1}", ConfigFileName, file);

			var fullPath = FileSystem.Path.GetFullPath(file);

			//All the .editorconfig files going from root =>.fileName
			editorConfigFiles = editorConfigFiles ?? GetConfigurationFilesTillRoot(file);

			var sections =
				from configFile in editorConfigFiles
				from section in configFile.Sections
				where IsMatch(section.Glob, fullPath, configFile.Directory)
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
		/// Gets all relevant <see cref="EditorConfigFile"/> for <paramref name="file"/> until the first config file upwards
		/// marked as root.
		/// </summary>
		public IList<EditorConfigFile> GetConfigurationFilesTillRoot(string file)
		{
			var fullPath = FileSystem.Path.GetFullPath(file);
			var configFiles = AllParentConfigFiles(fullPath);

			return ParseConfigFilesTillRoot(configFiles).Reverse().ToList();
		}

		private IEnumerable<EditorConfigFile> ParseConfigFilesTillRoot(IEnumerable<string> configFiles)
		{
			foreach (var configFile in configFiles.Select(Factory))
			{
				yield return configFile;
				if (configFile.IsRoot) yield break;
			}
		}

		private IEnumerable<string> AllParentConfigFiles(string fullPath) =>
			from parent in AllParentDirectories(fullPath)
			let configFile = FileSystem.Path.Combine(parent, ConfigFileName)
			where FileSystem.File.Exists(configFile)
			select configFile;

		private IEnumerable<string> AllParentDirectories(string fullPath)
		{
			var root = FileSystem.DirectoryInfo.New(fullPath).Root.FullName;
			var dir = FileSystem.Path.GetDirectoryName(fullPath);
			do
			{
				if (dir == null) yield break;
				yield return dir;
				var dirInfo = FileSystem.DirectoryInfo.New(dir);
				if (dirInfo.Parent == null) yield break;
				dir = dirInfo.Parent.FullName;
			} while (dir != root);
		}
	}
}
