# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 13/07/20 with libvips 8.10.0-beta2, Magick.NET 7.20.0.1, ImageSharp 1.0.0-rc0003, SkiaSharp 2.80.0 and System.Drawing.Common 4.7.0.

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.329 (2004/?/20H1)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=3.1.301
  [Host]            : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT
  .Net Core 3.1 CLI : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT

Job=.Net Core 3.1 CLI  Toolchain=.NET Core 3.1  

```
|         Method | input | output |      Mean |    Error |   StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|---------:|---------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **27.65 ms** | **0.394 ms** | **0.369 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 297.80 ms | 0.582 ms | 0.454 ms | 10.77 |    0.15 |
|     ImageSharp¹ | t.jpg | t2.jpg | 133.18 ms | 1.606 ms | 1.424 ms |  4.82 |    0.09 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 219.55 ms | 0.769 ms | 0.719 ms |  7.94 |    0.11 |
| System.Drawing² | t.jpg | t2.jpg | 265.86 ms | 0.606 ms | 0.567 ms |  9.62 |    0.12 |
|                |       |        |           |          |          |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **18.45 ms** | **0.356 ms** | **0.350 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 277.19 ms | 0.427 ms | 0.357 ms | 14.97 |    0.28 |
| System.Drawing² | t.tif | t2.tif | 258.49 ms | 2.099 ms | 1.963 ms | 14.00 |    0.29 |

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