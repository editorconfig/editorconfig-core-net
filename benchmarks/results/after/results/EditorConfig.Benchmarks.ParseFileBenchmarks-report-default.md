
BenchmarkDotNet v0.14.0, macOS 26.5 (25F71) [Darwin 25.5.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD


 Method                    | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
-------------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
 ParseFile                 | 10.385 μs | 0.2050 μs | 0.5142 μs | 4.2572 | 0.1221 |  34.86 KB |
 ParseFileAndCountSections |  9.980 μs | 0.1980 μs | 0.3570 μs | 4.2572 | 0.1221 |  34.86 KB |
