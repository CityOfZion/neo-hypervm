# Benchmarks

``` ini
BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
Frequency=2531246 Hz, Resolution=395.0624 ns, Timer=TSC
.NET Core SDK=2.1.400
  [Host]     : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
```

#### Factorial (1 iteration)

You could get the source [here](https://github.com/CityOfZion/neo-hypervm/blob/development/tests/Neo.HyperVM.Benchmarks/Benchmarks/VMBenchmarkFACTORIAL.cs)

<table>
<thead>
<tr><th>Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Min</th><th>Max</th><th>Median</th><th>Rank</th></tr>
</thead>
<tbody>

<tr><td>HyperVM</td><td>169.0 us</td><td>1.442 us</td><td>1.278 us</td><td>167.0 us</td><td>170.8 us</td><td>168.9 us</td><td>1</td>
</tr><tr><td>NeoVM</td><td>419.2 us</td><td>3.526 us</td><td>2.944 us</td><td>415.0 us</td><td>423.1 us</td><td>418.8 us</td><td>2</td>
</tr><tr><td>ApplicationEngine</td><td>926.2 us</td><td>8.375 us</td><td>7.425 us</td><td>912.1 us</td><td>943.7 us</td><td>924.7 us</td><td>3</td>
</tr>

</tbody></table>

<p align="center">
<img src="https://github.com/CityOfZion/neo-hypervm/raw/development/images/Neo.HyperVM.Benchmarks.VMBenchmarkFACTORIAL-barplot.jpg" width="300">
</p>

#### Fibonacci (1 iteration)

You could get the source [here](https://github.com/CityOfZion/neo-hypervm/blob/development/tests/Neo.HyperVM.Benchmarks/Benchmarks/VMBenchmarkFB.cs)

<table>
<thead>
<tr><th>Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Min</th><th>Max</th><th>Median</th><th>Rank</th></tr>
</thead>
<tbody>

<tr><td>HyperVM</td><td>78.18 ms</td><td>0.7943 ms</td><td>0.6633 ms</td><td>77.46 ms</td><td>79.68 ms</td><td>77.98 ms</td><td>1</td>
</tr><tr><td>NeoVM</td><td>250.81 ms</td><td>1.7531 ms</td><td>1.6398 ms</td><td>247.62 ms</td><td>253.24 ms</td><td>251.37 ms</td><td>2</td>
</tr><tr><td>ApplicationEngine</td><td>463.75 ms</td><td>5.4130 ms</td><td>4.7985 ms</td><td>455.83 ms</td><td>472.76 ms</td><td>463.05 ms</td><td>3</td>
</tr>

</tbody></table>

<p align="center">
<img src="https://github.com/CityOfZion/neo-hypervm/raw/development/images/Neo.HyperVM.Benchmarks.VMBenchmarkFB-barplot.jpg" width="300">
</p>

#### NOP (1000 iterations)

You could get the source [here](https://github.com/CityOfZion/neo-hypervm/blob/development/tests/Neo.HyperVM.Benchmarks/Benchmarks/VMBenchmarkNOP.cs)

<table>
<thead>
<tr><th>Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Min</th><th>Max</th><th>Median</th><th>Rank</th></tr>
</thead>
<tbody>

<tr><td>HyperVM</td><td>16.00 us</td><td>1.1392 us</td><td>3.3590 us</td><td>11.27 us</td><td>26.89 us</td><td>15.69 us</td><td>1</td>
</tr><tr><td>NeoVM</td><td>93.77 us</td><td>0.9077 us</td><td>0.8491 us</td><td>92.67 us</td><td>95.12 us</td><td>93.71 us</td><td>2</td>
</tr><tr><td>ApplicationEngine</td><td>174.88 us</td><td>1.7343 us</td><td>1.5374 us</td><td>170.79 us</td><td>177.40 us</td><td>175.12 us</td><td>3</td>
</tr>

</tbody></table>

<p align="center">
<img src="https://github.com/CityOfZion/neo-hypervm/raw/development/images/Neo.HyperVM.Benchmarks.VMBenchmarkNOP-barplot.jpg" width="300">
</p>

#### PUSH0+DROP (1000 iterations)

You could get the source [here](https://github.com/CityOfZion/neo-hypervm/blob/development/tests/Neo.HyperVM.Benchmarks/Benchmarks/VMBenchmarkPUSH0.cs)

<table>
<thead>
<tr><th>Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Min</th><th>Max</th><th>Median</th><th>Rank</th></tr>
</thead>
<tbody>

<tr><td>HyperVM</td><td>86.60 us</td><td>0.5659 us</td><td>0.4418 us</td><td>85.80 us</td><td>87.33 us</td><td>86.47 us</td><td>1</td>
</tr><tr><td>NeoVM</td><td>194.28 us</td><td>1.9218 us</td><td>1.5004 us</td><td>191.20 us</td><td>196.11 us</td><td>194.54 us</td><td>2</td>
</tr><tr><td>ApplicationEngine</td><td>431.37 us</td><td>8.5261 us</td><td>15.3742 us</td><td>399.66 us</td><td>465.16 us</td><td>433.15 us</td><td>3</td>
</tr>

</tbody></table>

<p align="center">
<img src="https://github.com/CityOfZion/neo-hypervm/raw/development/images/Neo.HyperVM.Benchmarks.VMBenchmarkPUSH0-barplot.jpg" width="300">
</p>

# Cryptography Benchmarks

*Made with Linux OpenSSL version*

``` ini
BenchmarkDotNet=v0.11.1, OS=ubuntu 16.04
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.400
  [Host]     : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
```

#### VERIFY (1000 iterations)

You could get the source [here](https://github.com/CityOfZion/neo-hypervm/blob/development/tests/Neo.HyperVM.Benchmarks/Benchmarks/VMBenchmarkVERIFY.cs)

<table>
<thead>
<tr><th>Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Min</th><th>Max</th><th>Median</th><th>Rank</th></tr>
</thead>
<tbody>
  
<tr><td>HyperVM</td><td>161.9 ms</td><td>3.177 ms</td><td>6.044 ms</td><td>149.9 ms</td><td>175.5 ms</td><td>162.9 ms</td><td>1</td>
</tr><tr><td>NeoVM</td><td>399.2 ms</td><td>7.917 ms</td><td>15.810 ms</td><td>363.2 ms</td><td>435.0 ms</td><td>398.2 ms</td><td>3</td>
</tr><tr><td>ApplicationEngine</td><td>382.2 ms</td><td>6.637 ms</td><td>6.208 ms</td><td>370.3 ms</td><td>392.7 ms</td><td>382.9 ms</td><td>2</td>
</tr>

</tbody></table>

#### SHA1 (1000 iterations)

You could get the source [here](https://github.com/CityOfZion/neo-hypervm/blob/development/tests/Neo.HyperVM.Benchmarks/Benchmarks/VMBenchmarkSHA1.cs)

<table>
<thead>
<tr><th>Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Min</th><th>Max</th><th>Median</th><th>Rank</th></tr>
</thead>
<tbody>

<tr><td>HyperVM</td><td>440.9 us</td><td>8.533 us</td><td>12.77 us</td><td>417.0 us</td><td>464.7 us</td><td>443.0 us</td><td>1</td>
</tr><tr><td>NeoVM</td><td>1,581.0 us</td><td>31.379 us</td><td>90.03 us</td><td>1,441.9 us</td><td>1,769.1 us</td><td>1,583.8 us</td><td>2</td>
</tr><tr><td>ApplicationEngine</td><td>1,954.4 us</td><td>39.037 us</td><td>105.54 us</td><td>1,779.2 us</td><td>2,183.6 us</td><td>1,944.8 us</td><td>3</td>
</tr>

</tbody></table>

# Notes

*NeoVM and ApplicationEngine come form NEO 3.0 binaries*
