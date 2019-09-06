namespace NetVips.Tests
{
    using Xunit;
    using Xunit.Abstractions;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using Extensions;
    using Image = Image;

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

            AssertPixelValue(black.WriteToMemory(), black.ToBitmap());
            AssertPixelValue(white.WriteToMemory(), white.ToBitmap());
        }

        [Fact]
        public void ToBitmap2Bands()
        {
            var black = (Image.Black(1, 1) + new[] {0, 0}).Cast(Enums.BandFormat.Uchar);
            var white = (Image.Black(1, 1) + new[] {255, 255}).Cast(Enums.BandFormat.Uchar);
            var grey = (Image.Black(1, 1) + new[] {128, 255}).Cast(Enums.BandFormat.Uchar);

            AssertPixelValue(black.WriteToMemory(), black.ToBitmap());
            AssertPixelValue(white.WriteToMemory(), white.ToBitmap());
            AssertPixelValue(grey.WriteToMemory(), grey.ToBitmap());
        }

        [Fact]
        public void ToBitmap3Bands()
        {
            var redColor = (Image.Black(1, 1) + new[] {255, 0, 0}).Cast(Enums.BandFormat.Uchar);
            var blueColor = (Image.Black(1, 1) + new[] {0, 0, 255}).Cast(Enums.BandFormat.Uchar);
            var greenColor = (Image.Black(1, 1) + new[] {0, 255, 0}).Cast(Enums.BandFormat.Uchar);

            AssertPixelValue(redColor.WriteToMemory(), redColor.ToBitmap());
            AssertPixelValue(blueColor.WriteToMemory(), blueColor.ToBitmap());
            AssertPixelValue(greenColor.WriteToMemory(), greenColor.ToBitmap());
        }

        [Fact]
        public void ToBitmap4Bands()
        {
            var redColor = (Image.Black(1, 1) + new[] {255, 0, 0, 255}).Cast(Enums.BandFormat.Uchar);
            var blueColor = (Image.Black(1, 1) + new[] {0, 0, 255, 255}).Cast(Enums.BandFormat.Uchar);
            var greenColor = (Image.Black(1, 1) + new[] {0, 255, 0, 255}).Cast(Enums.BandFormat.Uchar);

            AssertPixelValue(redColor.WriteToMemory(), redColor.ToBitmap());
            AssertPixelValue(blueColor.WriteToMemory(), blueColor.ToBitmap());
            AssertPixelValue(greenColor.WriteToMemory(), greenColor.ToBitmap());
        }

        private static void AssertPixelValue(byte[] expected, Bitmap actual)
        {
            if (actual.Width != 1 || actual.Height != 1)
                throw new Exception("1x1 image only");

            var pixels = new byte[expected.Length];
            var bitmapData = actual.LockBits(new Rectangle(0, 0, 1, 1), ImageLockMode.ReadOnly, actual.PixelFormat);
            Marshal.Copy(bitmapData.Scan0, pixels, 0, expected.Length);
            actual.UnlockBits(bitmapData);

            // Switch from BGR to RGB
            if (expected.Length == 3)
            {
                var t = pixels[0];
                pixels[0] = pixels[2];
                pixels[2] = t;
            }

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], pixels[i]);
            }
        }
    }
}