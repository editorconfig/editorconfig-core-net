# Benchmark Results — Before vs After Modernization

Machine: Apple M2 Pro, 12 cores · .NET 10.0.8 (Arm64 RyuJIT AdvSIMD)

## GlobMatch

| Method         | Before (ns) | After (ns) | Δ Time | Before Alloc | After Alloc | Δ Alloc   |
|--------------- |------------:|-----------:|-------:|-------------:|------------:|----------:|
| CreateAndMatch |    13,294   |   11,298   |  −15%  |    20,592 B  |   19,896 B  |    −3%    |
| MatchOnly      |     6,511   |    4,700   |  −28%  |      912 B   |     **0 B** | **−100%** |
| CreateSingle   |       797   |      587   |  −26%  |    1,920 B   |    1,944 B  |     0%    |

`MatchOnly` hits **zero allocations** — `ref struct MatchContext` with `ReadOnlySpan<char>` eliminates all per-match heap traffic.

## EndToEnd (`EditorConfigParser.Parse`)

| Method             | Before   | After    | Δ Time    | Before Alloc | After Alloc | Δ Alloc   |
|------------------- |---------:|---------:|----------:|-------------:|------------:|----------:|
| ParseFresh         | 71.01 μs | 48.65 μs |   **−31%** |   78.72 KB  |   43.99 KB  |  **−44%** |
| ParseWithFileCache | 55.03 μs | 11.17 μs |   **−80%** |   37.84 KB  |    7.34 KB  |  **−81%** |

`ParseWithFileCache` goes from 55 μs → 11 μs and 37 KB → 7 KB: the stat-based cache key
(mtime + size, no file read on hits) + matcher cache together eliminate nearly all repeated work.

## ParseFile (`EditorConfigFile.Parse` from `TextReader`)

| Method    | Before   | After    | Δ Time    | Before Alloc | After Alloc | Δ Alloc |
|---------- |---------:|---------:|----------:|-------------:|------------:|--------:|
| ParseFile | 21.22 μs |  10.39 μs | **−51%** |   40.76 KB  |   34.86 KB  |  −14%   |

Parse time halved by source-generated regexes, `HashSet` for known-property lookup, and
`.Match()` replacing `.Matches()`.
