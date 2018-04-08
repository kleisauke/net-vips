# Mono/.NET bindings for libvips

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
