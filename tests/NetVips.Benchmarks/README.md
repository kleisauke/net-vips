# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 30/03/21 with libvips 8.10.6, Magick.NET 7.23.3, ImageSharp 1.0.3, SkiaSharp 2.80.2 and System.Drawing.Common 5.0.2.

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=5.0.201
  [Host]       : .NET Core 5.0.4 (CoreCLR 5.0.421.11614, CoreFX 5.0.421.11614), X64 RyuJIT
  .Net 5.0 CLI : .NET Core 5.0.4 (CoreCLR 5.0.421.11614, CoreFX 5.0.421.11614), X64 RyuJIT

Job=.Net 5.0 CLI  Arguments=/p:DebugType=portable  Toolchain=.NET Core 5.0  

```
|         Method | input | output |      Mean |    Error |   StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|---------:|---------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **28.72 ms** | **0.143 ms** | **0.119 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 394.74 ms | 0.805 ms | 0.714 ms | 13.75 |    0.06 |
|     ImageSharp¹ | t.jpg | t2.jpg |  83.00 ms | 0.763 ms | 0.714 ms |  2.89 |    0.03 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 220.23 ms | 0.326 ms | 0.272 ms |  7.67 |    0.03 |
| System.Drawing² | t.jpg | t2.jpg | 281.00 ms | 0.948 ms | 0.887 ms |  9.79 |    0.05 |
|                |       |        |           |          |          |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **19.51 ms** | **0.191 ms** | **0.159 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 374.30 ms | 1.437 ms | 1.344 ms | 19.20 |    0.19 |
| System.Drawing² | t.tif | t2.tif | 266.86 ms | 1.494 ms | 1.324 ms | 13.68 |    0.13 |

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

# Benchmark with NuGet binaries
dotnet run -c Release

# Benchmark with a globally installed libvips
dotnet build -c Release /p:UseGlobalLibvips=true
dotnet run --no-build -c Release
```
