using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EditorConfig.Core
{
	/// <summary>
	/// Represents an ini section within the editorconfig file
	/// </summary>
	public class ConfigSection : IReadOnlyDictionary<string, string>
	{
		private readonly Dictionary<string, string> _backingDictionary;

		public ConfigSection() => _backingDictionary = new Dictionary<string, string>();

		public ConfigSection(string name, string configDirectory, Dictionary<string, string> backingDictionary)
		{
			Glob = FixGlob(name, configDirectory);
			_backingDictionary = backingDictionary ?? new Dictionary<string, string>();
		} 
		
		public string Glob { get; }
		
		private static string FixGlob(string glob, string directory)
		{
			switch (glob.IndexOf('/'))
			{
				case -1: glob = "**/" + glob; break;
				case 0: glob = glob.Substring(1); break;
			}
			directory = directory.Replace(@"\", "/");
			if (!directory.EndsWith("/")) directory += "/";

			return directory + glob;
		}

		IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => 
			((IEnumerable<KeyValuePair<string, string>>) _backingDictionary).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _backingDictionary).GetEnumerator();
		
		public int Count => _backingDictionary.Count;
		public bool ContainsKey(string key) => _backingDictionary.ContainsKey(key);
		public bool TryGetValue(string key, out string value) =>
			_backingDictionary.TryGetValue(key, out value);
		public string this[string key] => _backingDictionary[key];
		public IEnumerable<string> Keys => _backingDictionary.Keys;
		public IEnumerable<string> Values => _backingDictionary.Values;
	}
}