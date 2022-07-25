# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 25/07/22 with libvips 8.13.0, Magick.NET 11.3.0, ImageSharp 2.1.3, SkiaSharp 2.88.0 and System.Drawing.Common 6.0.0.

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1826 (21H2)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.302
  [Host]       : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT
  .Net 6.0 CLI : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT

Job=.Net 6.0 CLI  Arguments=/p:DebugType=portable  Toolchain=.NET 6.0  

```
|                     Method | input | output |       Mean |   Error |  StdDev | Ratio | RatioSD |
|--------------------------- |------ |------- |-----------:|--------:|--------:|------:|--------:|
|                    **NetVips** | **t.jpg** | **t2.jpg** |   **166.4 ms** | **2.26 ms** | **2.11 ms** |  **1.00** |    **0.00** |
|                 Magick.NET | t.jpg | t2.jpg | 2,958.9 ms | 3.96 ms | 3.70 ms | 17.79 |    0.23 |
|     ImageSharp<sup>1</sup> | t.jpg | t2.jpg |   949.7 ms | 8.10 ms | 7.57 ms |  5.71 |    0.09 |
|      SkiaSharp<sup>2</sup> | t.jpg | t2.jpg | 1,956.1 ms | 2.46 ms | 2.30 ms | 11.76 |    0.15 |
| System.Drawing<sup>3</sup> | t.jpg | t2.jpg | 2,347.6 ms | 4.56 ms | 4.26 ms | 14.11 |    0.18 |
|                            |       |        |            |         |         |       |         |
|                    **NetVips** | **t.tif** | **t2.tif** |   **114.4 ms** | **0.57 ms** | **0.47 ms** |  **1.00** |    **0.00** |
|                 Magick.NET | t.tif | t2.tif | 2,838.9 ms | 6.92 ms | 6.13 ms | 24.82 |    0.12 |
| System.Drawing<sup>3</sup> | t.tif | t2.tif | 2,020.0 ms | 4.09 ms | 3.42 ms | 17.66 |    0.08 |

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
