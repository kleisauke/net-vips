# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 27/03/19 with libvips 8.7.4, Magick.NET 7.11.1.0 and ImageSharp 1.0.0-beta0006.

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17763.379 (1809/October2018Update/Redstone5)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=2.2.105
  [Host]     : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Job-RQRRRV : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT

Toolchain=.NET Core 2.2.0  

```
|     Method | input | output |      Mean |     Error |    StdDev | Ratio | RatioSD |
|----------- |------ |------- |----------:|----------:|----------:|------:|--------:|
|    **NetVips** | **t.jpg** | **t2.jpg** |  **32.56 ms** | **0.3330 ms** | **0.2952 ms** |  **1.00** |    **0.00** |
| Magick.NET | t.jpg | t2.jpg | 368.35 ms | 0.8982 ms | 0.8402 ms | 11.32 |    0.10 |
| ImageSharp¹ | t.jpg | t2.jpg | 177.75 ms | 1.1484 ms | 1.0181 ms |  5.46 |    0.05 |
|            |       |        |           |           |           |       |         |
|    **NetVips** | **t.tif** | **t2.tif** |  **21.61 ms** | **0.3139 ms** | **0.2783 ms** |  **1.00** |    **0.00** |
| Magick.NET | t.tif | t2.tif | 353.73 ms | 2.0084 ms | 1.8786 ms | 16.37 |    0.21 |

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