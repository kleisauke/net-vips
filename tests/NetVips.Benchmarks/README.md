# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 04/05/19 with libvips 8.8.0-rc1, Magick.NET 7.12.0.0, ImageSharp 1.0.0-beta0006, SkiaSharp 1.68.0 and System.Drawing.Common 4.5.1.

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17763.437 (1809/October2018Update/Redstone5)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=2.2.203
  [Host]     : .NET Core 2.2.4 (CoreCLR 4.6.27521.02, CoreFX 4.6.27521.01), 64bit RyuJIT
  Job-EALIGQ : .NET Core 2.2.4 (CoreCLR 4.6.27521.02, CoreFX 4.6.27521.01), 64bit RyuJIT

Toolchain=.NET Core 2.2.0  

```
|         Method | input | output |      Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|----------:|----------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **31.09 ms** | **0.3412 ms** | **0.3025 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 372.31 ms | 0.5869 ms | 0.5202 ms | 11.97 |    0.11 |
|     ImageSharp¹ | t.jpg | t2.jpg | 178.19 ms | 1.0550 ms | 0.9869 ms |  5.74 |    0.06 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 593.18 ms | 1.6626 ms | 1.5552 ms | 19.08 |    0.21 |
| System.Drawing² | t.jpg | t2.jpg | 238.84 ms | 0.4684 ms | 0.4381 ms |  7.68 |    0.07 |
|                |       |        |           |           |           |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **21.28 ms** | **0.3483 ms** | **0.3258 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 354.27 ms | 0.3246 ms | 0.3036 ms | 16.65 |    0.26 |
| System.Drawing² | t.tif | t2.tif | 230.39 ms | 1.3283 ms | 1.1775 ms | 10.80 |    0.14 |

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

dotnet build /p:BenchmarkWithNuGetBinaries=true
dotnet run -c Release
```