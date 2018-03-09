# Mono/.NET bindings for libvips

This NuGet package provides a Mono/.NET binding for the [libvips image processing library](https://jcupitt.github.io/libvips).

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
