using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NetVips.Tests
{
    [TestFixture]
    class ConversionTests
    {
        private Image _image;
        private Image _colour;
        private Image _mono;
        private Image[] _allImages;

        [SetUp]
        public void Init()
        {
            Base.VipsInit();

            var im = Image.MaskIdeal(100, 100, 0.5, reject: true, optical: true);
            _colour = im * new[] {1, 2, 3} + new[] {2, 3, 4};
            _mono = _colour[1];
            _allImages = new[]
            {
                _mono,
                _colour
            };
            _image = Image.Jpegload(Helper.JpegFile);
        }

        [TearDown]
        public void Dispose()
        {
        }

        #region helpers

        /// <summary>
        /// run a function on an image,
        /// 50,50 and 10,10 should have different values on the test image
        /// don't loop over band elements
        /// </summary>
        /// <param name="im"></param>
        /// <param name="func"></param>
        public void RunImagePixels(Image im, Func<object, object> func)
        {
            Helper.RunCmp(im, 50, 50, func);
            Helper.RunCmp(im, 10, 10, func);
        }

        /// <summary>
        /// run a function on a pair of images
        /// 50,50 and 10,10 should have different values on the test image
        /// don't loop over band elements
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="func"></param>
        public void RunImagePixels2(Image left, Image right, Func<object, object, object> func)
        {
            Helper.RunCmp2(left, right, 50, 50, func);
            Helper.RunCmp2(left, right, 10, 10, func);
        }

        public void RunUnary(IEnumerable<Image> images, Func<object, object> func, string[] formats = null)
        {
            if (formats == null)
            {
                formats = Helper.AllFormats;
            }

            foreach (var x in images)
            {
                foreach (var y in formats)
                {
                    RunImagePixels(x.Cast(y), func);
                }
            }
        }

        public void RunBinary(IEnumerable<Image> images, Func<object, object, object> func, string[] formats = null)
        {
            if (formats == null)
            {
                formats = Helper.AllFormats;
            }

            foreach (var x in images)
            {
                foreach (var y in formats)
                {
                    foreach (var z in formats)
                    {
                        RunImagePixels2(x.Cast(y), x.Cast(z), func);
                    }
                }
            }
        }

        #endregion

        [Test]
        public void TestBandAnd()
        {
            dynamic BandAnd(dynamic x)
            {
                if (x is Image image)
                {
                    return image.BandAnd();
                }

                return ((IEnumerable<double>) x).Aggregate((a, b) => (int) a & (int) b);
            }

            RunUnary(_allImages, BandAnd, Helper.IntFormats);
        }

        [Test]
        public void TestBandOr()
        {
            dynamic BandOr(dynamic x)
            {
                if (x is Image image)
                {
                    return image.BandOr();
                }

                return ((IEnumerable<double>) x).Aggregate((a, b) => (int) a | (int) b);
            }

            RunUnary(_allImages, BandOr, Helper.IntFormats);
        }

        [Test]
        public void TestBandEor()
        {
            dynamic BandOr(dynamic x)
            {
                if (x is Image image)
                {
                    return image.BandEor();
                }

                return ((IEnumerable<double>) x).Aggregate((a, b) => (int) a ^ (int) b);
            }

            RunUnary(_allImages, BandOr, Helper.IntFormats);
        }

        [Test]
        public void TestBandJoin()
        {
            dynamic BandJoin(dynamic x, dynamic y)
            {
                if (x is Image left && y is Image right)
                {
                    return left.Bandjoin(right);
                }

                return ((IEnumerable<double>) x).Concat((IEnumerable<double>) y);
            }

            RunBinary(_allImages, BandJoin);
        }

        [Test]
        public void TestBandJoinConst()
        {
            var x = _colour.Bandjoin(1);

            Assert.AreEqual(4, x.Bands);
            Assert.AreEqual(1, x[3].Avg());

            x = _colour.Bandjoin(new[]
            {
                1,
                2
            });
            Assert.AreEqual(5, x.Bands);
            Assert.AreEqual(1, x[3].Avg());
            Assert.AreEqual(2, x[4].Avg());
        }

        [Test]
        public void TestBandMean()
        {
            dynamic BandMean(dynamic x)
            {
                if (x is Image image)
                {
                    return image.Bandmean();
                }

                return new[] {Math.Floor(((IEnumerable<double>) x).Sum() / x.Length)};
            }

            RunUnary(_allImages, BandMean, Helper.NonComplexFormats);
        }


        [Test]
        public void TestBandRank()
        {
            double[] Median(IEnumerable<double> x, IEnumerable<double> y)
            {
                var joined = x.Zip(y, (d, d1) => new[] {d, d1}).OrderBy(o => o[0]);

                return joined.Select(z => z[z.Length / 2]).ToArray();
            }

            dynamic BandRank(dynamic x, dynamic y)
            {
                if (x is Image left && y is Image right)
                {
                    return left.Bandrank(right);
                }

                return Median(x, y);
            }

            RunBinary(_allImages, BandRank, Helper.NonComplexFormats);

            // we can mix images and constants, and set the index arg
            var a = _mono.Bandrank(new[] {2}, index: 0);
            var b = (_mono < 2).Ifthenelse(_mono, 2);
            Assert.AreEqual(0, (a - b).Abs().Min());
        }

        [Test]
        public void TestCache()
        {
            dynamic Cache(dynamic x)
            {
                if (x is Image image)
                {
                    return image.Cache();
                }

                return x;
            }

            RunUnary(_allImages, Cache);
        }

        [Test]
        public void TestCopy()
        {
            var x = _colour.Copy(interpretation: Enums.Interpretation.Lab);
            Assert.AreEqual(Enums.Interpretation.Lab, x.Interpretation);
            x = _colour.Copy(xres: 42);
            Assert.AreEqual(42, x.Xres);
            x = _colour.Copy(yres: 42);
            Assert.AreEqual(42, x.Yres);
            x = _colour.Copy(xoffset: 42);
            Assert.AreEqual(42, x.Xoffset);
            x = _colour.Copy(yoffset: 42);
            Assert.AreEqual(42, x.Yoffset);
            x = _colour.Copy(coding: Enums.Coding.None);
            Assert.AreEqual(Enums.Coding.None, x.Coding);
        }

        [Test]
        public void TestBandfold()
        {
            var x = _mono.Bandfold();
            Assert.AreEqual(1, x.Width);
            Assert.AreEqual(_mono.Width, x.Bands);

            var y = x.Bandunfold();
            Assert.AreEqual(_mono.Width, y.Width);
            Assert.AreEqual(1, y.Bands);
            Assert.AreEqual(x.Avg(), y.Avg());

            x = _mono.Bandfold(factor: 2);
            Assert.AreEqual(_mono.Width / 2, x.Width);
            Assert.AreEqual(2, x.Bands);

            y = x.Bandunfold(factor: 2);
            Assert.AreEqual(_mono.Width, y.Width);
            Assert.AreEqual(1, y.Bands);
            Assert.AreEqual(x.Avg(), y.Avg());
        }

        [Test]
        public void TestByteswap()
        {
            var x = _mono.Cast("ushort");
            var y = x.Byteswap().Byteswap();
            Assert.AreEqual(x.Width, y.Width);
            Assert.AreEqual(x.Height, y.Height);
            Assert.AreEqual(x.Bands, y.Bands);
            Assert.AreEqual(x.Avg(), y.Avg());
        }

        [Test]
        public void TestEmbed()
        {
            foreach (var fmt in Helper.AllFormats)
            {
                var test = _colour.Cast(fmt);

                var im = test.Embed(20, 20, _colour.Width + 40, _colour.Height + 40);
                var pixel = im.Getpoint(10, 10);
                CollectionAssert.AreEqual(new[] {0, 0, 0}, pixel);
                pixel = im.Getpoint(30, 30);
                CollectionAssert.AreEqual(new[] {2, 3, 4}, pixel);
                pixel = im.Getpoint(im.Width - 10, im.Height - 10);
                CollectionAssert.AreEqual(new[] {0, 0, 0}, pixel);

                im = test.Embed(20, 20, _colour.Width + 40, _colour.Height + 40, extend: Enums.Extend.Copy);
                pixel = im.Getpoint(10, 10);
                CollectionAssert.AreEqual(new[] {2, 3, 4}, pixel);
                pixel = im.Getpoint(im.Width - 10, im.Height - 10);
                CollectionAssert.AreEqual(new[] {2, 3, 4}, pixel);

                im = test.Embed(20, 20, _colour.Width + 40, _colour.Height + 40, extend: Enums.Extend.Background,
                    background: new double[] {7, 8, 9});
                pixel = im.Getpoint(10, 10);
                CollectionAssert.AreEqual(new[] {7, 8, 9}, pixel);
                pixel = im.Getpoint(im.Width - 10, im.Height - 10);
                CollectionAssert.AreEqual(new[] {7, 8, 9}, pixel);

                im = test.Embed(20, 20, _colour.Width + 40, _colour.Height + 40, extend: Enums.Extend.White);

                pixel = im.Getpoint(10, 10);

                // uses 255 in all bytes of ints, 255.0 for float
                var pixelLongs = pixel.Select(x => Convert.ToInt64(x) & 255);
                CollectionAssert.AreEqual(new[] {255, 255, 255}, pixelLongs);
                pixel = im.Getpoint(im.Width - 10, im.Height - 10);
                pixelLongs = pixel.Select(x => Convert.ToInt64(x) & 255);
                CollectionAssert.AreEqual(new[] {255, 255, 255}, pixelLongs);
            }
        }

        [Test]
        public void TestGravity()
        {
            if (Base.TypeFind("VipsOperation", "gravity") == 0)
            {
                Console.WriteLine("no gravity in this vips, skipping test");
                Assert.Ignore();
            }

            var im = Image.Black(1, 1) + 255;
            var positions = new[]
            {
                new object[] {"centre", 1, 1},
                new object[] {"north", 1, 0},
                new object[] {"south", 1, 2},
                new object[] {"east", 2, 1},
                new object[] {"west", 0, 1},
                new object[] {"north-east", 2, 0},
                new object[] {"south-east", 2, 2},
                new object[] {"south-west", 0, 2},
                new object[] {"north-west", 0, 0}
            };

            foreach (var position in positions)
            {
                var direction = position[0] as string;
                var x = position[1] is int xInt ? xInt : 0;
                var y = position[2] is int yInt ? yInt : 0;
                var im2 = im.Gravity(direction, 3, 3);
                CollectionAssert.AreEqual(new[] {255}, im2.Getpoint(x, y));
                Assert.AreEqual(255.0 / 9.0, im2.Avg());
            }
        }

        [Test]
        public void TestExtract()
        {
            foreach (var fmt in Helper.AllFormats)
            {
                var test = _colour.Cast(fmt);

                var pixel = test.Getpoint(30, 30);
                CollectionAssert.AreEqual(new[] {2, 3, 4}, pixel);

                var sub = test.ExtractArea(25, 25, 10, 10);

                pixel = sub.Getpoint(5, 5);
                CollectionAssert.AreEqual(new[] {2, 3, 4}, pixel);

                sub = test.ExtractBand(1, n: 2);

                pixel = sub.Getpoint(30, 30);
                CollectionAssert.AreEqual(new[] {3, 4}, pixel);
            }
        }

        [Test]
        public void TestSlice()
        {
            var test = _colour;
            var bands = test.Bandsplit().Select(i => i.Avg()).ToArray();

            var x = test[0].Avg();
            Assert.AreEqual(bands[0], x);

            // [-1]
            x = test[test.Bands - 1].Avg();
            Assert.AreEqual(bands[2], x);

            // [1:3]
            x = test.ExtractBand(1, n: 2).Avg();
            Assert.AreEqual(bands.Skip(1).Take(2).Average(), x);

            // [1:-1]
            x = test.ExtractBand(1, n: test.Bands - 1).Avg();
            Assert.AreEqual(bands.Skip(1).Take(test.Bands - 1).Average(), x);

            // [:2]
            x = test.ExtractBand(0, n: 2).Avg();
            Assert.AreEqual(bands.Take(2).Average(), x);

            // [1:]
            x = test.ExtractBand(1, n: test.Bands - 1).Avg();
            Assert.AreEqual(bands.Skip(1).Take(test.Bands - 1).Average(), x);

            // [-1]
            x = test[test.Bands - 1].Avg();
            Assert.AreEqual(bands[test.Bands - 1], x);
        }

        [Test]
        public void TestCrop()
        {
            foreach (var fmt in Helper.AllFormats)
            {
                var test = _colour.Cast(fmt);
                var pixel = test.Getpoint(30, 30);
                CollectionAssert.AreEqual(new[] {2, 3, 4}, pixel);
                var sub = test.Crop(25, 25, 10, 10);
                pixel = sub.Getpoint(5, 5);
                CollectionAssert.AreEqual(new[] {2, 3, 4}, pixel);
            }
        }

        [Test]
        public void TestSmartcrop()
        {
            if (Base.TypeFind("VipsOperation", "smartcrop") == 0)
            {
                Console.WriteLine("no smartcrop, skipping test");
                Assert.Ignore();
            }

            var test = _image.Smartcrop(100, 100);
            Assert.AreEqual(100, test.Width);
            Assert.AreEqual(100, test.Height);
        }

        [Test]
        public void TestFalsecolour()
        {
            foreach (var fmt in Helper.AllFormats)
            {
                var test = _colour.Cast(fmt);

                var im = test.Falsecolour();

                Assert.AreEqual(test.Width, im.Width);
                Assert.AreEqual(test.Height, im.Height);
                Assert.AreEqual(3, im.Bands);

                var pixel = im.Getpoint(30, 30);
                CollectionAssert.AreEqual(new[] {20, 0, 41}, pixel);
            }
        }

        [Test]
        public void TestFlatten()
        {
            const int mx = 255;
            const double alpha = mx / 2.0;
            const double nalpha = mx - alpha;

            foreach (var fmt in Helper.UnsignedFormats
                .Concat(new[] {Enums.BandFormat.Short, Enums.BandFormat.Int})
                .Concat(Helper.FloatFormats))
            {
                var test = _colour.Bandjoin(alpha).Cast(fmt);
                var pixel = test.Getpoint(30, 30);

                var predict = pixel.Take(pixel.Length - 1)
                    .Select(x => Convert.ToInt32(x) * alpha / mx)
                    .ToArray();

                var im = test.Flatten();

                Assert.AreEqual(3, im.Bands);
                pixel = im.Getpoint(30, 30);
                foreach (var zip in pixel.Zip(predict, (d, d1) => new[] {d, d1}))
                {
                    var x = zip[0];
                    var y = zip[1];

                    // we use float arithetic for int and uint, so the rounding
                    // differs ... don't require huge accuracy
                    Assert.Less(Math.Abs(x - y), 2);
                }

                im = test.Flatten(background: new double[] {100, 100, 100});

                pixel = test.Getpoint(30, 30);
                predict = pixel.Take(pixel.Length - 1)
                    .Select(x => Convert.ToInt32(x) * alpha / mx + 100 * nalpha / mx)
                    .ToArray();

                Assert.AreEqual(3, im.Bands);
                pixel = im.Getpoint(30, 30);
                foreach (var zip in pixel.Zip(predict, (d, d1) => new[] {d, d1}))
                {
                    var x = zip[0];
                    var y = zip[1];

                    Assert.Less(Math.Abs(x - y), 2);
                }
            }
        }

        [Test]
        public void TestPremultiply()
        {
            const int mx = 255;
            const double alpha = mx / 2.0;

            foreach (var fmt in Helper.UnsignedFormats
                .Concat(new[] {Enums.BandFormat.Short, Enums.BandFormat.Int})
                .Concat(Helper.FloatFormats))
            {
                var test = _colour.Bandjoin(alpha).Cast(fmt);
                var pixel = test.Getpoint(30, 30);

                var predict = pixel.Take(pixel.Length - 1)
                    .Select(x => Convert.ToInt32(x) * alpha / mx)
                    .Concat(new[] {alpha})
                    .ToArray();

                var im = test.Premultiply();

                Assert.AreEqual(test.Bands, im.Bands);
                pixel = im.Getpoint(30, 30);
                foreach (var zip in pixel.Zip(predict, (d, d1) => new[] {d, d1}))
                {
                    var x = zip[0];
                    var y = zip[1];

                    // we use float arithetic for int and uint, so the rounding
                    // differs ... don't require huge accuracy
                    Assert.Less(Math.Abs(x - y), 2);
                }
            }
        }

        [Test]
        public void TestComposite()
        {
            if (Base.TypeFind("VipsConversion", "composite") == 0)
            {
                Console.WriteLine("no composite support, skipping test");
                Assert.Ignore();
            }

            // 50% transparent image
            var overlay = _colour.Bandjoin(128);
            var baseImage = _colour + 100;
            var comp = baseImage.Composite(overlay, Enums.BlendMode.Over);

            Helper.AssertAlmostEqualObjects(new[] {51.8, 52.8, 53.8, 255}, comp.Getpoint(0, 0), 0.1);
        }

        [Test]
        public void TestUnpremultiply()
        {
            const int mx = 255;
            const double alpha = mx / 2.0;

            foreach (var fmt in Helper.UnsignedFormats
                .Concat(new[] {Enums.BandFormat.Short, Enums.BandFormat.Int})
                .Concat(Helper.FloatFormats))
            {
                var test = _colour.Bandjoin(alpha).Cast(fmt);
                var pixel = test.Getpoint(30, 30);

                var predict = pixel.Take(pixel.Length - 1)
                    .Select(x => Convert.ToInt32(x) / (alpha / mx))
                    .Concat(new[] {alpha})
                    .ToArray();

                var im = test.Unpremultiply();

                Assert.AreEqual(test.Bands, im.Bands);
                pixel = im.Getpoint(30, 30);
                foreach (var zip in pixel.Zip(predict, (d, d1) => new[] {d, d1}))
                {
                    var x = zip[0];
                    var y = zip[1];

                    // we use float arithetic for int and uint, so the rounding
                    // differs ... don't require huge accuracy
                    Assert.Less(Math.Abs(x - y), 2);
                }
            }
        }

        [Test]
        public void TestFlip()
        {
            foreach (var fmt in Helper.AllFormats)
            {
                var test = _colour.Cast(fmt);

                var result = test.FlipHor();
                result = result.FlipVer();
                result = result.FlipHor();
                result = result.FlipVer();

                var diff = (test - result).Abs().Max();

                Assert.AreEqual(0, diff);
            }
        }

        [Test]
        public void TestGamma()
        {
            const double exponent = 2.4;
            foreach (var fmt in Helper.NonComplexFormats)
            {
                var mx = Helper.MaxValue[fmt];
                var test = (_colour + mx / 2.0).Cast(fmt);

                var norm = Math.Pow(mx, exponent) / mx;
                var result = test.Gamma();
                var before = test.Getpoint(30, 30);
                var after = result.Getpoint(30, 30);
                var predict = before.Select(x => Math.Pow(x, exponent) / norm);
                foreach (var zip in after.Zip(predict, (d, d1) => new[] {d, d1}))
                {
                    var a = zip[0];
                    var b = zip[1];

                    // ie. less than 1% error, rounding on 7-bit images
                    // means this is all we can expect
                    Assert.Less(Math.Abs(a - b), mx / 100.0);
                }
            }

            const double exponent2 = 1.2;
            foreach (var fmt in Helper.NonComplexFormats)
            {
                var mx = Helper.MaxValue[fmt];
                var test = (_colour + mx / 2.0).Cast(fmt);

                var norm = Math.Pow(mx, exponent2) / mx;
                var result = test.Gamma(exponent: 1.0 / exponent2);
                var before = test.Getpoint(30, 30);
                var after = result.Getpoint(30, 30);
                var predict = before.Select(x => Math.Pow(x, exponent2) / norm);
                foreach (var zip in after.Zip(predict, (d, d1) => new[] {d, d1}))
                {
                    var a = zip[0];
                    var b = zip[1];

                    // ie. less than 1% error, rounding on 7-bit images
                    // means this is all we can expect
                    Assert.Less(Math.Abs(a - b), mx / 100.0);
                }
            }
        }

        [Test]
        public void TestGrid()
        {
            var test = _colour.Replicate(1, 12);
            Assert.AreEqual(_colour.Width, test.Width);
            Assert.AreEqual(_colour.Height * 12, test.Height);

            foreach (var fmt in Helper.AllFormats)
            {
                var im = test.Cast(fmt);
                var result = im.Grid(test.Width, 3, 4);
                Assert.AreEqual(_colour.Width * 3, result.Width);
                Assert.AreEqual(_colour.Height * 4, result.Height);

                var before = im.Getpoint(10, 10);
                var after = result.Getpoint(10 + test.Width * 2, 10 + test.Width * 2);
                CollectionAssert.AreEqual(before, after);

                before = im.Getpoint(50, 50);
                after = result.Getpoint(50 + test.Width * 2, 50 + test.Width * 2);
                CollectionAssert.AreEqual(before, after);
            }
        }

        [Test]
        public void TestIfthenelse()
        {
            var test = _mono > 3;
            foreach (var x in Helper.AllFormats)
            {
                foreach (var y in Helper.AllFormats)
                {
                    var t = (_colour + 10).Cast(x);
                    var e = _colour.Cast(y);
                    var r = test.Ifthenelse(t, e);

                    Assert.AreEqual(_colour.Width, r.Width);
                    Assert.AreEqual(_colour.Height, r.Height);
                    Assert.AreEqual(_colour.Bands, r.Bands);

                    var predict = e.Getpoint(10, 10);
                    var result = r.Getpoint(10, 10);
                    CollectionAssert.AreEqual(predict, result);

                    predict = t.Getpoint(50, 50);
                    result = r.Getpoint(50, 50);
                    CollectionAssert.AreEqual(predict, result);
                }
            }

            test = _colour > 3;
            foreach (var x in Helper.AllFormats)
            {
                foreach (var y in Helper.AllFormats)
                {
                    var t = (_mono + 10).Cast(x);
                    var e = _mono.Cast(y);
                    var r = test.Ifthenelse(t, e);

                    Assert.AreEqual(_colour.Width, r.Width);
                    Assert.AreEqual(_colour.Height, r.Height);
                    Assert.AreEqual(_colour.Bands, r.Bands);

                    var cp = test.Getpoint(10, 10);
                    var tp = Enumerable.Repeat(t.Getpoint(10, 10)[0], 3).ToArray();
                    var ep = Enumerable.Repeat(e.Getpoint(10, 10)[0], 3).ToArray();
                    var predict = cp
                        .Zip(tp, (e1, e2) => new {e1, e2})
                        .Zip(ep, (z1, e3) => Tuple.Create(z1.e1, z1.e2, e3))
                        .Select(tuple => Convert.ToInt32(tuple.Item1) != 0 ? tuple.Item2 : tuple.Item3)
                        .ToArray();
                    var result = r.Getpoint(10, 10);
                    CollectionAssert.AreEqual(predict, result);

                    cp = test.Getpoint(50, 50);
                    tp = Enumerable.Repeat(t.Getpoint(50, 50)[0], 3).ToArray();
                    ep = Enumerable.Repeat(e.Getpoint(50, 50)[0], 3).ToArray();
                    predict = cp
                        .Zip(tp, (e1, e2) => new {e1, e2})
                        .Zip(ep, (z1, e3) => Tuple.Create(z1.e1, z1.e2, e3))
                        .Select(tuple => Convert.ToInt32(tuple.Item1) != 0 ? tuple.Item2 : tuple.Item3)
                        .ToArray();
                    result = r.Getpoint(50, 50);
                    CollectionAssert.AreEqual(predict, result);
                }
            }

            test = _colour > 3;
            foreach (var x in Helper.AllFormats)
            {
                foreach (var y in Helper.AllFormats)
                {
                    var t = (_mono + 10).Cast(x);
                    var e = _mono.Cast(y);
                    var r = test.Ifthenelse(t, e, blend: true);

                    Assert.AreEqual(_colour.Width, r.Width);
                    Assert.AreEqual(_colour.Height, r.Height);
                    Assert.AreEqual(_colour.Bands, r.Bands);

                    var result = r.Getpoint(10, 10);
                    CollectionAssert.AreEqual(new[] {3, 3, 13}, result);
                }
            }

            test = _mono > 3;
            var r2 = test.Ifthenelse(new[] {1, 2, 3}, _colour);
            Assert.AreEqual(_colour.Width, r2.Width);
            Assert.AreEqual(_colour.Height, r2.Height);
            Assert.AreEqual(_colour.Bands, r2.Bands);
            Assert.AreEqual(_colour.Format, r2.Format);
            Assert.AreEqual(_colour.Format, r2.Format);
            Assert.AreEqual(_colour.Interpretation, r2.Interpretation);
            var result2 = r2.Getpoint(10, 10);
            CollectionAssert.AreEqual(new[] {2, 3, 4}, result2);
            result2 = r2.Getpoint(50, 50);
            CollectionAssert.AreEqual(new[] {1, 2, 3}, result2);

            test = _mono;
            r2 = test.Ifthenelse(new[] {1, 2, 3}, _colour, blend: true);
            Assert.AreEqual(_colour.Width, r2.Width);
            Assert.AreEqual(_colour.Height, r2.Height);
            Assert.AreEqual(_colour.Bands, r2.Bands);
            Assert.AreEqual(_colour.Format, r2.Format);
            Assert.AreEqual(_colour.Format, r2.Format);
            Assert.AreEqual(_colour.Interpretation, r2.Interpretation);
            result2 = r2.Getpoint(10, 10);
            Helper.AssertAlmostEqualObjects(new double[] {2, 3, 4}, result2, 0.1);
            result2 = r2.Getpoint(50, 50);
            Helper.AssertAlmostEqualObjects(new[] {3.0, 4.9, 6.9}, result2, 0.1);
        }

        [Test]
        public void TestInsert()
        {
            foreach (var x in Helper.AllFormats)
            {
                foreach (var y in Helper.AllFormats)
                {
                    var main = _mono.Cast(x);
                    var sub = _colour.Cast(y);
                    var r = main.Insert(sub, 10, 10);

                    Assert.AreEqual(main.Width, r.Width);
                    Assert.AreEqual(main.Height, r.Height);
                    Assert.AreEqual(sub.Bands, r.Bands);

                    var a = r.Getpoint(10, 10);
                    var b = sub.Getpoint(0, 0);
                    CollectionAssert.AreEqual(a, b);

                    a = r.Getpoint(0, 0);
                    b = Enumerable.Repeat(main.Getpoint(0, 0)[0], 3).ToArray();
                    CollectionAssert.AreEqual(a, b);
                }
            }

            foreach (var x in Helper.AllFormats)
            {
                foreach (var y in Helper.AllFormats)
                {
                    var main = _mono.Cast(x);
                    var sub = _colour.Cast(y);
                    var r = main.Insert(sub, 10, 10, expand: true, background: new double[] {100});

                    Assert.AreEqual(main.Width + 10, r.Width);
                    Assert.AreEqual(main.Height + 10, r.Height);
                    Assert.AreEqual(sub.Bands, r.Bands);

                    var a = r.Getpoint(r.Width - 5, 5);
                    CollectionAssert.AreEqual(new[] {100, 100, 100}, a);
                }
            }
        }

        [Test]
        public void TestArrayjoin()
        {
            var maxWidth = 0;
            var maxHeight = 0;
            var maxBands = 0;
            foreach (var image in _allImages)
            {
                if (image.Width > maxWidth)
                {
                    maxWidth = image.Width;
                }

                if (image.Height > maxHeight)
                {
                    maxHeight = image.Height;
                }

                if (image.Bands > maxBands)
                {
                    maxBands = image.Bands;
                }
            }

            var im = Image.Arrayjoin(_allImages);
            Assert.AreEqual(maxWidth * _allImages.Length, im.Width);
            Assert.AreEqual(maxHeight, im.Height);
            Assert.AreEqual(maxBands, im.Bands);

            im = Image.Arrayjoin(_allImages, across: 1);

            Assert.AreEqual(maxWidth, im.Width);
            Assert.AreEqual(maxHeight * _allImages.Length, im.Height);
            Assert.AreEqual(maxBands, im.Bands);

            im = Image.Arrayjoin(_allImages, shim: 10);

            Assert.AreEqual(maxWidth * _allImages.Length + 10 * (_allImages.Length - 1), im.Width);
            Assert.AreEqual(maxHeight, im.Height);
            Assert.AreEqual(maxBands, im.Bands);
        }

        [Test]
        public void TestMsb()
        {
            foreach (var fmt in Helper.UnsignedFormats)
            {
                var mx = Helper.MaxValue[fmt];
                var size = Helper.SizeOfFormat[fmt];
                var test = (_colour + mx / 8.0).Cast(fmt);
                var im = test.Msb();

                var before = test.Getpoint(10, 10);
                var predict = before.Select(x => Convert.ToInt32(x) >> (size - 1) * 8).ToArray();
                var result = im.Getpoint(10, 10);
                CollectionAssert.AreEqual(predict, result);

                before = test.Getpoint(50, 50);
                predict = before.Select(x => Convert.ToInt32(x) >> (size - 1) * 8).ToArray();
                result = im.Getpoint(50, 50);
                CollectionAssert.AreEqual(predict, result);
            }

            foreach (var fmt in Helper.SignedFormats)
            {
                var mx = Helper.MaxValue[fmt];
                var size = Helper.SizeOfFormat[fmt];
                var test = (_colour + mx / 8.0).Cast(fmt);
                var im = test.Msb();

                var before = test.Getpoint(10, 10);
                var predict = before.Select(x => 128 + (Convert.ToInt32(x) >> (size - 1) * 8)).ToArray();
                var result = im.Getpoint(10, 10);
                CollectionAssert.AreEqual(predict, result);

                before = test.Getpoint(50, 50);
                predict = before.Select(x => 128 + (Convert.ToInt32(x) >> (size - 1) * 8)).ToArray();
                result = im.Getpoint(50, 50);
                CollectionAssert.AreEqual(predict, result);
            }

            foreach (var fmt in Helper.UnsignedFormats)
            {
                var mx = Helper.MaxValue[fmt];
                var size = Helper.SizeOfFormat[fmt];
                var test = (_colour + mx / 8.0).Cast(fmt);
                var im = test.Msb(band: 1);

                var before = test.Getpoint(10, 10)[1];
                var predict = Convert.ToInt32(before) >> (size - 1) * 8;
                var result = im.Getpoint(10, 10)[0];
                Assert.AreEqual(predict, result);

                before = test.Getpoint(50, 50)[1];
                predict = Convert.ToInt32(before) >> (size - 1) * 8;
                result = im.Getpoint(50, 50)[0];
                Assert.AreEqual(predict, result);
            }
        }

        [Test]
        public void TestRecomb()
        {
            var array = new[]
            {
                new[] {0.2, 0.5, 0.3}
            };

            dynamic Recomb(dynamic x)
            {
                if (x is Image image)
                {
                    return image.Recomb(Image.NewFromArray(array));
                }

                var sum = array[0]
                    .Zip((IEnumerable<double>) x, (d, o) => new[] {d, o})
                    .Select(zip => zip[0] * zip[1])
                    .Sum();

                return new[]
                {
                    sum
                };
            }

            RunUnary(new[] {_colour}, Recomb, Helper.NonComplexFormats);
        }

        [Test]
        public void TestReplicate()
        {
            foreach (var fmt in Helper.AllFormats)
            {
                var im = _colour.Cast(fmt);

                var test = im.Replicate(10, 10);
                Assert.AreEqual(_colour.Width * 10, test.Width);
                Assert.AreEqual(_colour.Height * 10, test.Height);

                var before = im.Getpoint(10, 10);
                var after = test.Getpoint(10 + im.Width * 2, 10 + im.Width * 2);
                CollectionAssert.AreEqual(before, after);

                before = im.Getpoint(50, 50);
                after = test.Getpoint(50 + im.Width * 2, 50 + im.Width * 2);
                CollectionAssert.AreEqual(before, after);
            }
        }

        [Test]
        public void TestRot45()
        {
            // test has a quarter-circle in the bottom right
            var test = _colour.ExtractArea(0, 0, 51, 51);
            foreach (var fmt in Helper.AllFormats)
            {
                var im = test.Cast(fmt);

                var im2 = im.Rot45();
                var before = im.Getpoint(50, 50);
                var after = im2.Getpoint(25, 50);
                CollectionAssert.AreEqual(before, after);

                foreach (var zip in Helper.Rot45Angles.Zip(Helper.Rot45AngleBonds, (s, s1) => new[] {s, s1}))
                {
                    var a = zip[0];
                    var b = zip[1];
                    im2 = im.Rot45(angle: a);
                    var after2 = im2.Rot45(angle: b);
                    var diff = (after2 - im).Abs().Max();
                    Assert.AreEqual(0, diff);
                }
            }
        }

        [Test]
        public void TestRot()
        {
            // test has a quarter-circle in the bottom right
            var test = _colour.ExtractArea(0, 0, 51, 51);
            foreach (var fmt in Helper.AllFormats)
            {
                var im = test.Cast(fmt);

                var im2 = im.Rot(Enums.Angle.D90);
                var before = im.Getpoint(50, 50);
                var after = im2.Getpoint(0, 50);
                CollectionAssert.AreEqual(before, after);

                foreach (var zip in Helper.RotAngles.Zip(Helper.RotAngleBonds, (s, s1) => new[] {s, s1}))
                {
                    var a = zip[0];
                    var b = zip[1];
                    im2 = im.Rot(a);
                    var after2 = im2.Rot(b);
                    var diff = (after2 - im).Abs().Max();
                    Assert.AreEqual(0, diff);
                }
            }
        }

        [Test]
        public void TestScaleImage()
        {
            foreach (var fmt in Helper.NonComplexFormats)
            {
                var test = _colour.Cast(fmt);

                var im = test.ScaleImage();
                Assert.AreEqual(255, im.Max());
                Assert.AreEqual(0, im.Min());

                im = test.ScaleImage(log: true);
                Assert.AreEqual(255, im.Max());
            }
        }

        [Test]
        public void TestSubsample()
        {
            foreach (var fmt in Helper.AllFormats)
            {
                var test = _colour.Cast(fmt);

                var im = test.Subsample(3, 3);
                Assert.AreEqual(test.Width / 3, im.Width);
                Assert.AreEqual(test.Height / 3, im.Height);

                var before = test.Getpoint(60, 60);
                var after = im.Getpoint(20, 20);
                CollectionAssert.AreEqual(before, after);
            }
        }

        [Test]
        public void TestZoom()
        {
            foreach (var fmt in Helper.AllFormats)
            {
                var test = _colour.Cast(fmt);

                var im = test.Zoom(3, 3);
                Assert.AreEqual(test.Width * 3, im.Width);
                Assert.AreEqual(test.Height * 3, im.Height);

                var before = test.Getpoint(50, 50);
                var after = im.Getpoint(150, 150);
                CollectionAssert.AreEqual(before, after);
            }
        }

        [Test]
        public void TestWrap()
        {
            foreach (var fmt in Helper.AllFormats)
            {
                var test = _colour.Cast(fmt);

                var im = test.Wrap();
                Assert.AreEqual(test.Width, im.Width);
                Assert.AreEqual(test.Height, im.Height);

                var before = test.Getpoint(0, 0);
                var after = im.Getpoint(50, 50);
                CollectionAssert.AreEqual(before, after);

                before = test.Getpoint(50, 50);
                after = im.Getpoint(0, 0);
                CollectionAssert.AreEqual(before, after);
            }
        }
    }
}