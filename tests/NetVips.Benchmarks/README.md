# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 01/01/26 with libvips 8.18.0, Magick.NET 14.10.1, ImageSharp 3.1.12, SkiaSharp 3.119.1 and System.Drawing.Common 10.0.1.

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7462/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 7900 3.70GHz, 1 CPU, 24 logical and 12 physical cores
.NET SDK 10.0.101
  [Host]                    : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v4
  .NET 10.0 CLI (NativeAOT) : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v4

Job=.NET 10.0 CLI (NativeAOT)  Runtime=NativeAOT 10.0  Toolchain=.NET 10.0  

```
| Method             | input | output | Mean        | Error     | StdDev    | Ratio | RatioSD |
|------------------- |------ |------- |------------:|----------:|----------:|------:|--------:|
| **NetVips**            | **t.jpg** | **t2.jpg** |    **74.47 ms** |  **0.695 ms** |  **0.650 ms** |  **1.00** |    **0.01** |
| Magick.NET         | t.jpg | t2.jpg | 1,626.85 ms |  8.041 ms |  7.521 ms | 21.85 |    0.21 |
| ImageSharp         | t.jpg | t2.jpg |   366.95 ms |  7.093 ms |  6.966 ms |  4.93 |    0.10 |
| SkiaSharp[^1]      | t.jpg | t2.jpg | 5,362.23 ms | 78.791 ms | 73.701 ms | 72.01 |    1.14 |
| System.Drawing[^2] | t.jpg | t2.jpg | 1,185.58 ms |  4.523 ms |  4.231 ms | 15.92 |    0.15 |
|                    |       |        |             |           |           |       |         |
| **NetVips**            | **t.tif** | **t2.tif** |    **49.80 ms** |  **0.293 ms** |  **0.260 ms** |  **1.00** |    **0.01** |
| Magick.NET         | t.tif | t2.tif | 1,533.49 ms | 29.978 ms | 23.405 ms | 30.79 |    0.48 |
| ImageSharp         | t.tif | t2.tif |   186.51 ms |  3.666 ms |  6.324 ms |  3.75 |    0.13 |
| System.Drawing[^2] | t.tif | t2.tif |   985.05 ms | 16.399 ms | 13.694 ms | 19.78 |    0.28 |

<!-- Note: when updating the benchmarks above, replace curly braces with square brackets to ensure the footnotes work correctly. -->

[^1]: SkiaSharp does not have TIFF support, so I only tested with JPEG files.
[^2]: System.Drawing does not have a sharpening or convolution operation, so I skipped that part of the benchmark.

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
