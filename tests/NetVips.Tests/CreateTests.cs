using Xunit;

namespace NetVips.Tests
{
    public class CreateTests : IClassFixture<TestsFixture>
    {
        [Fact]
        public void TestBlack()
        {
            var im = Image.Black(100, 100);

            Assert.Equal(100, im.Width);
            Assert.Equal(100, im.Height);
            Assert.Equal(Enums.BandFormat.Uchar, im.Format);
            Assert.Equal(1, im.Bands);

            for (var i = 0; i < 100; i++)
            {
                var pixel = im.Getpoint(i, i);
                Assert.Single(pixel);
                Assert.Equal(0, pixel[0]);
            }

            im = Image.Black(100, 100, bands: 3);

            Assert.Equal(100, im.Width);
            Assert.Equal(100, im.Height);
            Assert.Equal(Enums.BandFormat.Uchar, im.Format);
            Assert.Equal(3, im.Bands);

            for (var i = 0; i < 100; i++)
            {
                var pixel = im.Getpoint(i, i);
                Assert.Equal(3, pixel.Length);
                Assert.Equal(new double[] {0, 0, 0}, pixel);
            }
        }

        [Fact]
        public void TestBuildlut()
        {
            var m = Image.NewFromArray(new[,]
            {
                {0, 0},
                {255, 100}
            });
            var lut = m.Buildlut();
            Assert.Equal(256, lut.Width);
            Assert.Equal(1, lut.Height);
            Assert.Equal(1, lut.Bands);
            var p = lut.Getpoint(0, 0);
            Assert.Equal(0.0, p[0]);
            p = lut.Getpoint(255, 0);
            Assert.Equal(100.0, p[0]);
            p = lut.Getpoint(10, 0);
            Assert.Equal(100 * 10.0 / 255.0, p[0]);

            m = Image.NewFromArray(new[,]
            {
                {0, 0, 100},
                {255, 100, 0},
                {128, 10, 90}
            });
            lut = m.Buildlut();
            Assert.Equal(256, lut.Width);
            Assert.Equal(1, lut.Height);
            Assert.Equal(2, lut.Bands);
            p = lut.Getpoint(0, 0);
            Assert.Equal(new[] {0.0, 100.0}, p);
            p = lut.Getpoint(64, 0);
            Assert.Equal(new[] {5.0, 95.0}, p);
        }

