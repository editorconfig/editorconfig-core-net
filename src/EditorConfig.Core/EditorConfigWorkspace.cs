using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EditorConfig.Core
{
	/// <summary>
	/// Not ready yet the idea is to create a class that you can load and reuse in a long running process.
	/// This allows you to reuse parsed <see cref="EditorConfigFile"/> instances without performing an IO call everytime
	/// <see cref="EditorConfigParser.Parse(string[])"/> is called.
	///
	/// Will need to store all paths in a prefix tree to filter to relevant editorconfig files to apply to a given path
	/// </summary>
	internal class EditorConfigWorkspace
	{
		private EditorConfigParser _parser;
		private IEnumerable<EditorConfigFile> _editorconfigFiles;

		public EditorConfigWorkspace(FileInfo rootEditorConfigFile, string configFileName = ".editorconfig",
			Version developmentVersion = null)
		{
			_parser = new EditorConfigParser(configFileName, developmentVersion);

			var directory = rootEditorConfigFile.Directory;
			_editorconfigFiles =
				directory.EnumerateFiles(configFileName, SearchOption.AllDirectories)
					.Select(d=> EditorConfigFileCache.GetOrCreate(d.Name));
		}


		public FileConfiguration Parse(FileInfo fileName) => null;
	}
}
