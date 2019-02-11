using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorConfig.App
{
	class ArgumentsParser
	{
		public string[] FileNames { get; private set; } 
		public string ConfigFileName { get; private set; } 
		public Version DevelopVersion { get; private set; } 

		public bool PrintHelp { get; private set; }
		public bool PrintVersion { get; private set; }

		public ArgumentsParser(string[] args)
		{
			if (args.Length == 0)
				throw new ApplicationArgumentException("Must specify atleast one FILEPATH");
			
			while (args.Length > 0 && args[0].StartsWith("-"))
			{
				switch (args[0])
				{
					case "-h":
					case "--help":
						this.PrintHelp = true;
						return;
					case "-v":
					case "--version":
						this.PrintVersion = true;
						return;
					case "-f":
						if (args.Length == 1 || args[1].StartsWith("-"))
							throw new ApplicationArgumentException("Option '-f' needs argument <path>");

						this.ConfigFileName = args[1];
						args = args.Skip(2).ToArray();
						break;
					case "-b":
						if (args.Length == 1 || args[1].StartsWith("-"))
							throw new ApplicationArgumentException("Option '-b' needs argument <version>");
						Version version = null;
						if (!Version.TryParse(args[1], out version))
							throw new ApplicationArgumentException("Option '-b' argument '{0}' is not valid version", args[1]);
						DevelopVersion = version;
						args = args.Skip(2).ToArray();
						break;
					default:
						throw new ApplicationArgumentException("Unknown option '{0}'", args[0]);
				}
			}
			if (args.Length == 0)
				throw new ApplicationArgumentException("You need to specify atleast one file");

			this.FileNames = args;
		}
	}
}
