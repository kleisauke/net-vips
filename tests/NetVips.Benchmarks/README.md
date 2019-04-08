# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 08/04/19 with libvips 8.7.4, Magick.NET 7.11.1.0, ImageSharp 1.0.0-beta0006, SkiaSharp 1.68.0 and System.Drawing.Common 4.5.1.

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17763.404 (1809/October2018Update/Redstone5)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=2.2.202
  [Host]     : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  Job-LYOWLC : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT

Toolchain=.NET Core 2.2.0  

```
|         Method | input | output |      Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|----------:|----------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **32.48 ms** | **0.2043 ms** | **0.1706 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 368.46 ms | 0.8254 ms | 0.7721 ms | 11.34 |    0.07 |
|     ImageSharp¹ | t.jpg | t2.jpg | 178.66 ms | 1.1065 ms | 0.9809 ms |  5.50 |    0.03 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 592.51 ms | 0.2439 ms | 0.1904 ms | 18.24 |    0.10 |
| System.Drawing² | t.jpg | t2.jpg | 245.10 ms | 2.3623 ms | 2.0941 ms |  7.54 |    0.09 |
|                |       |        |           |           |           |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **21.59 ms** | **0.2054 ms** | **0.1715 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 354.83 ms | 1.0426 ms | 0.8706 ms | 16.44 |    0.15 |
| System.Drawing² | t.tif | t2.tif | 229.58 ms | 0.7594 ms | 0.7103 ms | 10.63 |    0.08 |

¹ ImageSharp and SkiaSharp does not have TIFF support, so I only tested with JPEG files.  
² System.Drawing does not have a sharpening or convolution operation, so I skipped that part of the benchmark.

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