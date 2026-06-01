
BenchmarkDotNet v0.14.0, macOS 26.5 (25F71) [Darwin 25.5.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD


 Method                    | Mean     | Error    | StdDev   | Gen0   | Gen1   | Allocated |
-------------------------- |---------:|---------:|---------:|-------:|-------:|----------:|
 ParseFile                 | 21.22 μs | 0.420 μs | 0.868 μs | 4.9744 | 0.1526 |  40.76 KB |
 ParseFileAndCountSections | 20.62 μs | 0.364 μs | 0.341 μs | 4.9744 | 0.1526 |  40.76 KB |
