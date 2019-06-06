# NetVips

[![NuGet](https://img.shields.io/nuget/v/NetVips.svg)](https://www.nuget.org/packages/NetVips)
[![Build Status](https://travis-ci.org/kleisauke/net-vips.svg?branch=master)](https://travis-ci.org/kleisauke/net-vips)
[![Build status](https://ci.appveyor.com/api/projects/status/d2r9uanb5yij07pt/branch/master?svg=true)](https://ci.appveyor.com/project/kleisauke/net-vips/branch/master)

This NuGet package provides a .NET binding for the [libvips image processing library](https://libvips.github.io/libvips).

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
`NetVips` is around 12 times faster than Magick.NET and 5 times faster than ImageSharp.

The [libvips documentation](https://libvips.github.io/libvips/API/current)
has a [chapter explaining how libvips opens
files](https://libvips.github.io/libvips/API/current/How-it-opens-files.md.html)
which gives some more background.

## Supported platforms

- .NET Framework (4.5 and higher)
- .NET Core (.NETStandard 2.0 and higher on Windows, Linux and macOS)
- Mono

## Install

You need the libvips shared library on your library search path, version 8.2 or
later. There are separate NuGet packages that will contain the pre-compiled 
libvips binaries for a few distros (see
[this repo](https://github.com/kleisauke/libvips-packaging) for details):

|                    |NuGet Package¹|
|--------------------|:------------:|
|**Windows 64-bit**  |[![NetVips.Native.win-x64](https://img.shields.io/nuget/v/NetVips.Native.win-x64.svg)](https://www.nuget.org/packages/NetVips.Native.win-x64)|
|**Windows 32-bit**  |[![NetVips.Native.win-x64](https://img.shields.io/nuget/v/NetVips.Native.win-x86.svg)](https://www.nuget.org/packages/NetVips.Native.win-x86)|
|**Linux x64 glibc²**|[![NetVips.Native.linux-x64](https://img.shields.io/nuget/v/NetVips.Native.linux-x64.svg)](https://www.nuget.org/packages/NetVips.Native.linux-x64)|
|**Linux x64 musl³** |[![NetVips.Native.linux-musl-x64](https://img.shields.io/nuget/v/NetVips.Native.linux-musl-x64.svg)](https://www.nuget.org/packages/NetVips.Native.linux-musl-x64)|
|**macOS x64**       |[![NetVips.Native.osx-x64](https://img.shields.io/nuget/v/NetVips.Native.osx-x64.svg)](https://www.nuget.org/packages/NetVips.Native.osx-x64)|

¹ The version number of these NuGet packages is in sync with libvips' version number.  
² Uses glibc as the standard C library (Ubuntu, Debian, etc).  
³ Uses musl as the standard C library (Alpine, Gentoo Linux, etc).

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

var im = Image.NewFromFile("image.jpg");

// put im at position (100, 100) in a 3000 x 3000 pixel image, 
// make the other pixels in the image by mirroring im up / down / 
// left / right, see
// https://libvips.github.io/libvips/API/current/libvips-conversion.html#vips-embed
im = im.Embed(100, 100, 3000, 3000, extend: Enums.Extend.Mirror);

// multiply the green (middle) band by 2, leave the other two alone
im *= new[] {1, 2, 1};

// make an image from an array constant, convolve with it
var mask = Image.NewFromArray(new[,]
{
    {-1, -1, -1},
    {-1, 16, -1},
    {-1, -1, -1}
}, 8);
im = im.Conv(mask, precision: Enums.Precision.Integer);

// finally, write the result back to a file on disk
im.WriteToFile("output.jpg");
```
