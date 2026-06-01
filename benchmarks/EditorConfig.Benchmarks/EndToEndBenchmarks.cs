using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using EditorConfig.Core;

namespace EditorConfig.Benchmarks;

/// <summary>
/// End-to-end benchmark: EditorConfigParser.Parse(path) for a file nested several
/// directories deep. This exercises the IsMatch-per-section hotspot (the biggest allocation
/// source before the matcher cache) and the directory-traversal path.
///
/// A temp directory tree with a realistic .editorconfig is created in [GlobalSetup]
/// and torn down in [GlobalCleanup].
/// </summary>
[MemoryDiagnoser]
public class EndToEndBenchmarks
{
    private string _rootDir = null!;
    private string _targetFile = null!;
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

    [GlobalSetup]
    public void Setup()
    {
        // Create a directory tree 4 levels deep so the parser traverses several parents.
        _rootDir = Path.Combine(Path.GetTempPath(), $"ec-bench-{Guid.NewGuid():N}");
        var deep = Path.Combine(_rootDir, "src", "EditorConfig", "Core");
        Directory.CreateDirectory(deep);

        // Place a root .editorconfig at the top
        File.WriteAllText(Path.Combine(_rootDir, ".editorconfig"), EditorConfigContent);

        // The target file being "parsed"
        _targetFile = Path.Combine(deep, "Parser.cs");
        File.WriteAllText(_targetFile, "// placeholder");

        _parser = new EditorConfigParser();
        _cachedParser = new EditorConfigParser(f => EditorConfigFileCache.GetOrCreate(f));

        // Warm up the file cache so disk I/O doesn't dominate
        _cachedParser.Parse(_targetFile);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_rootDir))
            Directory.Delete(_rootDir, recursive: true);
    }

    /// <summary>
    /// Parse with a fresh parser (no matcher cache, no file cache).
    /// Reflects the current worst-case behaviour.
    /// </summary>
    [Benchmark(Baseline = true)]
    public FileConfiguration ParseFresh() => _parser.Parse(_targetFile);

    /// <summary>
    /// Parse with the file cache enabled (EditorConfigFileCache).
    /// Still re-compiles globs on every call.
    /// </summary>
    [Benchmark]
    public FileConfiguration ParseWithFileCache() => _cachedParser.Parse(_targetFile);
}
