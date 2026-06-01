
BenchmarkDotNet v0.14.0, macOS 26.5 (25F71) [Darwin 25.5.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD


 Method             | Mean     | Error    | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
------------------- |---------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
 ParseFresh         | 71.01 μs | 0.416 μs | 0.368 μs |  1.00 | 9.5215 | 0.2441 |  78.72 KB |        1.00 |
 ParseWithFileCache | 55.03 μs | 0.979 μs | 1.047 μs |  0.77 | 4.5166 |      - |  37.84 KB |        0.48 |
