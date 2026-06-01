using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using EditorConfig.Core;

namespace EditorConfig.Benchmarks;

/// <summary>
/// End-to-end benchmark: EditorConfigParser.Parse(path) for a file nested several
/// directories deep. This exercises the directory traversal, IsMatch-per-section,
/// and the new EditorConfigResolvedChain directory-level cache.
///
/// A temp directory tree with a realistic .editorconfig is created in [GlobalSetup]
/// and torn down in [GlobalCleanup].
/// </summary>
[MemoryDiagnoser]
public class EndToEndBenchmarks
{
    private string _rootDir = null!;
    private string _targetFile = null!;
    private string[] _sameDirectoryFiles = null!;
    private EditorConfigParser _parser = null!;
    private EditorConfigParser _cachedParser = null!;

    private const string EditorConfigContent = """
        root = true

        [*]
        indent_style = space
        indent_size = 4
        end_of_line = lf
        charset = utf-8
        trim_trailing_whitespace = true
        insert_final_newline = true

        [*.{cs,csx}]
        indent_size = 4

        [*.{fs,fsx}]
        indent_size = 4

        [*.{js,ts,jsx,tsx}]
        indent_size = 2

        [*.{json,yml,yaml}]
        indent_size = 2

        [*.md]
        trim_trailing_whitespace = false

        [Makefile]
        indent_style = tab

        [**/*.min.{js,css}]
        indent_style = unset

        [**/vendor/**]
        indent_style = unset
        indent_size = unset

        [*.{bat,cmd}]
        end_of_line = crlf
        """;

    // Number of files in the same directory used by ParseSameDir benchmarks
    private const int SameDirFileCount = 50;

    [GlobalSetup]
    public void Setup()
    {
        // Create a directory tree 4 levels deep so the parser traverses several parents.
        _rootDir = Path.Combine(Path.GetTempPath(), $"ec-bench-{Guid.NewGuid():N}");
        var deep = Path.Combine(_rootDir, "src", "EditorConfig", "Core");
        Directory.CreateDirectory(deep);

        // Place a root .editorconfig at the top
        File.WriteAllText(Path.Combine(_rootDir, ".editorconfig"), EditorConfigContent);

        // Single target file for the single-file benchmarks
        _targetFile = Path.Combine(deep, "Parser.cs");
        File.WriteAllText(_targetFile, "// placeholder");

        // 50 .cs files all in the same directory — the CSharpier / formatter scenario
        _sameDirectoryFiles = new string[SameDirFileCount];
        for (var i = 0; i < SameDirFileCount; i++)
        {
            var path = Path.Combine(deep, $"File{i:D3}.cs");
            File.WriteAllText(path, "// placeholder");
            _sameDirectoryFiles[i] = path;
        }

        _parser       = new EditorConfigParser();
        _cachedParser = new EditorConfigParser();

        // Warm up the file cache and chain cache so disk I/O doesn't dominate
        _cachedParser.Parse(_targetFile);
        foreach (var f in _sameDirectoryFiles) _cachedParser.Parse(f);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_rootDir))
            Directory.Delete(_rootDir, recursive: true);
    }

    // ── Single-file benchmarks (baseline comparison with PR #37) ────────────────

    /// <summary>
    /// Parse one file with a fresh parser (no caches). PR #37 baseline: 48.65 μs / 43.99 KB.
    /// </summary>
    [Benchmark(Baseline = true)]
    public FileConfiguration ParseFresh() => _parser.Parse(_targetFile);

    /// <summary>
    /// Parse one file with the file cache warm. PR #37 baseline: 11.17 μs / 7.34 KB.
    /// </summary>
    [Benchmark]
    public FileConfiguration ParseWithFileCache() => _cachedParser.Parse(_targetFile);

    // ── Multi-file same-directory benchmarks (the CSharpier / formatter scenario) ──

    /// <summary>
    /// Parse 50 files all in the same directory, no caches.
    /// Each call does a full independent directory traversal — this is the pre-chain-cache worst case.
    /// </summary>
    [Benchmark]
    public void ParseSameDirFresh()
    {
        foreach (var f in _sameDirectoryFiles)
            _parser.Parse(f);
    }

    /// <summary>
    /// Parse 50 files all in the same directory, file cache + chain cache warm.
    /// After the first file, the chain is cached — all remaining files skip traversal entirely.
    /// </summary>
    [Benchmark]
    public void ParseSameDirCached()
    {
        foreach (var f in _sameDirectoryFiles)
            _cachedParser.Parse(f);
    }

    /// <summary>
    /// Parse 50 files using an explicit pre-resolved chain (the caller-controlled API path).
    /// </summary>
    [Benchmark]
    public void ParseSameDirWithExplicitChain()
    {
        var chain = _cachedParser.GetResolvedChain(_sameDirectoryFiles[0]);
        foreach (var f in _sameDirectoryFiles)
            _cachedParser.Parse(f, chain);
    }
}
