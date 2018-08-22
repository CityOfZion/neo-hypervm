### Benchmarks

You could get the sources [here](https://github.com/CityOfZion/neo-hypervm/tree/development/tests/Neo.HyperVM.Benchmarks/Benchmarks)

``` ini

BenchmarkDotNet=v0.11.0, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
Frequency=2531248 Hz, Resolution=395.0620 ns, Timer=TSC
.NET Core SDK=2.1.400
  [Host]     : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  Job-CAVTBJ : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT

InvocationCount=1000  IterationCount=10  LaunchCount=3  
UnrollFactor=1  WarmupCount=2  

```

|            Method |   Test |      Mean |     Error |    StdDev |       Min |       Max |    Median | Rank |
|------------------ |------- |----------:|----------:|----------:|----------:|----------:|----------:|-----:|
|           HyperVM | NOP*1K |  17.66 us | 1.187 us | 3.501 us |  12.27 us |  27.68 us |  17.57 us |    1 |
|             NeoVM | NOP*1K |  80.59 us | 1.530 us | 1.761 us |  77.33 us |  83.66 us |  80.52 us |    2 |
| ApplicationEngine | NOP*1K | 163.17 us | 2.280 us | 1.904 us | 159.16 us | 167.62 us | 162.97 us |    3 |
| | | | | | | | | |
|           HyperVM | (PUSH1+DROP)*1K |  89.62 us | 1.791 us |  3.699 us |  82.67 us |  99.01 us |  89.50 us |    1 |
|             NeoVM | (PUSH1+DROP)*1K | 229.47 us | 4.925 us | 14.521 us | 200.12 us | 267.15 us | 228.49 us |    2 |
| ApplicationEngine | (PUSH1+DROP)*1K | 454.61 us | 9.058 us | 10.431 us | 436.31 us | 473.51 us | 456.10 us |    3 |

*NeoVM and ApplicationEngine come form from NEO 3.0 binaries*

### Other
There is a `Makefile` in the root of the project. To build the `VM` source:
