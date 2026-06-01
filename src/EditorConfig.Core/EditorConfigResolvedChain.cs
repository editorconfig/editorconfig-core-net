using System.Collections.Generic;
using System.Linq;

namespace EditorConfig.Core
{
	/// <summary>
	/// Represents the fully resolved set of editorconfig sections that apply to all files in a
	/// specific directory. All <see cref="ConfigSection"/> objects from the entire chain of
	/// <see cref="EditorConfigFile"/> files (from the filesystem root down to the nearest
	/// <c>.editorconfig</c>) are pre-flattened into a single array.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Obtaining a chain via <see cref="EditorConfigParser.GetResolvedChain"/> is O(1) for any
	/// directory that has already been resolved — the chain is cached per
	/// <see cref="EditorConfigParser"/> instance. Callers that process many files in the same
	/// directory (e.g. a formatter or linter run over a whole repository) can call
	/// <see cref="EditorConfigParser.GetResolvedChain"/> once per directory and pass the result
	/// to <see cref="EditorConfigParser.Parse(string, EditorConfigResolvedChain)"/> for each file.
	/// </para>
	/// <para>
	/// The chain is valid for the lifetime of the <see cref="EditorConfigParser"/> instance that
	/// created it. If a <c>.editorconfig</c> file is added, removed, or has its <c>root = true</c>
	/// flag changed while the parser is in use, the cached chain will not reflect the change.
	/// Recreate the parser to force re-resolution.
	/// </para>
	/// </remarks>
	public sealed class EditorConfigResolvedChain
	{
		/// <summary>
		/// All <see cref="ConfigSection"/> objects from the entire <c>.editorconfig</c> chain,
		/// ordered root-first. Glob strings are already directory-qualified.
		/// This array is immutable after construction.
		/// </summary>
		public ConfigSection[] Sections { get; }

		internal EditorConfigResolvedChain(IList<EditorConfigFile> chain)
			=> Sections = chain.SelectMany(f => f.Sections).ToArray();
	}
}
