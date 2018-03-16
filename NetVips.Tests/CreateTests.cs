using System;
using NUnit.Framework;

namespace NetVips.Tests
{
    [TestFixture]
    class CreateTests
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
        public void TestBlack()
        {
            var im = Image.Black(100, 100);

            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(100, im.Height);
            Assert.AreEqual(Enums.BandFormat.Uchar, im.Format);
            Assert.AreEqual(1, im.Bands);

            for (var i = 0; i < 100; i++)
            {
                var pixel = im.Getpoint(i, i);
                Assert.AreEqual(1, pixel.Length);
                Assert.AreEqual(0, pixel[0]);
            }

            im = Image.Black(100, 100, bands: 3);

            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(100, im.Height);
            Assert.AreEqual(Enums.BandFormat.Uchar, im.Format);
            Assert.AreEqual(3, im.Bands);

            for (var i = 0; i < 100; i++)
            {
                var pixel = im.Getpoint(i, i);
                Assert.AreEqual(3, pixel.Length);
                CollectionAssert.AreEqual(new[] {0, 0, 0}, pixel);
            }
        }

        [Test]
        public void TestBuildlut()
        {
            var m = Image.NewFromArray(new[,]
            {
                {0, 0},
                {255, 100}
            });
            var lut = m.Buildlut();
            Assert.AreEqual(256, lut.Width);
            Assert.AreEqual(1, lut.Height);
            Assert.AreEqual(1, lut.Bands);
            var p = lut.Getpoint(0, 0);
            Assert.AreEqual(0.0, p[0]);
            p = lut.Getpoint(255, 0);
            Assert.AreEqual(100.0, p[0]);
            p = lut.Getpoint(10, 0);
            Assert.AreEqual(100 * 10.0 / 255.0, p[0]);

            m = Image.NewFromArray(new[,]
            {
                {0, 0, 100},
                {255, 100, 0},
                {128, 10, 90}
            });
            lut = m.Buildlut();
            Assert.AreEqual(256, lut.Width);
            Assert.AreEqual(1, lut.Height);
            Assert.AreEqual(2, lut.Bands);
            p = lut.Getpoint(0, 0);
            CollectionAssert.AreEqual(new[] {0.0, 100.0}, p);
            p = lut.Getpoint(64, 0);
            CollectionAssert.AreEqual(new[] {5.0, 95.0}, p);
        }

