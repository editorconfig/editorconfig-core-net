using System;
using System.Collections.Concurrent;
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
		/// Cache of compiled <see cref="GlobMatcher"/> instances keyed by glob string.
		/// Safe to key on the glob string alone because <see cref="_globOptions"/> is a fixed
		/// per-instance field — the cache is scoped to this parser instance.
		/// </summary>
		private readonly ConcurrentDictionary<string, GlobMatcher> _matcherCache = new ConcurrentDictionary<string, GlobMatcher>();

		/// <summary>
		/// Cache of resolved <see cref="EditorConfigResolvedChain"/> instances keyed by source
		/// directory path. All files in the same directory share the same chain.
		/// Valid for the lifetime of this parser instance — recreate to pick up structural
		/// changes to the <c>.editorconfig</c> tree.
		/// </summary>
		private readonly ConcurrentDictionary<string, EditorConfigResolvedChain> _chainCache = new ConcurrentDictionary<string, EditorConfigResolvedChain>();

		/// <summary>
		/// The configured name of the files holding editorconfig values, defaults to ".editorconfig"
		/// </summary>
		public string ConfigFileName { get; private set; }

		/// <summary>
		/// The editor config parser version in use, defaults to latest <see cref="EditorConfigParser.Version"/>
		/// </summary>
		public Version ParseVersion { get; private set; }

		/// <summary>
		/// The <see cref="IFileSystem"/> in use. Defaults to <see cref="FileSystem"/>.
		/// </summary>
		public IFileSystem FileSystem { get; private set; }

		/// <summary>
		/// Creates an <see cref="EditorConfigParser"/> with the default filesystem and
		/// <see cref="EditorConfigFileCache"/> enabled.
		/// </summary>
		public EditorConfigParser() : this((IFileSystem)null) { }

		/// <summary>
		/// Creates an <see cref="EditorConfigParser"/> with a custom config file name and
		/// optional development version. Uses the default filesystem and
		/// <see cref="EditorConfigFileCache"/>.
		/// </summary>
		/// <param name="configFileName">The config file name to look for. Defaults to <c>.editorconfig</c>.</param>
		/// <param name="developmentVersion">Only used in testing to simulate an older parser version.</param>
		public EditorConfigParser(string configFileName, Version developmentVersion = null)
			: this((IFileSystem)null, configFileName, developmentVersion) { }

		/// <summary>
		/// Creates an <see cref="EditorConfigParser"/> with the provided filesystem and
		/// <see cref="EditorConfigFileCache"/> enabled.
		/// </summary>
		/// <param name="fileSystem">The <see cref="IFileSystem"/> to use.</param>
		/// <param name="configFileName">The config file name to look for. Defaults to <c>.editorconfig</c>.</param>
		/// <param name="developmentVersion">Only used in testing to simulate an older parser version.</param>
		public EditorConfigParser(IFileSystem fileSystem, string configFileName = ".editorconfig", Version developmentVersion = null)
		{
			fileSystem ??= new FileSystem();
			Factory = path => EditorConfigFileCache.GetOrCreate(path, fileSystem);
			ConfigFileName = configFileName ?? ".editorconfig";
			ParseVersion = developmentVersion ?? Version;
			FileSystem = fileSystem;
		}

		/// <summary>
		/// Creates an <see cref="EditorConfigParser"/> with a custom factory.
		/// Use this for advanced scenarios such as testing with a mock factory or
		/// supplying pre-loaded <see cref="EditorConfigFile"/> instances.
		/// </summary>
		/// <param name="factory">Function that produces an <see cref="EditorConfigFile"/> from a path.</param>
		/// <param name="configFileName">The config file name to look for. Defaults to <c>.editorconfig</c>.</param>
		/// <param name="developmentVersion">Only used in testing to simulate an older parser version.</param>
		/// <param name="fileSystem">The <see cref="IFileSystem"/> to use. Defaults to <see cref="FileSystem"/>.</param>
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
		/// If null will use <see cref="GetResolvedChain"/> to find all relevant editorconfig files. <para/>
		/// For repeated calls in the same directory prefer <see cref="GetResolvedChain"/> or
		/// <see cref="Parse(string, EditorConfigResolvedChain)"/> which avoid re-traversal.
		/// </param>
		/// <returns></returns>
		public FileConfiguration Parse(string fileName, IEnumerable<EditorConfigFile> editorConfigFiles = null)
		{
			var file = fileName.Trim('\r', '\n', ' ');
			Debug.WriteLine(":: {0} :: {1}", ConfigFileName, file);
			var fullPath = FileSystem.Path.GetFullPath(file);

			if (editorConfigFiles != null)
			{
				// Caller supplied an explicit list — use it directly (backwards-compat path)
				var sections =
					from configFile in editorConfigFiles
					from section in configFile.Sections
					where IsMatch(section.Glob, fullPath)
					select section;
				return new FileConfiguration(ParseVersion, file, sections.ToList());
			}

			return Parse(file, fullPath, GetResolvedChain(fullPath));
		}

		/// <summary>
		/// Gets the editorconfig configuration for <paramref name="fileName"/> using a
		/// pre-resolved chain. All files in the same directory share the same chain, so callers
		/// processing many files in one directory should obtain the chain once via
		/// <see cref="GetResolvedChain"/> and reuse it.
		/// </summary>
		public FileConfiguration Parse(string fileName, EditorConfigResolvedChain chain)
		{
			var file     = fileName.Trim('\r', '\n', ' ');
			var fullPath = FileSystem.Path.GetFullPath(file);
			return Parse(file, fullPath, chain);
		}

		private FileConfiguration Parse(string file, string fullPath, EditorConfigResolvedChain chain)
		{
			var sections =
				from section in chain.Sections
				where IsMatch(section.Glob, fullPath)
				select section;
			return new FileConfiguration(ParseVersion, file, sections.ToList());
		}

		/// <summary>
		/// Returns the <see cref="EditorConfigResolvedChain"/> for the directory containing
		/// <paramref name="file"/>, resolving and caching it on first access.
		/// All files in the same directory share a single chain; the cache is scoped to this
		/// parser instance and is valid until the parser is recreated.
		/// </summary>
		public EditorConfigResolvedChain GetResolvedChain(string file)
		{
			var fullPath = FileSystem.Path.GetFullPath(file);
			var dir      = FileSystem.Path.GetDirectoryName(fullPath);
			return _chainCache.GetOrAdd(dir, _ => new EditorConfigResolvedChain(
				GetConfigurationFilesTillRoot(fullPath)));
		}

		private bool IsMatch(string glob, string fileName)
		{
			var matcher = _matcherCache.GetOrAdd(glob, g => GlobMatcher.Create(g, _globOptions));
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
			var dir = FileSystem.Path.GetDirectoryName(fullPath);
			while (dir != null)
			{
				yield return dir;
				dir = FileSystem.Path.GetDirectoryName(dir);
			}
		}
	}
}
