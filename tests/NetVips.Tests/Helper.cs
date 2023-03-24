namespace NetVips.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Xunit;

    public static class Helper
    {
        public static readonly string Images = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");

        public static readonly string JpegFile = Path.Combine(Images, "йцук.jpg");
        public static readonly string TruncatedFile = Path.Combine(Images, "truncated.jpg");
        public static readonly string SrgbFile = Path.Combine(Images, "sRGB.icm");
        public static readonly string MatlabFile = Path.Combine(Images, "sample.mat");
        public static readonly string PngFile = Path.Combine(Images, "sample.png");
        public static readonly string TifFile = Path.Combine(Images, "sample.tif");
        public static readonly string Tif1File = Path.Combine(Images, "1bit.tif");
        public static readonly string Tif2File = Path.Combine(Images, "2bit.tif");
        public static readonly string Tif4File = Path.Combine(Images, "4bit.tif");
        public static readonly string OmeFile = Path.Combine(Images, "multi-channel-z-series.ome.tif");
        public static readonly string AnalyzeFile = Path.Combine(Images, "t00740_tr1_segm.hdr");
        public static readonly string GifFile = Path.Combine(Images, "cramps.gif");
        public static readonly string WebpFile = Path.Combine(Images, "1.webp");
        public static readonly string ExrFile = Path.Combine(Images, "sample.exr");
        public static readonly string FitsFile = Path.Combine(Images, "WFPC2u5780205r_c0fx.fits");
        public static readonly string OpenslideFile = Path.Combine(Images, "CMU-1-Small-Region.svs");
        public static readonly string PdfFile = Path.Combine(Images, "ISO_12233-reschart.pdf");
        public static readonly string SvgFile = Path.Combine(Images, "logo.svg");
        public static readonly string SvgzFile = Path.Combine(Images, "logo.svgz");
        public static readonly string SvgGzFile = Path.Combine(Images, "logo.svg.gz");
        public static readonly string GifAnimFile = Path.Combine(Images, "cogs.gif");
        public static readonly string DicomFile = Path.Combine(Images, "dicom_test_image.dcm");
        public static readonly string BmpFile = Path.Combine(Images, "MARBLES.BMP");
        public static readonly string NiftiFile = Path.Combine(Images, "avg152T1_LR_nifti.nii.gz");
        public static readonly string IcoFile = Path.Combine(Images, "favicon.ico");
        public static readonly string AvifFile = Path.Combine(Images, "avif-orientation-6.avif");

        public static readonly Enums.BandFormat[] UnsignedFormats =
        {
            Enums.BandFormat.Uchar,
            Enums.BandFormat.Ushort,
            Enums.BandFormat.Uint
        };

        public static readonly Enums.BandFormat[] SignedFormats =
        {
            Enums.BandFormat.Char,
            Enums.BandFormat.Short,
            Enums.BandFormat.Int
        };

        public static readonly Enums.BandFormat[] FloatFormats =
        {
            Enums.BandFormat.Float,
            Enums.BandFormat.Double
        };

        public static readonly Enums.BandFormat[] ComplexFormats =
        {
            Enums.BandFormat.Complex,
            Enums.BandFormat.Dpcomplex
        };

        public static readonly Enums.BandFormat[] IntFormats = UnsignedFormats.Concat(SignedFormats).ToArray();

        public static readonly Enums.BandFormat[] NonComplexFormats = IntFormats.Concat(FloatFormats).ToArray();

        public static readonly Enums.BandFormat[] AllFormats = IntFormats.Concat(FloatFormats).Concat(ComplexFormats).ToArray();

        public static readonly Enums.Interpretation[] ColourColourspaces =
        {
            Enums.Interpretation.Xyz,
            Enums.Interpretation.Lab,
            Enums.Interpretation.Lch,
            Enums.Interpretation.Cmc,
            Enums.Interpretation.Labs,
            Enums.Interpretation.Scrgb,
            Enums.Interpretation.Hsv,
            Enums.Interpretation.Srgb,
            Enums.Interpretation.Yxy
        };

        public static readonly Enums.Interpretation[] CodedColourspaces =
        {
            Enums.Interpretation.Labq
        };

        public static readonly Enums.Interpretation[] MonoColourspaces =
        {
            Enums.Interpretation.Bw
        };

        public static readonly Enums.Interpretation[] SixteenbitColourspaces =
        {
            Enums.Interpretation.Grey16,
            Enums.Interpretation.Rgb16
        };

        public static readonly Enums.Interpretation[] CmykColourspaces =
        {
            Enums.Interpretation.Cmyk
        };

        public static Enums.Interpretation[] AllColourspaces = ColourColourspaces.Concat(MonoColourspaces)
            .Concat(CodedColourspaces)
            .Concat(SixteenbitColourspaces)
            .Concat(CmykColourspaces)
            .ToArray();

        public static readonly Dictionary<Enums.BandFormat, double> MaxValue = new Dictionary<Enums.BandFormat, double>
        {
            {
                Enums.BandFormat.Uchar,
                0xff
            },
            {
                Enums.BandFormat.Ushort,
                0xffff
            },
            {
                Enums.BandFormat.Uint,
                0xffffffff
            },
            {
                Enums.BandFormat.Char,
                0x7f
            },
            {
                Enums.BandFormat.Short,
                0x7fff
            },
            {
                Enums.BandFormat.Int,
                0x7fffffff
            },
            {
                Enums.BandFormat.Float,
                1.0
            },
            {
                Enums.BandFormat.Double,
                1.0
            },
            {
                Enums.BandFormat.Complex,
                1.0
            },
            {
                Enums.BandFormat.Dpcomplex,
                1.0
            }
        };

        public static readonly Dictionary<Enums.BandFormat, int> SizeOfFormat = new Dictionary<Enums.BandFormat, int>
        {
            {
                Enums.BandFormat.Uchar,
                1
            },
            {
                Enums.BandFormat.Ushort,
                2
            },
            {
                Enums.BandFormat.Uint,
                4
            },
            {
                Enums.BandFormat.Char,
                1
            },
            {
                Enums.BandFormat.Short,
                2
            },
            {
                Enums.BandFormat.Int,
                4
            },
            {
                Enums.BandFormat.Float,
                4
            },
            {
                Enums.BandFormat.Double,
                8
            },
            {
                Enums.BandFormat.Complex,
                8
            },
            {
                Enums.BandFormat.Dpcomplex,
                16
            }
        };

        public static readonly Enums.Angle45[] Rot45Angles =
        {
            Enums.Angle45.D0,
            Enums.Angle45.D45,
            Enums.Angle45.D90,
            Enums.Angle45.D135,
            Enums.Angle45.D180,
            Enums.Angle45.D225,
            Enums.Angle45.D270,
            Enums.Angle45.D315
        };

        public static readonly Enums.Angle45[] Rot45AngleBonds =
        {
            Enums.Angle45.D0,
            Enums.Angle45.D315,
            Enums.Angle45.D270,
            Enums.Angle45.D225,
            Enums.Angle45.D180,
            Enums.Angle45.D135,
            Enums.Angle45.D90,
            Enums.Angle45.D45
        };

        public static readonly Enums.Angle[] RotAngles =
        {
            Enums.Angle.D0,
            Enums.Angle.D90,
            Enums.Angle.D180,
            Enums.Angle.D270
        };

        public static readonly Enums.Angle[] RotAngleBonds =
        {
            Enums.Angle.D0,
            Enums.Angle.D270,
            Enums.Angle.D180,
            Enums.Angle.D90
        };

        /// <summary>
        /// an expanding zip ... if either of the args is a scalar or a one-element list,
        /// duplicate it down the other side
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IEnumerable<object[]> ZipExpand(object x, object y)
        {
            // handle singleton list case
            if (x is Array xArray && xArray.Length == 1)
            {
                x = xArray.GetValue(0);
            }

            if (y is Array yArray && yArray.Length == 1)
            {
                y = yArray.GetValue(0);
            }

            if (x is IEnumerable enumerable1 && y is IEnumerable enumerable2)
            {
                return enumerable1.Cast<object>()
                    .Zip(enumerable2.Cast<object>(), (xObj, yObj) => new[] { xObj, yObj })
                    .ToArray();
            }

            if (x is IEnumerable enumerableX)
            {
                return enumerableX.Cast<object>().Select(i => new[]
                {
                    i,
                    y
                }).ToArray();
            }

            if (y is IEnumerable enumerableY)
            {
                return enumerableY.Cast<object>().Select(j => new[]
                {
                    x,
                    j
                }).ToArray();
            }

            return new[]
            {
                new[] {x, y}
            };
        }

        /// <summary>
        /// run a 1-ary function on a thing -- loop over elements if the
        /// thing is a list
        /// </summary>
        /// <param name="func"></param>
        /// <param name="x"></param>
        public static object RunFn(Func<object, object> func, object x)
        {
            if (x is IEnumerable enumerable)
            {
                return enumerable.Cast<object>().Select(func).ToArray();
            }

            return func(x);
        }

        /// <summary>
        /// run a 2-ary function on two things -- loop over elements pairwise if the
        /// things are lists
        /// </summary>
        /// <param name="func"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static object RunFn2(Func<object, object, object> func, object x, object y)
        {
            if (x is Image || y is Image)
            {
                return func(x, y);
            }

            if (x is Array || y is Array)
            {
                return ZipExpand(x, y).Select(o => func(o[0], o[1])).ToArray();
            }

            return func(x, y);
        }

        /// <summary>
        /// run a function on an image and on a single pixel, the results
        /// should match
        /// </summary>
        /// <param name="im"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="func"></param>
        public static void RunCmp(Image im, int x, int y, Func<object, object> func)
        {
            var a = im[x, y];
            var v1 = func(a);
            var im2 = (Image)func(im);
            var v2 = im2[x, y];

            AssertAlmostEqualObjects(v1 is IEnumerable enumerable ? enumerable : new[] { v1 }, v2);
        }

        /// <summary>
        /// run a function on a pair of images and on a pair of pixels, the results
        /// should match
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static void RunCmp2(Image left, Image right, int x, int y, Func<object, object, object> func)
        {
            var a = left[x, y];
            var b = right[x, y];
            var v1 = func(a, b);
            var after = (Image)func(left, right);
            var v2 = after[x, y];

            AssertAlmostEqualObjects(v1 is IEnumerable enumerable ? enumerable : new[] { v1 }, v2);
        }

        /// <summary>
        /// run a function on a pair of images
        /// 50,50 and 10,10 should have different values on the test image
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static void RunImage2(Image left, Image right, Func<object, object, object> func)
        {
            RunCmp2(left, right, 50, 50, (x, y) => RunFn2(func, x, y));
            RunCmp2(left, right, 10, 10, (x, y) => RunFn2(func, x, y));
        }

        /// <summary>
        /// run a function on (image, constant), and on (constant, image).
        /// 50,50 and 10,10 should have different values on the test image
        /// </summary>
        public static void RunConst(Func<object, object, object> func, Image im, object c)
        {
            RunCmp(im, 50, 50, x => RunFn2(func, x, c));
            RunCmp(im, 50, 50, x => RunFn2(func, c, x));
            RunCmp(im, 10, 10, x => RunFn2(func, x, c));
            RunCmp(im, 10, 10, x => RunFn2(func, c, x));
        }

        /// <summary>
        /// test a pair of things which can be lists for approx. equality
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        public static void AssertAlmostEqualObjects(IEnumerable expected, IEnumerable actual, double delta = 0.0001)
        {
            Assert.True(expected.Cast<object>().SequenceEqual(actual.Cast<object>(), new ObjectComparerDelta(delta)));
        }

        /// <summary>
        /// test a pair of things which can be lists for difference less than a
        /// thresholdy
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="diff"></param>
        public static void AssertLessThreshold(object a, object b, double diff)
        {
            foreach (var expand in ZipExpand(a, b))
            {
                var x = Convert.ToDouble(expand[0]);
                var y = Convert.ToDouble(expand[1]);
                Assert.True(Math.Abs(x - y) < diff);
            }
        }

        public static string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public static string GetTemporaryFile(string path, string extension = "")
        {
            var fileName = Guid.NewGuid() + extension;
            return Path.Combine(path, fileName);
        }

        /// <summary>
        /// Test if an operator exists.
        /// </summary>
        /// <param name="name">Name of the operator</param>
        /// <returns><see langword="true" /> if the operator exists; otherwise, <see langword="false" /></returns>
        public static bool Have(string name)
        {
            return NetVips.TypeFind("VipsOperation", name) != IntPtr.Zero;
        }
    }

    internal class ObjectComparerDelta : IEqualityComparer<object>
    {
        private double delta;

        public ObjectComparerDelta(double delta)
        {
            this.delta = delta;
        }

        public new bool Equals(object x, object y)
        {
            var a = Convert.ToDouble(x);
            var b = Convert.ToDouble(y);

            return Math.Abs(a - b) <= delta;
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }
}