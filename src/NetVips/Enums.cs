namespace NetVips
{
    using System;

    /// <summary>
    /// This module contains the various libvips enums.
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
        public enum Access
        {
            /// <summary>Requests can come in any order.</summary>
            Random = 0, // "random"

            /// <summary>
            /// Means requests will be top-to-bottom, but with some
            /// amount of buffering behind the read point for small non-local
            /// accesses.
            /// </summary>
            Sequential = 1, // "sequential"

            /// <summary>Top-to-bottom without a buffer.</summary>
            SequentialUnbuffered = 2 // "sequential-unbuffered"
        }

        /// <summary>
        /// Various types of alignment.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Join"/>.
        /// </remarks>
        public enum Align
        {
            /// <summary>Align on the low coordinate edge.</summary>
            Low = 0, // "low"

            /// <summary>Align on the centre.</summary>
            Centre = 1, // "centre"

            /// <summary>Align on the high coordinate edge.</summary>
            High = 2 // "high"
        }

        /// <summary>
        /// Various fixed 90 degree rotation angles.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Rot"/>.
        /// </remarks>
        public enum Angle
        {
            /// <summary>No rotate.</summary>
            D0 = 0, // "d0"

            /// <summary>90 degrees clockwise.</summary>
            D90 = 1, // "d90"

            /// <summary>180 degrees.</summary>
            D180 = 2, // "d180"

            /// <summary>90 degrees anti-clockwise.</summary>
            D270 = 3 // "d270"
        }

        /// <summary>
        /// Various fixed 45 degree rotation angles.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Rot45"/>.
        /// </remarks>
        public enum Angle45
        {
            /// <summary>No rotate.</summary>
            D0 = 0, // "d0"

            /// <summary>45 degrees clockwise.</summary>
            D45 = 1, // "d45"

            /// <summary>90 degrees clockwise.</summary>
            D90 = 2, // "d90"

            /// <summary>135 degrees clockwise.</summary>
            D135 = 3, // "d135"

            /// <summary>180 degrees.</summary>
            D180 = 4, // "d180"

            /// <summary>135 degrees anti-clockwise.</summary>
            D225 = 5, // "d225"

            /// <summary>90 degrees anti-clockwise.</summary>
            D270 = 6, // "d270"

            /// <summary>45 degrees anti-clockwise.</summary>
            D315 = 7 // "d315"
        }

        /// <summary>
        /// The format of image bands.
        /// </summary>
        /// <remarks>
        /// The format used for each band element. Each corresponds to a native C type
        /// for the current machine.
        /// </remarks>
        public enum BandFormat
        {
            /// <summary>Invalid setting.</summary>
            Notset = -1, // "notset"

            /// <summary>unsigned char format.</summary>
            Uchar = 0, // "uchar"

            /// <summary>char format.</summary>
            Char = 1, // "char"

            /// <summary>unsigned short format.</summary>
            Ushort = 2, // "ushort"

            /// <summary>short format.</summary>
            Short = 3, // "short"

            /// <summary>unsigned int format.</summary>
            Uint = 4, // "uint"

            /// <summary>int format.</summary>
            Int = 5, // "int"

            /// <summary>float format.</summary>
            Float = 6, // "float"

            /// <summary>complex (two floats) format.</summary>
            Complex = 7, // "complex"

            /// <summary>double float format.</summary>
            Double = 8, // "double"

            /// <summary>double complex (two double) format.</summary>
            Dpcomplex = 9 // "dpcomplex"
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
        public enum BlendMode
        {
            /// <summary>Where the second object is drawn, the first is removed.</summary>
            Clear = 0, // "clear"

            /// <summary>The second object is drawn as if nothing were below.</summary>
            Source = 1, // "source"

            /// <summary>The image shows what you would expect if you held two semi-transparent slides on top of each other.</summary>
            Over = 2, // "over"

            /// <summary>The first object is removed completely, the second is only drawn where the first was.</summary>
            In = 3, // "in"

            /// <summary>The second is drawn only where the first isn't.</summary>
            Out = 4, // "out"

            /// <summary>This leaves the first object mostly intact, but mixes both objects in the overlapping area.</summary>
            Atop = 5, // "atop"

            /// <summary>Leaves the first object untouched, the second is discarded completely.</summary>
            Dest = 6, // "dest"

            /// <summary>Like Over, but swaps the arguments.</summary>
            DestOver = 7, // "dest-over"

            /// <summary>Like In, but swaps the arguments.</summary>
            DestIn = 8, // "dest-in"

            /// <summary>Like Out, but swaps the arguments.</summary>
            DestOut = 9, // "dest-out"

            /// <summary>Like Atop, but swaps the arguments.</summary>
            DestAtop = 10, // "dest-atop"

            /// <summary>Something like a difference operator.</summary>
            Xor = 11, // "xor"

            /// <summary>A bit like adding the two images.</summary>
            Add = 12, // "add"

            /// <summary>A bit like the darker of the two.</summary>
            Saturate = 13, // "saturate"

            /// <summary>At least as dark as the darker of the two inputs.</summary>
            Multiply = 14, // "multiply"

            /// <summary>At least as light as the lighter of the inputs.</summary>
            Screen = 15, // "screen"

            /// <summary>Multiplies or screens colors, depending on the lightness.</summary>
            Overlay = 16, // "overlay"

            /// <summary>The darker of each component.</summary>
            Darken = 17, // "darken"

            /// <summary>The lighter of each component.</summary>
            Lighten = 18, // "lighten"

            /// <summary>Brighten first by a factor second.</summary>
            ColourDodge = 19, // "colour-dodge"

            /// <summary>Darken first by a factor of second.</summary>
            ColourBurn = 20, // "colour-burn"

            /// <summary>Multiply or screen, depending on lightness.</summary>
            HardLight = 21, // "hard-light"

            /// <summary>Darken or lighten, depending on lightness.</summary>
            SoftLight = 22, // "soft-light"

            /// <summary>Difference of the two.</summary>
            Difference = 23, // "difference"

            /// <summary>Somewhat like Difference, but lower-contrast.</summary>
            Exclusion = 24 // "exclusion"
        }

        /// <summary>
        /// How pixels are coded.
        /// </summary>
        /// <remarks>
        /// Normally, pixels are uncoded and can be manipulated as you would expect.
        /// However some file formats code pixels for compression, and sometimes it's
        /// useful to be able to manipulate images in the coded format.
        /// </remarks>
        public enum Coding
        {
            /// <summary>Invalid setting.</summary>
            Error = -1, // "error"

            /// <summary>Pixels are not coded.</summary>
            None = 0, // "none"

            /// <summary>Pixels encode 3 float CIELAB values as 4 uchar.</summary>
            Labq = 2, // "labq"

            /// <summary>Pixels encode 3 float RGB as 4 uchar (Radiance coding).</summary>
            Rad = 6 // "rad"
        }

        /// <summary>
        /// How to combine passes.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Compass"/>.
        /// </remarks>
        public enum Combine
        {
            /// <summary>Take the maximum of all values.</summary>
            Max = 0, // "max"

            /// <summary>Take the sum of all values.</summary>
            Sum = 1, // "sum"

            /// <summary>Take the minimum value.</summary>
            Min = 2 // "min"
        }

        /// <summary>
        /// How to combine pixels.
        /// </summary>
        /// <remarks>
        /// Operations like <see cref="MutableImage.DrawImage"/> need to be told how to
        /// combine images from two sources. See also <see cref="Image.Join"/>.
        /// </remarks>
        public enum CombineMode
        {
            /// <summary>Set pixels to the new value.</summary>
            Set = 0, // "set"

            /// <summary>Add pixels.</summary>
            Add = 1 // "add"
        }

        /// <summary>
        /// A direction on a compass. Used for <see cref="Image.Gravity"/>, for example.
        /// </summary>
        public enum CompassDirection
        {
            /// <summary>Centre</summary>
            Centre = 0, // "centre"

            /// <summary>North</summary>
            North = 1, // "north"

            /// <summary>East</summary>
            East = 2, // "east"

            /// <summary>South</summary>
            South = 3, // "south"

            /// <summary>West</summary>
            West = 4, // "west"

            /// <summary>North-east</summary>
            NorthEast = 5, // "north-east"

            /// <summary>South-east</summary>
            SouthEast = 6, // "south-east"

            /// <summary>South-west</summary>
            SouthWest = 7, // "south-west"

            /// <summary>North-west</summary>
            NorthWest = 8 // "north-west"
        }

        /// <summary>
        /// A hint about the kind of demand geometry VIPS images prefer.
        /// </summary>
        public enum DemandStyle
        {
            /// <summary>Invalid setting.</summary>
            Error = -1, // "error"

            /// <summary>Demand in small (typically 64x64 pixel) tiles.</summary>
            Smalltile = 0, // "smalltile"

            /// <summary>Demand in fat (typically 10 pixel high) strips.</summary>
            Fatstrip = 1, // "fatstrip"

            /// <summary>Demand in thin (typically 1 pixel high) strips.</summary>
            Thinstrip = 2 // "thinstrip"
        }

        /// <summary>
        /// A direction.
        /// </summary>
        /// <remarks>
        /// Operations like <see cref="Image.Flip"/> need to be told whether to flip
        /// left-right or top-bottom.
        /// </remarks>
        public enum Direction
        {
            /// <summary>left-right.</summary>
            Horizontal = 0, // "horizontal"

            /// <summary>top-bottom.</summary>
            Vertical = 1 // "vertical"
        }

        /// <summary>
        /// How to extend image edges.
        /// </summary>
        /// <remarks>
        /// When the edges of an image are extended, you can specify how you want
        /// the extension done. See <see cref="Image.Embed"/>, <see cref="Image.Conv"/>, <see cref="Image.Affine"/>
        /// and so on.
        /// </remarks>
        public enum Extend
        {
            /// <summary>New pixels are black, ie. all bits are zero.</summary>
            Black = 0, // "black"

            /// <summary>Each new pixel takes the value of the nearest edge pixel.</summary>
            Copy = 1, // "copy"

            /// <summary>The image is tiled to fill the new area.</summary>
            Repeat = 2, // "repeat"

            /// <summary>The image is reflected and tiled to reduce hash edges.</summary>
            Mirror = 3, // "mirror"

            /// <summary>New pixels are white, ie. all bits are set.</summary>
            White = 4, // "white"

            /// <summary>Colour set from the @background property.</summary>
            Background = 5 // "background"
        }

        /// <summary>
        /// How sensitive loaders are to errors, from never stop (very insensitive), to
        /// stop on the smallest warning (very sensitive).
        ///
        /// Each one implies the ones before it, so <see cref="Error"/> implies
        /// <see cref="Truncated"/>.
        /// </summary>
        public enum FailOn
        {
            /// <summary>Never stop,</summary>
            None = 0, // "none"

            /// <summary>Stop on image truncated, nothing else.</summary>
            Truncated = 1, // "truncated"

            /// <summary>Stop on serious error or truncation.</summary>
            Error = 2, // "error"

            /// <summary>Stop on anything, even warnings.</summary>
            Warning = 3 // "warning"
        }

        /// <summary>
        /// The container type of the pyramid.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Dzsave"/>.
        /// </remarks>
        public enum ForeignDzContainer
        {
            /// <summary>Write tiles to the filesystem.</summary>
            Fs = 0, // "fs"

            /// <summary>Write tiles to a zip file.</summary>
            Zip = 1, // "zip"

            /// <summary>Write to a szi file.</summary>
            Szi = 2 // "szi"
        }

        /// <summary>
        /// How many pyramid layers to create.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Dzsave"/>.
        /// </remarks>
        public enum ForeignDzDepth
        {
            /// <summary>Create layers down to 1x1 pixel.</summary>
            Onepixel = 0, // "onepixel"

            /// <summary>Create layers down to 1x1 tile.</summary>
            Onetile = 1, // "onetile"

            /// <summary>Only create a single layer.</summary>
            One = 2 // "one"
        }

        /// <summary>
        /// What directory layout and metadata standard to use.
        /// </summary>
        public enum ForeignDzLayout
        {
            /// <summary>Use DeepZoom directory layout.</summary>
            Dz = 0, // "dz"

            /// <summary>Use Zoomify directory layout.</summary>
            Zoomify = 1, // "zoomify"

            /// <summary>Use Google maps directory layout.</summary>
            Google = 2, // "google"

            /// <summary>Use IIIF v2 directory layout.</summary>
            Iiif = 3, // "iiif"

            /// <summary>Use IIIF v3 directory layout</summary>
            Iiif3 = 4 // "iiif3"
        }

        /// <summary>
        /// The compression format to use inside a HEIF container.
        /// </summary>
        public enum ForeignHeifCompression
        {
            /// <summary>x265</summary>
            Hevc = 1, // "hevc"

            /// <summary>x264</summary>
            Avc = 2, // "avc"

            /// <summary>JPEG</summary>
            Jpeg = 3, // "jpeg"

            /// <summary>AOM</summary>
            Av1 = 4 // "av1"
        }

        /// <summary>
        /// The PNG filter to use.
        /// </summary>
        [Flags]
        public enum ForeignPngFilter
        {
            /// <summary>No filtering.</summary>
            None = 0x08, // "none"

            /// <summary>Difference to the left.</summary>
            Sub = 0x10, // "sub"

            /// <summary>Difference up.</summary>
            Up = 0x20, // "up"

            /// <summary>Average of left and up.</summary>
            Avg = 0x40, // "avg"

            /// <summary>Pick best neighbor predictor automatically.</summary>
            Paeth = 0x80, // "paeth"

            /// <summary>Adaptive.</summary>
            All = None | Sub | Up | Avg | Paeth // "all"
        }

        /// <summary>
        /// Which metadata to retain.
        /// </summary>
        [Flags]
        public enum ForeignKeep
        {
            /// <summary>Don't attach metadata.</summary>
            None = 0, // "none"

            /// <summary>Keep Exif metadata.</summary>
            Exif = 1 << 0, // "exif"

            /// <summary>Keep XMP metadata.</summary>
            Xmp = 1 << 1, // "xmp"

            /// <summary>Keep IPTC metadata.</summary>
            Iptc = 1 << 2, // "iptc"

            /// <summary>Keep ICC metadata.</summary>
            Icc = 1 << 3, // "icc"

            /// <summary>Keep other metadata (e.g. PNG comments and some TIFF tags)</summary>
            Other = 1 << 4, // "other"

            /// <summary>Keep all metadata.</summary>
            All = Exif | Xmp | Iptc | Icc | Other // "all"
        }

        /// <summary>
        /// The selected encoder to use.
        /// </summary>
        /// <remarks>
        /// If libheif hasn't been compiled with the selected encoder, it will
        /// fallback to the default encoder based on <see cref="ForeignHeifCompression"/>.
        /// </remarks>
        public enum ForeignHeifEncoder
        {
            /// <summary>Pick encoder automatically.</summary>
            Auto = 0, // "auto"

            /// <summary>AOM</summary>
            Aom = 1, // "aom"

            /// <summary>RAV1E</summary>
            Rav1e = 2, // "rav1e"

            /// <summary>SVT-AV1</summary>
            Svt = 3, // "svt"

            /// <summary>x265</summary>
            X265 = 4 // "x265"
        }

        /// <summary>
        /// The netpbm file format to save as.
        /// </summary>
        public enum ForeignPpmFormat
        {
            /// <summary>Images are single bit.</summary>
            Pbm = 0, // "pbm"

            /// <summary>Images are 8, 16, or 32-bits, one band.</summary>
            Pgm = 1, // "pgm"

            /// <summary>Images are 8, 16, or 32-bits, three bands.</summary>
            Ppm = 2, // "ppm"

            /// <summary>Images are 32-bit float pixels.</summary>
            Pfm = 3, // "pfm"

            /// <summary>Images are anymap images -- the image format is used to pick the saver.</summary>
            Pnm = 4 // "pnm"
        }

        /// <summary>
        /// Set JPEG/HEIF subsampling mode.
        /// </summary>
        public enum ForeignSubsample
        {
            /// <summary>Prevent subsampling when quality > 90.</summary>
            Auto = 0, // "auto"

            /// <summary>Always perform subsampling.</summary>
            On = 1, // "on"

            /// <summary>Never perform subsampling.</summary>
            Off = 2 // "off"
        }

        /// <summary>
        /// The compression types supported by the tiff writer.
        /// </summary>
        public enum ForeignTiffCompression
        {
            /// <summary>No compression.</summary>
            None = 0, // "none"

            /// <summary>JPEG compression.</summary>
            Jpeg = 1, // "jpeg"

            /// <summary>Deflate (zip) compression.</summary>
            Deflate = 2, // "deflate"

            /// <summary>Packbits compression.</summary>
            Packbits = 3, // "packbits"

            /// <summary>Fax4 compression.</summary>
            Ccittfax4 = 4, // "ccittfax4"

            /// <summary>LZW compression.</summary>
            Lzw = 5, // "lzw"

            /// <summary>WebP compression.</summary>
            Webp = 6, // "webp"

            /// <summary>ZSTD compression.</summary>
            Zstd = 7, // "zstd"

            /// <summary>JP2K compression.</summary>
            Jp2k = 8 // "jp2k"
        }

        /// <summary>
        /// The predictor can help deflate and lzw compression.
        /// The values are fixed by the tiff library.
        /// </summary>
        public enum ForeignTiffPredictor
        {
            /// <summary>No prediction.</summary>
            None = 1, // "none"

            /// <summary>Horizontal differencing.</summary>
            Horizontal = 2, // "horizontal"

            /// <summary>Float predictor.</summary>
            Float = 3 // "float"
        }

        /// <summary>
        /// Use inches or centimeters as the resolution unit for a tiff file.
        /// </summary>
        public enum ForeignTiffResunit
        {
            /// <summary>Use centimeters.</summary>
            Cm = 0, // "cm"

            /// <summary>Use inches.</summary>
            Inch = 1 // "inch"
        }

        /// <summary>
        /// Tune lossy encoder settings for different image types.
        /// </summary>
        public enum ForeignWebpPreset
        {
            /// <summary>Default preset.</summary>
            Default = 0, // "default"

            /// <summary>Digital picture, like portrait, inner shot.</summary>
            Picture = 1, // "picture"

            /// <summary>Outdoor photograph, with natural lighting.</summary>
            Photo = 2, // "photo"

            /// <summary>Hand or line drawing, with high-contrast details.</summary>
            Drawing = 3, // "drawing"

            /// <summary>Small-sized colorful images/</summary>
            Icon = 4, // "icon"

            /// <summary>Text-like.</summary>
            Text = 5 // "text"
        }

        /// <summary>
        /// The rendering intent.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.IccTransform"/>.
        /// </remarks>
        public enum Intent
        {
            /// <summary>Perceptual rendering intent.</summary>
            Perceptual = 0, // "perceptual"

            /// <summary>Relative colorimetric rendering intent.</summary>
            Relative = 1, // "relative"

            /// <summary>Saturation rendering intent.</summary>
            Saturation = 2, // "saturation"

            /// <summary>Absolute colorimetric rendering intent.</summary>
            Absolute = 3 // "absolute"
        }

        /// <summary>
        /// Pick the algorithm vips uses to decide image "interestingness".
        /// This is used by <see cref="O:Image.Smartcrop"/>, for example, to decide what parts of the image to keep.
        /// </summary>
        public enum Interesting
        {
            /// <summary>Do nothing.</summary>
            None = 0, // "none"

            /// <summary>Just take the centre.</summary>
            Centre = 1, // "centre"

            /// <summary>Use an entropy measure.</summary>
            Entropy = 2, // "entropy"

            /// <summary>Look for features likely to draw human attention.</summary>
            Attention = 3, // "attention"

            /// <summary>Position the crop towards the low coordinate.</summary>
            Low = 4, // "low"

            /// <summary>Position the crop towards the high coordinate.</summary>
            High = 5, // "high"

            /// <summary>Everything is interesting.</summary>
            All = 6 // "all"
        }

        /// <summary>
        /// How the values in an image should be interpreted.
        /// </summary>
        /// <remarks>
        /// For example, a three-band float image of type LAB should have its
        /// pixels interpreted as coordinates in CIE Lab space.
        /// </remarks>
        public enum Interpretation
        {
            /// <summary>Invalid setting.</summary>
            Error = -1, // "error"

            /// <summary>Generic many-band image.</summary>
            Multiband = 0, // "multiband"

            /// <summary>Some kind of single-band image.</summary>
            Bw = 1, // "b-w"

            /// <summary>A 1D image, eg. histogram or lookup table.</summary>
            Histogram = 10, // "histogram"

            /// <summary>The first three bands are CIE XYZ.</summary>
            Xyz = 12, // "xyz"

            /// <summary>Pixels are in CIE Lab space.</summary>
            Lab = 13, // "lab"

            /// <summary>The first four bands are in CMYK space.</summary>
            Cmyk = 15, // "cmyk"

            /// <summary>Implies <see cref="Coding.Labq"/>.</summary>
            Labq = 16, // "labq"

            /// <summary>Generic RGB space.</summary>
            Rgb = 17, // "rgb"

            /// <summary>A uniform colourspace based on CMC(1:1).</summary>
            Cmc = 18, // "cmc"

            /// <summary>Pixels are in CIE LCh space.</summary>
            Lch = 19, // "lch"

            /// <summary>CIE LAB coded as three signed 16-bit values.</summary>
            Labs = 21, // "labs"

            /// <summary>Pixels are sRGB.</summary>
            Srgb = 22, // "srgb"

            /// <summary>Pixels are CIE Yxy.</summary>
            Yxy = 23, // "yxy"

            /// <summary>Image is in fourier space.</summary>
            Fourier = 24, // "fourier"

            /// <summary>Generic 16-bit RGB.</summary>
            Rgb16 = 25, // "rgb16"

            /// <summary>Generic 16-bit mono.</summary>
            Grey16 = 26, // "grey16"

            /// <summary>A matrix.</summary>
            Matrix = 27, // "matrix"

            /// <summary>Pixels are scRGB.</summary>
            Scrgb = 28, // "scrgb"

            /// <summary>Pixels are HSV.</summary>
            Hsv = 29 // "hsv"
        }

        /// <summary>
        /// A resizing kernel. One of these can be given to operations like
        /// <see cref="Image.Reduce"/> or <see cref="Image.Resize"/> to select the resizing kernel to use.
        /// </summary>
        public enum Kernel
        {
            /// <summary>Nearest-neighbour interpolation.</summary>
            Nearest = 0, // "nearest"

            /// <summary>Linear interpolation.</summary>
            Linear = 1, // "linear"

            /// <summary>Cubic interpolation.</summary>
            Cubic = 2, // "cubic"

            /// <summary>Mitchell</summary>
            Mitchell = 3, // "mitchell"

            /// <summary>Two-lobe Lanczos.</summary>
            Lanczos2 = 4, // "lanczos2"

            /// <summary>Three-lobe Lanczos.</summary>
            Lanczos3 = 5 // "lanczos3"
        }

        /// <summary>
        /// Boolean operations.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Boolean"/>.
        /// </remarks>
        public enum OperationBoolean
        {
            /// <summary>&amp;</summary>
            And = 0, // "and"

            /// <summary>|</summary>
            Or = 1, // "or"

            /// <summary>^</summary>
            Eor = 2, // "eor"

            /// <summary>&lt;&lt;</summary>
            Lshift = 3, // "lshift"

            /// <summary>&gt;&gt;</summary>
            Rshift = 4 // "rshift"
        }

        /// <summary>
        /// Operations on complex images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Complex"/>.
        /// </remarks>
        public enum OperationComplex
        {
            /// <summary>Convert to polar coordinates.</summary>
            Polar = 0, // "polar"

            /// <summary>Convert to rectangular coordinates.</summary>
            Rect = 1, // "rect"

            /// <summary>Complex conjugate.</summary>
            Conj = 2 // "conj"
        }

        /// <summary>
        /// Binary operations on complex images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Complex2"/>.
        /// </remarks>
        public enum OperationComplex2
        {
            /// <summary>Convert to polar coordinates.</summary>
            CrossPhase = 0 // "cross-phase"
        }

        /// <summary>
        /// Components of complex images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Complexget"/>.
        /// </remarks>
        public enum OperationComplexget
        {
            /// <summary>Get real component.</summary>
            Real = 0, // "real"

            /// <summary>Get imaginary component.</summary>
            Imag = 1 // "imag"
        }

        /// <summary>
        /// Various math functions on images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Math"/>.
        /// </remarks>
        public enum OperationMath
        {
            /// <summary>sin(), angles in degrees.</summary>
            Sin = 0, // "sin"

            /// <summary>cos(), angles in degrees.</summary>
            Cos = 1, // "cos"

            /// <summary>tan(), angles in degrees.</summary>
            Tan = 2, // "tan"

            /// <summary>asin(), angles in degrees.</summary>
            Asin = 3, // "asin"

            /// <summary>acos(), angles in degrees.</summary>
            Acos = 4, // "acos"

            /// <summary>atan(), angles in degrees.</summary>
            Atan = 5, // "atan"

            /// <summary>log base e.</summary>
            Log = 6, // "log"

            /// <summary>log base 10.</summary>
            Log10 = 7, // "log10"

            /// <summary>e to the something.</summary>
            Exp = 8, // "exp"

            /// <summary>10 to the something.</summary>
            Exp10 = 9, // "exp10"

            /// <summary>sinh(), angles in radians.</summary>
            Sinh = 10, // "sinh"

            /// <summary>cosh(), angles in radians.</summary>
            Cosh = 11, // "cosh"

            /// <summary>tanh(), angles in radians.</summary>
            Tanh = 12, // "tanh"

            /// <summary>asinh(), angles in radians.</summary>
            Asinh = 13, // "asinh"

            /// <summary>acosh(), angles in radians.</summary>
            Acosh = 14, // "acosh"

            /// <summary>atanh(), angles in radians.</summary>
            Atanh = 15 // "atanh"
        }

        /// <summary>
        /// Various math functions on images.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Math2"/>.
        /// </remarks>
        public enum OperationMath2
        {
            /// <summary>pow( left, right ).</summary>
            Pow = 0, // "pow"

            /// <summary>pow( right, left ).</summary>
            Wop = 1, // "wop"

            /// <summary>atan2( left, right )</summary>
            Atan2 = 2 // "atan2"
        }

        /// <summary>
        /// Morphological operations.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Morph"/>.
        /// </remarks>
        public enum OperationMorphology
        {
            /// <summary>true if all set.</summary>
            Erode = 0, // "erode"

            /// <summary>true if one set.</summary>
            Dilate = 1 // "dilate"
        }

        /// <summary>
        /// Various relational operations.
        /// </summary>
        /// <remarks>
        /// See <see cref="Image.Relational"/>.
        /// </remarks>
        public enum OperationRelational
        {
            /// <summary>==</summary>
            Equal = 0, // "equal"

            /// <summary>!=</summary>
            Noteq = 1, // "noteq"

            /// <summary>&lt;</summary>
            Less = 2, // "less"

            /// <summary>&lt;=</summary>
            Lesseq = 3, // "lesseq"

            /// <summary>&gt;</summary>
            More = 4, // "more"

            /// <summary>&gt;=</summary>
            Moreeq = 5 // "moreeq"
        }

        /// <summary>
        /// Round operations.
        /// </summary>
        public enum OperationRound
        {
            /// <summary>Round to nearest.</summary>
            Rint = 0, // "rint"

            /// <summary>The smallest integral value not less than.</summary>
            Ceil = 1, // "ceil"

            /// <summary>Largest integral value not greater than.</summary>
            Floor = 2 // "floor"
        }

        /// <summary>
        /// Set Profile Connection Space.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.IccImport"/>.
        /// </remarks>
        public enum PCS
        {
            /// <summary>CIE Lab space.</summary>
            Lab = 0, // "lab"

            /// <summary>CIE XYZ space.</summary>
            Xyz = 1 // "xyz"
        }

        /// <summary>
        /// Computation precision.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Conv"/>.
        /// </remarks>
        public enum Precision
        {
            /// <summary>Integer.</summary>
            Integer = 0, // "integer"

            /// <summary>Floating point.</summary>
            Float = 1, // "float"

            /// <summary>Compute approximate result.</summary>
            Approximate = 2 // "approximate"
        }

        /// <summary>
        /// How to calculate the output pixels when shrinking a 2x2 region.
        /// </summary>
        public enum RegionShrink
        {
            /// <summary>Use the average.</summary>
            Mean = 0, // "mean"

            /// <summary>Use the median.</summary>
            Median = 1, // "median"

            /// <summary>Use the mode.</summary>
            Mode = 2, // "mode"

            /// <summary>Use the maximum.</summary>
            Max = 3, // "max"

            /// <summary>Use the minimum.</summary>
            Min = 4, // "min"

            /// <summary>Use the top-left pixel.</summary>
            Nearest = 5 // "nearest"
        }

        /// <summary>
        /// Controls whether an operation should upsize, downsize, both up and downsize, or force a size.
        /// </summary>
        /// <remarks>
        /// See for example <see cref="Image.Thumbnail"/>.
        /// </remarks>
        public enum Size
        {
            /// <summary>Size both up and down.</summary>
            Both = 0, // "both"

            /// <summary>Only upsize.</summary>
            Up = 1, // "up"

            /// <summary>Only downsize.</summary>
            Down = 2, // "down"

            /// <summary>Force size, that is, break aspect ratio.</summary>
            Force = 3 // "force"
        }

        /// <summary>
        /// Sets the word wrapping style for <see cref="O:Image.Text"/> when used with a maximum width.
        /// </summary>
        public enum TextWrap
        {
            /// <summary>Wrap at word boundaries.</summary>
            Word = 0, // "word"

            /// <summary>Wrap at character boundaries.</summary>
            Char = 1, // "char"

            /// <summary>Wrap at word boundaries, but fall back to character boundaries if there is not enough space for a full word.</summary>
            WordChar = 2, // "word-char"

            /// <summary>No wrapping.</summary>
            None = 3 // "none"
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
            FlagRecursion = 1 << 0,

            /// <summary>Internal flag.</summary>
            FlagFatal = 1 << 1,

            #endregion

            #region GLib log levels

            /// <summary>Log level for errors.</summary>
            Error = 1 << 2, // always fatal

            /// <summary>Log level for critical warning messages.</summary>
            Critical = 1 << 3,

            /// <summary>Log level for warnings.</summary>
            Warning = 1 << 4,

            /// <summary>Log level for messages.</summary>
            Message = 1 << 5,

            /// <summary>Log level for informational messages.</summary>
            Info = 1 << 6,

            /// <summary>Log level for debug messages.</summary>
            Debug = 1 << 7,

            #endregion

            #region Convenience values

            /// <summary>All log levels except fatal.</summary>
            AllButFatal = All & ~FlagFatal,

            /// <summary>All log levels except recursion.</summary>
            AllButRecursion = All & ~FlagRecursion,

            /// <summary>All log levels.</summary>
            All = FlagMask | Error | Critical | Warning | Message | Info | Debug,

            /// <summary>Flag mask.</summary>
            FlagMask = ~LevelMask,

            /// <summary>A mask including all log levels.</summary>
            LevelMask = ~(FlagRecursion | FlagFatal),

            /// <summary>A mask with log levels that are considered fatal by default.</summary>
            FatalMask = FlagRecursion | Error

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
            REQUIRED = 1 << 0,

            /// <summary>Can only be set in the constructor.</summary>
            CONSTRUCT = 1 << 1,

            /// <summary>Can only be set once.</summary>
            SET_ONCE = 1 << 2,

            /// <summary>Don't do use-before-set checks.</summary>
            SET_ALWAYS = 1 << 3,

            /// <summary>Is an input argument (one we depend on).</summary>
            INPUT = 1 << 4,

            /// <summary>Is an output argument (depends on us).</summary>
            OUTPUT = 1 << 5,

            /// <summary>Just there for back-compat, hide.</summary>
            DEPRECATED = 1 << 6,

            /// <summary>The input argument will be modified.</summary>
            MODIFY = 1 << 7
        }

        /// <summary>
        /// Flags we associate with an <see cref="Operation"/>.
        /// </summary>
        [Flags]
        public enum OperationFlags : uint
        {
            /// <summary>No flags.</summary>
            NONE = 0,

            /// <summary>Can work sequentially with a small buffer.</summary>
            SEQUENTIAL = 1 << 0,

            /// <summary>Can work sequentially without a buffer.</summary>
            SEQUENTIAL_UNBUFFERED = 1 << 1,

            /// <summary>Must not be cached.</summary>
            NOCACHE = 1 << 2,

            /// <summary>A compatibility thing.</summary>
            DEPRECATED = 1 << 3,

            /// <summary>Not hardened for untrusted input.</summary>
            UNTRUSTED = 1 << 4,

            /// <summary>Prevent this operation from running.</summary>
            BLOCKED = 1 << 5
        }

        /// <summary>
        /// Flags we associate with a file load operation.
        /// </summary>
        [Flags]
        public enum ForeignFlags : uint
        {
            /// <summary>No flags set.</summary>
            NONE = 0,

            /// <summary>Lazy read OK (eg. tiled tiff).</summary>
            PARTIAL = 1 << 0,

            /// <summary>Most-significant byte first.</summary>
            BIGENDIAN = 1 << 1,

            /// <summary>Top-to-bottom lazy read OK.</summary>
            SEQUENTIAL = 1 << 2,

            /// <summary>All flags set.</summary>
            ALL = 1 << 3
        }

        /// <summary>
        /// Signals that can be used on an <see cref="Image"/>. See <see cref="GObject.SignalConnect{T}"/>.
        /// </summary>
        public enum Signals
        {
            /// <summary>Evaluation is starting.</summary>
            /// <remarks>
            /// The preeval signal is emitted once before computation of <see cref="Image"/>
            /// starts. It's a good place to set up evaluation feedback.
            /// </remarks>
            PreEval = 0, // "preeval"

            /// <summary>Evaluation progress.</summary>
            /// <remarks>
            /// The eval signal is emitted once per work unit (typically a 128 x
            /// 128 area of pixels) during image computation.
            ///
            /// You can use this signal to update user-interfaces with progress
            /// feedback. Beware of updating too frequently: you will usually
            /// need some throttling mechanism.
            /// </remarks>
            Eval = 1, // "eval"

            /// <summary>Evaluation is ending.</summary>
            /// <remarks>
            /// The posteval signal is emitted once at the end of the computation
            /// of <see cref="Image"/>. It's a good place to shut down evaluation feedback.
            /// </remarks>
            PostEval = 2 // "posteval"
        }
    }
}