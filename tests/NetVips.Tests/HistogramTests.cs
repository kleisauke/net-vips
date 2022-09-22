namespace NetVips.Tests
{
    using System;
    using Xunit;
    using Xunit.Abstractions;

    public class HistogramTests : IClassFixture<TestsFixture>
    {
        public HistogramTests(TestsFixture testsFixture, ITestOutputHelper output)
        {
            testsFixture.SetUpLogging(output);
        }

        [Fact]
        public void TestHistCum()
        {
            var im = Image.Identity();

            var sum = im.Avg() * 256;

            var cum = im.HistCum();

            var p = cum[255, 0];
            Assert.Equal(sum, p[0]);
        }

        [Fact]
        public void TestHistEqual()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            var im2 = im.HistEqual();

            Assert.Equal(im.Width, im2.Width);
            Assert.Equal(im.Height, im2.Height);

            Assert.True(im.Avg() < im2.Avg());
            Assert.True(im.Deviate() < im2.Deviate());
        }

        [Fact]
        public void TestHistIsmonotonic()
        {
            var im = Image.Identity();
            Assert.True(im.HistIsmonotonic());
        }

        [Fact]
        public void TestHistLocal()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            var im2 = im.HistLocal(10, 10);

            Assert.Equal(im.Width, im2.Width);
            Assert.Equal(im.Height, im2.Height);

            Assert.True(im.Avg() < im2.Avg());
            Assert.True(im.Deviate() < im2.Deviate());

            if (NetVips.AtLeastLibvips(8, 5))
            {
                var im3 = im.HistLocal(10, 10, maxSlope: 3);
                Assert.Equal(im.Width, im3.Width);
                Assert.Equal(im.Height, im3.Height);

                Assert.True(im3.Deviate() < im2.Deviate());
            }
        }

        [Fact]
        public void TestHistMatch()
        {
            var im = Image.Identity();
            var im2 = Image.Identity();

            var matched = im.HistMatch(im2);

            Assert.Equal(0.0, (im - matched).Abs().Max());
        }

        [Fact]
        public void TestHistNorm()
        {
            var im = Image.Identity();
            var im2 = im.HistNorm();
            Assert.Equal(0.0, (im - im2).Abs().Max());
        }

        [Fact]
        public void TestHistPlot()
        {
            var im = Image.Identity();
            var im2 = im.HistPlot();

            Assert.Equal(256, im2.Width);
            Assert.Equal(256, im2.Height);
            Assert.Equal(Enums.BandFormat.Uchar, im2.Format);
            Assert.Equal(1, im2.Bands);
        }

        [Fact]
        public void TestHistMap()
        {
            var im = Image.Identity();

            var im2 = im.Maplut(im);

            Assert.Equal(0.0, (im - im2).Abs().Max());
        }

        [Fact]
        public void TestPercent()
        {
            var im = Image.NewFromFile(Helper.JpegFile)[1];

            var pc = im.Percent(90);

            var msk = im <= pc;
            var nSet = (msk.Avg() * msk.Width * msk.Height) / 255.0;
            var pcSet = 100 * nSet / (msk.Width * msk.Height);

            Assert.True(Math.Abs(pcSet - 90) < 1);
        }

        [Fact]
        public void TestHistEntropy()
        {
            var im = Image.NewFromFile(Helper.JpegFile)[1];

            var ent = im.HistFind().HistEntropy();

            Assert.Equal(4.37, ent, 2);
        }

        [Fact]
        public void TestStdif()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            var im2 = im.Stdif(10, 10);

            Assert.Equal(im.Width, im2.Width);
            Assert.Equal(im.Height, im2.Height);

            // new mean should be closer to target mean
            Assert.True(Math.Abs(im.Avg() - 128) > Math.Abs(im2.Avg() - 128));
        }

        [SkippableFact]
        public void TestCase()
        {
            // case was added in libvips 8.9.
            Skip.IfNot(NetVips.AtLeastLibvips(8, 9), "requires libvips >= 8.9");

            var x = Image.Grey(256, 256, uchar: true);

            // slice into two at 128, we should get 50% of pixels in each half
            var index = Image.Switch(x < 128, x >= 128);
            var y = index.Case(10, 20);
            Assert.Equal(15, y.Avg());

            // slice into four
            index = Image.Switch(
                x < 64,
                x >= 64 && x < 128,
                x >= 128 && x < 192,
                x >= 192
            );
            Assert.Equal(25, index.Case(10, 20, 30, 40).Avg());

            // values over N should use the last value
            Assert.Equal(22.5, index.Case(10, 20, 30).Avg());
        }
    }
}