# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/jcupitt/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 23/04/18 with libvips 8.6.3, ImageMagick 7.0.7.29 Q8 Beta and ImageSharp 1.0.0-beta0003.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.16299.371 (1709/FallCreatorsUpdate/Redstone3)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
Frequency=3515626 Hz, Resolution=284.4444 ns, Timer=TSC
.NET Core SDK=2.1.104
  [Host]     : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT
  Job-GMSODR : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT

Toolchain=.NET Core 2.0.6

```
|     Method | input | output |      Mean |     Error |    StdDev | Scaled | ScaledSD |
|----------- |------ |------- |----------:|----------:|----------:|-------:|---------:|
|    **NetVips** | **t.jpg** | **t2.jpg** |  **52.02 ms** | **0.4218 ms** | **0.3945 ms** |   **1.00** |     **0.00** |
| Magick.NET | t.jpg | t2.jpg | 366.68 ms | 2.9389 ms | 2.7490 ms |   7.05 |     0.07 |
| ImageSharp¹ | t.jpg | t2.jpg | 215.86 ms | 0.5902 ms | 0.5521 ms |   4.15 |     0.03 |
|            |       |        |           |           |           |        |          |
|    **NetVips** | **t.tif** | **t2.tif** |  **38.24 ms** | **0.7550 ms** | **1.3613 ms** |   **1.00** |     **0.00** |
| Magick.NET | t.tif | t2.tif | 350.72 ms | 1.0981 ms | 0.9169 ms |   9.18 |     0.32 |

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