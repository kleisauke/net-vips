---
_disableToc: true
_disableContribution: false
---

Introduction
============================

See the main libvips site for an introduction to the underlying library. These
notes introduce the .NET binding.

https://www.libvips.org/

## Example

This example loads a file, boosts the green channel, sharpens the image,
and saves it back to disc again:

```csharp
using NetVips;

using var image = Image.NewFromFile("some-image.jpg", access: Enums.Access.Sequential);

using var multiply = image * new[] { 1, 2, 1 };

using var mask = Image.NewFromArray(new[,]
{
    {-1, -1, -1},
    {-1, 16, -1},
    {-1, -1, -1}
}, scale: 8);
using var convolve = multiply.Conv(mask, precision: Enums.Precision.Integer);

convolve.WriteToFile("x.jpg");
```

Reading this example line by line, we have:

```csharp
using var image = Image.NewFromFile("some-image.jpg", access: Enums.Access.Sequential);
```

[`NewFromFile`](xref:NetVips.Image.NewFromFile*) can load any image file supported by libvips.
When you load an image, only the header is fetched from the file. Pixels will
not be read until you have built a pipeline of operations and connected it
to an output.

When you load, you can hint what type of access you will need. In this
example, we will be accessing pixels top-to-bottom as we sweep through
the image reading and writing, so `sequential` access mode is best for us.
The default mode is `random` which allows for full random access to image
pixels, but is slower and needs more memory. See [`Enums.Access`](xref:NetVips.Enums.Access)
for details on the various modes available.

