# NetVips benchmarks

The goal of this project is to demonstrate the performance of the NetVips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/jcupitt/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 08/04/18 with libvips 8.6.3, ImageMagick 7.0.7.26 Q8 Beta and ImageSharp 1.0.0-beta0003.

``` ini

BenchmarkDotNet=v0.10.13.507-nightly, OS=Windows 10.0.16299.334 (1709/FallCreatorsUpdate/Redstone3)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
Frequency=3515622 Hz, Resolution=284.4447 ns, Timer=TSC
.NET Core SDK=2.1.104
  [Host]     : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT
  Job-LNGDRA : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT

Toolchain=.NET Core 2.0.6

```
|     Method | input | output |      Mean |     Error |    StdDev | Scaled | ScaledSD |
|----------- |------ |------- |----------:|----------:|----------:|-------:|---------:|
|    **NetVips** | **t.jpg** | **t2.jpg** |  **53.10 ms** | **0.5003 ms** | **0.4178 ms** |   **1.00** |     **0.00** |
| Magick.NET | t.jpg | t2.jpg | 387.44 ms | 0.3698 ms | 0.3278 ms |   7.30 |     0.06 |
| ImageSharp¹ | t.jpg | t2.jpg | 218.09 ms | 1.0380 ms | 0.9709 ms |   4.11 |     0.04 |
|            |       |        |           |           |           |        |          |
|    **NetVips** | **t.tif** | **t2.tif** |  **40.48 ms** | **0.7673 ms** | **0.7178 ms** |   **1.00** |     **0.00** |
| Magick.NET | t.tif | t2.tif | 366.94 ms | 1.5753 ms | 1.4735 ms |   9.07 |     0.16 |

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