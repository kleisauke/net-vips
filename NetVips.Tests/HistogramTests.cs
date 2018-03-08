using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace NetVips.Tests
{
    [TestFixture]
    class HistogramTests
    {
        [SetUp]
        public void Init()
        {
            Base.VipsInit();
        }

        [TearDown]
        public void Dispose()
        {
        }

        [Test]
        public void TestHistCum()
        {
            var im = Image.Identity();

            var sum = im.Avg() * 256;

            var cum = im.HistCum();

            var p = cum.Getpoint(255, 0);
            Assert.AreEqual(sum, p[0]);
        }

        [Test]
        public void TestHistEqual()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            var im2 = im.HistEqual();

            Assert.AreEqual(im.Width, im2.Width);
            Assert.AreEqual(im.Height, im2.Height);

            Assert.IsTrue(im.Avg() < im2.Avg());
            Assert.IsTrue(im.Deviate() < im2.Deviate());
        }

        [Test]
        public void TestHistIsmonotonic()
        {
            var im = Image.Identity();
            Assert.IsTrue(im.HistIsmonotonic());
        }

        [Test]
        public void TestHistLocal()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            var im2 = im.HistLocal(10, 10);

            Assert.AreEqual(im.Width, im2.Width);
            Assert.AreEqual(im.Height, im2.Height);

            Assert.IsTrue(im.Avg() < im2.Avg());
            Assert.IsTrue(im.Deviate() < im2.Deviate());

            if (Base.AtLeastLibvips(8, 5))
            {
                var im3 = im.HistLocal(10, 10, new Dictionary<string, object>
                {
                    {"max_slope", 3}
                });
                Assert.AreEqual(im.Width, im3.Width);
                Assert.AreEqual(im.Height, im3.Height);

                Assert.IsTrue(im3.Deviate() < im2.Deviate());
            }
        }

        [Test]
        public void TestHistMatch()
        {
            var im = Image.Identity();
            var im2 = Image.Identity();

            var matched = im.HistMatch(im2);

            Assert.AreEqual(0.0, (im - matched).Abs().Max());
        }

        [Test]
        public void TestHistNorm()
        {
            var im = Image.Identity();
            var im2 = im.HistNorm();
            Assert.AreEqual(0.0, (im - im2).Abs().Max());
        }

        [Test]
        public void TestHistPlot()
        {
            var im = Image.Identity();
            var im2 = im.HistPlot();

            Assert.AreEqual(256, im2.Width);
            Assert.AreEqual(256, im2.Height);
            Assert.AreEqual(Enums.BandFormat.Uchar, im2.Format);
            Assert.AreEqual(1, im2.Bands);
        }

        [Test]
        public void TestHistMap()
        {
            var im = Image.Identity();

            var im2 = im.Maplut(im);

            Assert.AreEqual(0.0, (im - im2).Abs().Max());
        }

        [Test]
        public void TestPercent()
        {
            var im = Image.NewFromFile(Helper.JpegFile)[1];

            var pc = im.Percent(90);

            var msk = im <= pc;
            var nSet = (msk.Avg() * msk.Width * msk.Height) / 255.0;
            var pcSet = 100 * nSet / (msk.Width * msk.Height);

            Assert.AreEqual(90, pcSet, 0.5);
        }

        [Test]
        public void TestHistEntropy()
        {
            var im = Image.NewFromFile(Helper.JpegFile)[1];

            var ent = im.HistFind().HistEntropy();

            Assert.AreEqual(4.37, ent, 0.01);
        }

        [Test]
        public void TestStdif()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            var im2 = im.Stdif(10, 10);

            Assert.AreEqual(im.Width, im2.Width);
            Assert.AreEqual(im.Height, im2.Height);

            // new mean should be closer to target mean
            Assert.IsTrue(Math.Abs(im.Avg() - 128) > Math.Abs(im2.Avg() - 128));
        }
    }
}