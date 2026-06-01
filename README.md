# EditorConfig .NET Core

The EditorConfig .NET core provides the same functionality as the
[EditorConfig C Core][] and [EditorConfig Python Core][].

## Installation

**Library** (NuGet):

```
dotnet add package editorconfig
```

**CLI tool**:

```
dotnet tool install --global editorconfig-tool
```

The tool ships as a **native AOT binary** for each supported platform. When you
run `dotnet tool install`, the correct pre-compiled native binary is selected
automatically — no .NET runtime required at invocation time:

| Platform | RID |
|---|---|
| Linux x64 | `linux-x64` |
| Linux Arm64 | `linux-arm64` |
| Windows x64 | `win-x64` |
| Windows Arm64 | `win-arm64` |
| macOS Apple Silicon | `osx-arm64` |

A framework-dependent fallback (`any`) is included for other platforms.

## Usage

### Library

```csharp
// Recommended: caching and default filesystem
var parser = new EditorConfigParser();
var config = parser.Parse(fileName);
foreach (var kv in config.Properties)
    Console.WriteLine("{0}={1}", kv.Key, kv.Value);
```

#### Parsing many files efficiently

When processing many files in one run (e.g. a formatter or linter), reuse the
same `EditorConfigParser` instance. The parser caches both compiled glob matchers
and the resolved `.editorconfig` chain per directory — so the second and subsequent
files in the same directory cost only a dictionary lookup plus glob matching,
with zero traversal overhead.

```csharp
var parser = new EditorConfigParser();

// All files share one parser instance; files in the same directory
// share one resolved chain automatically.
var results = parser.Parse(file1, file2, file3 /*, ... */);
```

For explicit control (e.g. grouping files by directory yourself):

```csharp
var chain = parser.GetResolvedChain(file1);   // traverses once for this directory
var cfg1  = parser.Parse(file1, chain);
var cfg2  = parser.Parse(file2, chain);       // zero traversal — same directory
```

#### Injecting a file system

Pass an [`IFileSystem`](https://www.nuget.org/packages/System.IO.Abstractions)
from [System.IO.Abstractions](https://github.com/TestableIO/System.IO.Abstractions)
when you need a testable or virtual file system:

```csharp
// Caching is enabled automatically
var parser = new EditorConfigParser(myFileSystem);
```

When `fileSystem` is omitted the library uses `new FileSystem()` (the real disk).

## Performance

The library is designed to be high-performance and allocation-light.

- **Zero-allocation glob matching** — the match engine is a `ref struct` over
  `ReadOnlySpan<char>`, so matching produces no heap allocations.
- **Source-generated regexes** — the INI parser uses `[GeneratedRegex]` on
  .NET 7+ for optimal regex throughput.
- **Per-directory chain cache** — `.editorconfig` files are resolved once per
  directory per parser instance. For a repository with thousands of source files,
  the traversal and `File.Exists` checks are paid at most once per unique directory.
- **File-content cache** — `EditorConfigFileCache` (used by default) caches
  parsed files by path + modification time + size. Cache hits require only a
  single metadata stat with no file read.

Benchmark results on Apple M2 Pro · .NET 10 · Arm64:

| Scenario | Time | Allocations |
|---|---:|---:|
| Single file, warm parser | 1.3 μs | 1.5 KB |
| 50 files, same directory, warm parser | 63 μs | 82 KB |
| Glob match (pre-compiled, per match) | 4.7 μs | **0 B** |

## Target frameworks

The library targets `netstandard2.0`, `net462`, and `net10.0`. All three targets
are functionally equivalent. The `net10.0` target enables additional performance
features (source-generated regexes, AOT compatibility).

## CLI tool

You can omit `dotnet` if installed as a global tool:

```
> dotnet editorconfig

    Usage: editorconfig [OPTIONS] FILEPATH1 [FILEPATH2 FILEPATH3 ...]

    EditorConfig .NET Core Version 0.12

    Options:

        -h, --help     output usage information
        -V, --version  output the version number
        -f <path>      Specify conf filename other than ".editorconfig"
        -b <version>   Specify version (used by devs to test compatibility)
```

Example:

```
> dotnet editorconfig anatomy.md
charset=utf-8
insert_final_newline=true
end_of_line=lf
tab_width=8
trim_trailing_whitespace=sometimes
```

## Development

Clone the repository and initialise the conformance test submodule:

```
git clone git@github.com:editorconfig/editorconfig-core-net.git
git submodule init
git submodule update
```

Build:

```
dotnet build
```

Run unit tests ([TUnit](https://github.com/thomhurst/TUnit)):

```
dotnet run --project src/EditorConfig.Tests -c Release
```

Run the upstream [editorconfig conformance suite](https://github.com/editorconfig/editorconfig-core-test)
(requires [CMake](https://cmake.org)):

```
cmake .
ctest .
```

Run benchmarks ([BenchmarkDotNet](https://benchmarkdotnet.org)):

```
dotnet run -c Release --project benchmarks/EditorConfig.Benchmarks
```

[EditorConfig C Core]: https://github.com/editorconfig/editorconfig-core
[EditorConfig Python Core]: https://github.com/editorconfig/editorconfig-core-py
