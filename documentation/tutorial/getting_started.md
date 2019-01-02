Getting Started with NetVips
============================

See the main libvips site for an introduction to the underlying library. These
notes introduce the Mono/.NET binding.

https://libvips.github.io/libvips 

## Example

This example loads a file, boosts the green channel, sharpens the image,
and saves it back to disc again:

```csharp
using NetVips;

var image = Image.NewFromFile("some-image.jpg", access: Enums.Access.Sequential);

image *= new[] {1, 2, 1};

var mask = Image.NewFromArray(new[,]
{
    {-1, -1, -1},
    {-1, 16, -1},
    {-1, -1, -1}
}, scale: 8);
image = image.Conv(mask, precision: Enums.Precision.Integer);

image.WriteToFile("x.jpg");
```

Reading this example line by line, we have:

```csharp
var image = Image.NewFromFile("some-image.jpg", access: Enums.Access.Sequential);
```

[`NewFromFile`](xref:NetVips.Image.NewFromFile*) can load any image file supported by libvips. In
this example, we will be accessing pixels top-to-bottom as we sweep through
the image reading and writing, so `sequential` access mode is best for us.

The default mode is `random` which allows for full random access to image
pixels, but is slower and needs more memory. See [`Enums.Access`](xref:NetVips.Enums.Access)
for full details on the various modes available.

You can also load formatted images from memory, create images that
wrap C-style memory arrays held as an byte array, or make images from 
constants.

The next line:

```csharp
image *= new[] {1, 2, 1};
```

Multiplying the image by an array constant uses one array element for each
image band. This line assumes that the input image has three bands and will
double the middle band. For RGB images, that's doubling green.

There are the usual range of arithmetic operator overloads.

Next we have:

```csharp
var mask = Image.NewFromArray(new[,]
{
    {-1, -1, -1},
    {-1, 16, -1},
    {-1, -1, -1}
}, scale: 8);
image = image.Conv(mask, precision: Enums.Precision.Integer);
```

[`NewFromArray`](xref:NetVips.Image.NewFromArray*) creates an image from an array constant. The
scale is the amount to divide the image by after integer convolution.

