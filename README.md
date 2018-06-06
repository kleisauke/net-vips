# Mono/.NET bindings for libvips

[![NuGet](https://img.shields.io/nuget/v/NetVips.svg)](https://www.nuget.org/packages/NetVips)
[![Build Status](https://travis-ci.org/kleisauke/net-vips.svg?branch=master)](https://travis-ci.org/kleisauke/net-vips)
[![Build status](https://ci.appveyor.com/api/projects/status/d2r9uanb5yij07pt/branch/master?svg=true)](https://ci.appveyor.com/project/kleisauke/net-vips/branch/master)

This NuGet package provides a Mono/.NET binding for the [libvips image processing library](https://jcupitt.github.io/libvips).

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

Loads a large tiff image, shrinks by 10%, sharpens, and saves again. On this
test `NetVips` is around 8 times faster than Magick.NET and 4 times faster
than ImageSharp.

There's a handy blog post explaining how libvips opens files, which gives
some more background.

http://libvips.blogspot.co.uk/2012/06/how-libvips-opens-file.html

## Install

You need the libvips shared library on your library search path, version 8.2 or
later. On Linux and macOS, you can install via your package manager; on 
Windows the pre-compiled binary is bundled with NuGet.

Then just install this package, perhaps:

    Install-Package NetVips

To test your install, try this test program:

```csharp
Console.WriteLine(ModuleInitializer.VipsInitialized
? $"Inited libvips {Base.Version(0)}.{Base.Version(1)}.{Base.Version(2)}"
: "Unable to init libvips");
Console.ReadLine();
```

If NetVips was able to find the libvips shared library, you should see:

    Inited libvips [VERSION_NUMBER]

If NetVips was not able to find libvips you might see:

    Unable to init libvips

## Bundled libvips Windows binary

From NetVips version 1.0.3 upwards the pre-compiled libvips Windows binary is
bundled with NuGet. It's therefore no longer necessary to download the
pre-compiled binary and to set the `PATH` environment variable.

If you wish to not use the bundled libvips, you could set the
`UseGlobalLibvips` property to `true`:
```xml
<PropertyGroup>
  <UseGlobalLibvips>true</UseGlobalLibvips>
</PropertyGroup>
```

```bash
dotnet build /p:UseGlobalLibvips=true
```

This property prevents that the bundled libvips binary and its
dependencies will be copied to your project's output directory.

## Example

```csharp
using NetVips;

var im = Image.NewFromFile("image.jpg");

// put im at position (100, 100) in a 3000 x 3000 pixel image, 
// make the other pixels in the image by mirroring im up / down / 
// left / right, see
// https://jcupitt.github.io/libvips/API/current/libvips-conversion.html#vips-embed
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
