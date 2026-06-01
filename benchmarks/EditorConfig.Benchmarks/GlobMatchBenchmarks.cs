using BenchmarkDotNet.Attributes;
using EditorConfig.Core;

namespace EditorConfig.Benchmarks;

/// <summary>
/// Benchmarks for the glob matching hot path.
/// Separates "create + match" (as the parser currently does on every call)
/// vs "match only" (pre-compiled matcher) to expose the compile vs match split.
/// Globs and paths are drawn from the upstream conformance suite inputs.
/// </summary>
[MemoryDiagnoser]
public class GlobMatchBenchmarks
{
    private static readonly GlobMatcherOptions Options = new()
    {
        MatchBase = true,
        Dot = true,
        AllowWindowsPaths = true,
    };

    // Representative sample of glob patterns from tests/glob/*.in
    private static readonly string[] Globs =
    [
        // star.in
        "a*e.c",
        "Bar/*",
        "*",
        // star_star.in
        "a**z.c",
        "b/**z.c",
        "d/**/z.c",
        "**/*.cs",
        // braces.in
        "*.{py,js,html}",
        "{single}.b",
        "a{b,c,}.d",
        "{0..9}.h",
        // brackets.in
        "[ab].a",
        "[!ab].b",
        "[d-g].c",
        "[-ab].f",
        // question.in
        "?.a",
        "?oo.b",
        // realistic real-world patterns
        "[*.{cs,csx,vb,vbx}]",
        "**/*.{cs,fs,vb}",
        "src/**/*.cs",
    ];

    // Representative paths to match against
    private static readonly string[] Paths =
    [
        "/home/user/project/src/EditorConfig/Parser.cs",
        "/home/user/project/src/EditorConfig/Minimatcher.cs",
        "/home/user/project/tests/glob/star.in",
        "/home/user/project/abc.js",
        "/home/user/project/foo.py",
        "/home/user/project/Bar/baz.txt",
        "/home/user/project/d/x/y/z/z.c",
        "/home/user/project/a.a",
        "/home/user/project/1.h",
    ];

    // Pre-compiled matchers for the "match only" benchmark
    private GlobMatcher[] _compiled = null!;

    [GlobalSetup]
    public void Setup()
    {
        _compiled = new GlobMatcher[Globs.Length];
        for (var i = 0; i < Globs.Length; i++)
            _compiled[i] = GlobMatcher.Create(Globs[i], Options);
    }

    /// <summary>
    /// Simulates current EditorConfigParser behaviour: compile the glob fresh then match.
    /// This is the hottest path before the caching fix (Step 3).
    /// </summary>
    [Benchmark(Baseline = true)]
    public bool CreateAndMatch()
    {
        var hit = false;
        foreach (var glob in Globs)
        {
            var matcher = GlobMatcher.Create(glob, Options);
            foreach (var path in Paths)
                hit |= matcher.IsMatch(path);
        }
        return hit;
    }

    /// <summary>
    /// Match with pre-compiled matchers — the steady-state after the cache lands.
    /// </summary>
    [Benchmark]
    public bool MatchOnly()
    {
        var hit = false;
        foreach (var matcher in _compiled)
        {
            foreach (var path in Paths)
                hit |= matcher.IsMatch(path);
        }
        return hit;
    }

    /// <summary>
    /// Single glob compile (useful for measuring allocation of Create alone).
    /// </summary>
    [Benchmark]
    public GlobMatcher CreateSingle() => GlobMatcher.Create("**/*.{cs,fs,vb}", Options);
}
