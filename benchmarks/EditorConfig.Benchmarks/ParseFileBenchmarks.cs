using System.IO;
using BenchmarkDotNet.Attributes;
using EditorConfig.Core;

namespace EditorConfig.Benchmarks;

/// <summary>
/// Benchmarks for parsing a single .editorconfig file from a TextReader.
/// Uses a realistic multi-section config representative of a real project.
/// </summary>
[MemoryDiagnoser]
public class ParseFileBenchmarks
{
    /// <summary>
    /// A realistic multi-section .editorconfig representing a typical project
    /// (covers all property types, multiple globs including braces/stars/brackets).
    /// </summary>
    private const string SampleEditorConfig = """
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

        [*.{vb,vbx}]
        indent_size = 4

        [*.{js,ts,jsx,tsx}]
        indent_size = 2

        [*.{json,yml,yaml}]
        indent_size = 2

        [*.md]
        trim_trailing_whitespace = false

        [*.{py,pyw}]
        indent_size = 4
        max_line_length = 120

        [Makefile]
        indent_style = tab

        [**/*.min.{js,css}]
        indent_style = unset

        [**/vendor/**]
        indent_style = unset
        indent_size = unset

        [*.{bat,cmd}]
        end_of_line = crlf

        [*.sh]
        end_of_line = lf
        """;

    [Benchmark]
    public EditorConfigFile ParseFile()
    {
        using var reader = new StringReader(SampleEditorConfig);
        return EditorConfigFile.Parse(reader, "/home/user/project", ".editorconfig");
    }

    [Benchmark]
    public int ParseFileAndCountSections()
    {
        using var reader = new StringReader(SampleEditorConfig);
        var file = EditorConfigFile.Parse(reader, "/home/user/project", ".editorconfig");
        return file.Sections.Count;
    }
}
