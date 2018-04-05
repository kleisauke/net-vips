# NetVips benchmarks

The goal of this project is to demonstrate the performance of the netvips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/jcupitt/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 05/04/18 with libvips 8.6.3, ImageMagick 7.0.7.26 Q8 Beta and ImageSharp 1.0.0-beta0003.

``` ini

BenchmarkDotNet=v0.10.13.498-nightly, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.334)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical cores and 6 physical cores
Frequency=3515624 Hz, Resolution=284.4445 ns, Timer=TSC
.NET Core SDK=2.1.104
  [Host]     : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT
  Job-CKWMCF : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT

Toolchain=.NET Core 2.0.6  

```
|     Method | input | output |      Mean |     Error |    StdDev | Scaled | ScaledSD |
|----------- |------ |------- |----------:|----------:|----------:|-------:|---------:|
|    **NetVips** | **t.jpg** | **t2.jpg** |  **32.89 ms** | **0.4014 ms** | **0.3755 ms** |   **1.00** |     **0.00** |
| Magick.NET | t.jpg | t2.jpg | 382.82 ms | 0.4148 ms | 0.3880 ms |  11.64 |     0.13 |
| ImageSharp¹ | t.jpg | t2.jpg | 217.15 ms | 1.3181 ms | 1.2329 ms |   6.60 |     0.08 |
|            |       |        |           |           |           |        |          |
|    **NetVips** | **t.tif** | **t2.tif** |  **24.19 ms** | **0.1022 ms** | **0.0798 ms** |   **1.00** |     **0.00** |
| Magick.NET | t.tif | t2.tif | 356.33 ms | 1.0060 ms | 0.8918 ms |  14.73 |     0.06 |

¹ ImageSharp does not have TIFF support, so I only tested with JPEG files.

## Performance test design

The project contains a `Benchmark.cs` file with specific scripts 
using various libraries available on .NET.

Each script is coded to execute the same scenario (see Scenario section).

See "Do it yourself" section for how to run benchmark scenario.

## Scenario

Test scenario was taken from [Speed and Memory
use](https://github.com/jcupitt/libvips/wiki/Speed-and-memory-use)
page from libvips [Home
page](https://jcupitt.github.io/libvips/).

## Do it yourself

```bash
git clone https://github.com/kleisauke/net-vips

cd net-vips/tests/NetVips.Benchmarks

dotnet run -c Release
```