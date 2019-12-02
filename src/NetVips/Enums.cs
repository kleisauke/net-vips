namespace NetVips
{
    using System;

    /// <summary>
    /// This module contains the various libvips enums as C# classes
    /// Enums values are represented in NetVips as strings. These classes contain the valid strings for each enum.
    /// </summary>
    public static class Enums
    {
        /// <summary>
        /// Flags specifying the level of log messages.
        /// </summary>
        [Flags]
        public enum LogLevelFlags
        {
            #region Internal log flags

            /// <summary>Internal flag.</summary>
            FlagRecursion = 1 << 0,

            /// <summary>internal flag.</summary>
            FlagFatal = 1 << 1,

            #endregion

            #region GLib log levels

            /// <summary>log level for errors.</summary>
            Error = 1 << 2, /* always fatal */

            /// <summary>log level for critical warning messages.</summary>
            Critical = 1 << 3,

            /// <summary>log level for warnings.</summary>
            Warning = 1 << 4,

            /// <summary>log level for messages.</summary>
            Message = 1 << 5,

            /// <summary>log level for informational messages.</summary>
            Info = 1 << 6,

            /// <summary>log level for debug messages.</summary>
            Debug = 1 << 7,

            #endregion

            #region Convenience values

            /// <summary>All log levels except fatal.</summary>
            AllButFatal = 253,

            /// <summary>All log levels except recursion.</summary>
            AllButRecursion = 254,

            /// <summary>All log levels.</summary>
            All = 255,

            /// <summary>Flag mask.</summary>
            FlagMask = 3,

            /// <summary>A mask including all log levels..</summary>
            LevelMask = unchecked((int)0xFFFFFFFC)

            #endregion
        }

        /// <summary>
        /// Flags we associate with each object argument.
        /// </summary>
        [Flags]
        public enum ArgumentFlags
        {
            /// <summary>no flags.</summary>
            NONE = 0,

            /// <summary>must be set in the constructor.</summary>
            REQUIRED = 1,

            /// <summary>can only be set in the constructor.</summary>
            CONSTRUCT = 2,

            /// <summary>can only be set once.</summary>
            SET_ONCE = 4,

            /// <summary>don't do use-before-set checks.</summary>
            SET_ALWAYS = 8,

            /// <summary>is an input argument (one we depend on).</summary>
            INPUT = 16,

            /// <summary>is an output argument (depends on us).</summary>
            OUTPUT = 32,

            /// <summary>just there for back-compat, hide.</summary>
            DEPRECATED = 64,

            /// <summary>the input argument will be modified.</summary>
            MODIFY = 128
        }

        /// <summary>
        /// Flags we associate with an <see cref="Operation"/>.
        /// </summary>
        [Flags]
        public enum OperationFlags
        {
            /// <summary>no flags.</summary>
            NONE = 0,

            /// <summary>can work sequentially with a small buffer.</summary>
            SEQUENTIAL = 1,

            /// <summary>can work sequentially without a buffer.</summary>
            SEQUENTIAL_UNBUFFERED = 2,

            /// <summary>must not be cached.</summary>
            NOCACHE = 4,

            /// <summary>a compatibility thing.</summary>
            DEPRECATED = 8
        }

        /// <summary>
        /// Signals that can be used on an <see cref="Image"/>. See <see cref="GObject.SignalConnect"/>.
        /// </summary>
        internal static class Signals
        {
            /// <summary>Evaluation is starting.</summary>
            /// <remarks>
            /// The preeval signal is emitted once before computation of <see cref="Image"/>
            /// starts. It's a good place to set up evaluation feedback.
            /// </remarks>
            public const string PreEval = "preeval";

            /// <summary>Evaluation progress.</summary>
            /// <remarks>
            /// The eval signal is emitted once per work unit (typically a 128 x
            /// 128 area of pixels) during image computation.
            ///
            /// You can use this signal to update user-interfaces with progress
            /// feedback. Beware of updating too frequently: you will usually
            /// need some throttling mechanism.
            /// </remarks>
            public const string Eval = "eval";

            /// <summary>Evaluation is ending.</summary>
            /// <remarks>
            /// The posteval signal is emitted once at the end of the computation
            /// of <see cref="Image"/>. It's a good place to shut down evaluation feedback.
            /// </remarks>
            public const string PostEval = "posteval";
        }

        /// <summary>
        /// The format of image bands.
        /// </summary>
        /// <remarks>
        /// The format used for each band element. Each corresponds to a native C type
        /// for the current machine.
        /// </remarks>
        public static class BandFormat
        {
            /// <summary>unsigned char format.</summary>
            public const string Uchar = "uchar";

            /// <summary>char format.</summary>
            public const string Char = "char";

            /// <summary>unsigned short format.</summary>
            public const string Ushort = "ushort";

            /// <summary>short format.</summary>
            public const string Short = "short";

            /// <summary>unsigned int format.</summary>
            public const string Uint = "uint";

            /// <summary>int format.</summary>
            public const string Int = "int";

            /// <summary>float format.</summary>
            public const string Float = "float";

            /// <summary>complex (two floats) format.</summary>
            public const string Complex = "complex";

            /// <summary>double float format.</summary>
            public const string Double = "double";

            /// <summary>double complex (two double) format.</summary>
            public const string Dpcomplex = "dpcomplex";
        }

        /// <summary>
        /// The various Porter-Duff and PDF blend modes. See <see cref="Image.Composite2"/>.
        /// </summary>
        /// <remarks>
        /// The Cairo docs have a nice explanation of all the blend modes:
        /// https://www.cairographics.org/operators
        ///
        /// The non-separable modes are not implemented.
        /// </remarks>
        public static class BlendMode
        {
            /// <summary>Where the second object is drawn, the first is removed.</summary>
            public const string Clear = "clear";

            /// <summary>The second object is drawn as if nothing were below.</summary>
            public const string Source = "source";

            /// <summary>The image shows what you would expect if you held two semi-transparent slides on top of each other.</summary>
            public const string Over = "over";

            /// <summary>The first object is removed completely, the second is only drawn where the first was.</summary>
            public const string In = "in";

            /// <summary>The second is drawn only where the first isn't.</summary>
            public const string Out = "out";

            /// <summary>This leaves the first object mostly intact, but mixes both objects in the overlapping area.</summary>
            public const string Atop = "atop";

            /// <summary>Leaves the first object untouched, the second is discarded completely.</summary>
            public const string Dest = "dest";

            /// <summary>Like Over, but swaps the arguments.</summary>
            public const string DestOver = "dest-over";

            /// <summary>Like In, but swaps the arguments.</summary>
            public const string DestIn = "dest-in";

            /// <summary>Like Out, but swaps the arguments.</summary>
            public const string DestOut = "dest-out";

            /// <summary>Like Atop, but swaps the arguments.</summary>
            public const string DestAtop = "dest-atop";

            /// <summary>Something like a difference operator.</summary>
            public const string Xor = "xor";

            /// <summary>A bit like adding the two images.</summary>
            public const string Add = "add";

            /// <summary>A bit like the darker of the two.</summary>
            public const string Saturate = "saturate";

            /// <summary>At least as dark as the darker of the two inputs.</summary>
            public const string Multiply = "multiply";

            /// <summary>At least as light as the lighter of the inputs.</summary>
            public const string Screen = "screen";

            /// <summary>Multiplies or screens colors, depending on the lightness.</summary>
            public const string Overlay = "overlay";

            /// <summary>The darker of each component.</summary>
            public const string Darken = "darken";

            /// <summary>The lighter of each component.</summary>
            public const string Lighten = "lighten";

            /// <summary>Brighten first by a factor second.</summary>
            public const string ColourDodge = "colour-dodge";

            /// <summary>Darken first by a factor of second.</summary>
            public const string ColourBurn = "colour-burn";

            /// <summary>Multiply or screen, depending on lightness.</summary>
            public const string HardLight = "hard-light";

            /// <summary>Darken or lighten, depending on lightness.</summary>
            public const string SoftLight = "soft-light";

            /// <summary>Difference of the two.</summary>
            public const string Difference = "difference";

            /// <summary>Somewhat like Difference, but lower-contrast.</summary>
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
            /// <summary>Requests can come in any order.</summary>
            public const string Random = "random";

            /// <summary>
            /// Means requests will be top-to-bottom, but with some
            /// amount of buffering behind the read point for small non-local
            /// accesses.
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
            /// <summary>Generic many-band image.</summary>
            public const string Multiband = "multiband";

            /// <summary>Some kind of single-band image.</summary>
            public const string Bw = "b-w";

            /// <summary>A 1D image, eg. histogram or lookup table.</summary>
            public const string Histogram = "histogram";

            /// <summary>The first three bands are CIE XYZ.</summary>
            public const string Xyz = "xyz";

            /// <summary>Pixels are in CIE Lab space.</summary>
            public const string Lab = "lab";

            /// <summary>The first four bands are in CMYK space.</summary>
            public const string Cmyk = "cmyk";

            /// <summary>Implies #VIPS_CODING_LABQ.</summary>
            public const string Labq = "labq";

            /// <summary>Generic RGB space.</summary>
            public const string Rgb = "rgb";

            /// <summary>A uniform colourspace based on CMC(1:1).</summary>
            public const string Cmc = "cmc";

            /// <summary>Pixels are in CIE LCh space.</summary>
            public const string Lch = "lch";

            /// <summary>CIE LAB coded as three signed 16-bit values.</summary>
            public const string Labs = "labs";

            /// <summary>Pixels are sRGB.</summary>
            public const string Srgb = "srgb";

            /// <summary>Pixels are CIE Yxy.</summary>
            public const string Yxy = "yxy";

            /// <summary>Image is in fourier space.</summary>
            public const string Fourier = "fourier";

            /// <summary>Generic 16-bit RGB.</summary>
            public const string Rgb16 = "rgb16";

            /// <summary>Generic 16-bit mono.</summary>
            public const string Grey16 = "grey16";

            /// <summary>A matrix.</summary>
            public const string Matrix = "matrix";

            /// <summary>Pixels are scRGB.</summary>
            public const string Scrgb = "scrgb";

            /// <summary>Pixels are HSV.</summary>
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
            /// <summary>No rotate.</summary>
            public const string D0 = "d0";

            /// <summary>90 degrees clockwise.</summary>
            public const string D90 = "d90";

            /// <summary>180 degrees.</summary>
            public const string D180 = "d180";

            /// <summary>90 degrees anti-clockwise.</summary>
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
            /// <summary>No rotate.</summary>
            public const string D0 = "d0";

            /// <summary>45 degrees clockwise.</summary>
            public const string D45 = "d45";

            /// <summary>90 degrees clockwise.</summary>
            public const string D90 = "d90";

            /// <summary>135 degrees clockwise.</summary>
            public const string D135 = "d135";

            /// <summary>180 degrees.</summary>
            public const string D180 = "d180";

            /// <summary>135 degrees anti-clockwise.</summary>
            public const string D225 = "d225";

            /// <summary>90 degrees anti-clockwise.</summary>
            public const string D270 = "d270";

            /// <summary>45 degrees anti-clockwise.</summary>
            public const string D315 = "d315";
        }

        /// <summary>
        /// The rendering intent.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.IccTransform"/>.
        /// </remarks>
        public static class Intent
        {
            /// <summary>Perceptual rendering intent.</summary>
            public const string Perceptual = "perceptual";

            /// <summary>Relative colorimetric rendering intent.</summary>
            public const string Relative = "relative";

            /// <summary>Saturation rendering intent.</summary>
            public const string Saturation = "saturation";

            /// <summary>Absolute colorimetric rendering intent.</summary>
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
            /// <summary>New pixels are black, ie. all bits are zero.</summary>
            public const string Black = "black";

            /// <summary>Each new pixel takes the value of the nearest edge pixel.</summary>
            public const string Copy = "copy";

            /// <summary>The image is tiled to fill the new area.</summary>
            public const string Repeat = "repeat";

            /// <summary>The image is reflected and tiled to reduce hash edges.</summary>
            public const string Mirror = "mirror";

            /// <summary>New pixels are white, ie. all bits are set.</summary>
            public const string White = "white";

            /// <summary>Colour set from the @background property.</summary>
            public const string Background = "background";
        }

        /// <summary>
        /// A resizing kernel. One of these can be given to operations like
        /// <see cref="Image.Reduce"/> or <see cref="Image.Resize"/> to select the resizing kernel to use.
        /// </summary>
        public static class Kernel
        {
            /// <summary>Nearest-neighbour interpolation.</summary>
            public const string Nearest = "nearest";

            /// <summary>Linear interpolation.</summary>
            public const string Linear = "linear";

            /// <summary>Cubic interpolation.</summary>
            public const string Cubic = "cubic";

            /// <summary>Two-lobe Lanczos.</summary>
            public const string Lanczos2 = "lanczos2";

            /// <summary>Three-lobe Lanczos.</summary>
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
            /// <summary>Integer.</summary>
            public const string Integer = "integer";

            /// <summary>Floating point.</summary>
            public const string Float = "float";

            /// <summary>Compute approximate result.</summary>
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
            /// <summary>Pixels are not coded.</summary>
            public const string None = "none";

            /// <summary>Pixels encode 3 float CIELAB values as 4 uchar.</summary>
            public const string Labq = "labq";

            /// <summary>Pixels encode 3 float RGB as 4 uchar (Radiance coding).</summary>
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
            /// <summary>left-right.</summary>
            public const string Horizontal = "horizontal";

            /// <summary>top-bottom.</summary>
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
            /// <summary>Align on the low coordinate edge.</summary>
            public const string Low = "low";

            /// <summary>Align on the centre.</summary>
            public const string Centre = "centre";

            /// <summary>Align on the high coordinate edge.</summary>
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
            /// <summary>Take the maximum of all values.</summary>
            public const string Max = "max";

            /// <summary>Take the sum of all values.</summary>
            public const string Sum = "sum";
        }

        /// <summary>
        /// Set Profile Connection Space.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.IccImport"/>.
        /// </remarks>
        public static class PCS
        {
            /// <summary>CIE Lab space.</summary>
            public const string Lab = "lab";

            /// <summary>CIE XYZ space.</summary>
            public const string Xyz = "xyz";
        }
    }
}