using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using NetVips;

namespace NetVips.Tests
{
    public static class Helper
    {
        public static readonly string Images =
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\", "TestData"));

        // TODO Support non-ascii characters in filenames. (see: https://github.com/jcupitt/libvips/issues/294)
        public static readonly string JpegFile = Path.Combine(Images, /*"йцук.jpg"*/"sample.jpg");
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

        public static string[] NoncomplexFormats = IntFormats.Concat(FloatFormats).ToArray();

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
    }
}