        [Test]
        public void TestEye()
        {
            var im = Image.Eye(100, 90);

            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(90, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            Assert.AreEqual(1.0, im.Max());
            Assert.AreEqual(-1.0, im.Min());

            im = Image.Eye(100, 90, uchar: true);
            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(90, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Uchar, im.Format);
            Assert.AreEqual(255.0, im.Max());
            Assert.AreEqual(0.0, im.Min());
        }

        [Test]
        public void TestFractsurf()
        {
            var im = Image.Fractsurf(100, 90, 2.5);
            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(90, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
        }

        [Test]
        public void TestGaussmat()
        {
            var im = Image.Gaussmat(1, 0.1);
            Assert.AreEqual(5, im.Width);
            Assert.AreEqual(5, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Double, im.Format);
            Assert.AreEqual(20, im.Max());
            var total = im.Avg() * im.Width * im.Height;
            var scale = im.Get("scale");
            Assert.AreEqual(total, scale);
            var p = im.Getpoint(im.Width / 2, im.Height / 2);
            Assert.AreEqual(20.0, p[0]);

            im = Image.Gaussmat(1, 0.1, separable: true, precision: Enums.Precision.Float);
            Assert.AreEqual(5, im.Width);
            Assert.AreEqual(1, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Double, im.Format);
            Assert.AreEqual(1.0, im.Max());
            total = im.Avg() * im.Width * im.Height;
            scale = im.Get("scale");
            Assert.AreEqual(total, scale);
            p = im.Getpoint(im.Width / 2, im.Height / 2);
            Assert.AreEqual(p[0], 1.0);
        }

        [Test]
        public void TestGaussnoise()
        {
            var im = Image.Gaussnoise(100, 90);
            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(90, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);

            im = Image.Gaussnoise(100, 90, sigma: 10, mean: 100);
            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(90, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);

            var sigma = im.Deviate();
            var mean = im.Avg();

            Assert.AreEqual(10, sigma, 0.2);
            Assert.AreEqual(100, mean, 0.2);
        }

        [Test]
        public void TestGrey()
        {
            var im = Image.Grey(100, 90);
            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(90, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);

            var p = im.Getpoint(0, 0);
            Assert.AreEqual(0.0, p[0]);
            p = im.Getpoint(99, 0);
            Assert.AreEqual(1.0, p[0]);
            p = im.Getpoint(0, 89);
            Assert.AreEqual(0.0, p[0]);
            p = im.Getpoint(99, 89);
            Assert.AreEqual(1.0, p[0]);

            im = Image.Grey(100, 90, uchar: true);
            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(90, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Uchar, im.Format);

            p = im.Getpoint(0, 0);
            Assert.AreEqual(0, p[0]);
            p = im.Getpoint(99, 0);
            Assert.AreEqual(255, p[0]);
            p = im.Getpoint(0, 89);
            Assert.AreEqual(0, p[0]);
            p = im.Getpoint(99, 89);
            Assert.AreEqual(255, p[0]);
        }

        [Test]
        public void TestIdentity()
        {
            var im = Image.Identity();
            Assert.AreEqual(256, im.Width);
            Assert.AreEqual(1, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Uchar, im.Format);

            var p = im.Getpoint(0, 0);
            Assert.AreEqual(0.0, p[0]);
            p = im.Getpoint(255, 0);
            Assert.AreEqual(255.0, p[0]);
            p = im.Getpoint(128, 0);
            Assert.AreEqual(128.0, p[0]);

            im = Image.Identity(@ushort: true);
            Assert.AreEqual(65536, im.Width);
            Assert.AreEqual(1, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Ushort, im.Format);

            p = im.Getpoint(0, 0);
            Assert.AreEqual(0, p[0]);
            p = im.Getpoint(99, 0);
            Assert.AreEqual(99, p[0]);
            p = im.Getpoint(65535, 0);
            Assert.AreEqual(65535, p[0]);
        }

        [Test]
        public void TestInvertlut()
        {
            var lut = Image.NewFromArray(new[,]
            {
                {0.1, 0.2, 0.3, 0.1},
                {0.2, 0.4, 0.4, 0.2},
                {0.7, 0.5, 0.6, 0.3}
            });
            var im = lut.Invertlut();
            Assert.AreEqual(256, im.Width);
            Assert.AreEqual(1, im.Height);
            Assert.AreEqual(3, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Double, im.Format);

            var p = im.Getpoint(0, 0);
            CollectionAssert.AreEqual(new[] {0, 0, 0}, p);
            p = im.Getpoint(255, 0);
            CollectionAssert.AreEqual(new[] {1, 1, 1}, p);
            p = im.Getpoint((int) 0.2 * 255, 0);
            Assert.AreEqual(0.1, p[0], 0.1);
            p = im.Getpoint((int) 0.3 * 255, 0);
            Assert.AreEqual(0.1, p[1], 0.1);
            p = im.Getpoint((int) 0.1 * 255, 0);
            Assert.AreEqual(0.1, p[2], 0.1);
        }

        [Test]
        public void TestLogmat()
        {
            var im = Image.Logmat(1, 0.1);
            Assert.AreEqual(7, im.Width);
            Assert.AreEqual(7, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Double, im.Format);
            Assert.AreEqual(20, im.Max());

            var total = im.Avg() * im.Width * im.Height;
            var scale = im.Get("scale");
            Assert.AreEqual(total, scale);
            var p = im.Getpoint(im.Width / 2, im.Height / 2);
            Assert.AreEqual(20.0, p[0]);

            im = Image.Logmat(1, 0.1, separable: true, precision: Enums.Precision.Float);
            Assert.AreEqual(7, im.Width);
            Assert.AreEqual(1, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Double, im.Format);
            Assert.AreEqual(1.0, im.Max());
            total = im.Avg() * im.Width * im.Height;
            scale = im.Get("scale");
            Assert.AreEqual(total, scale);
            p = im.Getpoint(im.Width / 2, im.Height / 2);
            Assert.AreEqual(1.0, p[0]);
        }

        [Test]
        public void TestMaskButterworthBand()
        {
            var im = Image.MaskButterworthBand(128, 128, 2, 0.5, 0.5, 0.7, 0.1);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            Assert.AreEqual(1, im.Max(), 0.01);
            var p = im.Getpoint(32, 32);
            Assert.AreEqual(1.0, p[0]);

            im = Image.MaskButterworthBand(128, 128, 2, 0.5, 0.5, 0.7, 0.1, uchar: true, optical: true);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Uchar, im.Format);
            Assert.AreEqual(255, im.Max());
            p = im.Getpoint(32, 32);
            Assert.AreEqual(255.0, p[0]);
            p = im.Getpoint(64, 64);
            Assert.AreEqual(255.0, p[0]);

            im = Image.MaskButterworthBand(128, 128, 2, 0.5, 0.5, 0.7, 0.1, uchar: true, optical: true, nodc: true);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Uchar, im.Format);
            Assert.AreEqual(255, im.Max());
            p = im.Getpoint(32, 32);
            Assert.AreEqual(255.0, p[0]);
            p = im.Getpoint(64, 64);
            Assert.AreNotEqual(255.0, p[0]);
        }

