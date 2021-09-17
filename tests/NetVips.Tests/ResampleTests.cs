namespace NetVips.Tests
{
    using System;
    using System.IO;
    using Xunit;
    using Xunit.Abstractions;

    public class ResampleTests : IClassFixture<TestsFixture>
    {
        public ResampleTests(TestsFixture testsFixture, ITestOutputHelper output)
        {
            testsFixture.SetUpLogging(output);
        }

        #region helpers

        /// <summary>
        /// Run a function expecting a complex image on a two-band image
        /// </summary>
        /// <param name="func"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public Image RunCmplx(Func<Image, Image> func, Image image)
        {
            Enums.BandFormat newFormat;
            switch (image.Format)
            {
                case Enums.BandFormat.Float:
                    newFormat = Enums.BandFormat.Complex;
                    break;
                case Enums.BandFormat.Double:
                    newFormat = Enums.BandFormat.Dpcomplex;
                    break;
                default:
                    throw new Exception("run_cmplx: not float or double");
            }

            // tag as complex, run, revert tagging
            var cmplx = image.Copy(bands: 1, format: newFormat);
            var cmplxResult = func(cmplx);

            return cmplxResult.Copy(bands: 2, format: image.Format);
        }

        /// <summary>
        /// Transform image coordinates to polar
        /// </summary>
        /// <remarks>
        /// The image is transformed so that it is wrapped around a point in the
        /// centre. Vertical straight lines become circles or segments of circles,
        /// horizontal straight lines become radial spokes.
        /// </remarks>
        /// <param name="image"></param>
        /// <returns></returns>
        public Image ToPolar(Image image)
        {
            // xy image, zero in the centre, scaled to fit image to a circle
            var xy = Image.Xyz(image.Width, image.Height);
            xy -= new[]
            {
                image.Width / 2.0,
                image.Height / 2.0
            };
            var scale = Math.Min(image.Width, image.Height) / Convert.ToDouble(image.Width);
            xy *= 2.0 / scale;

            // to polar, scale vertical axis to 360 degrees
            var index = RunCmplx(x => x.Polar(), xy);
            index *= new[]
            {
                1,
                image.Height / 360.0
            };

            return image.Mapim(index);
        }

        /// <summary>
        /// Transform image coordinates to rectangular.
        /// </summary>
        /// <remarks>
        /// The image is transformed so that it is unwrapped from a point in the
        /// centre. Circles or segments of circles become vertical straight lines,
        /// radial lines become horizontal lines.
        /// </remarks>
        /// <param name="image"></param>
        /// <returns></returns>
        public Image ToRectangular(Image image)
        {
            // xy image, vertical scaled to 360 degrees
            var xy = Image.Xyz(image.Width, image.Height);
            xy *= new[]
            {
                1,
                360.0 / image.Height
            };

            // to rect, scale to image rect
            var index = RunCmplx(x => x.Rect(), xy);
            var scale = Math.Min(image.Width, image.Height) / Convert.ToDouble(image.Width);
            index *= scale / 2.0;
            index += new[]
            {
                image.Width / 2.0,
                image.Height / 2.0
            };

            return image.Mapim(index);
        }

        #endregion

        [Fact]
        public void TestAffine()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            // vsqbs is non-interpolatory, don't test this way
            foreach (var name in new[] { "nearest", "bicubic", "bilinear", "nohalo", "lbb" })
            {
                var x = im;
                var interpolate = Interpolate.NewFromName(name);
                for (var i = 0; i < 4; i++)
                {
                    x = x.Affine(new double[] { 0, 1, 1, 0 }, interpolate: interpolate);
                }

                Assert.Equal(0, (x - im).Abs().Max());
            }
        }

        [Fact]
        public void TestReduce()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            // cast down to 0-127, the smallest range, so we aren't messed up by
            // clipping
            im = im.Cast(Enums.BandFormat.Char);

            foreach (var fac in new[] { 1, 1.1, 1.5, 1.999 })
            {
                foreach (var fmt in Helper.AllFormats)
                {
                    foreach (var kernel in new[]
                    {
                        Enums.Kernel.Nearest,
                        Enums.Kernel.Linear,
                        Enums.Kernel.Cubic,
                        Enums.Kernel.Lanczos2,
                        Enums.Kernel.Lanczos3
                    })
                    {
                        var x = im.Cast(fmt);
                        var r = x.Reduce(fac, fac, kernel: kernel);

                        var d = Math.Abs(r.Avg() - im.Avg());
                        Assert.True(d < 2);
                    }
                }
            }

            // try constant images ... should not change the constant
            foreach (var @const in new[] { 0, 1, 2, 254, 255 })
            {
                im = (Image.Black(10, 10) + @const).Cast(Enums.BandFormat.Uchar);
                foreach (var kernel in new[]
                {
                    Enums.Kernel.Nearest,
                    Enums.Kernel.Linear,
                    Enums.Kernel.Cubic,
                    Enums.Kernel.Lanczos2,
                    Enums.Kernel.Lanczos3
                })
                {
                    // Console.WriteLine($"testing kernel = {kernel}");
                    // Console.WriteLine($"testing const = {@const}");
                    var shr = im.Reduce(2, 2, kernel: kernel);
                    var d = Math.Abs(shr.Avg() - im.Avg());
                    Assert.Equal(0, d);
                }
            }
        }

        [Fact]
        public void TestResize()
        {
            var im = Image.NewFromFile(Helper.JpegFile);
            var im2 = im.Resize(0.25);
            Assert.Equal(Math.Round(im.Width / 4.0), im2.Width);
            Assert.Equal(Math.Round(im.Height / 4.0), im2.Height);

            // test geometry rounding corner case
            im = Image.Black(100, 1);
            var x = im.Resize(0.5);
            Assert.Equal(50, x.Width);
            Assert.Equal(1, x.Height);
        }

        [Fact]
        public void TestShrink()
        {
            var im = Image.NewFromFile(Helper.JpegFile);
            var im2 = im.Shrink(4, 4);
            Assert.Equal(Math.Round(im.Width / 4.0), im2.Width);
            Assert.Equal(Math.Round(im.Height / 4.0), im2.Height);
            Assert.True(Math.Abs(im.Avg() - im2.Avg()) < 1);

            im2 = im.Shrink(2.5, 2.5);
            Assert.Equal(Math.Round(im.Width / 2.5), im2.Width);
            Assert.Equal(Math.Round(im.Height / 2.5), im2.Height);
            Assert.True(Math.Abs(im.Avg() - im2.Avg()) < 1);
        }

        [SkippableFact]
        public void TestThumbnail()
        {
            Skip.IfNot(NetVips.AtLeastLibvips(8, 5), "requires libvips >= 8.5");

            var im = Image.Thumbnail(Helper.JpegFile, 100);
            Assert.Equal(100, im.Width);
            Assert.Equal(3, im.Bands);

            // the average shouldn't move too much
            var imOrig = Image.NewFromFile(Helper.JpegFile);
            Assert.True(Math.Abs(imOrig.Avg() - im.Avg()) < 1);

            // make sure we always get the right width
            for (var width = 1000; width >= 1; width -= 13)
            {
                im = Image.Thumbnail(Helper.JpegFile, width);
                Assert.Equal(width, im.Width);
            }

            // should fit one of width or height
            im = Image.Thumbnail(Helper.JpegFile, 100, height: 300);
            Assert.Equal(100, im.Width);
            Assert.NotEqual(300, im.Height);
            im = Image.Thumbnail(Helper.JpegFile, 300, height: 100);
            Assert.NotEqual(300, im.Width);
            Assert.Equal(100, im.Height);

            // with @crop, should fit both width and height
            im = Image.Thumbnail(Helper.JpegFile, 100, height: 300, crop: Enums.Interesting.Centre);
            Assert.Equal(100, im.Width);
            Assert.Equal(300, im.Height);

            var im1 = Image.Thumbnail(Helper.JpegFile, 100);
            var buf = File.ReadAllBytes(Helper.JpegFile);
            var im2 = Image.ThumbnailBuffer(buf, 100);
            Assert.True(Math.Abs(im1.Avg() - im2.Avg()) < 1);

            // OME-TIFF subifd thumbnail support added in 8.10
            if (NetVips.AtLeastLibvips(8, 10))
            {
                // should be able to thumbnail many-page tiff
                im = Image.Thumbnail(Helper.OmeFile, 100);
                Assert.Equal(100, im.Width);
                Assert.Equal(38, im.Height);

                // should be able to thumbnail individual pages from many-page tiff
                im = Image.Thumbnail(Helper.OmeFile + "[page=0]", 100);
                Assert.Equal(100, im.Width);
                Assert.Equal(38, im.Height);
                im2 = Image.Thumbnail(Helper.OmeFile + "[page=1]", 100);
                Assert.Equal(100, im2.Width);
                Assert.Equal(38, im2.Height);
                Assert.True((im - im2).Abs().Max() != 0);

                // should be able to thumbnail entire many-page tiff as a toilet-roll
                // image
                im = Image.Thumbnail(Helper.OmeFile + "[n=-1]", 100);
                Assert.Equal(100, im.Width);
                Assert.Equal(570, im.Height);

                if (Helper.Have("heifload"))
                {
                    // this image is orientation 6 ... thumbnail should flip it
                    var thumb = Image.Thumbnail(Helper.AvifFile, 100);

                    // thumb should be portrait
                    Assert.True(thumb.Width < thumb.Height);
                    Assert.Equal(100, thumb.Height);
                }
            }
        }

        [Fact]
        public void TestSimilarity()
        {
            var im = Image.NewFromFile(Helper.JpegFile);
            var im2 = im.Similarity(angle: 90);
            var im3 = im.Affine(new double[] { 0, -1, 1, 0 });

            // rounding in calculating the affine transform from the angle stops
            // this being exactly true
            Assert.True((im2 - im3).Abs().Max() < 50);
        }

        [Fact]
        public void TestSimilarityScale()
        {
            var im = Image.NewFromFile(Helper.JpegFile);
            var im2 = im.Similarity(scale: 2);
            var im3 = im.Affine(new double[] { 2, 0, 0, 2 });
            Assert.Equal(0, (im2 - im3).Abs().Max());
        }

        [SkippableFact]
        public void TestRotate()
        {
            // added in 8.7
            Skip.IfNot(Helper.Have("rotate"), "no rotate support in this vips, skipping test");

            var im = Image.NewFromFile(Helper.JpegFile);
            var im2 = im.Rotate(90);
            var im3 = im.Affine(new double[] { 0, -1, 1, 0 });
            // rounding in calculating the affine transform from the angle stops
            // this being exactly true
            Assert.True((im2 - im3).Abs().Max() < 50);
        }

        [Fact]
        public void TestMapim()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            var p = ToPolar(im);
            var r = ToRectangular(p);

            // the left edge (which is squashed to the origin) will be badly
            // distorted, but the rest should not be too bad
            var a = r.Crop(50, 0, im.Width - 50, im.Height).Gaussblur(2);
            var b = im.Crop(50, 0, im.Width - 50, im.Height).Gaussblur(2);
            Assert.True((a - b).Abs().Max() < 20);

            // this was a bug at one point, strangely, if executed with debug
            // enabled
            // fixed in 8.7.3
            if (NetVips.AtLeastLibvips(8, 7, 3))
            {
                var mp = Image.Xyz(im.Width, im.Height);
                var interp = Interpolate.NewFromName("bicubic");
                Assert.Equal(im.Avg(), im.Mapim(mp, interp).Avg());
            }
        }
    }
}