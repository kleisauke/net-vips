# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/libvips/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 05/06/19 with libvips 8.8.0-rc3, Magick.NET 7.13.1, ImageSharp 1.0.0-dev002724, SkiaSharp 1.68.0 and System.Drawing.Common 4.5.1.

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET Core SDK=2.2.300
  [Host]     : .NET Core 2.2.5 (CoreCLR 4.6.27617.05, CoreFX 4.6.27618.01), 64bit RyuJIT
  Job-XOHXLS : .NET Core 2.2.5 (CoreCLR 4.6.27617.05, CoreFX 4.6.27618.01), 64bit RyuJIT

Toolchain=.NET Core 2.2.0  

```
|         Method | input | output |      Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------- |------ |------- |----------:|----------:|----------:|------:|--------:|
|        **NetVips** | **t.jpg** | **t2.jpg** |  **28.94 ms** | **0.2865 ms** | **0.2392 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.jpg | t2.jpg | 303.95 ms | 0.4404 ms | 0.3904 ms | 10.50 |    0.09 |
|     ImageSharp¹ | t.jpg | t2.jpg | 157.19 ms | 2.1586 ms | 2.0191 ms |  5.43 |    0.09 |
|      SkiaSharp¹ | t.jpg | t2.jpg | 235.71 ms | 0.4436 ms | 0.4150 ms |  8.15 |    0.07 |
| System.Drawing² | t.jpg | t2.jpg | 262.26 ms | 0.2574 ms | 0.2407 ms |  9.06 |    0.08 |
|                |       |        |           |           |           |       |         |
|        **NetVips** | **t.tif** | **t2.tif** |  **19.66 ms** | **0.3163 ms** | **0.2959 ms** |  **1.00** |    **0.00** |
|     Magick.NET | t.tif | t2.tif | 288.56 ms | 1.4766 ms | 1.3812 ms | 14.68 |    0.24 |
| System.Drawing² | t.tif | t2.tif | 275.45 ms | 0.9736 ms | 0.9107 ms | 14.01 |    0.23 |

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