You can also load formatted images from memory with
[`NewFromBuffer`](xref:NetVips.Image.NewFromBuffer*), create images that wrap C-style memory arrays
held as C# arrays with [`NewFromMemory`](xref:NetVips.Image.NewFromMemory*), or make images
from constants with [`NewFromArray`](xref:NetVips.Image.NewFromArray*). You can also create custom
sources and targets that link image processing pipelines to your own code,
see [Custom sources and targets](introduction.md#custom-sources-and-targets).

The next line:

```csharp
using var multiply = image * new[] { 1, 2, 1 };
```

Multiplies the image by an array constant using one array element for each
image band. This line assumes that the input image has three bands and will
double the middle band. For RGB images, that's doubling green.

There is [a full set arithmetic operator overloads](introduction.md#overloads), so you can compute with
entire images just as you would with single numbers.

Next we have:

```csharp
using var mask = Image.NewFromArray(new[,]
{
    {-1, -1, -1},
    {-1, 16, -1},
    {-1, -1, -1}
}, scale: 8);
using var convolve = multiply.Conv(mask, precision: Enums.Precision.Integer);
```

[`NewFromArray`](xref:NetVips.Image.NewFromArray*) creates an image from an array constant. The
scale is the amount to divide the image by after integer convolution.

See the libvips API docs for [`vips_conv()`](https://www.libvips.org/API/current/libvips-convolution.html#vips-conv)
(the operation invoked by [`Conv`](xref:NetVips.Image.Conv*)) for details on the convolution operator. By
default, it computes with a float mask, but `integer` is fine for this case,
and is much faster.

Finally:

```csharp
convolve.WriteToFile("x.jpg");
```

[`WriteToFile`](xref:NetVips.Image.WriteToFile*) writes an image back to the filesystem. It can
write any format supported by vips: the file type is set from the filename
suffix. You can also write formatted images to memory, or dump
image data to a C-style array in an C# byte array.

## Metadata and attributes

NetVips has a [`Get`](xref:NetVips.Image.Get*) method to look up unknown names in libvips.
To make it a bit easier, common properties that libvips keeps for images are accessible by C# properties,
see [`.Width`](xref:NetVips.Image.Width*) and friends.

As well as the core properties, you can read and write the metadata
that libvips keeps for images with [`Get`](xref:NetVips.Image.Get*) and
friends. For example:

```csharp
using var image = Image.NewFromFile("some-image.jpg");
var iptcString = image.Get("iptc-data");
var exifDateString = image.Get("exif-ifd0-DateTime");
```

Use [`GetFields()`](xref:NetVips.Image.GetFields*) to get a list of all the field names you can use with
[`Get`](xref:NetVips.Image.Get*).

libvips caches and shares images between different parts of your program. This
means that you can't modify an image unless you are certain that you have
the only reference to it. You can make a private copy of an image with
[`Copy`](xref:NetVips.Image.Copy*), for example:

```csharp
using var newImage = image.Copy(xres: 12, yres: 13);
```

Now `newImage` is a private clone of `image` with `xres` and `yres` changed.

You can also set and remove image metadata fields. Images are immutable, so you must
make any changes inside a [`Mutate`](xref:NetVips.Image.Mutate*) delegate. For example:

```csharp
using var mutated = image.Mutate(mutable =>
{
    foreach (var field in image.GetFields())
    {
        if (field == "icc-profile-data") continue;
        mutable.Remove(field);
    }
});
```

To remove all metadata except the icc profile.

You can use [`Set`](xref:NetVips.MutableImage.Set*) to change the value of an existing field,
or to create a new field with a specified type.

## Calling libvips operations

All libvips operations were generated automatically to a PascalCase method in NetVips.
For example, the libvips operation `add`, which appears in C as
[`vips_add()`](https://www.libvips.org/API/current/libvips-arithmetic.html#vips-add),
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
using var resultImage = image.Real().Cos();
```

> [!WARNING]
> Chaining does not automatically dispose the temporary images.
> To ensure that these images are disposed early, you should use:
> ```csharp
> using var real = image.Real();
> using var resultImage = real.Cos();
> ```
> Otherwise, these images will not be disposed until the next GC cycle runs.

to calculate the cosine of the real part of a complex image. There is
also [a full set of arithmetic operator overloads](introduction.md#overloads).

If an operation takes several input images, you can use a constant for all but
one of them and the wrapper will expand the constant to an image for you. For
example, [`Ifthenelse`](xref:NetVips.Image.Ifthenelse*) uses a condition image to pick
pixels between a then and an else image:

```csharp
using var resultImage = conditionImage.Ifthenelse(thenImage, elseImage);
```

You can use a constant instead of either the then or the else parts and it
will be expanded to an image for you. If you use a constant for both then and
else, it will be expanded to match the condition image. For example:

```csharp
using var resultImage = conditionImage.Ifthenelse(new[] { 0, 255, 0 }, new[] { 255, 0, 0 });
```

Will make an image where true pixels are green and false pixels are red.

This is useful for [`Bandjoin`](xref:NetVips.Image.Bandjoin*), the thing to join two or more
images up bandwise. You can write:

```csharp
using var rgba = rgb.Bandjoin(255);
```

to append a constant 255 band to an image, perhaps to add an alpha channel. Of
course you can also write:

```csharp
using var bandjoin = image1.Bandjoin(image2);
using var bandjoin2 = image1.Bandjoin(image2, image3);
using var bandjoin3 = image1.Bandjoin(image2, 255);
```

and so on.

## Logging and warnings

NetVips can log warnings and debug messages from libvips. Some warnings are important,
for example truncated files, and you might want to see them.

Add these lines somewhere near the start of your program:

```csharp
_handlerId = Log.SetLogHandler("VIPS", Enums.LogLevelFlags.Warning, (domain, level, message) =>
{
    Console.WriteLine("Domain: '{0}' Level: {1}", domain, level);
    Console.WriteLine("Message: {0}", message);
});
```

Make sure to remove the log handler, if you do not need it anymore:

```csharp
Log.RemoveLogHandler("VIPS", _handlerId);
```

## Exceptions

The wrapper spots errors from vips operations and raises the [`VipsException`](xref:NetVips.VipsException).
You can catch it in the usual way.

## Enums

The libvips enums, such as `VipsBandFormat`, appear in NetVips as C# enums
like `Enums.BandFormat.Uchar`. They are documented for convenience,
see [`Enums.Access`](xref:NetVips.Enums.Access), for example.

## Overloads

The wrapper defines the usual set of arithmetic, boolean and relational
overloads on image. You can mix images, constants and lists of constants
freely. For example, you can write:

```csharp
using var resultImage = ((image * new[] { 1, 2, 3 }).Abs() < 128) | 4;
```

> [!WARNING]
> Chaining does not automatically dispose the temporary images.
> To ensure that these images are disposed early, you should use:
> ```csharp
> using var multiply = image * new[] { 1, 2, 3 };
> using var absolute = multiply.Abs();
> using var threshold = absolute < 128;
> using var resultImage = threshold | 4;
> ```
> Otherwise, these images will not be disposed until the next GC cycle runs.

## Expansions

Some vips operators take an enum to select an action, for example
[`Math`](xref:NetVips.Image.Math*) can be used to calculate sine of every pixel
like this:

```csharp
using var resultImage = image.Math(Enums.OperationMath.Sin);
```

This is annoying, so the wrapper expands all these enums into separate members
named after the enum value. So you can also write:

```csharp
using var resultImage = image.Sin();
```

## Convenience functions

The wrapper defines a few extra useful utility functions:
[`Bandsplit`](xref:NetVips.Image.Bandsplit*),
[`MaxPos`](xref:NetVips.Image.MaxPos*),
[`MinPos`](xref:NetVips.Image.MinPos*),
and [`Median`](xref:NetVips.Image.Median*).

## Tracking and interrupting computation

You can attach progress handlers to images to watch the progress of
computation.

For example:

```csharp
using var image = Image.Black(1, 500);

var progress = new Progress<int>(percent =>
{
    Console.Write($"\r{percent}% complete");
});

var cts = new CancellationTokenSource();
cts.CancelAfter(5000);

// Uncomment to kill the image after 5 sec
image.SetProgress(progress/*, cts.Token*/);

var avg = image.Avg();
```

Or:
```csharp
using var image = Image.Black(1, 500);
image.SetProgress(true);
image.SignalConnect(Enums.Signals.PreEval, PreEvalHandler);
image.SignalConnect(Enums.Signals.Eval, EvalHandler);
image.SignalConnect(Enums.Signals.PostEval, PostEvalHandler);

var avg = image.Avg();
```

Handlers are given a [`VipsProgress`](xref:NetVips.VipsProgress) struct containing a number
of useful fields. For example:

```csharp
private void EvalHandler(Image image, VipsProgress progress)
{
    Console.WriteLine($"run time so far (secs) = {progress.Run}");
    Console.WriteLine($"estimated time of arrival (secs) = {progress.Eta}");
    Console.WriteLine($"total number of pels to process = {progress.TPels}");
    Console.WriteLine($"number of pels processed so far = {progress.NPels}");
    Console.WriteLine($"percent complete = {progress.Percent}");
}
```

Use [`SetKill`](xref:NetVips.Image.SetKill*) on the image to stop computation early.

For example:

```csharp
private void EvalHandler(Image image, VipsProgress progress)
{
    if (progress.Percent > 50)
    {
        image.SetKill(true);
    }
}
```

## Custom sources and targets

You can load and save images to and from [`Source`](xref:NetVips.Source) and
[`Target`](xref:NetVips.Target).

For example:

```csharp
using var source = Source.NewFromFile("example.jpg");
using var image = Image.NewFromSource(source, access: Enums.Access.Sequential);
using var target = Target.NewToFile("example.png");
image.WriteToTarget(target, ".png");
```

Sources and targets can be files, descriptors (eg. pipes) and areas of memory.

You can define [`SourceCustom`](xref:NetVips.SourceCustom) and [`TargetCustom`](xref:NetVips.TargetCustom) too.

For example:

```csharp
using var input = File.OpenRead("example.jpg");

using var source = new SourceCustom();
source.OnRead += (buffer, length) => input.Read(buffer, 0, length);
source.OnSeek += (offset, origin) => input.Seek(offset, origin);

using var output = File.OpenWrite("example.png");

using var target = new TargetCustom();
target.OnWrite += (buffer, length) =>
{
    output.Write(buffer, 0, length);
    return length;
};
target.OnFinish += () => output.Close();

using var image = Image.NewFromSource(source, access: Enums.Access.Sequential);
image.WriteToTarget(target, ".png");
```

The wrapper also defines a few extra useful stream functions. For example, the above can be written as:

```csharp
using var input = File.OpenRead("example.jpg")
using var image = Image.NewFromStream(input, access: Enums.Access.Sequential);

using var output = File.OpenWrite("example.png");
image.WriteToStream(output, ".png");
```

## Automatic documentation

These API docs are generated automatically by DocFX. It generates API reference documentation
from triple-slash comments in our source code.

## Generated methods

The `Image.Generated.cs` file where all libvips operations are located
is generated automatically by [`GenerateImageClass.cs`](https://github.com/kleisauke/net-vips/blob/master/samples/NetVips.Samples/Samples/GenerateImageClass.cs).
It examines libvips and writes the XML documentation and the corresponding code of each operation.

Use the C API docs for more detail:

https://www.libvips.org/API/current/

## Draw operations

Paint operations like [`DrawCircle`](xref:NetVips.MutableImage.DrawCircle*) and
[`DrawLine`](xref:NetVips.MutableImage.DrawLine*) modify their input image. This makes them
hard to use with the rest of libvips: you need to be very careful about
the order in which operations execute or you can get nasty crashes.

The wrapper handles this type of operations with the [`Mutate`](xref:NetVips.Image.Mutate*)
function. This can be used to make a [`MutableImage`](xref:NetVips.MutableImage). This is an
image which is unshared and is only available inside the [`Mutate`](xref:NetVips.Image.Mutate*)
delegate. Within this delegate, you can use draw operations to modify images. For example:

```csharp
using var mutated = image.Mutate(mutable =>
{
    for (var i = 0; i <= 100; i++)
    {
        var j = i / 100.0;
        mutable.DrawLine(new[] { 255.0 }, (int)(mutable.Width * j), 0, 0, (int)(mutable.Height * (1 - j)));
    }
});
```

Now each [`DrawLine`](xref:NetVips.MutableImage.DrawLine*) will directly modify the mutable image.
