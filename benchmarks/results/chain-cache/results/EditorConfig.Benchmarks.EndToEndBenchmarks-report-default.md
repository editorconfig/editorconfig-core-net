
BenchmarkDotNet v0.14.0, macOS 26.5 (25F71) [Darwin 25.5.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD


 Method                        | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
------------------------------ |----------:|----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
 ParseFresh                    |  1.258 μs | 0.0035 μs | 0.0030 μs |  1.258 μs |  1.00 |    0.00 | 0.1869 |   1.54 KB |        1.00 |
 ParseWithFileCache            |  1.294 μs | 0.0257 μs | 0.0548 μs |  1.255 μs |  1.03 |    0.04 | 0.1869 |   1.54 KB |        1.00 |
 ParseSameDirFresh             | 63.453 μs | 0.1820 μs | 0.1702 μs | 63.406 μs | 50.44 |    0.17 | 9.8877 |  81.64 KB |       53.05 |
 ParseSameDirCached            | 63.168 μs | 0.1743 μs | 0.1456 μs | 63.123 μs | 50.21 |    0.16 | 9.8877 |  81.64 KB |       53.05 |
 ParseSameDirWithExplicitChain | 60.670 μs | 1.1077 μs | 1.0361 μs | 60.648 μs | 48.23 |    0.81 | 8.0566 |  66.62 KB |       43.28 |
