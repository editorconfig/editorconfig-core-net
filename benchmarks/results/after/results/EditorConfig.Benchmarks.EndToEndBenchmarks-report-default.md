
BenchmarkDotNet v0.14.0, macOS 26.5 (25F71) [Darwin 25.5.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD


 Method             | Mean     | Error    | StdDev   | Median   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
------------------- |---------:|---------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
 ParseFresh         | 48.65 μs | 0.385 μs | 0.361 μs | 48.63 μs |  1.00 | 5.3711 | 0.2441 |  43.99 KB |        1.00 |
 ParseWithFileCache | 11.17 μs | 0.221 μs | 0.467 μs | 10.98 μs |  0.23 | 0.8850 |      - |   7.34 KB |        0.17 |