See the libvips API docs for [`vips_conv()`](http://libvips.github.io/libvips/API/current/libvips-convolution.html#vips-conv) 
(the operation invoked by [`Conv`](xref:NetVips.Image.Conv*)) for details on the convolution operator. By
default, it computes with a float mask, but `integer` is fine for this case,
and is much faster.

Finally:

```csharp
image.WriteToFile("x.jpg");
```

[`WriteToFile`](xref:NetVips.Image.WriteToFile*) writes an image back to the filesystem. It can
write any format supported by vips: the file type is set from the filename
suffix. You can also write formatted images to memory, or dump
image data to a C-style array in an byte array.

## Metadata and attributes

NetVips has a [`Get`](xref:NetVips.Image.Get*) method to look up unknown names in libvips.
To make it a bit easier, common properties that libvips keeps for images are accessible by C# properties, 
see [`.Width`](xref:NetVips.Image.Width*) and friends.

As well as the core properties, you can read and write the metadata
that libvips keeps for images with [`Get`](xref:NetVips.Image.Get*) and
friends. For example:

```csharp
var image = Image.NewFromFile("some-image.jpg");
var iptcString = image.Get("iptc-data");
var exifDateString = image.Get("exif-ifd0-DateTime");
```

Use [`GetFields()`](xref:NetVips.Image.GetFields*) to get a list of all the field names you can use with
[`Get`](xref:NetVips.Image.Get*).

libvips caches and shares images behind your back, so you can't change an image
unless you are certain you have the only reference to it. 

Set image properties, like [`.Xres`](xref:NetVips.Image.Xres*) with [`Copy`](xref:NetVips.Image.Copy*). For
example:

```csharp
var newImage = image.Copy(xres: 12, yres: 13);
```

Now `newImage` is a private clone of `image` with `xres` and `yres`
changed.

Set image metadata with [`Set`](xref:NetVips.Image.Set*). Use [`Copy`](xref:NetVips.Image.Copy*) to make
a private copy of the image first, for example:

```csharp
var newImage = image.Copy();
newImage.Set("icc-profile-data", newProfile);
```

Now `newImage` is a clone of `image` with a new ICC profile attached to
it.

## Calling libvips operations

All libvips operations were generated automatically to a PascalCase method in NetVips.
For example, the libvips operation `add`, which appears in C as
[`vips_add()`](http://libvips.github.io/libvips/API/current/libvips-arithmetic.html#vips-add), 
appears in C# as [`Add`](xref:NetVips.Image.Add*) method.

By taking advantage of nullable types (which allows you to omit any parameters in any position),
we are able to call libvips operations that have optional arguments.

Some libvips operations have optional output arguments, for such operations we generated
the corresponding method overloads. For example, [`Min`](xref:NetVips.Image.Min*), the vips operation 
that searches an image for the minimum value, has a large number of optional arguments.
You can use it to find the minimum value like this:

```csharp
var minValue = image.Min();
```

You can ask it to return the position of the minimum with `out var xPos` and `out var yPos`:

```csharp
var minValue = image.Min(out var xPos, out var yPos);
```

Now `xPos` and `yPos` will have the coordinates of the minimum value.
There's actually a convenience method for this, [`MinPos`](xref:NetVips.Image.MinPos*).

You can also ask for the top *n* minimum, for example:

```csharp
// We explicitly discard the first three arguments
var minValue = image.Max(out _, out _, out _, out var xPos, out var yPos);
```

Now `xPos` and `yPos` will be 10-element arrays.

Because operations are member functions and return the result image, you can
chain them. For example, you can write:

```csharp
var resultImage = image.Real().Cos();
```

to calculate the cosine of the real part of a complex image. There is
also a full set of arithmetic operator overloads, see below.

If an operation takes several input images, you can use a constant for all but
one of them and the wrapper will expand the constant to an image for you. For
example, [`Ifthenelse`](xref:NetVips.Image.Ifthenelse*) uses a condition image to pick 
pixels between a then and an else image:

```csharp
var resultImage = conditionImage.Ifthenelse(thenImage, elseImage);
```

You can use a constant instead of either the then or the else parts and it
will be expanded to an image for you. If you use a constant for both then and
else, it will be expanded to match the condition image. For example:

```csharp
var resultImage = conditionImage.Ifthenelse(new[] {0, 255, 0}, new[] {255, 0, 0});
```

Will make an image where true pixels are green and false pixels are red.

This is useful for [`Bandjoin`](xref:NetVips.Image.Bandjoin*), the thing to join two or more
images up bandwise. You can write:

```csharp
var rgba = rgb.Bandjoin(255);
```

to append a constant 255 band to an image, perhaps to add an alpha channel. Of
course you can also write:

```csharp
var resultImage = image1.Bandjoin(image2);
resultImage = image1.Bandjoin(new[] {image2, image3});
resultImage = image1.Bandjoin(new[] {image2, 255});
```

and so on.

## Logging and warnings

NetVips can log warnings and debug messages from libvips. Some warnings are important, 
for example truncated files, and you might want to see them.

Add these lines somewhere near the start of your program:

```csharp
_handlerId = Log.SetLogHandler("VIPS", Enums.LogLevelFlags.Critical, (domain, level, message) =>
{
    Console.WriteLine("Domain: '{0}' Level: {1}", domain, level);
    Console.WriteLine("Message: {0}", message);
});
```

Make sure to remove the log handler, if you do not need it anymore:

```csharp
Log.RemoveLogHandler("VIPS", _handlerId);
```

## Automatic documentation

These API docs are generated automatically by DocFX. It generates API reference documentation
from triple-slash comments in our source code.

## Generated methods

The `Image.Generated.cs` file where all libvips operations are located 
is generated automatically by [`Operation.GenerateImageClass`](xref:NetVips.Operation.GenerateImageClass*).
It examines libvips and writes the XML documentation and the corresponding code of each operation.

Use the C API docs for more detail:

https://libvips.github.io/libvips/API/current

## Exceptions

The wrapper spots errors from vips operations and raises the [`VipsException`](xref:NetVips.VipsException).
You can catch it in the usual way.

## Enums

The libvips enums, such as `VipsBandFormat`, appear in NetVips as strings constants
like `"uchar"`. They are documented as a set of classes for convenience, see 
[`Enums.Access`](xref:NetVips.Enums.Access), for example.

## Draw operations

Paint operations like [`DrawCircle`](xref:NetVips.Image.DrawCircle*) and
[`DrawLine`](xref:NetVips.Image.DrawLine*) modify their input image. This makes them
hard to use with the rest of libvips: you need to be very careful about
the order in which operations execute or you can get nasty crashes.

The wrapper spots operations of this type and makes a private copy of the
image in memory before calling the operation. This stops crashes, but it does
make it inefficient. If you draw 100 lines on an image, for example, you'll
copy the image 100 times. The wrapper does make sure that memory is recycled
where possible, so you won't have 100 copies in memory.

If you want to avoid the copies, you'll need to call drawing operations
yourself.

## Overloads

The wrapper defines the usual set of arithmetic, boolean and relational
overloads on image. You can mix images, constants and lists of constants
freely. For example, you can write:

```csharp
var resultImage = ((image * new[] {1, 2, 3}).Abs() < 128) | 4;
```

## Expansions

Some vips operators take an enum to select an action, for example
[`Math`](xref:NetVips.Image.Math*) can be used to calculate sine of every pixel
like this:

```csharp
var resultImage = image.Math("sin");
```

This is annoying, so the wrapper expands all these enums into separate members
named after the enum value. So you can also write:

```csharp
var resultImage = image.Sin();
```

## Convenience functions

The wrapper defines a few extra useful utility functions:
[`Bandsplit`](xref:NetVips.Image.Bandsplit*), 
[`MaxPos`](xref:NetVips.Image.MaxPos*), 
[`MinPos`](xref:NetVips.Image.MinPos*), 
and [`Median`](xref:NetVips.Image.Median*).
