namespace NetVips
{
    /// <summary>
    /// This module contains the various libvips enums as C# classes
    /// Enums values are represented in NetVips as strings. These classes contain the valid strings for each enum.
    /// </summary>
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
            public const string Uchar = "uchar";
            public const string Char = "char";
            public const string Ushort = "ushort";
            public const string Short = "short";
            public const string Uint = "uint";
            public const string Int = "int";
            public const string Float = "float";
            public const string Complex = "complex";
            public const string Double = "double";
            public const string Dpcomplex = "dpcomplex";
        }

        /// <summary>
        /// The various Porter-Duff and PDF blend modes. See <see cref="Image.Composite"/>.
        /// </summary>
        /// <remarks>
        /// The Cairo docs have a nice explanation of all the blend modes:
        /// https://www.cairographics.org/operators
        /// 
        /// The non-separable modes are not implemented.
        /// 
        /// Attributes:
        ///     Clear (string): where the second object is drawn, the first is removed
        ///     Source (string): the second object is drawn as if nothing were below
        ///     Over (string): the image shows what you would expect if you held two semi-transparent slides on top of each other
        ///     In (string): the first object is removed completely, the second is only drawn where the first was
        ///     Out (string): the second is drawn only where the first isn't
        ///     Atop (string): this leaves the first object mostly intact, but mixes both objects in the overlapping area
        ///     Dest (string): leaves the first object untouched, the second is discarded completely
        ///     DestOver (string): like Over, but swaps the arguments
        ///     DestIn (string): like In, but swaps the arguments
        ///     DestOut (string): like Out, but swaps the arguments
        ///     DestAtop (string): like Atop, but swaps the arguments
        ///     Xor (string): something like a difference operator
        ///     Add (string): a bit like adding the two images
        ///     Saturate (string): a bit like the darker of the two
        ///     Multiply (string): at least as dark as the darker of the two inputs
        ///     Screen (string): at least as light as the lighter of the inputs
        ///     Overlay (string): multiplies or screens colors, depending on the lightness
        ///     Darken (string): the darker of each component
        ///     Lighten (string): the lighter of each component
        ///     ColourDodge (string): brighten first by a factor second
        ///     ColourBurn (string): darken first by a factor of second
        ///     HardLight (string): multiply or screen, depending on lightness
        ///     SoftLight (string): darken or lighten, depending on lightness
        ///     Difference (string): difference of the two
        ///     Exclusion (string): somewhat like Difference, but lower-contrast 
        /// </remarks>
        public static class BlendMode
        {
            public const string Clear = "clear";
            public const string Source = "source";
            public const string Over = "over";
            public const string In = "in";
            public const string Out = "out";
            public const string Atop = "atop";
            public const string Dest = "dest";
            public const string DestOver = "dest-over";
            public const string DestIn = "dest-in";
            public const string DestOut = "dest-out";
            public const string DestAtop = "dest-atop";
            public const string Xor = "xor";
            public const string Add = "add";
            public const string Saturate = "saturate";
            public const string Multiply = "multiply";
            public const string Screen = "screen";
            public const string Overlay = "overlay";
            public const string Darken = "darken";
            public const string Lighten = "lighten";
            public const string ColourDodge = "colour-dodge";
            public const string ColourBurn = "colour-burn";
            public const string HardLight = "hard-light";
            public const string SoftLight = "soft-light";
            public const string Difference = "difference";
            public const string Exclusion = "exclusion";
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
            public const string Random = "random";
            public const string Sequential = "sequential";
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
            public const string Multiband = "multiband";
            public const string Bw = "b-w";
            public const string Histogram = "histogram";
            public const string Xyz = "xyz";
            public const string Lab = "lab";
            public const string Cmyk = "cmyk";
            public const string Labq = "labq";
            public const string Rgb = "rgb";
            public const string Cmc = "cmc";
            public const string Lch = "lch";
            public const string Labs = "labs";
            public const string Srgb = "srgb";
            public const string Yxy = "yxy";
            public const string Fourier = "fourier";
            public const string Rgb16 = "rgb16";
            public const string Grey16 = "grey16";
            public const string Matrix = "matrix";
            public const string Scrgb = "scrgb";
            public const string Hsv = "hsv";
        }

        /// <summary>
        /// Various fixed 90 degree rotation angles.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Rot"/>.
        /// 
        /// Attributes:
        ///     D0 (string): no rotate
        ///     D90 (string): 90 degrees clockwise
        ///     D180 (string): 180 degrees
        ///     D270 (string): 90 degrees anti-clockwise
        /// </remarks>
        public static class Angle
        {
            public const string D0 = "d0";
            public const string D90 = "d90";
            public const string D180 = "d180";
            public const string D270 = "d270";
        }

        /// <summary>
        /// Various fixed 45 degree rotation angles.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Rot45"/>.
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
        public static class Angle45
        {
            public const string D0 = "d0";
            public const string D45 = "d45";
            public const string D90 = "d90";
            public const string D135 = "d135";
            public const string D180 = "d180";
            public const string D225 = "d225";
            public const string D270 = "d270";
            public const string D315 = "d315";
        }

        /// <summary>
        /// The rendering intent.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.IccTransform"/>
        /// 
        /// Attributes:
        ///     Perceptual (string):
        ///     Relative (string):
        ///     Saturation (string):
        ///     Absolute (string):
        /// </remarks>
        public static class Intent
        {
            public const string Perceptual = "perceptual";
            public const string Relative = "relative";
            public const string Saturation = "saturation";
            public const string Absolute = "absolute";
        }

        /// <summary>
        /// How to extend image edges.
        /// </summary>
        /// <remarks>
        /// When the edges of an image are extended, you can specify how you want
        /// the extension done. See <see cref="Image.Embed"/>, <see cref="Image.Conv"/>, <see cref="Image.Affine"/>
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
        public static class Extend
        {
            public const string Black = "black";
            public const string Copy = "copy";
            public const string Repeat = "repeat";
            public const string Mirror = "mirror";
            public const string White = "white";
            public const string Background = "background";
        }

        /// <summary>
        /// Computation precision.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Conv"/>.
        /// 
        /// Attributes:
        ///     Integer (string): Integer.
        ///     Float (string): Floating point.
        ///     Approximate (string): Compute approximate result.
        /// </remarks>
        public static class Precision
        {
            public const string Integer = "integer";
            public const string Float = "float";
            public const string Approximate = "approximate";
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
        public static class Coding
        {
            public const string None = "none";
            public const string Labq = "labq";
            public const string Rad = "rad";
        }

        /// <summary>
        /// A direction.
        /// </summary>
        /// <remarks>
        /// Operations like <see cref="Image.Flip"/> need to be told whether to flip
        /// left-right or top-bottom.
        /// 
        /// Attributes:
        ///     Horizontal (string): left-right
        ///     Vertical (string): top-bottom
        /// </remarks>
        public static class Direction
        {
            public const string Horizontal = "horizontal";
            public const string Vertical = "vertical";
        }

        /// <summary>
        /// Various types of alignment.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Join"/>, for example.
        /// 
        /// Attributes:
        ///     Low (string): Align on the low coordinate edge
        ///     Centre (string): Align on the centre
        ///     High (string): Align on the high coordinate edge
        /// </remarks>
        public static class Align
        {
            public const string Low = "low";
            public const string Centre = "centre";
            public const string High = "high";
        }

        /// <summary>
        /// How to combine passes.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Compass"/>.
        /// 
        /// Attributes:
        ///     Max (string): Take the maximum of all values.
        ///     Sum (string): Take the sum of all values.
        /// </remarks>
        public static class Combine
        {
            public const string Max = "max";
            public const string Sum = "sum";
        }

        /// <summary>
        /// Set Perofile Connection Space.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.IccImport"/>.
        /// 
        /// Attributes:
        ///     Lab (string): CIE Lab space.
        ///     Xyz (string): CIE XYZ space.
        /// </remarks>
        public static class PCS
        {
            public const string Lab = "lab";
            public const string Xyz = "xyz";
        }
    }
}