        [Fact]
        public void TestEye()
        {
            var im = Image.Eye(100, 90);

            Assert.Equal(100, im.Width);
            Assert.Equal(90, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            Assert.Equal(1.0, im.Max());
            Assert.Equal(-1.0, im.Min());

            im = Image.Eye(100, 90, uchar: true);
            Assert.Equal(100, im.Width);
            Assert.Equal(90, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Uchar, im.Format);
            Assert.Equal(255.0, im.Max());
            Assert.Equal(0.0, im.Min());
        }

        [SkippableFact]
        public void TestFractsurf()
        {
            Skip.IfNot(Helper.Have("fwfft"), "no FFTW support in this vips, skipping test");

            var im = Image.Fractsurf(100, 90, 2.5);
            Assert.Equal(100, im.Width);
            Assert.Equal(90, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
        }

        [Fact]
        public void TestGaussmat()
        {
            var im = Image.Gaussmat(1, 0.1);
            Assert.Equal(5, im.Width);
            Assert.Equal(5, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Double, im.Format);
            Assert.Equal(20, im.Max());
            var total = im.Avg() * im.Width * im.Height;
            var scale = im.Get("scale");
            Assert.Equal(total, scale);
            var p = im.Getpoint(im.Width / 2, im.Height / 2);
            Assert.Equal(20.0, p[0]);

            im = Image.Gaussmat(1, 0.1, separable: true, precision: Enums.Precision.Float);
            Assert.Equal(5, im.Width);
            Assert.Equal(1, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Double, im.Format);
            Assert.Equal(1.0, im.Max());
            total = im.Avg() * im.Width * im.Height;
            scale = im.Get("scale");
            Assert.Equal(total, scale);
            p = im.Getpoint(im.Width / 2, im.Height / 2);
            Assert.Equal(1.0, p[0]);
        }

        [Fact]
        public void TestGaussnoise()
        {
            var im = Image.Gaussnoise(100, 90);
            Assert.Equal(100, im.Width);
            Assert.Equal(90, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);

            im = Image.Gaussnoise(100, 90, sigma: 10, mean: 100);
            Assert.Equal(100, im.Width);
            Assert.Equal(90, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);

            var sigma = im.Deviate();
            var mean = im.Avg();

            Assert.Equal(10, sigma, 0);
            Assert.Equal(100, mean, 0);
        }

        [Fact]
        public void TestGrey()
        {
            var im = Image.Grey(100, 90);
            Assert.Equal(100, im.Width);
            Assert.Equal(90, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);

            var p = im.Getpoint(0, 0);
            Assert.Equal(0.0, p[0]);
            p = im.Getpoint(99, 0);
            Assert.Equal(1.0, p[0]);
            p = im.Getpoint(0, 89);
            Assert.Equal(0.0, p[0]);
            p = im.Getpoint(99, 89);
            Assert.Equal(1.0, p[0]);

            im = Image.Grey(100, 90, uchar: true);
            Assert.Equal(100, im.Width);
            Assert.Equal(90, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Uchar, im.Format);

            p = im.Getpoint(0, 0);
            Assert.Equal(0, p[0]);
            p = im.Getpoint(99, 0);
            Assert.Equal(255, p[0]);
            p = im.Getpoint(0, 89);
            Assert.Equal(0, p[0]);
            p = im.Getpoint(99, 89);
            Assert.Equal(255, p[0]);
        }

        [Fact]
        public void TestIdentity()
        {
            var im = Image.Identity();
            Assert.Equal(256, im.Width);
            Assert.Equal(1, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Uchar, im.Format);

            var p = im.Getpoint(0, 0);
            Assert.Equal(0.0, p[0]);
            p = im.Getpoint(255, 0);
            Assert.Equal(255.0, p[0]);
            p = im.Getpoint(128, 0);
            Assert.Equal(128.0, p[0]);

            im = Image.Identity(@ushort: true);
            Assert.Equal(65536, im.Width);
            Assert.Equal(1, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Ushort, im.Format);

            p = im.Getpoint(0, 0);
            Assert.Equal(0, p[0]);
            p = im.Getpoint(99, 0);
            Assert.Equal(99, p[0]);
            p = im.Getpoint(65535, 0);
            Assert.Equal(65535, p[0]);
        }

        [Fact]
        public void TestInvertlut()
        {
            var lut = Image.NewFromArray(new[,]
            {
                {0.1, 0.2, 0.3, 0.1},
                {0.2, 0.4, 0.4, 0.2},
                {0.7, 0.5, 0.6, 0.3}
            });
            var im = lut.Invertlut();
            Assert.Equal(256, im.Width);
            Assert.Equal(1, im.Height);
            Assert.Equal(3, im.Bands);
            Assert.Equal(Enums.BandFormat.Double, im.Format);

            var p = im.Getpoint(0, 0);
            Assert.Equal(new double[] {0, 0, 0}, p);
            p = im.Getpoint(255, 0);
            Assert.Equal(new double[] {1, 1, 1}, p);
            p = im.Getpoint((int) 0.2 * 255, 0);
            Assert.Equal(0, p[0], 1);
            p = im.Getpoint((int) 0.3 * 255, 0);
            Assert.Equal(0, p[1], 1);
            p = im.Getpoint((int) 0.1 * 255, 0);
            Assert.Equal(0, p[2], 1);
        }

        [Fact]
        public void TestLogmat()
        {
            var im = Image.Logmat(1, 0.1);
            Assert.Equal(7, im.Width);
            Assert.Equal(7, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Double, im.Format);
            Assert.Equal(20, im.Max());

            var total = im.Avg() * im.Width * im.Height;
            var scale = im.Get("scale");
            Assert.Equal(total, scale);
            var p = im.Getpoint(im.Width / 2, im.Height / 2);
            Assert.Equal(20.0, p[0]);

            im = Image.Logmat(1, 0.1, separable: true, precision: Enums.Precision.Float);
            Assert.Equal(7, im.Width);
            Assert.Equal(1, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Double, im.Format);
            Assert.Equal(1.0, im.Max());
            total = im.Avg() * im.Width * im.Height;
            scale = im.Get("scale");
            Assert.Equal(total, scale);
            p = im.Getpoint(im.Width / 2, im.Height / 2);
            Assert.Equal(1.0, p[0]);
        }

        [Fact]
        public void TestMaskButterworthBand()
        {
            var im = Image.MaskButterworthBand(128, 128, 2, 0.5, 0.5, 0.7, 0.1);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            Assert.Equal(1, im.Max(), 1);
            var p = im.Getpoint(32, 32);
            Assert.Equal(1.0, p[0]);

            im = Image.MaskButterworthBand(128, 128, 2, 0.5, 0.5, 0.7, 0.1, uchar: true, optical: true);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Uchar, im.Format);
            Assert.Equal(255, im.Max());
            p = im.Getpoint(32, 32);
            Assert.Equal(255.0, p[0]);
            p = im.Getpoint(64, 64);
            Assert.Equal(255.0, p[0]);

            im = Image.MaskButterworthBand(128, 128, 2, 0.5, 0.5, 0.7, 0.1, uchar: true, optical: true, nodc: true);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Uchar, im.Format);
            Assert.Equal(255, im.Max());
            p = im.Getpoint(32, 32);
            Assert.Equal(255.0, p[0]);
            p = im.Getpoint(64, 64);
            Assert.NotEqual(255.0, p[0]);
        }

