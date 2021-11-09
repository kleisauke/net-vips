# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 23/06/21 with libvips 8.11.0, Magick.NET 7.24.1, ImageSharp 1.0.3, SkiaSharp 2.80.2 and System.Drawing.Common 5.0.2.

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1052 (21H1/May2021Update)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=5.0.301
  [Host]       : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
  .Net 5.0 CLI : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT

Job=.Net 5.0 CLI  Arguments=/p:DebugType=portable  Toolchain=.NET 5.0  

```
|                     Method | input | output |        Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------------------- |------ |------- |------------:|----------:|----------:|------:|--------:|
|                    **NetVips** | **t.jpg** | **t2.jpg** |   **169.56 ms** |  **3.275 ms** |  **3.217 ms** |  **1.00** |    **0.00** |
|                 Magick.NET | t.jpg | t2.jpg | 3,036.39 ms |  6.992 ms |  6.540 ms | 17.89 |    0.34 |
|     ImageSharp<sup>1</sup> | t.jpg | t2.jpg |   775.82 ms |  9.826 ms |  9.191 ms |  4.57 |    0.09 |
|      SkiaSharp<sup>2</sup> | t.jpg | t2.jpg | 1,943.25 ms | 17.594 ms | 16.458 ms | 11.45 |    0.23 |
| System.Drawing<sup>3</sup> | t.jpg | t2.jpg | 2,363.23 ms |  5.788 ms |  5.414 ms | 13.93 |    0.27 |
|                            |       |        |             |           |           |       |         |
|                    **NetVips** | **t.tif** | **t2.tif** |    **83.88 ms** |  **0.829 ms** |  **0.775 ms** |  **1.00** |    **0.00** |
|                 Magick.NET | t.tif | t2.tif | 2,886.12 ms |  6.449 ms |  6.033 ms | 34.41 |    0.34 |
| System.Drawing<sup>3</sup> | t.tif | t2.tif | 2,023.22 ms |  4.330 ms |  3.838 ms | 24.13 |    0.25 |

<sup>1</sup> ImageSharp does not support tiled TIFF images, so I only tested with JPEG files.  
<sup>2</sup> SkiaSharp does not have TIFF support, so I only tested with JPEG files.  
<sup>3</sup> System.Drawing does not have a sharpening or convolution operation, so I skipped that part of the benchmark.

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

# Benchmark with NuGet binaries
dotnet run -c Release

# Benchmark with a globally installed libvips
dotnet build -c Release /p:UseGlobalLibvips=true
dotnet run --no-build -c Release
```
