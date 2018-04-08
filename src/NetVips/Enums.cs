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
        /// </remarks>
        public static class BandFormat
        {
            /// <summary>unsigned char format</summary>
            public const string Uchar = "uchar";

            /// <summary>char format</summary>
            public const string Char = "char";

            /// <summary>unsigned short format</summary>
            public const string Ushort = "ushort";

            /// <summary>short format</summary>
            public const string Short = "short";

            /// <summary>unsigned int format</summary>
            public const string Uint = "uint";

            /// <summary>int format</summary>
            public const string Int = "int";

            /// <summary>float format</summary>
            public const string Float = "float";

            /// <summary>complex (two floats) format</summary>
            public const string Complex = "complex";

            /// <summary>double float format</summary>
            public const string Double = "double";

            /// <summary>double complex (two double) format</summary>
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
        /// </remarks>
        public static class BlendMode
        {
            /// <summary>where the second object is drawn, the first is removed</summary>
            public const string Clear = "clear";

            /// <summary>the second object is drawn as if nothing were below</summary>
            public const string Source = "source";

            /// <summary>the image shows what you would expect if you held two semi-transparent slides on top of each other</summary>
            public const string Over = "over";

            /// <summary>the first object is removed completely, the second is only drawn where the first was</summary>
            public const string In = "in";

            /// <summary>the second is drawn only where the first isn't</summary>
            public const string Out = "out";

            /// <summary>this leaves the first object mostly intact, but mixes both objects in the overlapping area</summary>
            public const string Atop = "atop";

            /// <summary>leaves the first object untouched, the second is discarded completely</summary>
            public const string Dest = "dest";

            /// <summary>like Over, but swaps the arguments</summary>
            public const string DestOver = "dest-over";

            /// <summary>like In, but swaps the arguments</summary>
            public const string DestIn = "dest-in";

            /// <summary>like Out, but swaps the arguments</summary>
            public const string DestOut = "dest-out";

            /// <summary>like Atop, but swaps the arguments</summary>
            public const string DestAtop = "dest-atop";

            /// <summary>something like a difference operator</summary>
            public const string Xor = "xor";

            /// <summary>a bit like adding the two images</summary>
            public const string Add = "add";

            /// <summary>a bit like the darker of the two</summary>
            public const string Saturate = "saturate";

            /// <summary>at least as dark as the darker of the two inputs</summary>
            public const string Multiply = "multiply";

            /// <summary>at least as light as the lighter of the inputs</summary>
            public const string Screen = "screen";

            /// <summary>multiplies or screens colors, depending on the lightness</summary>
            public const string Overlay = "overlay";

            /// <summary>the darker of each component</summary>
            public const string Darken = "darken";

            /// <summary>the lighter of each component</summary>
            public const string Lighten = "lighten";

            /// <summary>brighten first by a factor second</summary>
            public const string ColourDodge = "colour-dodge";

            /// <summary>darken first by a factor of second</summary>
            public const string ColourBurn = "colour-burn";

            /// <summary>multiply or screen, depending on lightness</summary>
            public const string HardLight = "hard-light";

            /// <summary>darken or lighten, depending on lightness</summary>
            public const string SoftLight = "soft-light";

            /// <summary>difference of the two</summary>
            public const string Difference = "difference";

            /// <summary>somewhat like Difference, but lower-contrast</summary>
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
            /// <summary>Requests can come in any order</summary>
            public const string Random = "random";

            /// <summary>
            /// Means requests will be top-to-bottom, but with some
            /// amount of buffering behind the read point for small non-local
            /// accesses
            /// </summary>
            public const string Sequential = "sequential";
        }

        /// <summary>
        /// How the values in an image should be interpreted.
        /// </summary>
        /// <remarks>
        /// For example, a three-band float image of type LAB should have its
        /// pixels interpreted as coordinates in CIE Lab space.
        /// </remarks>
        public static class Interpretation
        {
            /// <summary>generic many-band image</summary>
            public const string Multiband = "multiband";

            /// <summary>some kind of single-band image</summary>
            public const string Bw = "b-w";

            /// <summary>a 1D image, eg. histogram or lookup table</summary>
            public const string Histogram = "histogram";

            /// <summary>the first three bands are CIE XYZ</summary>
            public const string Xyz = "xyz";

            /// <summary>pixels are in CIE Lab space</summary>
            public const string Lab = "lab";

            /// <summary>the first four bands are in CMYK space</summary>
            public const string Cmyk = "cmyk";

            /// <summary>implies #VIPS_CODING_LABQ</summary>
            public const string Labq = "labq";

            /// <summary>generic RGB space</summary>
            public const string Rgb = "rgb";

            /// <summary>a uniform colourspace based on CMC(1:1)</summary>
            public const string Cmc = "cmc";

            /// <summary>pixels are in CIE LCh space</summary>
            public const string Lch = "lch";

            /// <summary>CIE LAB coded as three signed 16-bit values</summary>
            public const string Labs = "labs";

            /// <summary>pixels are sRGB</summary>
            public const string Srgb = "srgb";

            /// <summary>pixels are CIE Yxy</summary>
            public const string Yxy = "yxy";

            /// <summary>image is in fourier space</summary>
            public const string Fourier = "fourier";

            /// <summary>generic 16-bit RGB</summary>
            public const string Rgb16 = "rgb16";

            /// <summary>generic 16-bit mono</summary>
            public const string Grey16 = "grey16";

            /// <summary>a matrix</summary>
            public const string Matrix = "matrix";

            /// <summary>pixels are scRGB</summary>
            public const string Scrgb = "scrgb";

            /// <summary>pixels are HSV</summary>
            public const string Hsv = "hsv";
        }

        /// <summary>
        /// Various fixed 90 degree rotation angles.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Rot"/>.
        /// </remarks>
        public static class Angle
        {
            /// <summary>no rotate</summary>
            public const string D0 = "d0";

            /// <summary>90 degrees clockwise</summary>
            public const string D90 = "d90";

            /// <summary>180 degrees</summary>
            public const string D180 = "d180";

            /// <summary>90 degrees anti-clockwise</summary>
            public const string D270 = "d270";
        }

        /// <summary>
        /// Various fixed 45 degree rotation angles.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Rot45"/>.
        /// </remarks>
        public static class Angle45
        {
            /// <summary>no rotate</summary>
            public const string D0 = "d0";

            /// <summary>45 degrees clockwise</summary>
            public const string D45 = "d45";

            /// <summary>90 degrees clockwise</summary>
            public const string D90 = "d90";

            /// <summary>135 degrees clockwise</summary>
            public const string D135 = "d135";

            /// <summary>180 degrees</summary>
            public const string D180 = "d180";

            /// <summary>135 degrees anti-clockwise</summary>
            public const string D225 = "d225";

            /// <summary>90 degrees anti-clockwise</summary>
            public const string D270 = "d270";

            /// <summary>45 degrees anti-clockwise</summary>
            public const string D315 = "d315";
        }

        /// <summary>
        /// The rendering intent.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.IccTransform"/>
        /// </remarks>
        public static class Intent
        {
            /// <summary>perceptual rendering intent</summary>
            public const string Perceptual = "perceptual";

            /// <summary>relative colorimetric rendering intent</summary>
            public const string Relative = "relative";

            /// <summary>saturation rendering intent</summary>
            public const string Saturation = "saturation";

            /// <summary>absolute colorimetric rendering intent</summary>
            public const string Absolute = "absolute";
        }

        /// <summary>
        /// How to extend image edges.
        /// </summary>
        /// <remarks>
        /// When the edges of an image are extended, you can specify how you want
        /// the extension done. See <see cref="Image.Embed"/>, <see cref="Image.Conv"/>, <see cref="Image.Affine"/>
        /// and so on.
        /// </remarks>
        public static class Extend
        {
            /// <summary>new pixels are black, ie. all bits are zero</summary>
            public const string Black = "black";

            /// <summary>each new pixel takes the value of the nearest edge pixel</summary>
            public const string Copy = "copy";

            /// <summary>the image is tiled to fill the new area</summary>
            public const string Repeat = "repeat";

            /// <summary>the image is reflected and tiled to reduce hash edges</summary>
            public const string Mirror = "mirror";

            /// <summary>new pixels are white, ie. all bits are set</summary>
            public const string White = "white";

            /// <summary>colour set from the @background property</summary>
            public const string Background = "background";
        }

        /// <summary>
        /// A resizing kernel. One of these can be given to operations like
        /// <see cref="Image.Reduce"/> or <see cref="Image.Resize"/> to select the resizing kernel to use. 
        /// </summary>
        public static class Kernel
        {
            /// <summary>Nearest-neighbour interpolation</summary>
            public const string Nearest = "nearest";

            /// <summary>Linear interpolation</summary>
            public const string Linear = "linear";

            /// <summary>Cubic interpolation</summary>
            public const string Cubic = "cubic";

            /// <summary>Two-lobe Lanczos</summary>
            public const string Lanczos2 = "lanczos2";

            /// <summary>Three-lobe Lanczos</summary>
            public const string Lanczos3 = "lanczos3";
        }

        /// <summary>
        /// Computation precision.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Conv"/>.
        /// </remarks>
        public static class Precision
        {
            /// <summary>Integer</summary>
            public const string Integer = "integer";

            /// <summary>Floating point</summary>
            public const string Float = "float";

            /// <summary>Compute approximate result</summary>
            public const string Approximate = "approximate";
        }

        /// <summary>
        /// How pixels are coded.
        /// </summary>
        /// <remarks>
        /// Normally, pixels are uncoded and can be manipulated as you would expect.
        /// However some file formats code pixels for compression, and sometimes it's
        /// useful to be able to manipulate images in the coded format.
        /// </remarks>
        public static class Coding
        {
            /// <summary>pixels are not coded</summary>
            public const string None = "none";

            /// <summary>pixels encode 3 float CIELAB values as 4 uchar</summary>
            public const string Labq = "labq";

            /// <summary>pixels encode 3 float RGB as 4 uchar (Radiance coding)</summary>
            public const string Rad = "rad";
        }

        /// <summary>
        /// A direction.
        /// </summary>
        /// <remarks>
        /// Operations like <see cref="Image.Flip"/> need to be told whether to flip
        /// left-right or top-bottom.
        /// </remarks>
        public static class Direction
        {
            /// <summary>left-right</summary>
            public const string Horizontal = "horizontal";

            /// <summary>top-bottom</summary>
            public const string Vertical = "vertical";
        }

        /// <summary>
        /// Various types of alignment.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Join"/>, for example.
        /// </remarks>
        public static class Align
        {
            /// <summary>Align on the low coordinate edge</summary>
            public const string Low = "low";

            /// <summary>Align on the centre</summary>
            public const string Centre = "centre";

            /// <summary>Align on the high coordinate edge</summary>
            public const string High = "high";
        }

        /// <summary>
        /// How to combine passes.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Compass"/>.
        /// </remarks>
        public static class Combine
        {
            /// <summary>Take the maximum of all values</summary>
            public const string Max = "max";

            /// <summary>Take the sum of all values</summary>
            public const string Sum = "sum";
        }

        /// <summary>
        /// Set Perofile Connection Space.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.IccImport"/>.
        /// </remarks>
        public static class PCS
        {
            /// <summary>CIE Lab space</summary>
            public const string Lab = "lab";

            /// <summary>CIE XYZ space</summary>
            public const string Xyz = "xyz";
        }
    }
}