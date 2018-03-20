# NetVips benchmarks

The goal of this project is to demonstrate the performance of the netvips
library compared to other image processing libraries on .NET.

Be sure to check out the official benchmarks page: [VIPS - Speed and Memory
Use](https://github.com/jcupitt/libvips/wiki/Speed-and-memory-use)
for complete demonstration of performance and memory usage characteristics
of VIPS library.

## Benchmarks

Run on 20/03/18 with libvips 8.6.3 and ImageMagick 7.0.7.26 Q16 Beta.

``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical cores and 6 physical cores
Frequency=3515632 Hz, Resolution=284.4439 ns, Timer=TSC
.NET Core SDK=2.1.102
  [Host]     : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT
  Job-VTPKYC : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT

Runtime=Core  EnvironmentVariables=OutputDir=.\bin\Release\netcoreapp2.0  

```
|     Method | InputFile | OutputFile |      Mean |     Error |    StdDev | Scaled | ScaledSD |
|----------- |---------- |----------- |----------:|----------:|----------:|-------:|---------:|
|    **NetVips** |     **t.jpg** |     **t2.jpg** |  **31.91 ms** | **0.7726 ms** | **1.1080 ms** |   **1.00** |     **0.00** |
| Magick.NET |     t.jpg |     t2.jpg | 388.80 ms | 2.6901 ms | 2.3847 ms |  12.20 |     0.39 |
|            |           |            |           |           |           |        |          |
|    **NetVips** |     **t.jpg** |     **t2.tif** |  **30.37 ms** | **0.2902 ms** | **0.2266 ms** |   **1.00** |     **0.00** |
| Magick.NET |     t.jpg |     t2.tif | 373.05 ms | 0.9409 ms | 0.7857 ms |  12.28 |     0.09 |
|            |           |            |           |           |           |        |          |
|    **NetVips** |     **t.tif** |     **t2.jpg** |  **26.74 ms** | **0.2969 ms** | **0.2479 ms** |   **1.00** |     **0.00** |
| Magick.NET |     t.tif |     t2.jpg | 399.12 ms | 0.7963 ms | 0.7059 ms |  14.93 |     0.13 |
|            |           |            |           |           |           |        |          |
|    **NetVips** |     **t.tif** |     **t2.tif** |  **24.72 ms** | **0.3736 ms** | **0.3119 ms** |   **1.00** |     **0.00** |
| Magick.NET |     t.tif |     t2.tif | 363.26 ms | 0.9853 ms | 0.9216 ms |  14.70 |     0.18 |

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

dotnet restore

dotnet build -c Release

dotnet run -c Release
```