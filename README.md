# NetVips

[![NuGet](https://img.shields.io/nuget/v/NetVips.svg)](https://www.nuget.org/packages/NetVips)
[![CI status (x64 Linux, macOS and Windows)](https://github.com/kleisauke/net-vips/workflows/CI/badge.svg)](https://github.com/kleisauke/net-vips/actions)
[![CI status (Linux ARM64v8)](https://circleci.com/gh/kleisauke/net-vips.svg?style=shield)](https://circleci.com/gh/kleisauke/net-vips)
[![CI status (NetVips nightly packaging)](https://ci.appveyor.com/api/projects/status/d2r9uanb5yij07pt/branch/master?svg=true)](https://ci.appveyor.com/project/kleisauke/net-vips/branch/master)

This NuGet package provides a .NET binding for the [libvips image processing library](https://www.libvips.org/).

This binding passes the vips test suite cleanly with no leaks on Windows, macOS and Linux.

We have formatted docs online here:

https://kleisauke.github.io/net-vips/

## How it works

Programs that use `NetVips` don't manipulate images directly, instead
they create pipelines of image processing operations building on a source
image. When the end of the pipe is connected to a destination, the whole
pipeline executes at once, streaming the image in parallel from source to
destination a section at a time.

Because `NetVips` is parallel, it's quick, and because it doesn't need to
keep entire images in memory, it's light. For example, the `NetVips` benchmark:

[NetVips.Benchmarks](https://github.com/kleisauke/net-vips/tree/master/tests/NetVips.Benchmarks)

Loads a large image, shrinks by 10%, sharpens, and saves again. On this test
`NetVips` is around 20 times faster than Magick.NET and 3 times faster than ImageSharp.

The [libvips documentation](https://www.libvips.org/API/current/)
has a [chapter explaining how libvips opens files](
https://www.libvips.org/API/current/How-it-opens-files.html)
which gives some more background.

## Supported platforms

- .NET Framework 4.5.2 and higher
- .NET Standard 2.0 and higher
- Mono

## Install

You need the libvips shared library on your library search path, version 8.2 or
later. There are separate NuGet packages that will contain the pre-compiled
libvips binaries for the most common platforms (see
[this repo](https://github.com/kleisauke/libvips-packaging) for details):

|                                     | NuGet Package<sup>1</sup>                                                         |
|-------------------------------------|:---------------------------------------------------------------------------------:|
| **Windows 64-bit**                  | [![NetVips.Native.win-x64-badge]][NetVips.Native.win-x64-nuget]                   |
| **Windows 32-bit**                  | [![NetVips.Native.win-x86-badge]][NetVips.Native.win-x86-nuget]                   |
| **Windows ARM64**                   | [![NetVips.Native.win-arm64-badge]][NetVips.Native.win-arm64-nuget]               |
| **Linux x64 glibc**<sup>2</sup>     | [![NetVips.Native.linux-x64-badge]][NetVips.Native.linux-x64-nuget]               |
| **Linux x64 musl**<sup>3</sup>      | [![NetVips.Native.linux-musl-x64-badge]][NetVips.Native.linux-musl-x64-nuget]     |
| **Linux ARM64v8 glibc**<sup>2</sup> | [![NetVips.Native.linux-arm64-badge]][NetVips.Native.linux-arm64-nuget]           |
| **Linux ARM64v8 musl**<sup>3</sup>  | [![NetVips.Native.linux-musl-arm64-badge]][NetVips.Native.linux-musl-arm64-nuget] |
| **Linux ARMv7**                     | [![NetVips.Native.linux-arm-badge]][NetVips.Native.linux-arm-nuget]               |
| **macOS x64**                       | [![NetVips.Native.osx-x64-badge]][NetVips.Native.osx-x64-nuget]                   |
| **macOS ARM64**<sup>4</sup>         | [![NetVips.Native.osx-arm64-badge]][NetVips.Native.osx-arm64-nuget]               |

[NetVips.Native.win-x64-badge]: https://img.shields.io/nuget/v/NetVips.Native.win-x64.svg
[NetVips.Native.win-x64-nuget]: https://www.nuget.org/packages/NetVips.Native.win-x64
[NetVips.Native.win-x86-badge]: https://img.shields.io/nuget/v/NetVips.Native.win-x86.svg
[NetVips.Native.win-x86-nuget]: https://www.nuget.org/packages/NetVips.Native.win-x86
[NetVips.Native.win-arm64-badge]: https://img.shields.io/nuget/v/NetVips.Native.win-arm64.svg
[NetVips.Native.win-arm64-nuget]: https://www.nuget.org/packages/NetVips.Native.win-arm64
[NetVips.Native.linux-x64-badge]: https://img.shields.io/nuget/v/NetVips.Native.linux-x64.svg
[NetVips.Native.linux-x64-nuget]: https://www.nuget.org/packages/NetVips.Native.linux-x64
[NetVips.Native.linux-musl-x64-badge]: https://img.shields.io/nuget/v/NetVips.Native.linux-musl-x64.svg
[NetVips.Native.linux-musl-x64-nuget]: https://www.nuget.org/packages/NetVips.Native.linux-musl-x64
[NetVips.Native.linux-arm64-badge]: https://img.shields.io/nuget/v/NetVips.Native.linux-arm64.svg
[NetVips.Native.linux-arm64-nuget]: https://www.nuget.org/packages/NetVips.Native.linux-arm64
[NetVips.Native.linux-musl-arm64-badge]: https://img.shields.io/nuget/v/NetVips.Native.linux-musl-arm64.svg
[NetVips.Native.linux-musl-arm64-nuget]: https://www.nuget.org/packages/NetVips.Native.linux-musl-arm64
[NetVips.Native.linux-arm-badge]: https://img.shields.io/nuget/v/NetVips.Native.linux-arm.svg
[NetVips.Native.linux-arm-nuget]: https://www.nuget.org/packages/NetVips.Native.linux-arm
[NetVips.Native.osx-x64-badge]: https://img.shields.io/nuget/v/NetVips.Native.osx-x64.svg
[NetVips.Native.osx-x64-nuget]: https://www.nuget.org/packages/NetVips.Native.osx-x64
[NetVips.Native.osx-arm64-badge]: https://img.shields.io/nuget/v/NetVips.Native.osx-arm64.svg
[NetVips.Native.osx-arm64-nuget]: https://www.nuget.org/packages/NetVips.Native.osx-arm64

<sup>1</sup> The version number of these NuGet packages is in sync with libvips' version number.  
<sup>2</sup> Uses glibc as the standard C library (Ubuntu, Debian, etc).  
<sup>3</sup> Uses musl as the standard C library (Alpine, Gentoo Linux, etc).  
<sup>4</sup> Requires .NET 6.0 or higher.

Then just install this package, perhaps:

    Install-Package NetVips

To test your install, try this test program:

```csharp
if (ModuleInitializer.VipsInitialized)
{
    Console.WriteLine($"Inited libvips {NetVips.Version(0)}.{NetVips.Version(1)}.{NetVips.Version(2)}");
}
else
{
    Console.WriteLine(ModuleInitializer.Exception.Message);
}
Console.ReadLine();
```

If NetVips was able to find the libvips shared library, you should see:

    Inited libvips [VERSION_NUMBER]

However, if you see something else, NetVips was unable to initialize libvips.
This can happen for a variety of reasons, even though most of the times it's because NetVips
was not able to find libvips or due to x86/x64 architecture problems:

| Inner exception | HRESULT | Solution |
| :--- | :--- | :--- |
| DllNotFoundException | 0x8007007E | Make sure to add the `bin` folder of the libvips Windows build to your `PATH` environment variable (if you wish to not use the separate NuGet packages). |
| BadImageFormatException | 0x8007000B | Make sure when you target the `AnyCPU` platform the `Prefer 32-bit` option is unchecked. Or try to target `x64` instead. |

## Example

```csharp
using NetVips;

using var im = Image.NewFromFile("image.jpg");

// put im at position (100, 100) in a 3000 x 3000 pixel image,
// make the other pixels in the image by mirroring im up / down /
// left / right, see
// https://www.libvips.org/API/current/libvips-conversion.html#vips-embed
using var embed = im.Embed(100, 100, 3000, 3000, extend: Enums.Extend.Mirror);

// multiply the green (middle) band by 2, leave the other two alone
using var multiply = embed * new[] { 1, 2, 1 };

// make an image from an array constant, convolve with it
using var mask = Image.NewFromArray(new[,]
{
    {-1, -1, -1},
    {-1, 16, -1},
    {-1, -1, -1}
}, 8);
using var convolve = multiply.Conv(mask, precision: Enums.Precision.Integer);

// finally, write the result back to a file on disk
convolve.WriteToFile("output.jpg");
```
