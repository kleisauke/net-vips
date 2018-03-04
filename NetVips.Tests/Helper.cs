using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace NetVips.Tests
{
    public static class Helper
    {
        public static readonly string Images =
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\", "TestData"));

        public static readonly string JpegFile = Path.Combine(Images, "йцук.jpg");
        public static readonly string SrgbFile = Path.Combine(Images, "sRGB.icm");
        public static readonly string MatlabFile = Path.Combine(Images, "sample.mat");
        public static readonly string PngFile = Path.Combine(Images, "sample.png");
        public static readonly string TifFile = Path.Combine(Images, "sample.tif");
        public static readonly string OmeFile = Path.Combine(Images, "multi-channel-z-series.ome.tif");
        public static readonly string AnalyzeFile = Path.Combine(Images, "t00740_tr1_segm.hdr");
        public static readonly string GifFile = Path.Combine(Images, "cramps.gif");
        public static readonly string WebpFile = Path.Combine(Images, "1.webp");
        public static readonly string ExrFile = Path.Combine(Images, "sample.exr");
        public static readonly string FitsFile = Path.Combine(Images, "WFPC2u5780205r_c0fx.fits");
        public static readonly string OpenslideFile = Path.Combine(Images, "CMU-1-Small-Region.svs");
        public static readonly string PdfFile = Path.Combine(Images, "ISO_12233-reschart.pdf");
        public static readonly string CmykPdfFile = Path.Combine(Images, "cmyktest.pdf");
        public static readonly string SvgFile = Path.Combine(Images, "vips-profile.svg");
        public static readonly string SvgzFile = Path.Combine(Images, "vips-profile.svgz");
        public static readonly string SvgGzFile = Path.Combine(Images, "vips-profile.svg.gz");
        public static readonly string GifAnimFile = Path.Combine(Images, "cogs.gif");
        public static readonly string DicomFile = Path.Combine(Images, "dicom_test_image.dcm");

        public static string[] UnsignedFormats =
        {
            Enums.BandFormat.Uchar,
            Enums.BandFormat.Ushort,
            Enums.BandFormat.Uint
        };

        public static string[] SignedFormats =
        {
            Enums.BandFormat.Char,
            Enums.BandFormat.Short,
            Enums.BandFormat.Int
        };

        public static string[] FloatFormats =
        {
            Enums.BandFormat.Float,
            Enums.BandFormat.Double
        };

        public static string[] ComplexFormats =
        {
            Enums.BandFormat.Complex,
            Enums.BandFormat.Dpcomplex
        };

        public static string[] IntFormats = UnsignedFormats.Concat(SignedFormats).ToArray();

        public static string[] NonComplexFormats = IntFormats.Concat(FloatFormats).ToArray();

        public static string[] AllFormats = IntFormats.Concat(FloatFormats).Concat(ComplexFormats).ToArray();

        public static string[] ColourColourspaces =
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

        public static string[] CodedColourspaces =
        {
            Enums.Interpretation.Labq
        };

        public static string[] MonoColourspaces =
        {
            Enums.Interpretation.Bw
        };

        public static string[] SixteenbitColourspaces =
        {
            Enums.Interpretation.Grey16,
            Enums.Interpretation.Rgb16
        };

        public static string[] AllColourspaces = ColourColourspaces.Concat(MonoColourspaces)
            .Concat(CodedColourspaces)
            .Concat(SixteenbitColourspaces)
            .ToArray();

        public static Dictionary<string, double> MaxValue = new Dictionary<string, double>
        {
            {
                Enums.BandFormat.Uchar,
                255
            },
            {
                Enums.BandFormat.Ushort,
                65535
            },
            {
                Enums.BandFormat.Uint,
                -1
            },
            {
                Enums.BandFormat.Char,
                127
            },
            {
                Enums.BandFormat.Short,
                32767
            },
            {
                Enums.BandFormat.Int,
                2147483647
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

        public static Dictionary<string, int> SizeofFormat = new Dictionary<string, int>
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

        public static string[] Rot45Angles =
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

        public static string[] Rot45AngleBonds =
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

        public static string[] RotAngles =
        {
            Enums.Angle.D0,
            Enums.Angle.D90,
            Enums.Angle.D180,
            Enums.Angle.D270
        };

        public static string[] RotAngleBonds =
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
        public static object[][] ZipExpand(object x, object y)
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
                    .Zip(enumerable2.Cast<object>(), (xObj, yObj) => new[] {xObj, yObj})
                    .ToArray();
            }
            else if (x is IEnumerable enumerableX)
            {
                return enumerableX.Cast<object>().Select(i => new[]
                {
                    i,
                    y
                }).ToArray();
            }
            else if (y is IEnumerable enumerableY)
            {
                return enumerableY.Cast<object>().Select(j => new[]
                {
                    x,
                    j
                }).ToArray();
            }
            else
            {
                return new[]
                {
                    new[] {x, y}
                };
            }
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
            else
            {
                return func(x);
            }
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
            } else if (x is Array || y is Array)
            {
                return ZipExpand(x, y).Select(o => func(o[0], o[1])).ToArray();
            }
            else
            {
                return func(x, y);
            }
        }

        /// <summary>
        /// run a function on an image and on a single pixel, the results
        /// should match
        /// </summary>
        /// <param name="message"></param>
        /// <param name="im"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="func"></param>
        public static void RunCmp(
            string message,
            Image im,
            int x,
            int y,
            Func<object, object> func)
        {
            var a = im.Getpoint(x, y);
            var v1 = func(a);
            var im2 = func(im) as Image;
            var v2 = im2?.Getpoint(x, y);

            AssertAlmostEqualObjects(v1, v2, message);
        }

        /// <summary>
        // run a function on a pair of images and on a pair of pixels, the results
        // should match
        /// </summary>
        /// <param name="message"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static void RunCmp2(
            string message,
            Image left,
            Image right,
            int x,
            int y,
            Func<object, object, object> func)
        {
            var a = left.Getpoint(x, y);
            var b = right.Getpoint(x, y);
            var v1 = func(a, b);
            var after = func(left, right) as Image;
            var v2 = after?.Getpoint(x, y);

            AssertAlmostEqualObjects(v1, v2, message);
        }

        /// <summary>
        /// run a function on a pair of images
        /// 50,50 and 10,10 should have different values on the test image
        /// </summary>
        /// <param name="message"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static void RunImage2(string message, Image left, Image right, Func<object, object, object> func)
        {
            RunCmp2(message, left, right, 50, 50, (x, y) => RunFn2(func, x, y));
            RunCmp2(message, left, right, 10, 10, (x, y) => RunFn2(func, x, y));
        }

        /// <summary>
        /// run a function on (image, constant), and on (constant, image).
        /// 50,50 and 10,10 should have different values on the test image
        /// </summary>
        public static void RunConst(string message, Func<object, object, object> func, Image im, object c)
        {
            RunCmp(message, im, 50, 50, x => RunFn2(func, x, c));
            RunCmp(message, im, 50, 50, x => RunFn2(func, c, x));
            RunCmp(message, im, 10, 10, x => RunFn2(func, x, c));
            RunCmp(message, im, 10, 10, x => RunFn2(func, c, x));
        }

        /// <summary>
        /// test a pair of things which can be lists for approx. equality
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="msg"></param>
        /// <param name="delta"></param>
        public static void AssertAlmostEqualObjects(object a, object b, string msg = "", double delta = 0.0001)
        {
            foreach (var array in ZipExpand(a, b))
            {
                if (array[0] is double dbl1 && array[1] is double dbl2)
                {
                    Assert.AreEqual(dbl1, dbl2, delta, msg);
                }
                else
                {
                    Assert.AreEqual(array[0], array[1], msg);
                }
            }
        }

        public static string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public static string GetTemporaryFile(string path, string extension)
        {
            var fileName = Guid.NewGuid() + extension;
            return Path.Combine(path, fileName);
        }
    }
}