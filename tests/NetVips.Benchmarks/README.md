# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 21/02/19 with libvips 8.7.4, Magick.NET 7.11.0.0 and ImageSharp 1.0.0-beta0006.

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17763.316 (1809/October2018Update/Redstone5)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=2.2.103
  [Host]     : .NET Core 2.2.1 (CoreCLR 4.6.27207.03, CoreFX 4.6.27207.03), 64bit RyuJIT
  Job-BVUXGY : .NET Core 2.2.1 (CoreCLR 4.6.27207.03, CoreFX 4.6.27207.03), 64bit RyuJIT

Toolchain=.NET Core 2.2.0  

```
|     Method | input | output |      Mean |     Error |    StdDev | Ratio | RatioSD |
|----------- |------ |------- |----------:|----------:|----------:|------:|--------:|
|    **NetVips** | **t.jpg** | **t2.jpg** |  **32.76 ms** | **0.1900 ms** | **0.1777 ms** |  **1.00** |    **0.00** |
| Magick.NET | t.jpg | t2.jpg | 375.10 ms | 2.5946 ms | 2.4270 ms | 11.45 |    0.11 |
| ImageSharp¹ | t.jpg | t2.jpg | 175.63 ms | 1.6125 ms | 1.4295 ms |  5.36 |    0.05 |
|            |       |        |           |           |           |       |         |
|    **NetVips** | **t.tif** | **t2.tif** |  **21.75 ms** | **0.4179 ms** | **0.3909 ms** |  **1.00** |    **0.00** |
| Magick.NET | t.tif | t2.tif | 356.96 ms | 1.4376 ms | 1.2744 ms | 16.41 |    0.29 |

¹ ImageSharp does not have TIFF support, so I only tested with JPEG files.

## Performance test design

The project contains a `Benchmark.cs` file with specific scripts 
using various libraries available on .NET.

Each script is coded to execute the same scenario (see Scenario section).

See "Do it yourself" section for how to run benchmark scenario.

## Scenario

Test scenario was taken from [Speed and Memory
use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
page from libvips [Home
page](https://libvips.github.io/libvips/).

## Do it yourself

```bash
git clone https://github.com/kleisauke/net-vips

cd net-vips/tests/NetVips.Benchmarks

dotnet run -c Release
```