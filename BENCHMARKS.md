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

<tr><td>HyperVM</td><td>178.6 us</td><td>3.496 us</td><td>3.271 us</td><td>175.6 us</td><td>188.9 us</td><td>177.7 us</td><td>1</td>
</tr><tr><td>NeoVM</td><td>436.0 us</td><td>4.957 us</td><td>4.637 us</td><td>429.9 us</td><td>442.8 us</td><td>434.5 us</td><td>2</td>
</tr><tr><td>ApplicationEngine</td><td>963.5 us</td><td>19.009 us</td><td>26.648 us</td><td>931.9 us</td><td>1,023.3 us</td><td>949.0 us</td><td>3</td>
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

<tr><td>HyperVM</td><td>88.45 ms</td><td>1.695 ms</td><td>1.741 ms</td><td>83.87 ms</td><td>91.09 ms</td><td>88.78 ms</td><td>1</td>
</tr><tr><td>NeoVM</td><td>252.29 ms</td><td>5.020 ms</td><td>11.932 ms</td><td>232.90 ms</td><td>277.30 ms</td><td>252.74 ms</td><td>2</td>
</tr><tr><td>ApplicationEngine</td><td>474.58 ms</td><td>6.831 ms</td><td>6.390 ms</td><td>465.98 ms</td><td>483.58 ms</td><td>474.88 ms</td><td>3</td>
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

<tr><td>HyperVM</td><td>16.73 us</td><td>1.398 us</td><td>4.123 us</td><td>11.24 us</td><td>24.85 us</td><td>16.05 us</td><td>1</td>
</tr><tr><td>NeoVM</td><td>93.37 us</td><td>2.380 us</td><td>2.338 us</td><td>90.75 us</td><td>98.58 us</td><td>92.50 us</td><td>2</td>
</tr><tr><td>ApplicationEngine</td><td>178.83 us</td><td>3.558 us</td><td>8.176 us</td><td>167.61 us</td><td>195.04 us</td><td>174.22 us</td><td>3</td>
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

<tr><td>HyperVM</td><td>93.79 us</td><td>1.863 us</td><td>3.061 us</td><td>85.67 us</td><td>99.53 us</td><td>93.77 us</td><td>1</td>
</tr><tr><td>NeoVM</td><td>191.83 us</td><td>3.385 us</td><td>2.826 us</td><td>186.77 us</td><td>197.29 us</td><td>191.87 us</td><td>2</td>
</tr><tr><td>ApplicationEngine</td><td>398.59 us</td><td>8.375 us</td><td>7.424 us</td><td>388.67 us</td><td>416.32 us</td><td>396.74 us</td><td>3</td>
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
