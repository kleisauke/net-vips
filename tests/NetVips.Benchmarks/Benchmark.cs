using System;
using System.IO;
using BenchmarkDotNet.Attributes;

using ImageMagick;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
// using SixLabors.ImageSharp.Processing.Processors.Convolution;

using SkiaSharp;

// Alias to handle conflicting namespaces
using Utils = NetVips.NetVips;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using ImageSharpRectangle = SixLabors.ImageSharp.Rectangle;

#if Windows_NT
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using SystemDrawingImage = System.Drawing.Image;
using SystemDrawingRectangle = System.Drawing.Rectangle;
#endif

namespace NetVips.Benchmarks;

[Config(typeof(Config))]
public class Benchmark
{
    private const int Quality = 75;

    private readonly IImageProcessor _processor = new ImageSharp.ConvolutionProcessor(new float[,]
    {
        {-1, -1, -1},
        {-1, 16, -1},
        {-1, -1, -1}
    }, false);

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Turn off OpenCL acceleration in ImageMagick
        OpenCL.IsEnabled = false;

        // Disable libvips cache to ensure tests are as fair as they can be
        Cache.Max = 0;

        // Reduce concurrency, as a large thread pool can slow down overall processing
        Utils.Concurrency = 4;
    }

    [Benchmark(Description = "NetVips", Baseline = true)]
    [Arguments("t.tif", "t2.tif")]
    [Arguments("t.jpg", "t2.jpg")]
    public void NetVips(string input, string output)
    {
        using var im = Image.NewFromFile(input, access: Enums.Access.Sequential);
        using var crop = im.Crop(100, 100, im.Width - 200, im.Height - 200);
        using var reduce = crop.Reduce(1.0 / 0.9, 1.0 / 0.9, kernel: Enums.Kernel.Linear);
        using var mask = Image.NewFromArray(new[,]
        {
            {-1, -1, -1},
            {-1, 16, -1},
            {-1, -1, -1}
        }, 8);
        using var convolve = reduce.Conv(mask, precision: Enums.Precision.Integer);

        // Default quality is 75
        convolve.WriteToFile(output);
    }

    [Benchmark(Description = "Magick.NET")]
    [Arguments("t.tif", "t2.tif")]
    [Arguments("t.jpg", "t2.jpg")]
    public void MagickNet(string input, string output)
    {
        using var im = new MagickImage(input);
        im.Interpolate = PixelInterpolateMethod.Bilinear;
        im.FilterType = FilterType.Triangle;

        im.Shave(100, 100);
        im.Resize(new Percentage(90.0));

        var kernel = new ConvolveMatrix(3, -1, -1, -1, -1, 16, -1, -1, -1, -1);

        im.SetArtifact("convolve:scale", "0.125");
        im.Convolve(kernel);

        // Default quality of ImageMagick is 92, we need 75
        im.Quality = Quality;

        im.Write(output);
    }

    [Benchmark(Description = "ImageSharp")]
    [Arguments("t.tif", "t2.tif")]
    [Arguments("t.jpg", "t2.jpg")]
    public void ImageSharp(string input, string output)
    {
        using var image = ImageSharpImage.Load(input);
        image.Mutate(x => x
            .Crop(new ImageSharpRectangle(100, 100, image.Width - 200, image.Height - 200))
            .Resize((int)Math.Round(image.Width * .9F), (int)Math.Round(image.Height * .9F),
                KnownResamplers.Triangle)
            .ApplyProcessor(_processor, image.Bounds));

        // Default quality is 75
        image.Save(output);
    }

    [Benchmark(Description = "SkiaSharp<sup>1</sup>")]
    [Arguments("t.jpg", "t2.jpg")] // SkiaSharp doesn't have TIFF support
    public void SkiaSharp(string input, string output)
    {
        using var bitmap = SKBitmap.Decode(input);
        bitmap.ExtractSubset(bitmap, SKRectI.Create(100, 100, bitmap.Width - 200, bitmap.Height - 200));

        var targetWidth = (int)Math.Round(bitmap.Width * .9F);
        var targetHeight = (int)Math.Round(bitmap.Height * .9F);

        // bitmap.Resize(new SKImageInfo(targetWidth, targetHeight), SKBitmapResizeMethod.Triangle)
        // and
        // bitmap.Resize(new SKImageInfo(targetWidth, targetHeight), SKFilterQuality.Low)
        // are deprecated, so we use `new SKSamplingOptions(SKFilterMode.Linear)` instead, see:
        // https://github.com/mono/SkiaSharp/blob/v2.88.9/binding/Binding/SKBitmap.cs#L26
        // https://github.com/mono/SkiaSharp/blob/v3.116.0/binding/SkiaSharp/SKPaint.cs#L31
        using var resized =
            bitmap.Resize(new SKImageInfo(targetWidth, targetHeight), new SKSamplingOptions(SKFilterMode.Linear));
        using var surface =
            SKSurface.Create(new SKImageInfo(targetWidth, targetHeight, bitmap.ColorType, bitmap.AlphaType));
        using var canvas = surface.Canvas;
        using var paint = new SKPaint { IsAntialias = true };
        var kernel = new[]
        {
            -1f, -1f, -1f,
            -1f, 16f, -1f,
            -1f, -1f, -1f
        };
        var kernelSize = new SKSizeI(3, 3);
        var kernelOffset = new SKPointI(1, 1);

        paint.ImageFilter = SKImageFilter.CreateMatrixConvolution(kernelSize, kernel, 0.125f, 0f, kernelOffset,
            SKShaderTileMode.Repeat, false);

        canvas.DrawBitmap(resized, 0, 0, paint);
        canvas.Flush();

        using var fileStream = File.OpenWrite(output);
        surface.Snapshot()
            .Encode(SKEncodedImageFormat.Jpeg, Quality)
            .SaveTo(fileStream);
    }

#if Windows_NT
    [Benchmark(Description = "System.Drawing<sup>2</sup>")]
    [Arguments("t.tif", "t2.tif")]
    [Arguments("t.jpg", "t2.jpg")]
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public void SystemDrawing(string input, string output)
    {
        using var image = SystemDrawingImage.FromFile(input, true);
        var cropRect = new SystemDrawingRectangle(100, 100, image.Width - 200, image.Height - 200);
        var resizeRect = new SystemDrawingRectangle(0, 0, (int)Math.Round(cropRect.Width * .9F),
            (int)Math.Round(cropRect.Height * .9F));

        using var src = new Bitmap(cropRect.Width, cropRect.Height);
        using (var cropGraphics = Graphics.FromImage(src))
        {
            cropGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            cropGraphics.CompositingMode = CompositingMode.SourceCopy;
            cropGraphics.InterpolationMode = InterpolationMode.Bilinear;

            // Crop
            cropGraphics.DrawImage(image, new SystemDrawingRectangle(0, 0, src.Width, src.Height), cropRect,
                GraphicsUnit.Pixel);
        }

        using var resized = new Bitmap(resizeRect.Width, resizeRect.Height);
        using var resizeGraphics = Graphics.FromImage(resized);
        using var attributes = new ImageAttributes();

        // Get rid of the annoying artifacts
        attributes.SetWrapMode(WrapMode.TileFlipXY);

        resizeGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        resizeGraphics.CompositingMode = CompositingMode.SourceCopy;
        resizeGraphics.InterpolationMode = InterpolationMode.Bilinear;

        // Resize
        resizeGraphics.DrawImage(src, resizeRect, 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, attributes);

        // No sharpening or convolution operation seems to be available

        // Default quality is 75, see:
        // https://stackoverflow.com/a/3959115
        resized.Save(output);
    }
#endif
}