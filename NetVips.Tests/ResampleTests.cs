using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace NetVips.Tests
{
    [TestFixture]
    class ResampleTests
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

        #region helpers

        /// <summary>
        /// Run a function expecting a complex image on a two-band image
        /// </summary>
        /// <param name="func"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public Image RunCmplx(Func<Image, Image> func, Image image)
        {
            string newFormat;
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
            var cmplx = image.Copy(new Dictionary<string, object>
            {
                {"bands", 1},
                {"format", newFormat}
            });
            var cmplxResult = func(cmplx);

            return cmplxResult.Copy(new Dictionary<string, object>
            {
                {"bands", 2},
                {"format", image.Format}
            });
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
            index += new []
            {
                image.Width / 2.0,
                image.Height / 2.0
            };

            return image.Mapim(index);
        }

        #endregion

        [Test]
        public void TestAffine()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            // vsqbs is non-interpolatory, don't test this way
            foreach (var name in new[] {"nearest", "bicubic", "bilinear", "nohalo", "lbb"})
            {
                var x = im;
                var interpolate = Interpolate.NewFromName(name);
                for (var i = 0; i < 4; i++)
                {
                    x = x.Affine(new double[] {0, 1, 1, 0}, new Dictionary<string, object>
                    {
                        {"interpolate", interpolate}
                    });
                }

                Assert.AreEqual(0, (x - im).Abs().Max());
            }
        }

        [Test]
        public void TestReduce()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            // cast down to 0-127, the smallest range, so we aren't messed up by
            // clipping
            im = im.Cast(Enums.BandFormat.Char);

            foreach (var fac in new[] {1, 1.1, 1.5, 1.999})
            {
                foreach (var fmt in Helper.AllFormats)
                {
                    foreach (var kernel in new[] {"nearest", "linear", "cubic", "lanczos2", "lanczos3"})
                    {
                        var x = im.Cast(fmt);
                        var r = x.Reduce(fac, fac, new Dictionary<string, object>
                        {
                            {"kernel", kernel}
                        });

                        var d = Math.Abs(r.Avg() - im.Avg());
                        Assert.Less(d, 2);
                    }
                }
            }

            // try constant images ... should not change the constant
            foreach (var @const in new[] {0, 1, 2, 254, 255})
            {
                im = (Image.Black(10, 10) + @const).Cast("uchar");
                foreach (var kernel in new[] {"nearest", "linear", "cubic", "lanczos2", "lanczos3"})
                {
                    // Console.WriteLine($"testing kernel = {kernel}");
                    // Console.WriteLine($"testing const = {@const}");
                    var shr = im.Reduce(2, 2, new Dictionary<string, object>
                    {
                        {"kernel", kernel}
                    });
                    var d = Math.Abs(shr.Avg() - im.Avg());
                    Assert.AreEqual(0, d);
                }
            }
        }

        [Test]
        public void TestResize()
        {
            var im = Image.NewFromFile(Helper.JpegFile);
            var im2 = im.Resize(0.25);
            Assert.AreEqual(Math.Round(im.Width / 4.0), im2.Width);
            Assert.AreEqual(Math.Round(im.Height / 4.0), im2.Height);

            // test geometry rounding corner case
            im = Image.Black(100, 1);
            var x = im.Resize(0.5);
            Assert.AreEqual(50, x.Width);
            Assert.AreEqual(1, x.Height);
        }

        [Test]
        public void TestShrink()
        {
            var im = Image.NewFromFile(Helper.JpegFile);
            var im2 = im.Shrink(4, 4);
            Assert.AreEqual(Math.Round(im.Width / 4.0), im2.Width);
            Assert.AreEqual(Math.Round(im.Height / 4.0), im2.Height);
            Assert.IsTrue(Math.Abs(im.Avg() - im2.Avg()) < 1);

            im2 = im.Shrink(2.5, 2.5);
            Assert.AreEqual(Math.Round(im.Width / 2.5), im2.Width);
            Assert.AreEqual(Math.Round(im.Height / 2.5), im2.Height);
            Assert.IsTrue(Math.Abs(im.Avg() - im2.Avg()) < 1);
        }

        [Test]
        public void TestThumbnail()
        {
            if (!Base.AtLeastLibvips(8, 5))
            {
                Assert.Ignore();
            }

            var im = Image.Thumbnail(Helper.JpegFile, 100);
            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(3, im.Bands);

            // the average shouldn't move too much
            var imOrig = Image.NewFromFile(Helper.JpegFile);
            Assert.Less(Math.Abs(imOrig.Avg() - im.Avg()), 1);

            // make sure we always get the right width
            for (var width = 1000; width >= 1; width -= 13)
            {
                im = Image.Thumbnail(Helper.JpegFile, width);
                Assert.AreEqual(width, im.Width);
            }

            // should fit one of width or height
            im = Image.Thumbnail(Helper.JpegFile, 100, new Dictionary<string, object>
            {
                {"height", 300}
            });
            Assert.AreEqual(100, im.Width);
            Assert.AreNotEqual(300, im.Height);
            im = Image.Thumbnail(Helper.JpegFile, 300, new Dictionary<string, object>
            {
                {"height", 100}
            });
            Assert.AreNotEqual(300, im.Width);
            Assert.AreEqual(100, im.Height);

            // with @crop, should fit both width and height
            im = Image.Thumbnail(Helper.JpegFile, 100, new Dictionary<string, object>
            {
                {"height", 300},
                {"crop", true}
            });
            Assert.AreEqual(100, im.Width);
            Assert.AreEqual(300, im.Height);

            var im1 = Image.Thumbnail(Helper.JpegFile, 100);
            var buf = File.ReadAllBytes(Helper.JpegFile);
            var im2 = Image.ThumbnailBuffer(buf, 100);
            Assert.Less(Math.Abs(im1.Avg() - im2.Avg()), 1);
        }

        [Test]
        public void TestSimilarity()
        {
            var im = Image.NewFromFile(Helper.JpegFile);
            var im2 = im.Similarity(new Dictionary<string, object>
            {
                {"angle", 90}
            });
            var im3 = im.Affine(new double[] {0, -1, 1, 0});

            // rounding in calculating the affine transform from the angle stops
            // this being exactly true
            Assert.Less((im2 - im3).Abs().Max(), 50);
        }

        [Test]
        public void TestSimilarityScale()
        {
            var im = Image.NewFromFile(Helper.JpegFile);
            var im2 = im.Similarity(new Dictionary<string, object>
            {
                {"scale", 2}
            });
            var im3 = im.Affine(new double[] {2, 0, 0, 2});
            Assert.AreEqual(0, (im2 - im3).Abs().Max());
        }

        [Test]
        public void TestMapim()
        {
            var im = Image.NewFromFile(Helper.JpegFile);

            var p = ToPolar(im);
            var r = ToRectangular(p);

            // the left edge (which is squashed to the origin) will be badly
            // distorted, but the rest should not be too bad
            var a = r.Crop(50, 0, im.Width - 50, im.Height).Gaussblur(2);
            var b = im.Crop(50, 0, im.Width - 50, im.Height).Gaussblur(2);
            Assert.Less((a - b).Abs().Max(), 20);
        }
    }
}