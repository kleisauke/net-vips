# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 02/12/21 with libvips 8.12.1, Magick.NET 8.4.0, ImageSharp 1.0.4, SkiaSharp 2.80.3 and System.Drawing.Common 6.0.0.

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1348 (21H2)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.100
  [Host]       : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  .Net 6.0 CLI : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=.Net 6.0 CLI  Arguments=/p:DebugType=portable  Toolchain=.NET 6.0  

```
|                     Method | input | output |        Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------------------- |------ |------- |------------:|----------:|----------:|------:|--------:|
|                    **NetVips** | **t.jpg** | **t2.jpg** |   **167.16 ms** |  **3.121 ms** |  **2.920 ms** |  **1.00** |    **0.00** |
|                 Magick.NET | t.jpg | t2.jpg | 2,908.85 ms |  6.820 ms |  6.379 ms | 17.41 |    0.28 |
|     ImageSharp<sup>1</sup> | t.jpg | t2.jpg | 1,189.96 ms | 12.071 ms | 11.291 ms |  7.12 |    0.12 |
|      SkiaSharp<sup>2</sup> | t.jpg | t2.jpg | 1,960.73 ms | 18.292 ms | 17.110 ms | 11.73 |    0.21 |
| System.Drawing<sup>3</sup> | t.jpg | t2.jpg | 2,359.04 ms |  4.273 ms |  3.997 ms | 14.12 |    0.23 |
|                            |       |        |             |           |           |       |         |
|                    **NetVips** | **t.tif** | **t2.tif** |    **83.10 ms** |  **0.418 ms** |  **0.391 ms** |  **1.00** |    **0.00** |
|                 Magick.NET | t.tif | t2.tif | 2,777.57 ms |  5.643 ms |  4.406 ms | 33.40 |    0.14 |
| System.Drawing<sup>3</sup> | t.tif | t2.tif | 2,048.51 ms | 27.646 ms | 24.508 ms | 24.64 |    0.28 |

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