        [Test]
        public void TestMaskButterworth()
        {
            var im = Image.MaskButterworth(128, 128, 2, 0.7, 0.1, nodc: true);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            Assert.AreEqual(0, im.Min(), 0.01);
            var p = im.Getpoint(0, 0);
            Assert.AreEqual(0.0, p[0]);
            var maxPos = im.MaxPos();
            var x = maxPos[1];
            var y = maxPos[2];
            Assert.AreEqual(64, x);
            Assert.AreEqual(64, y);

            im = Image.MaskButterworth(128, 128, 2, 0.7, 0.1, optical: true, uchar: true);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Uchar, im.Format);
            Assert.AreEqual(0, im.Min(), 0.01);
            p = im.Getpoint(64, 64);
            Assert.AreEqual(255, p[0]);
        }

        [Test]
        public void TestMaskButterworthRing()
        {
            var im = Image.MaskButterworthRing(128, 128, 2, 0.7, 0.1, 0.5, nodc: true);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            var p = im.Getpoint(45, 0);
            Assert.AreEqual(1.0, p[0], 0.0001);

            var minPos = im.MinPos();
            var x = minPos[1];
            var y = minPos[2];
            Assert.AreEqual(64, x);
            Assert.AreEqual(64, y);
        }

        [Test]
        public void TestMaskFractal()
        {
            var im = Image.MaskFractal(128, 128, 2.3);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
        }

        [Test]
        public void TestMaskGaussianBand()
        {
            var im = Image.MaskGaussianBand(128, 128, 0.5, 0.5, 0.7, 0.1);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            Assert.AreEqual(1, im.Max(), 0.01);
            var p = im.Getpoint(32, 32);
            Assert.AreEqual(1.0, p[0]);
        }

        [Test]
        public void TestMaskGaussian()
        {
            var im = Image.MaskGaussian(128, 128, 0.7, 0.1, nodc: true);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            Assert.AreEqual(0, im.Min(), 0.01);
            var p = im.Getpoint(0, 0);
            Assert.AreEqual(0.0, p[0]);
        }

        [Test]
        public void TestMaskGaussianRing()
        {
            var im = Image.MaskGaussianRing(128, 128, 0.7, 0.1, 0.5, nodc: true);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            var p = im.Getpoint(45, 0);
            Assert.AreEqual(1.0, p[0], 0.001);
        }

        [Test]
        public void TestMaskIdealBand()
        {
            var im = Image.MaskIdealBand(128, 128, 0.5, 0.5, 0.7);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            Assert.AreEqual(1, im.Max(), 0.01);
            var p = im.Getpoint(32, 32);
            Assert.AreEqual(1.0, p[0]);
        }

        [Test]
        public void TestMaskIdeal()
        {
            var im = Image.MaskIdeal(128, 128, 0.7, nodc: true);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            Assert.AreEqual(0, im.Min(), 0.01);
            var p = im.Getpoint(0, 0);
            Assert.AreEqual(0.0, p[0]);
        }

        [Test]
        public void TestMaskGaussianRing2()
        {
            var im = Image.MaskIdealRing(128, 128, 0.7, 0.5, nodc: true);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
            var p = im.Getpoint(45, 0);
            Assert.AreEqual(1.0, p[0], 0.001);
        }

        [Test]
        public void TestSines()
        {
            var im = Image.Sines(128, 128);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
        }

        [Test]
        public void TestText()
        {
            if (!Helper.Have("text"))
            {
                Console.WriteLine("no text in this vips, skipping test");
                Assert.Ignore();
            }

            var im = Image.Text("Hello, world!");
            Assert.IsTrue(im.Width > 10);
            Assert.IsTrue(im.Height > 10);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Uchar, im.Format);
            Assert.AreEqual(255, im.Max());
            Assert.AreEqual(0, im.Min());
        }

        [Test]
        public void TestTonelut()
        {
            var im = Image.Tonelut();
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Ushort, im.Format);
            Assert.AreEqual(32768, im.Width);
            Assert.AreEqual(1, im.Height);
            Assert.IsTrue(im.HistIsmonotonic());
        }

        [Test]
        public void TestXyz()
        {
            var im = Image.Xyz(128, 128);
            Assert.AreEqual(2, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Uint, im.Format);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            var p = im.Getpoint(45, 35);
            CollectionAssert.AreEqual(new[] {45, 35}, p);
        }

        [Test]
        public void TestZone()
        {
            var im = Image.Zone(128, 128);
            Assert.AreEqual(128, im.Width);
            Assert.AreEqual(128, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
        }

        [Test]
        public void TestWorley()
        {
            if (!Helper.Have("worley"))
            {
                Console.WriteLine("no worley, skipping test");
                Assert.Ignore();
            }

            var im = Image.Worley(512, 512);
            Assert.AreEqual(512, im.Width);
            Assert.AreEqual(512, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
        }

        [Test]
        public void TestPerlin()
        {
            if (!Helper.Have("perlin"))
            {
                Console.WriteLine("no perlin, skipping test");
                Assert.Ignore();
            }

            var im = Image.Perlin(512, 512);
            Assert.AreEqual(512, im.Width);
            Assert.AreEqual(512, im.Height);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(Enums.BandFormat.Float, im.Format);
        }
    }
}