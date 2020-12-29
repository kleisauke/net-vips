namespace NetVips
{
    using System;

    /// <summary>
    /// This module contains the various libvips enums as C# classes
    /// Enums values are represented in NetVips as strings. These classes contain the valid strings for each enum.
    /// </summary>
    public static class Enums
    {
        #region semi-generated enums

        /// <summary>
        /// The type of access an operation has to supply.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Tilecache"/>.
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

            /// <summary>Top-to-bottom without a buffer.</summary>
            public const string SequentialUnbuffered = "sequential-unbuffered";
        }

        /// <summary>
        /// Various types of alignment.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Join"/>.
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

            /// <summary>Take the minimum value.</summary>
            public const string Min = "min";
        }

        /// <summary>
        /// How to combine pixels.
        /// </summary>
        /// <remarks>
        /// Operations like <see cref="Image.DrawImage"/> need to be told how to
        /// combine images from two sources. See also <see cref="Image.Join"/>.
        /// </remarks>
        public static class CombineMode
        {
            /// <summary>Set pixels to the new value.</summary>
            public const string Set = "set";

            /// <summary>Add pixels.</summary>
            public const string Add = "add";
        }

        /// <summary>
        /// A direction on a compass. Used for <see cref="Image.Gravity"/>, for example.
        /// </summary>
        public static class CompassDirection
        {
            /// <summary>Centre</summary>
            public const string Centre = "centre";

            /// <summary>North</summary>
            public const string North = "north";

            /// <summary>East</summary>
            public const string East = "east";

            /// <summary>South</summary>
            public const string South = "south";

            /// <summary>West</summary>
            public const string West = "west";

            /// <summary>North-east</summary>
            public const string NorthEast = "north-east";

            /// <summary>South-east</summary>
            public const string SouthEast = "south-east";

            /// <summary>South-west</summary>
            public const string SouthWest = "south-west";

            /// <summary>North-west</summary>
            public const string NorthWest = "north-west";
        }

        /// <summary>
        /// A hint about the kind of demand geometry VIPS images prefer.
        /// </summary>
        public static class DemandStyle
        {
            /// <summary>Demand in small (typically 64x64 pixel) tiles.</summary>
            public const string Smalltile = "smalltile";

            /// <summary>Demand in fat (typically 10 pixel high) strips.</summary>
            public const string Fatstrip = "fatstrip";

            /// <summary>Demand in thin (typically 1 pixel high) strips.</summary>
            public const string Thinstrip = "thinstrip";
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
        /// The container type of the pyramid.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Dzsave"/>.
        /// </remarks>
        public static class ForeignDzContainer
        {
            /// <summary>Write tiles to the filesystem.</summary>
            public const string Fs = "fs";

            /// <summary>Write tiles to a zip file.</summary>
            public const string Zip = "zip";

            /// <summary>Write to a szi file.</summary>
            public const string Szi = "szi";
        }

        /// <summary>
        /// How many pyramid layers to create.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Dzsave"/>.
        /// </remarks>
        public static class ForeignDzDepth
        {
            /// <summary>Create layers down to 1x1 pixel.</summary>
            public const string Onepixel = "onepixel";

            /// <summary>Create layers down to 1x1 tile.</summary>
            public const string Onetile = "onetile";

            /// <summary>Only create a single layer.</summary>
            public const string One = "one";
        }

        /// <summary>
        /// What directory layout and metadata standard to use.
        /// </summary>
        public static class ForeignDzLayout
        {
            /// <summary>Use DeepZoom directory layout.</summary>
            public const string Dz = "dz";

            /// <summary>Use Zoomify directory layout.</summary>
            public const string Zoomify = "zoomify";

            /// <summary>Use Google maps directory layout.</summary>
            public const string Google = "google";

            /// <summary>Use IIIF directory layout.</summary>
            public const string Iiif = "iiif";
        }

        /// <summary>
        /// The compression format to use inside a HEIF container.
        /// </summary>
        public static class ForeignHeifCompression
        {
            /// <summary>x265</summary>
            public const string Hevc = "hevc";

            /// <summary>x264</summary>
            public const string Avc = "avc";

            /// <summary>JPEG</summary>
            public const string Jpeg = "jpeg";

            /// <summary>AOM</summary>
            public const string Av1 = "av1";
        }

        /// <summary>
        /// The PNG filter to use.
        /// </summary>
        [Flags]
        public enum ForeignPngFilter
        {
          /// <summary>No filtering.</summary>
          None = 0x08, // "none"

          // <summary>Difference to the left.</summary>
          Sub = 0x10, // "sub"

          // <summary>Difference up.</summary>
          Up = 0x20, // "up"

          // <summary>Average of left and up.</summary>
          Avg = 0x40, // "avg"

          // <summary>Pick best neighbor predictor automatically.</summary>
          Paeth = 0x80, // "paeth"

          // <summary>Adaptive.</summary>
          All = 0xF8, // "all"
        }

        /// <summary>
        /// Set jpeg subsampling mode.
        /// </summary>
        public static class ForeignJpegSubsample
        {
            /// <summary>Default preset.</summary>
            public const string Auto = "auto";

            /// <summary>Always perform subsampling.</summary>
            public const string On = "on";

            /// <summary>Never perform subsampling.</summary>
            public const string Off = "off";
        }

        /// <summary>
        /// The compression types supported by the tiff writer.
        /// </summary>
        public static class ForeignTiffCompression
        {
            /// <summary>No compression.</summary>
            public const string None = "none";

            /// <summary>JPEG compression.</summary>
            public const string Jpeg = "jpeg";

            /// <summary>Deflate (zip) compression.</summary>
            public const string Deflate = "deflate";

            /// <summary>Packbits compression.</summary>
            public const string Packbits = "packbits";

            /// <summary>Fax4 compression.</summary>
            public const string Ccittfax4 = "ccittfax4";

            /// <summary>LZW compression.</summary>
            public const string Lzw = "lzw";

            /// <summary>WebP compression.</summary>
            public const string Webp = "webp";

            /// <summary>ZSTD compression.</summary>
            public const string Zstd = "zstd";
        }

        /// <summary>
        /// The predictor can help deflate and lzw compression.
        /// The values are fixed by the tiff library.
        /// </summary>
        public static class ForeignTiffPredictor
        {
            /// <summary>No prediction.</summary>
            public const string None = "none";

            /// <summary>Horizontal differencing.</summary>
            public const string Horizontal = "horizontal";

            /// <summary>Float predictor.</summary>
            public const string Float = "float";
        }

        /// <summary>
        /// Use inches or centimeters as the resolution unit for a tiff file.
        /// </summary>
        public static class ForeignTiffResunit
        {
            /// <summary>Use centimeters.</summary>
            public const string Cm = "cm";

            /// <summary>Use inches.</summary>
            public const string Inch = "inch";
        }

        /// <summary>
        /// Tune lossy encoder settings for different image types.
        /// </summary>
        public static class ForeignWebpPreset
        {
            /// <summary>Default preset.</summary>
            public const string Default = "default";

            /// <summary>Digital picture, like portrait, inner shot.</summary>
            public const string Picture = "picture";

            /// <summary>Outdoor photograph, with natural lighting.</summary>
            public const string Photo = "photo";

            /// <summary>Hand or line drawing, with high-contrast details.</summary>
            public const string Drawing = "drawing";

            /// <summary>Small-sized colorful images/</summary>
            public const string Icon = "icon";

            /// <summary>Text-like.</summary>
            public const string Text = "text";
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
        /// Pick the algorithm vips uses to decide image "interestingness".
        /// This is used by <see cref="Image.Smartcrop"/>, for example, to decide what parts of the image to keep.
        /// </summary>
        public static class Interesting
        {
            /// <summary>Do nothing.</summary>
            public const string None = "none";

            /// <summary>Just take the centre.</summary>
            public const string Centre = "centre";

            /// <summary>Use an entropy measure.</summary>
            public const string Entropy = "entropy";

            /// <summary>Look for features likely to draw human attention.</summary>
            public const string Attention = "attention";

            /// <summary>Position the crop towards the low coordinate.</summary>
            public const string Low = "low";

            /// <summary>Position the crop towards the high coordinate.</summary>
            public const string High = "high";
			
            /// <summary>Everything is interesting.</summary>
            public const string All = "all";
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

            /// <summary>Mitchell</summary>
            public const string Mitchell = "mitchell";

            /// <summary>Two-lobe Lanczos.</summary>
            public const string Lanczos2 = "lanczos2";

            /// <summary>Three-lobe Lanczos.</summary>
            public const string Lanczos3 = "lanczos3";
        }

        /// <summary>
        /// Boolean operations.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Boolean"/>.
        /// </remarks>
        public static class OperationBoolean
        {
            /// <summary>&amp;</summary>
            public const string And = "and";

            /// <summary>|</summary>
            public const string Or = "or";

            /// <summary>^</summary>
            public const string Eor = "eor";

            /// <summary>&lt;&lt;</summary>
            public const string Lshift = "lshift";

            /// <summary>&gt;&gt;</summary>
            public const string Rshift = "rshift";
        }

        /// <summary>
        /// Operations on complex images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Complex"/>.
        /// </remarks>
        public static class OperationComplex
        {
            /// <summary>Convert to polar coordinates.</summary>
            public const string Polar = "polar";

            /// <summary>Convert to rectangular coordinates.</summary>
            public const string Rect = "rect";

            /// <summary>Complex conjugate.</summary>
            public const string Conj = "conj";
        }

        /// <summary>
        /// Binary operations on complex images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Complex2"/>.
        /// </remarks>
        public static class OperationComplex2
        {
            /// <summary>Convert to polar coordinates.</summary>
            public const string CrossPhase = "cross-phase";
        }

        /// <summary>
        /// Components of complex images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Complexget"/>.
        /// </remarks>
        public static class OperationComplexget
        {
            /// <summary>Get real component.</summary>
            public const string Real = "real";

            /// <summary>Get imaginary component.</summary>
            public const string Imag = "imag";
        }

        /// <summary>
        /// Various math functions on images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Math"/>.
        /// </remarks>
        public static class OperationMath
        {
            /// <summary>sin(), angles in degrees.</summary>
            public const string Sin = "sin";

            /// <summary>cos(), angles in degrees.</summary>
            public const string Cos = "cos";

            /// <summary>tan(), angles in degrees.</summary>
            public const string Tan = "tan";

            /// <summary>asin(), angles in degrees.</summary>
            public const string Asin = "asin";

            /// <summary>acos(), angles in degrees.</summary>
            public const string Acos = "acos";

            /// <summary>atan(), angles in degrees.</summary>
            public const string Atan = "atan";

            /// <summary>log base e.</summary>
            public const string Log = "log";

            /// <summary>log base 10.</summary>
            public const string Log10 = "log10";

            /// <summary>e to the something.</summary>
            public const string Exp = "exp";

            /// <summary>10 to the something.</summary>
            public const string Exp10 = "exp10";
        }

        /// <summary>
        /// Various math functions on images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Math"/>.
        /// </remarks>
        public static class OperationMath2
        {
            /// <summary>pow( left, right ).</summary>
            public const string Pow = "pow";

            /// <summary>pow( right, left ).</summary>
            public const string Wop = "wop";
        }

        /// <summary>
        /// Morphological operations.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Morph"/>.
        /// </remarks>
        public static class OperationMorphology
        {
            /// <summary>true if all set.</summary>
            public const string Erode = "erode";

            /// <summary>true if one set.</summary>
            public const string Dilate = "dilate";
        }

        /// <summary>
        /// Various relational operations.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Relational"/>.
        /// </remarks>
        public static class OperationRelational
        {
            /// <summary>==</summary>
            public const string Equal = "equal";

            /// <summary>!=</summary>
            public const string Noteq = "noteq";

            /// <summary>&lt;</summary>
            public const string Less = "less";

            /// <summary>&lt;=</summary>
            public const string Lesseq = "lesseq";

            /// <summary>&gt;</summary>
            public const string More = "more";

            /// <summary>&gt;=</summary>
            public const string Moreeq = "moreeq";
        }

        /// <summary>
        /// Round operations.
        /// </summary>
        public static class OperationRound
        {
            /// <summary>Round to nearest.</summary>
            public const string Rint = "rint";

            /// <summary>The smallest integral value not less than.</summary>
            public const string Ceil = "ceil";

            /// <summary>Largest integral value not greater than.</summary>
            public const string Floor = "floor";
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
        /// How to calculate the output pixels when shrinking a 2x2 region.
        /// </summary>
        public static class RegionShrink
        {
            /// <summary>Use the average.</summary>
            public const string Mean = "mean";

            /// <summary>Use the median.</summary>
            public const string Median = "median";

            /// <summary>Use the mode.</summary>
            public const string Mode = "mode";

            /// <summary>Use the maximum.</summary>
            public const string Max = "max";

            /// <summary>Use the minimum.</summary>
            public const string Min = "min";

            /// <summary>Use the top-left pixel.</summary>
            public const string Nearest = "nearest";
        }

        /// <summary>
        /// Some hints about the image saver.
        /// </summary>
        public static class Saveable
        {
            /// <summary>1 band (eg. CSV)</summary>
            public const string Mono = "mono";

            /// <summary>1 or 3 bands (eg. PPM)</summary>
            public const string Rgb = "rgb";

            /// <summary>1, 2, 3 or 4 bands (eg. PNG)</summary>
            public const string Rgba = "rgba";

            /// <summary>3 or 4 bands (eg. WEBP)</summary>
            public const string RgbaOnly = "rgba-only";

            /// <summary>1, 3 or 4 bands (eg.JPEG)</summary>
            public const string RgbCmyk = "rgb-cmyk";

            /// <summary>Any number of bands (eg. TIFF)</summary>
            public const string Any = "any";
        }

        /// <summary>
        /// Controls whether an operation should upsize, downsize, both up and downsize, or force a size.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Thumbnail"/>.
        /// </remarks>
        public static class Size
        {
            /// <summary>Size both up and down.</summary>
            public const string Both = "both";

            /// <summary>Only upsize.</summary>
            public const string Up = "up";

            /// <summary>Only downsize.</summary>
            public const string Down = "down";

            /// <summary>Force size, that is, break aspect ratio.</summary>
            public const string Force = "force";
        }

        #endregion

        /// <summary>
        /// Flags specifying the level of log messages.
        /// </summary>
        [Flags]
        public enum LogLevelFlags
        {
            #region Internal log flags

            /// <summary>Internal flag.</summary>
            FlagRecursion = 1/* << 0*/,

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
            LevelMask = unchecked((int) 0xFFFFFFFC)

            #endregion
        }

        /// <summary>
        /// Flags we associate with each object argument.
        /// </summary>
        [Flags]
        public enum ArgumentFlags
        {
            /// <summary>No flags.</summary>
            NONE = 0,

            /// <summary>Must be set in the constructor.</summary>
            REQUIRED = 1,

            /// <summary>Can only be set in the constructor.</summary>
            CONSTRUCT = 2,

            /// <summary>Can only be set once.</summary>
            SET_ONCE = 4,

            /// <summary>Don't do use-before-set checks.</summary>
            SET_ALWAYS = 8,

            /// <summary>Is an input argument (one we depend on).</summary>
            INPUT = 16,

            /// <summary>Is an output argument (depends on us).</summary>
            OUTPUT = 32,

            /// <summary>Just there for back-compat, hide.</summary>
            DEPRECATED = 64,

            /// <summary>The input argument will be modified.</summary>
            MODIFY = 128
        }

        /// <summary>
        /// Flags we associate with an <see cref="Operation"/>.
        /// </summary>
        [Flags]
        public enum OperationFlags
        {
            /// <summary>No flags.</summary>
            NONE = 0,

            /// <summary>Can work sequentially with a small buffer.</summary>
            SEQUENTIAL = 1,

            /// <summary>Can work sequentially without a buffer.</summary>
            SEQUENTIAL_UNBUFFERED = 2,

            /// <summary>Must not be cached.</summary>
            NOCACHE = 4,

            /// <summary>A compatibility thing.</summary>
            DEPRECATED = 8
        }

        /// <summary>
        /// Signals that can be used on an <see cref="Image"/>. See <see cref="GObject.SignalConnect"/>.
        /// </summary>
        public static class Signals
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
    }
}
