# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 13/06/25 with libvips 8.17.0, Magick.NET 14.6.0, ImageSharp 3.1.10, SkiaSharp 3.119.0 and System.Drawing.Common 9.0.6.

```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
AMD Ryzen 9 7900 3.70GHz, 1 CPU, 24 logical and 12 physical cores
.NET SDK 9.0.301
  [Host]                   : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  .NET 9.0 CLI (NativeAOT) : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=.NET 9.0 CLI (NativeAOT)  Runtime=NativeAOT 9.0  Toolchain=.NET 9.0  

```
| Method                     | input | output | Mean        | Error      | StdDev     | Ratio | RatioSD |
|--------------------------- |------ |------- |------------:|-----------:|-----------:|------:|--------:|
| **NetVips**                    | **t.jpg** | **t2.jpg** |    **75.60 ms** |   **1.493 ms** |   **2.235 ms** |  **1.00** |    **0.04** |
| Magick.NET                 | t.jpg | t2.jpg | 1,713.72 ms |   6.188 ms |   5.167 ms | 22.69 |    0.64 |
| ImageSharp                 | t.jpg | t2.jpg |   450.68 ms |   8.907 ms |  22.992 ms |  5.97 |    0.35 |
| SkiaSharp<sup>1</sup>      | t.jpg | t2.jpg | 5,935.11 ms | 115.547 ms | 102.429 ms | 78.57 |    2.57 |
| System.Drawing<sup>2</sup> | t.jpg | t2.jpg | 1,521.53 ms |   5.685 ms |   5.040 ms | 20.14 |    0.57 |
|                            |       |        |             |            |            |       |         |
| **NetVips**                    | **t.tif** | **t2.tif** |    **39.39 ms** |   **0.443 ms** |   **0.393 ms** |  **1.00** |    **0.01** |
| Magick.NET                 | t.tif | t2.tif | 1,606.64 ms |   7.674 ms |   7.179 ms | 40.79 |    0.43 |
| ImageSharp                 | t.tif | t2.tif |   263.84 ms |   5.094 ms |   6.256 ms |  6.70 |    0.17 |
| System.Drawing<sup>2</sup> | t.tif | t2.tif | 1,361.53 ms |  22.712 ms |  20.133 ms | 34.57 |    0.60 |

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
