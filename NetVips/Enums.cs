using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetVips
{
    public static class Enums
    {
        /// <summary>
        /// The format of image bands.
        /// </summary>
        /// <remarks>
        /// The format used for each band element. Each corresponds to a native C type
        /// for the current machine.
        /// 
        /// Attributes:
        ///     Uchar (string): unsigned char format
        ///     Char (string): char format
        ///     Ushort (string): unsigned short format
        ///     Short (string): short format
        ///     Uint (string): unsigned int format
        ///     Int (string): int format
        ///     Float (string): float format
        ///     Complex (string): complex (two floats) format
        ///     Double (string): double float format
        ///     Dpcomplex (string): double complex (two double) format
        /// </remarks>
        public static class BandFormat
        {
            public static string Uchar = "uchar";
            public static string Char = "char";
            public static string Ushort = "ushort";
            public static string Short = "short";
            public static string Uint = "uint";
            public static string Int = "int";
            public static string Float = "float";
            public static string Complex = "complex";
            public static string Double = "double";
            public static string Dpcomplex = "dpcomplex";
        }

        /// <summary>
        /// The type of access an operation has to supply.
        /// </summary>
        /// <remarks>
        /// Attributes:
        ///     Random (string): Requests can come in any order.
        ///     Sequential (string): Means requests will be top-to-bottom, but with some
        ///         amount of buffering behind the read point for small non-local
        ///         accesses.
        /// </remarks>
        public static class Access
        {
            public static string Random = "random";
            public static string Sequential = "sequential";
        }

        /// <summary>
        /// How the values in an image should be interpreted.
        /// </summary>
        /// <remarks>
        /// For example, a three-band float image of type LAB should have its
        /// pixels interpreted as coordinates in CIE Lab space.
        /// 
        /// Attributes:
        ///     Multiband (string): generic many-band image
        ///     Bw (string): some kind of single-band image
        ///     Histogram (string): a 1D image, eg. histogram or lookup table
        ///     Fourier (string): image is in fourier space
        ///     Xyz (string): the first three bands are CIE XYZ
        ///     Lab (string): pixels are in CIE Lab space
        ///     Cmyk (string): the first four bands are in CMYK space
        ///     Labq (string): implies #VIPS_CODING_LABQ
        ///     Rgb (string): generic RGB space
        ///     Cmc (string): a uniform colourspace based on CMC(1:1)
        ///     Lch (string): pixels are in CIE LCh space
        ///     Labs (string): CIE LAB coded as three signed 16-bit values
        ///     Srgb (string): pixels are sRGB
        ///     Hsv (string): pixels are HSV
        ///     Scrgb (string): pixels are scRGB
        ///     Yxy (string): pixels are CIE Yxy
        ///     Rgb16 (string): generic 16-bit RGB
        ///     Grey16 (string): generic 16-bit mono
        ///     Matrix (string): a matrix
        /// </remarks>
        public static class Interpretation
        {
            public static string Multiband = "multiband";
            public static string Bw = "b-w";
            public static string Histogram = "histogram";
            public static string Xyz = "xyz";
            public static string Lab = "lab";
            public static string Cmyk = "cmyk";
            public static string Labq = "labq";
            public static string Rgb = "rgb";
            public static string Cmc = "cmc";
            public static string Lch = "lch";
            public static string Labs = "labs";
            public static string Srgb = "srgb";
            public static string Yxy = "yxy";
            public static string Fourier = "fourier";
            public static string Rgb16 = "rgb16";
            public static string Grey16 = "grey16";
            public static string Matrix = "matrix";
            public static string Scrgb = "scrgb";
            public static string Hsv = "hsv";
        }

        /// <summary>
        /// Various fixed 90 degree rotation angles.
        /// </summary>
        /// <remarks>
        /// See for example :meth:`.rot`.
        /// 
        /// Attributes:
        ///     D0 (string): no rotate
        ///     D90 (string): 90 degrees clockwise
        ///     D180 (string): 180 degrees
        ///     D270 (string): 90 degrees anti-clockwise
        /// </remarks>
        public class Angle
        {
            public static string D0 = "d0";
            public static string D90 = "d90";
            public static string D180 = "d180";
            public static string D270 = "d270";
        }

        /// <summary>
        /// Various fixed 45 degree rotation angles.
        /// </summary>
        /// <remarks>
        /// See for example :meth:`.rot45`.
        /// 
        /// Attributes:
        ///     D0 (string): no rotate
        ///     D45 (string): 45 degrees clockwise
        ///     D90 (string): 90 degrees clockwise
        ///     D135 (string): 135 degrees clockwise
        ///     D180 (string): 180 degrees
        ///     D225 (string): 135 degrees anti-clockwise
        ///     D270 (string): 90 degrees anti-clockwise
        ///     D315 (string): 45 degrees anti-clockwise
        /// </remarks>
        public class Angle45
        {
            public static string D0 = "d0";
            public static string D45 = "d45";
            public static string D90 = "d90";
            public static string D135 = "d135";
            public static string D180 = "d180";
            public static string D225 = "d225";
            public static string D270 = "d270";
            public static string D315 = "d315";
        }

        /// <summary>
        /// The rendering intent.
        /// </summary>
        /// <remarks>
        /// See :meth:`.icc_transform`.
        /// 
        /// Attributes:
        ///     Perceptual (string):
        ///     Relative (string):
        ///     Saturation (string):
        ///     Absolute (string):
        /// </remarks>
        public class Intent
        {
            public static string Perceptual = "perceptual";
            public static string Relative = "relative";
            public static string Saturation = "saturation";
            public static string Absolute = "absolute";
        }

        /// <summary>
        /// How to extend image edges.
        /// </summary>
        /// <remarks>
        /// When the edges of an image are extended, you can specify how you want
        /// the extension done.  See :meth:`.embed`, :meth:`.conv`, :meth:`.affine`
        /// and so on.
        /// 
        /// Attributes:
        ///     Black (string): new pixels are black, ie. all bits are zero.
        ///     Copy (string): each new pixel takes the value of the nearest edge pixel
        ///     Repeat (string): the image is tiled to fill the new area
        ///     Mirror (string): the image is reflected and tiled to reduce hash edges
        ///     White (string): new pixels are white, ie. all bits are set
        ///     Background (string): colour set from the @background property
        /// </remarks>
        public class Extend
        {
            public static string Black = "black";
            public static string Copy = "copy";
            public static string Repeat = "repeat";
            public static string Mirror = "mirror";
            public static string White = "white";
            public static string Background = "background";
        }

        /// <summary>
        /// Computation precision.
        /// </summary>
        /// <remarks>
        /// See for example :meth:`.conv`.
        /// 
        /// Attributes:
        ///     Integer (string): Integer.
        ///     Float (string): Floating point.
        ///     Approximate (string): Compute approximate result.
        /// </remarks>
        public class Precision
        {
            public static string Integer = "integer";
            public static string Float = "float";
            public static string Approximate = "approximate";
        }

        /// <summary>
        /// How pixels are coded.
        /// </summary>
        /// <remarks>
        /// Normally, pixels are uncoded and can be manipulated as you would expect.
        /// However some file formats code pixels for compression, and sometimes it's
        /// useful to be able to manipulate images in the coded format.
        /// 
        /// Attributes:
        ///     None (string): pixels are not coded
        ///     Labq (string): pixels encode 3 float CIELAB values as 4 uchar
        ///     Rad (string): pixels encode 3 float RGB as 4 uchar (Radiance coding)
        /// </remarks>
        public class Coding
        {
            public static string None = "none";
            public static string Labq = "labq";
            public static string Rad = "rad";
        }

        /// <summary>
        /// A direction.
        /// </summary>
        /// <remarks>
        /// Operations like :meth:`.flip` need to be told whether to flip
        /// left-right or top-bottom.
        /// 
        /// Attributes:
        ///     Horizontal (string): left-right
        ///     Vertical (string): top-bottom
        /// </remarks>
        public class Direction
        {
            public static string Horizontal = "horizontal";
            public static string Vertical = "vertical";
        }

        /// <summary>
        /// Various types of alignment.
        /// </summary>
        /// <remarks>
        /// See :meth:`.join`, for example.
        /// 
        /// Attributes:
        ///     Low (string): Align on the low coordinate edge
        ///     Centre (string): Align on the centre
        ///     High (string): Align on the high coordinate edge
        /// </remarks>
        public class Align
        {
            public static string Low = "low";
            public static string Centre = "centre";
            public static string High = "high";
        }

        /// <summary>
        /// How to combine passes.
        /// </summary>
        /// <remarks>
        /// See for example :meth:`.compass`.
        /// 
        /// Attributes:
        ///     Max (string): Take the maximum of all values.
        ///     Sum (string): Take the sum of all values.
        /// </remarks>
        public class Combine
        {
            public static string Max = "max";
            public static string Sum = "sum";
        }

        /// <summary>
        /// Set Perofile Connection Space.
        /// </summary>
        /// <remarks>
        /// See for example :meth:`.icc_import`.
        /// 
        /// Attributes:
        ///     Lab (string): CIE Lab space.
        ///     Xyz (string): CIE XYZ space.
        /// </remarks>
        public class PCS
        {
            public static string Lab = "lab";
            public static string Xyz = "xyz";
        }
    }
}