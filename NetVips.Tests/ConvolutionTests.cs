using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NetVips.Tests
{
    [TestFixture]
    class ConvolutionTests
    {
        private Image _colour;
        private Image _mono;
        private Image[] _allImages;

        private Image _sharp;
        private Image _blur;
        private Image _line;
        private Image _sobel;
        private Image[] _allMasks;

        [SetUp]
        public void Init()
        {
            Base.VipsInit();

            var im = Image.MaskIdeal(100, 100, 0.5, reject: true, optical: true);
            _colour = im * new[] {1, 2, 3} + new[] {2, 3, 4};
            _colour = _colour.Copy(interpretation: Enums.Interpretation.Srgb);
            _mono = _colour[0];
            _mono = _mono.Copy(interpretation: Enums.Interpretation.Bw);
            _allImages = new[]
            {
                _mono,
                _colour
            };
            _sharp = Image.NewFromArray(new[,]
            {
                {-1, -1, -1},
                {-1, 16, -1},
                {-1, -1, -1}
            }, 8);
            _blur = Image.NewFromArray(new[,]
            {
                {1, 1, 1},
                {1, 1, 1},
                {1, 1, 1}
            }, 9);
            _line = Image.NewFromArray(new[,]
            {
                {1, 1, 1},
                {-2, -2, -2},
                {1, 1, 1}
            });
            _sobel = Image.NewFromArray(new[,]
            {
                {1, 2, 1},
                {0, 0, 0},
                {-1, -2, -1}
            });
            _allMasks = new[] {_sharp, _blur, _line, _sobel};
        }

        [TearDown]
        public void Dispose()
        {
        }

        #region helpers

        public object Conv(Image image, Image mask, int xPosition, int yPosition)
        {
            var s = new object[] {0.0};
            for (var x = 0; x < mask.Width; x++)
            {
                for (var y = 0; y < mask.Height; y++)
                {
                    var m = mask.Getpoint(x, y);
                    var i = image.Getpoint(x + xPosition, y + yPosition);
                    var p = Helper.RunFn2((dynamic a, dynamic b) => a * b, m, i);
                    s = (object[]) Helper.RunFn2((dynamic a, dynamic b) => a + b, s, p);
                }
            }

            return Helper.RunFn2((dynamic a, dynamic b) => a / b, s, mask.Get("scale"));
        }

        public object Compass(Image image, Image mask, int xPosition, int yPosition, int nRot,
            Func<object, object, object> func)
        {
            var acc = new List<object>();
            for (var i = 0; i < nRot; i++)
            {
                var result = Conv(image, mask, xPosition, yPosition);
                result = Helper.RunFn((dynamic a) => Math.Abs(a), result);
                acc.Add(result);
                mask = mask.Rot45();
            }

            return acc.Aggregate((a, b) => Helper.RunFn2(func, a, b));
        }

        #endregion

        [Test]
        public void TestConv()
        {
            foreach (var im in _allImages)
            {
                foreach (var msk in _allMasks)
                {
                    foreach (var prec in new[] {Enums.Precision.Integer, Enums.Precision.Float})
                    {
                        var convolved = im.Conv(msk, precision: prec);

                        var result = convolved.Getpoint(25, 50);
                        var @true = Conv(im, msk, 24, 49) as IEnumerable;
                        Helper.AssertAlmostEqualObjects(@true, result);

                        result = convolved.Getpoint(50, 50);
                        @true = Conv(im, msk, 49, 49) as IEnumerable;
                        Helper.AssertAlmostEqualObjects(@true, result);
                    }
                }
            }
        }

        [Test]
        public void TestConva()
        {
            // don't test conva, it's still not done
            Assert.Ignore();

            foreach (var im in _allImages)
            {
                foreach (var msk in _allMasks)
                {
                    Console.WriteLine("msk:");
                    msk.Matrixprint();
                    Console.WriteLine($"im.bands = {im.Bands}");

                    var convolved = im.Conv(msk, precision: Enums.Precision.Approximate);

                    var result = convolved.Getpoint(25, 50);
                    var @true = Conv(im, msk, 24, 49);
                    Console.WriteLine($"result = {result}, true = {@true}");
                    Helper.AssertLessThreshold(@true, result, 5);

                    result = convolved.Getpoint(50, 50);
                    @true = Conv(im, msk, 49, 49);
                    Console.WriteLine($"result = {result}, true = {@true}");
                    Helper.AssertLessThreshold(@true, result, 5);
                }
            }
        }

        [Test]
        public void TestCompass()
        {
            foreach (var im in _allImages)
            {
                foreach (var msk in _allMasks)
                {
                    foreach (var prec in new[] {Enums.Precision.Integer, Enums.Precision.Float})
                    {
                        for (var times = 1; times < 4; times++)
                        {
                            var convolved = im.Compass(msk, times: times, angle: Enums.Angle45.D45,
                                combine: Enums.Combine.Max, precision: prec);

                            var result = convolved.Getpoint(25, 50);
                            var @true =
                                Compass(im, msk, 24, 49, times,
                                    (dynamic a, dynamic b) => Math.Max(a, b)) as IEnumerable;
                            Helper.AssertAlmostEqualObjects(@true, result);
                        }
                    }
                }
            }

            foreach (var im in _allImages)
            {
                foreach (var msk in _allMasks)
                {
                    foreach (var prec in new[] {Enums.Precision.Integer, Enums.Precision.Float})
                    {
                        for (var times = 1; times < 4; times++)
                        {
                            var convolved = im.Compass(msk, times: times, angle: Enums.Angle45.D45,
                                combine: Enums.Combine.Sum, precision: prec);

                            var result = convolved.Getpoint(25, 50);
                            var @true = Compass(im, msk, 24, 49, times, (dynamic a, dynamic b) => a + b) as IEnumerable;
                            Helper.AssertAlmostEqualObjects(@true, result);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestConvsep()
        {
            foreach (var im in _allImages)
            {
                foreach (var prec in new[] {Enums.Precision.Integer, Enums.Precision.Float})
                {
                    var gmask = Image.Gaussmat(2, 0.1, precision: prec);
                    var gmaskSep = Image.Gaussmat(2, 0.1, separable: true, precision: prec);

                    Assert.AreEqual(gmask.Width, gmask.Height);
                    Assert.AreEqual(gmask.Width, gmaskSep.Width);
                    Assert.AreEqual(1, gmaskSep.Height);

                    var a = im.Conv(gmask, precision: prec);
                    var b = im.Convsep(gmaskSep, precision: prec);

                    var aPoint = a.Getpoint(25, 50);
                    var bPoint = b.Getpoint(25, 50);

                    Helper.AssertAlmostEqualObjects(aPoint, bPoint, 0.1);
                }
            }
        }

        [Test]
        public void TestFastcor()
        {
            foreach (var im in _allImages)
            {
                foreach (var fmt in Helper.NonComplexFormats)
                {
                    var small = im.ExtractArea(20, 45, 10, 10).Cast(fmt);
                    var cor = im.Fastcor(small);
                    var minPos = cor.MinPos();
                    var v = minPos[0];
                    var x = minPos[1];
                    var y = minPos[2];

                    Assert.AreEqual(0, v);
                    Assert.AreEqual(25, x);
                    Assert.AreEqual(50, y);
                }
            }
        }

        [Test]
        public void TestSpcor()
        {
            foreach (var im in _allImages)
            {
                foreach (var fmt in Helper.NonComplexFormats)
                {
                    var small = im.ExtractArea(20, 45, 10, 10).Cast(fmt);
                    var cor = im.Spcor(small);
                    var maxPos = cor.MaxPos();
                    var v = maxPos[0];
                    var x = maxPos[1];
                    var y = maxPos[2];

                    Assert.AreEqual(1.0, v);
                    Assert.AreEqual(25, x);
                    Assert.AreEqual(50, y);
                }
            }
        }

        [Test]
        public void TestGaussblur()
        {
            foreach (var im in _allImages)
            {
                foreach (var prec in new[] {Enums.Precision.Integer, Enums.Precision.Float})
                {
                    for (var i = 5; i < 10; i++)
                    {
                        var sigma = i / 5.0;
                        var gmask = Image.Gaussmat(sigma, 0.2, precision: prec);

                        var a = im.Conv(gmask, precision: prec);
                        var b = im.Gaussblur(sigma, minAmpl: 0.2, precision: prec);

                        var aPoint = a.Getpoint(25, 50);
                        var bPoint = b.Getpoint(25, 50);

                        Helper.AssertAlmostEqualObjects(aPoint, bPoint, 0.1);
                    }
                }
            }
        }

        [Test]
        public void TestSharpen()
        {
            foreach (var im in _allImages)
            {
                foreach (var fmt in Helper.NonComplexFormats)
                {
                    foreach (var sigma in new[] {0.5, 1, 1.5, 2})
                    {
                        var im2 = im.Cast(fmt);
                        var sharp = im2.Sharpen(sigma: sigma);

                        // hard to test much more than this
                        Assert.AreEqual(sharp.Width, im.Width);
                        Assert.AreEqual(sharp.Height, im.Height);

                        // if m1 and m2 are zero, sharpen should do nothing
                        sharp = im.Sharpen(sigma: sigma, m1: 0, m2: 0);
                        sharp = sharp.Colourspace(im.Interpretation);
                        // Console.WriteLine($"testing sig = {sigma}");
                        // Console.WriteLine($"testing fmt = {fmt}");
                        // Console.WriteLine($"max diff = {(im - sharp).Abs().Max()}");
                        Assert.AreEqual(0, (im - sharp).Abs().Max());
                    }
                }
            }
        }
    }
}