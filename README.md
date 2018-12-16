# Mono/.NET bindings for libvips

[![NuGet](https://img.shields.io/nuget/v/NetVips.svg)](https://www.nuget.org/packages/NetVips)
[![Build Status](https://travis-ci.org/kleisauke/net-vips.svg?branch=master)](https://travis-ci.org/kleisauke/net-vips)
[![Build status](https://ci.appveyor.com/api/projects/status/d2r9uanb5yij07pt/branch/master?svg=true)](https://ci.appveyor.com/project/kleisauke/net-vips/branch/master)

This NuGet package provides a Mono/.NET binding for the [libvips image processing library](https://libvips.github.io/libvips).

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
if (ModuleInitializer.VipsInitialized)
{
    Console.WriteLine($"Inited libvips {Base.Version(0)}.{Base.Version(1)}.{Base.Version(2)}");
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
| DllNotFoundException | 0x8007007E | Make sure to add the `bin` folder of the libvips Windows build to your `PATH` environment variable (if you wish to not use the bundled libvips binaries). |
| BadImageFormatException | 0x8007000B | Make sure when you target the `AnyCPU` platform the `Prefer 32-bit` option is unchecked. Or try to target `x64` instead. |

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

This property ensures that the bundled libvips binaries are not copied
to your project's output directory. Instead, it will search for the
required binaries in the directories that are specified in the `PATH` 
environment variable.

The libvips web-distribution bundled with NetVips contains 37 DLLs.
If you want to not bloat your project's output directory, you could 
set the `LibvipsOutputBase` property:
```xml
<PropertyGroup>
  <LibvipsOutputBase>vips</LibvipsOutputBase>
</PropertyGroup>
```

This property ensures that bundled libvips binaries are copied to
to the specified subdirectory within your project's output directory.
Note that it's still required to add this directory to the `PATH` 
environment variable. See [here](https://github.com/kleisauke/net-vips/issues/20#issuecomment-439394316)
for an example how this can be done at runtime.

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
