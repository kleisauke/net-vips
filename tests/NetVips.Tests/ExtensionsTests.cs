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

        /// <summary>
        /// Disable the extensions tests, if we're running inside a musl-based Linux container.
        /// Saves us the installation of libgdiplus.
        /// </summary>
        private static bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        [SkippableFact]
        public void ToBitmap1Band()
        {
            Skip.If(InDocker, "running in Docker, skipping test");

            var black = Image.Black(1, 1).Cast(Enums.BandFormat.Uchar);
            var white = (Image.Black(1, 1) + 255).Cast(Enums.BandFormat.Uchar);

            AssertPixelValue(black.WriteToMemory(), black.ToBitmap());
            AssertPixelValue(white.WriteToMemory(), white.ToBitmap());
        }

        [SkippableFact]
        public void ToBitmap2Bands()
        {
            Skip.If(InDocker || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "running in Docker or not on Windows, skipping test");

            var black = (Image.Black(1, 1) + new[] { 0, 0 }).Cast(Enums.BandFormat.Uchar);
            var white = (Image.Black(1, 1) + new[] { 255, 255 }).Cast(Enums.BandFormat.Uchar);
            var grey = (Image.Black(1, 1) + new[] { 128, 255 }).Cast(Enums.BandFormat.Uchar);

            AssertPixelValue(black.WriteToMemory(), black.ToBitmap());
            AssertPixelValue(white.WriteToMemory(), white.ToBitmap());
            AssertPixelValue(grey.WriteToMemory(), grey.ToBitmap());
        }

        [SkippableFact]
        public void ToBitmap3Bands()
        {
            Skip.If(InDocker, "running in Docker, skipping test");

            var redColor = (Image.Black(1, 1) + new[] { 255, 0, 0 }).Cast(Enums.BandFormat.Uchar);
            var blueColor = (Image.Black(1, 1) + new[] { 0, 0, 255 }).Cast(Enums.BandFormat.Uchar);
            var greenColor = (Image.Black(1, 1) + new[] { 0, 255, 0 }).Cast(Enums.BandFormat.Uchar);

            AssertPixelValue(redColor.WriteToMemory(), redColor.ToBitmap());
            AssertPixelValue(blueColor.WriteToMemory(), blueColor.ToBitmap());
            AssertPixelValue(greenColor.WriteToMemory(), greenColor.ToBitmap());
        }

        [SkippableFact]
        public void ToBitmap4Bands()
        {
            Skip.If(InDocker, "running in Docker, skipping test");

            var redColor = (Image.Black(1, 1) + new[] { 255, 0, 0, 255 }).Cast(Enums.BandFormat.Uchar);
            var blueColor = (Image.Black(1, 1) + new[] { 0, 0, 255, 255 }).Cast(Enums.BandFormat.Uchar);
            var greenColor = (Image.Black(1, 1) + new[] { 0, 255, 0, 255 }).Cast(Enums.BandFormat.Uchar);

            AssertPixelValue(redColor.WriteToMemory(), redColor.ToBitmap());
            AssertPixelValue(blueColor.WriteToMemory(), blueColor.ToBitmap());
            AssertPixelValue(greenColor.WriteToMemory(), greenColor.ToBitmap());
        }

        private static void AssertPixelValue(byte[] expected, Bitmap actual)
        {
            if (actual.Width != 1 || actual.Height != 1)
                throw new Exception("1x1 image only");

            var length = expected.Length;

            // An additional band is added for greyscale images
            if (length == 2)
            {
                length++;
            }

            var pixels = new byte[length];
            var bitmapData = actual.LockBits(new Rectangle(0, 0, 1, 1), ImageLockMode.ReadOnly, actual.PixelFormat);
            Marshal.Copy(bitmapData.Scan0, pixels, 0, length);
            actual.UnlockBits(bitmapData);

            // Switch from BGR(A) to RGB(A)
            if (length > 2)
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