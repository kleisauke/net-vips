# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 19/05/19 with libvips 8.8.0-rc3, Magick.NET 7.13.0.0, ImageSharp 1.0.0-beta0006, SkiaSharp 1.68.0 and System.Drawing.Common 4.5.1.

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17763.503 (1809/October2018Update/Redstone5)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=2.2.204
  [Host]     : .NET Core 2.2.5 (CoreCLR 4.6.27617.05, CoreFX 4.6.27618.01), 64bit RyuJIT
  Job-FDUBJL : .NET Core 2.2.5 (CoreCLR 4.6.27617.05, CoreFX 4.6.27618.01), 64bit RyuJIT

Toolchain=.NET Core 2.2.0  

```
|         Method | input | output |      Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|----------:|----------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **29.24 ms** | **0.1968 ms** | **0.1643 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 372.70 ms | 1.1416 ms | 1.0679 ms | 12.75 |    0.07 |
|     ImageSharp¹ | t.jpg | t2.jpg | 177.57 ms | 1.2384 ms | 1.0978 ms |  6.08 |    0.04 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 590.92 ms | 3.8533 ms | 3.6043 ms | 20.19 |    0.16 |
| System.Drawing² | t.jpg | t2.jpg | 240.98 ms | 0.7461 ms | 0.6979 ms |  8.24 |    0.05 |
|                |       |        |           |           |           |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **19.17 ms** | **0.2241 ms** | **0.2096 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 354.08 ms | 1.8045 ms | 1.6880 ms | 18.48 |    0.17 |
| System.Drawing² | t.tif | t2.tif | 232.79 ms | 1.2958 ms | 1.2121 ms | 12.15 |    0.16 |

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

dotnet build /p:BenchmarkWithNuGetBinaries=true
dotnet run -c Release
```