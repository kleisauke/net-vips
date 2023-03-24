# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 24/03/23 with libvips 8.14.2, Magick.NET 13.0.0, ImageSharp 3.0.0, SkiaSharp 2.88.3 and System.Drawing.Common 7.0.0.

``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1413/22H2/2022Update/SunValley2)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.202
  [Host]                   : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  .NET 7.0 CLI (NativeAOT) : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2

Job=.NET 7.0 CLI (NativeAOT)  Runtime=NativeAOT 7.0  Toolchain=.NET 7.0  

```
|                     Method | input | output |       Mean |   Error |  StdDev | Ratio | RatioSD |
|--------------------------- |------ |------- |-----------:|--------:|--------:|------:|--------:|
|                    **NetVips** | **t.jpg** | **t2.jpg** |   **175.8 ms** | **1.33 ms** | **1.18 ms** |  **1.00** |    **0.00** |
|                 Magick.NET | t.jpg | t2.jpg | 3,582.5 ms | 5.45 ms | 4.83 ms | 20.38 |    0.15 |
|                 ImageSharp | t.jpg | t2.jpg |   508.3 ms | 2.54 ms | 2.25 ms |  2.89 |    0.02 |
|      SkiaSharp<sup>1</sup> | t.jpg | t2.jpg | 1,991.4 ms | 7.27 ms | 6.07 ms | 11.34 |    0.06 |
| System.Drawing<sup>2</sup> | t.jpg | t2.jpg | 2,407.3 ms | 5.26 ms | 4.66 ms | 13.69 |    0.09 |
|                            |       |        |            |         |         |       |         |
|                    **NetVips** | **t.tif** | **t2.tif** |   **131.7 ms** | **0.96 ms** | **0.80 ms** |  **1.00** |    **0.00** |
|                 Magick.NET | t.tif | t2.tif | 3,443.5 ms | 5.82 ms | 5.44 ms | 26.14 |    0.18 |
|                 ImageSharp | t.tif | t2.tif |   382.6 ms | 5.25 ms | 4.91 ms |  2.90 |    0.04 |
| System.Drawing<sup>2</sup> | t.tif | t2.tif | 2,063.0 ms | 4.03 ms | 3.77 ms | 15.66 |    0.09 |

<sup>1</sup> SkiaSharp does not have TIFF support, so I only tested with JPEG files.  
<sup>2</sup> System.Drawing does not have a sharpening or convolution operation, so I skipped that part of the benchmark.

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
