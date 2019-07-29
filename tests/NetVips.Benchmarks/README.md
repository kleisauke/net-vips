# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 29/07/19 with libvips 8.8.1, Magick.NET 7.14.1, ImageSharp 1.0.0-dev002762, SkiaSharp 1.68.0 and System.Drawing.Common 4.5.1.

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=2.2.401
  [Host]     : .NET Core 2.2.6 (CoreCLR 4.6.27817.03, CoreFX 4.6.27818.02), 64bit RyuJIT
  Job-HVKOBT : .NET Core 2.2.6 (CoreCLR 4.6.27817.03, CoreFX 4.6.27818.02), 64bit RyuJIT

Toolchain=.NET Core 2.2.0  

```
|         Method | input | output |      Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|----------:|----------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **28.95 ms** | **0.2038 ms** | **0.1906 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 309.09 ms | 0.3860 ms | 0.3610 ms | 10.68 |    0.07 |
|     ImageSharp¹ | t.jpg | t2.jpg | 158.57 ms | 2.3961 ms | 2.2413 ms |  5.48 |    0.09 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 236.11 ms | 0.0936 ms | 0.0875 ms |  8.16 |    0.05 |
| System.Drawing² | t.jpg | t2.jpg | 264.08 ms | 1.0843 ms | 1.0143 ms |  9.12 |    0.07 |
|                |       |        |           |           |           |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **19.57 ms** | **0.2321 ms** | **0.2171 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 292.67 ms | 0.7910 ms | 0.6605 ms | 14.98 |    0.15 |
| System.Drawing² | t.tif | t2.tif | 275.03 ms | 1.1997 ms | 1.1222 ms | 14.05 |    0.17 |

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

dotnet run -c Release
```