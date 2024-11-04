# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 04/11/24 with libvips 8.16.0, Magick.NET 14.0.0, ImageSharp 3.1.5, SkiaSharp 2.88.8 and System.Drawing.Common 8.0.10.

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
AMD Ryzen 9 7900, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.403
  [Host]                   : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  .NET 8.0 CLI (NativeAOT) : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=.NET 8.0 CLI (NativeAOT)  Runtime=NativeAOT 8.0  Toolchain=.NET 8.0  

```
| Method                     | input | output | Mean        | Error     | StdDev    | Ratio | RatioSD |
|--------------------------- |------ |------- |------------:|----------:|----------:|------:|--------:|
| **NetVips**                    | **t.jpg** | **t2.jpg** |    **92.70 ms** |  **1.020 ms** |  **0.904 ms** |  **1.00** |    **0.01** |
| Magick.NET                 | t.jpg | t2.jpg | 1,634.68 ms |  6.062 ms |  5.062 ms | 17.64 |    0.17 |
| ImageSharp                 | t.jpg | t2.jpg |   352.31 ms |  5.753 ms |  5.650 ms |  3.80 |    0.07 |
| SkiaSharp<sup>1</sup>      | t.jpg | t2.jpg |   890.21 ms |  2.238 ms |  2.094 ms |  9.60 |    0.09 |
| System.Drawing<sup>2</sup> | t.jpg | t2.jpg | 3,220.71 ms |  9.438 ms |  8.367 ms | 34.75 |    0.34 |
|                            |       |        |             |           |           |       |         |
| **NetVips**                    | **t.tif** | **t2.tif** |    **54.46 ms** |  **1.000 ms** |  **1.228 ms** |  **1.00** |    **0.03** |
| Magick.NET                 | t.tif | t2.tif | 1,524.11 ms | 15.602 ms | 14.594 ms | 28.00 |    0.67 |
| ImageSharp                 | t.tif | t2.tif |   248.38 ms |  1.802 ms |  1.505 ms |  4.56 |    0.10 |
| System.Drawing<sup>2</sup> | t.tif | t2.tif | 3,000.09 ms | 13.126 ms | 10.961 ms | 55.12 |    1.23 |

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
