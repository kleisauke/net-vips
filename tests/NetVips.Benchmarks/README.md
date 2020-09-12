# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 12/09/20 with libvips 8.10.1, Magick.NET 7.21.1, ImageSharp 1.0.1, SkiaSharp 2.80.2 and System.Drawing.Common 4.7.0.

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=3.1.402
  [Host]            : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  .Net Core 3.1 CLI : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT

Job=.Net Core 3.1 CLI  Toolchain=.NET Core 3.1  

```
|         Method | input | output |      Mean |    Error |   StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|---------:|---------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **27.38 ms** | **0.269 ms** | **0.252 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 326.70 ms | 1.323 ms | 1.173 ms | 11.94 |    0.13 |
|     ImageSharp¹ | t.jpg | t2.jpg | 134.19 ms | 1.293 ms | 1.210 ms |  4.90 |    0.05 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 219.52 ms | 0.337 ms | 0.282 ms |  8.02 |    0.07 |
| System.Drawing² | t.jpg | t2.jpg | 268.42 ms | 0.527 ms | 0.467 ms |  9.81 |    0.09 |
|                |       |        |           |          |          |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **18.34 ms** | **0.344 ms** | **0.322 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 304.36 ms | 0.328 ms | 0.307 ms | 16.60 |    0.29 |
| System.Drawing² | t.tif | t2.tif | 260.29 ms | 1.988 ms | 1.860 ms | 14.20 |    0.31 |

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