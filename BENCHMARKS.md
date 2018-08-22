### Benchmarks

You could get the sources [here](https://github.com/CityOfZion/neo-hypervm/tree/development/tests/Neo.HyperVM.Benchmarks/Benchmarks)

``` ini

BenchmarkDotNet=v0.11.0, OS=ubuntu 16.04
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.400
  [Host]     : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT

```

|            Method |   OpCodes | Repetitions |       Mean |     Error |    StdDev |        Min |        Max |     Median | Rank |
|------------------ |---------- |------------ |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-----:|
|           HyperVM | FACTORIAL |           1 |   175.9 us |  3.378 us |  3.469 us |   166.1 us |   182.8 us |   175.7 us |    1 |
|             NeoVM | FACTORIAL |           1 |   501.2 us | 10.006 us | 22.584 us |   462.0 us |   552.2 us |   502.3 us |    2 |
| ApplicationEngine | FACTORIAL |           1 | 1,133.0 us | 22.495 us | 34.353 us | 1,060.5 us | 1,198.3 us | 1,129.2 us |    3 |
| | | | | | | | | | |
|           HyperVM | FIBONACCI |           1 |  83.97 ms |  0.9685 ms |  0.7561 ms |  82.20 ms |  85.42 ms |  83.93 ms |    1 |
|             NeoVM | FIBONACCI |           1 | 291.73 ms |  7.9826 ms |  8.5413 ms | 281.57 ms | 310.59 ms | 288.72 ms |    2 |
| ApplicationEngine | FIBONACCI |           1 | 549.11 ms | 10.5625 ms | 12.5739 ms | 524.01 ms | 568.39 ms | 552.76 ms |    3 |
| | | | | | | | | | |
|           HyperVM | SHA1 |        1000 |   489.4 us |  9.713 us | 14.54 us |   490.3 us |   466.2 us |   523.7 us |    1 |
|             NeoVM | SHA1 |        1000 | 1,650.0 us | 32.977 us | 61.12 us | 1,643.6 us | 1,579.0 us | 1,819.8 us |    2 |
| ApplicationEngine | SHA1 |        1000 | 1,953.2 us | 38.063 us | 56.97 us | 1,918.0 us | 1,895.0 us | 2,074.3 us |    3 |
| | | | | | | | | | |
|           HyperVM |     NOP |        1000 |  12.04 us | 0.2432 us | 0.3565 us |  11.33 us |  12.96 us |  12.03 us |    1 |
|             NeoVM |     NOP |        1000 | 101.07 us | 1.2668 us | 1.1230 us |  99.89 us | 103.42 us | 100.79 us |    2 |
| ApplicationEngine |     NOP |        1000 | 198.10 us | 3.7617 us | 3.6945 us | 191.56 us | 204.89 us | 198.66 us |    3 |
| | | | | | | | | | |
|           HyperVM | PUSH0+DROP |        1000 |  82.24 us |  1.615 us |  2.562 us |  82.15 us |  79.07 us |  88.77 us |    1 |
|             NeoVM | PUSH0+DROP |        1000 | 272.53 us | 11.613 us | 32.756 us | 260.33 us | 234.83 us | 376.89 us |    2 |
| ApplicationEngine | PUSH0+DROP |        1000 | 482.80 us |  9.946 us | 16.342 us | 475.53 us | 465.78 us | 532.22 us |    3 |

*NeoVM and ApplicationEngine come form NEO 3.0 binaries*

### Other
There is a `Makefile` in the root of the project. To build the `VM` source:
