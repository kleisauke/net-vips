using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using NetVips.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace NetVips.Tests;

[SupportedOSPlatform("windows")]
public class ExtensionsTests : IClassFixture<TestsFixture>
{
    public ExtensionsTests(TestsFixture testsFixture, ITestOutputHelper output)
    {
        testsFixture.SetUpLogging(output);
    }

    [Fact]
    public void ToBitmap1Band()
    {
        var black = Image.Black(1, 1).Cast(Enums.BandFormat.Uchar);
        var white = (Image.Black(1, 1) + 255).Cast(Enums.BandFormat.Uchar);

        AssertPixelValue(black.WriteToMemory<byte>(), black.ToBitmap());
        AssertPixelValue(white.WriteToMemory<byte>(), white.ToBitmap());
    }

    [Fact]
    public void ToBitmap2Bands()
    {
        var black = Image.Black(1, 1, bands: 2).Cast(Enums.BandFormat.Uchar);
        var white = (Image.Black(1, 1) + new[] { 255, 255 }).Cast(Enums.BandFormat.Uchar);
        var grey = (Image.Black(1, 1) + new[] { 128, 255 }).Cast(Enums.BandFormat.Uchar);

        AssertPixelValue(black.WriteToMemory<byte>(), black.ToBitmap());
        AssertPixelValue(white.WriteToMemory<byte>(), white.ToBitmap());
        AssertPixelValue(grey.WriteToMemory<byte>(), grey.ToBitmap());
    }

    [Fact]
    public void ToBitmap3Bands()
    {
        var redColor = (Image.Black(1, 1) + new[] { 255, 0, 0 }).Cast(Enums.BandFormat.Uchar);
        var blueColor = (Image.Black(1, 1) + new[] { 0, 0, 255 }).Cast(Enums.BandFormat.Uchar);
        var greenColor = (Image.Black(1, 1) + new[] { 0, 255, 0 }).Cast(Enums.BandFormat.Uchar);

        AssertPixelValue(redColor.WriteToMemory<byte>(), redColor.ToBitmap());
        AssertPixelValue(blueColor.WriteToMemory<byte>(), blueColor.ToBitmap());
        AssertPixelValue(greenColor.WriteToMemory<byte>(), greenColor.ToBitmap());
    }

    [Fact]
    public void ToBitmap4Bands()
    {
        var redColor = (Image.Black(1, 1) + new[] { 255, 0, 0, 255 }).Cast(Enums.BandFormat.Uchar);
        var blueColor = (Image.Black(1, 1) + new[] { 0, 0, 255, 255 }).Cast(Enums.BandFormat.Uchar);
        var greenColor = (Image.Black(1, 1) + new[] { 0, 255, 0, 255 }).Cast(Enums.BandFormat.Uchar);

        AssertPixelValue(redColor.WriteToMemory<byte>(), redColor.ToBitmap());
        AssertPixelValue(blueColor.WriteToMemory<byte>(), blueColor.ToBitmap());
        AssertPixelValue(greenColor.WriteToMemory<byte>(), greenColor.ToBitmap());
    }

    private static void AssertPixelValue(byte[] expected, Bitmap actual)
    {
        if (actual.Width != 1 || actual.Height != 1)
        {
            throw new Exception("1x1 image only");
        }

        // An additional band is added for greyscale images
        if (expected.Length == 2)
        {
            expected = [expected[0], expected[1], 255];
        }

        var pixels = new byte[expected.Length];
        var bitmapData = actual.LockBits(new Rectangle(0, 0, 1, 1), ImageLockMode.ReadOnly, actual.PixelFormat);
        Marshal.Copy(bitmapData.Scan0, pixels, 0, expected.Length);
        actual.UnlockBits(bitmapData);

        // Switch from BGR(A) to RGB(A)
        if (expected.Length > 2)
        {
            (pixels[0], pixels[2]) = (pixels[2], pixels[0]);
        }

        Assert.Equal(expected, pixels);
    }
}