```

BenchmarkDotNet v0.14.0, macOS 26.5 (25F71) [Darwin 25.5.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.8 (10.0.826.23019), Arm64 RyuJIT AdvSIMD


```
| Method         | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CreateAndMatch | 11,298.3 ns | 222.70 ns | 439.59 ns |  1.00 |    0.05 | 2.3651 |   19896 B |        1.00 |
| MatchOnly      |  4,700.0 ns |  55.14 ns |  48.88 ns |  0.42 |    0.02 |      - |         - |        0.00 |
| CreateSingle   |    587.4 ns |   6.30 ns |   5.58 ns |  0.05 |    0.00 | 0.2317 |    1944 B |        0.10 |
