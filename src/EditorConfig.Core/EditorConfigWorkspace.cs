using System;
using System.IO;
using System.Linq;

namespace EditorConfig.Core
{
	public class EditorConfigWorkspace
	{
		private EditorConfigParser _parser;

		public EditorConfigWorkspace(FileInfo rootEditorConfigFile, string configFileName = ".editorconfig",
			Version developmentVersion = null)
		{
			this._parser = new EditorConfigParser(configFileName, developmentVersion);

			var directory = rootEditorConfigFile.Directory;
			var editorconfigFiles = 
				directory.EnumerateFiles($"*{configFileName}", SearchOption.AllDirectories)
					.Select(d=>new EditorConfigFile(d.Name));

		}


		public FileConfiguration Parse(string fileName)
		{
			return null;
		}
	}
}