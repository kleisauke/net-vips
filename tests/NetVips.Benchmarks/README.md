# NetVips benchmarks

The goal of this project is to demonstrate the performance of the netvips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/jcupitt/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 23/03/18 with libvips 8.6.3, ImageMagick 7.0.7.26 Q8 Beta and ImageSharp 1.0.0-beta0002.

``` ini

BenchmarkDotNet=v0.10.13.477-nightly, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical cores and 6 physical cores
Frequency=3515626 Hz, Resolution=284.4444 ns, Timer=TSC
.NET Core SDK=2.1.103
  [Host]     : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT
  Job-DIBRAB : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT

Toolchain=.NET Core 2.0.6  

```
|     Method | input | output |      Mean |     Error |    StdDev | Scaled | ScaledSD |
|----------- |------ |------- |----------:|----------:|----------:|-------:|---------:|
|    **NetVips** | **t.jpg** | **t2.jpg** |  **31.51 ms** | **0.1891 ms** | **0.1676 ms** |   **1.00** |     **0.00** |
| Magick.NET | t.jpg | t2.jpg | 369.88 ms | 1.1740 ms | 0.9803 ms |  11.74 |     0.07 |
| ImageSharp¹ | t.jpg | t2.jpg | 163.27 ms | 0.6703 ms | 0.6270 ms |   5.18 |     0.03 |
|            |       |        |           |           |           |        |          |
|    **NetVips** | **t.tif** | **t2.tif** |  **22.78 ms** | **0.1273 ms** | **0.1063 ms** |   **1.00** |     **0.00** |
| Magick.NET | t.tif | t2.tif | 347.48 ms | 0.9129 ms | 0.8539 ms |  15.25 |     0.08 |

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