        [Fact]
        public void TestMaskButterworth()
        {
            var im = Image.MaskButterworth(128, 128, 2, 0.7, 0.1, nodc: true);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            Assert.Equal(0, im.Min(), 2);
            var p = im.Getpoint(0, 0);
            Assert.Equal(0.0, p[0]);
            var maxPos = im.MaxPos();
            var x = maxPos[1];
            var y = maxPos[2];
            Assert.Equal(64, x);
            Assert.Equal(64, y);

            im = Image.MaskButterworth(128, 128, 2, 0.7, 0.1, optical: true, uchar: true);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Uchar, im.Format);
            Assert.Equal(0, im.Min(), 2);
            p = im.Getpoint(64, 64);
            Assert.Equal(255, p[0]);
        }

        [Fact]
        public void TestMaskButterworthRing()
        {
            var im = Image.MaskButterworthRing(128, 128, 2, 0.7, 0.1, 0.5, nodc: true);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            var p = im.Getpoint(45, 0);
            Assert.Equal(1.0, p[0], 4);

            var minPos = im.MinPos();
            var x = minPos[1];
            var y = minPos[2];
            Assert.Equal(64, x);
            Assert.Equal(64, y);
        }

        [Fact]
        public void TestMaskFractal()
        {
            var im = Image.MaskFractal(128, 128, 2.3);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
        }

        [Fact]
        public void TestMaskGaussianBand()
        {
            var im = Image.MaskGaussianBand(128, 128, 0.5, 0.5, 0.7, 0.1);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            Assert.Equal(1, im.Max(), 2);
            var p = im.Getpoint(32, 32);
            Assert.Equal(1.0, p[0]);
        }

        [Fact]
        public void TestMaskGaussian()
        {
            var im = Image.MaskGaussian(128, 128, 0.7, 0.1, nodc: true);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            Assert.Equal(0, im.Min(), 2);
            var p = im.Getpoint(0, 0);
            Assert.Equal(0.0, p[0]);
        }

        [Fact]
        public void TestMaskGaussianRing()
        {
            var im = Image.MaskGaussianRing(128, 128, 0.7, 0.1, 0.5, nodc: true);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            var p = im.Getpoint(45, 0);
            Assert.Equal(1.0, p[0], 3);
        }

        [Fact]
        public void TestMaskIdealBand()
        {
            var im = Image.MaskIdealBand(128, 128, 0.5, 0.5, 0.7);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            Assert.Equal(1, im.Max(), 2);
            var p = im.Getpoint(32, 32);
            Assert.Equal(1.0, p[0]);
        }

        [Fact]
        public void TestMaskIdeal()
        {
            var im = Image.MaskIdeal(128, 128, 0.7, nodc: true);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            Assert.Equal(0, im.Min(), 2);
            var p = im.Getpoint(0, 0);
            Assert.Equal(0.0, p[0]);
        }

        [Fact]
        public void TestMaskGaussianRing2()
        {
            var im = Image.MaskIdealRing(128, 128, 0.7, 0.5, nodc: true);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
            var p = im.Getpoint(45, 0);
            Assert.Equal(1.0, p[0], 3);
        }

        [Fact]
        public void TestSines()
        {
            var im = Image.Sines(128, 128);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
        }

        [SkippableFact]
        public void TestText()
        {
            Skip.IfNot(Helper.Have("text"), "no text in this vips, skipping test");

            var im = Image.Text("Hello, world!", dpi: 300);
            Assert.True(im.Width > 10);
            Assert.True(im.Height > 10);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Uchar, im.Format);
            Assert.Equal(255, im.Max());
            Assert.Equal(0, im.Min());
        }

        [Fact]
        public void TestTonelut()
        {
            var im = Image.Tonelut();
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Ushort, im.Format);
            Assert.Equal(32768, im.Width);
            Assert.Equal(1, im.Height);
            Assert.True(im.HistIsmonotonic());
        }

        [Fact]
        public void TestXyz()
        {
            var im = Image.Xyz(128, 128);
            Assert.Equal(2, im.Bands);
            Assert.Equal(Enums.BandFormat.Uint, im.Format);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            var p = im.Getpoint(45, 35);
            Assert.Equal(new double[] {45, 35}, p);
        }

        [Fact]
        public void TestZone()
        {
            var im = Image.Zone(128, 128);
            Assert.Equal(128, im.Width);
            Assert.Equal(128, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
        }

        [SkippableFact]
        public void TestWorley()
        {
            Skip.IfNot(Helper.Have("worley"), "no worley, skipping test");

            var im = Image.Worley(512, 512);
            Assert.Equal(512, im.Width);
            Assert.Equal(512, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
        }

        [SkippableFact]
        public void TestPerlin()
        {
            Skip.IfNot(Helper.Have("perlin"), "no perlin, skipping test");

            var im = Image.Perlin(512, 512);
            Assert.Equal(512, im.Width);
            Assert.Equal(512, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(Enums.BandFormat.Float, im.Format);
        }
    }
}