# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 18/11/23 with libvips 8.15.0, Magick.NET 13.4.0, ImageSharp 3.0.2, SkiaSharp 2.88.6 and System.Drawing.Common 8.0.0.

```

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22631.2715/23H2/2023Update/SunValley3)
AMD Ryzen 9 7900, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]                   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 CLI (NativeAOT) : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0 CLI (NativeAOT)  Runtime=NativeAOT 8.0  Toolchain=.NET 8.0  

```
| Method                     | input | output | Mean        | Error    | StdDev   | Ratio | RatioSD |
|--------------------------- |------ |------- |------------:|---------:|---------:|------:|--------:|
| **NetVips**                    | **t.jpg** | **t2.jpg** |   **105.43 ms** | **2.054 ms** | **2.522 ms** |  **1.00** |    **0.00** |
| Magick.NET                 | t.jpg | t2.jpg | 1,741.20 ms | 6.126 ms | 5.116 ms | 16.44 |    0.46 |
| ImageSharp                 | t.jpg | t2.jpg |   336.34 ms | 4.142 ms | 3.672 ms |  3.17 |    0.08 |
| SkiaSharp<sup>1</sup>      | t.jpg | t2.jpg |   895.18 ms | 5.438 ms | 4.541 ms |  8.45 |    0.24 |
| System.Drawing<sup>2</sup> | t.jpg | t2.jpg | 1,435.57 ms | 2.944 ms | 2.754 ms | 13.55 |    0.37 |
|                            |       |        |             |          |          |       |         |
| **NetVips**                    | **t.tif** | **t2.tif** |    **65.26 ms** | **1.258 ms** | **1.235 ms** |  **1.00** |    **0.00** |
| Magick.NET                 | t.tif | t2.tif | 1,647.24 ms | 4.682 ms | 4.380 ms | 25.27 |    0.49 |
| ImageSharp                 | t.tif | t2.tif |   250.84 ms | 2.152 ms | 2.013 ms |  3.85 |    0.07 |
| System.Drawing<sup>2</sup> | t.tif | t2.tif | 1,212.72 ms | 4.588 ms | 4.292 ms | 18.61 |    0.36 |

<sup>1</sup> SkiaSharp does not have TIFF support, so I only tested with JPEG files.  
<sup>2</sup> System.Drawing does not have a sharpening or convolution operation, so I skipped that part of the benchmark.

## Performance test design

The project contains a `Benchmark.cs` file with specific scripts
using various libraries available on .NET.

Each script is coded to execute the same scenario (see Scenario section).

See "Do it yourself" section for how to run benchmark scenario.

## Scenario

Test scenario was taken from [Speed and Memory use](
https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
page from libvips [Home page](https://www.libvips.org/).

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
