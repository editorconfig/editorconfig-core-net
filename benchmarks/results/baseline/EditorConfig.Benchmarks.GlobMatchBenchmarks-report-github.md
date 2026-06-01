```

BenchmarkDotNet v0.14.0, macOS 26.5 (25F71) [Darwin 25.5.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD


```
| Method         | Mean        | Error     | StdDev    | Median      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------- |------------:|----------:|----------:|------------:|------:|--------:|-------:|----------:|------------:|
| CreateAndMatch | 13,293.7 ns | 264.42 ns | 463.11 ns | 13,093.7 ns |  1.00 |    0.05 | 2.4567 |   20592 B |        1.00 |
| MatchOnly      |  6,511.0 ns |  50.88 ns |  45.11 ns |  6,507.0 ns |  0.49 |    0.02 | 0.1068 |     912 B |        0.04 |
| CreateSingle   |    796.8 ns |  15.86 ns |  33.46 ns |    782.6 ns |  0.06 |    0.00 | 0.2289 |    1920 B |        0.09 |
