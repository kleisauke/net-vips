# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 30/01/20 with libvips 8.9.1, Magick.NET 7.15.1, ImageSharp 1.0.0-unstable0608, SkiaSharp 1.68.1.1 and System.Drawing.Common 4.7.0.

``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=3.1.101
  [Host]            : .NET Core 3.1.1 (CoreCLR 4.700.19.60701, CoreFX 4.700.19.60801), X64 RyuJIT
  .Net Core 3.1 CLI : .NET Core 3.1.1 (CoreCLR 4.700.19.60701, CoreFX 4.700.19.60801), X64 RyuJIT

Job=.Net Core 3.1 CLI  Toolchain=.NET Core 3.1  

```
|         Method | input | output |      Mean |    Error |   StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|---------:|---------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **29.53 ms** | **0.528 ms** | **0.494 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 307.29 ms | 2.742 ms | 2.565 ms | 10.41 |    0.16 |
|     ImageSharp¹ | t.jpg | t2.jpg | 149.57 ms | 0.808 ms | 0.716 ms |  5.06 |    0.10 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 229.10 ms | 0.150 ms | 0.140 ms |  7.76 |    0.13 |
| System.Drawing² | t.jpg | t2.jpg | 261.82 ms | 0.252 ms | 0.235 ms |  8.87 |    0.15 |
|                |       |        |           |          |          |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **18.87 ms** | **0.374 ms** | **0.431 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 287.80 ms | 0.316 ms | 0.280 ms | 15.29 |    0.36 |
| System.Drawing² | t.tif | t2.tif | 256.64 ms | 0.782 ms | 0.731 ms | 13.60 |    0.33 |

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