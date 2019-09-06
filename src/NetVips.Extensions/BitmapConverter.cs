namespace NetVips.Extensions
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using Image = Image;

    /// <summary>
    /// Static class which provides conversion between <see cref="Bitmap"/> and <see cref="Image"/>.
    /// </summary>
    public static class BitmapConverter
    {
        /// <summary>
        /// Number of bytes for a band format.
        /// </summary>
        private static readonly Dictionary<string, int> SizeofBandFormat = new Dictionary<string, int>
        {
            {Enums.BandFormat.Uchar, sizeof(byte)},
            {Enums.BandFormat.Char, sizeof(char)},
            {Enums.BandFormat.Ushort, sizeof(ushort)},
            {Enums.BandFormat.Short, sizeof(short)},
            {Enums.BandFormat.Uint, sizeof(uint)},
            {Enums.BandFormat.Int, sizeof(int)},
            {Enums.BandFormat.Float, sizeof(float)},
            {Enums.BandFormat.Complex, 2 * sizeof(float)},
            {Enums.BandFormat.Double, sizeof(double)},
            {Enums.BandFormat.Dpcomplex, 2 * sizeof(double)},
        };

        /// <summary>
        /// Guess the number of bands for a <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="pixelFormat"><see cref="PixelFormat"/> to guess for.</param>
        /// <returns>The number of bands.</returns>
        private static int GuessBands(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                    return 1;
                case PixelFormat.Format16bppGrayScale:
                    return 2;
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format48bppRgb:
                    return 3;
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 4;
                default:
                    throw new NotImplementedException($"GuessBands({pixelFormat}) is not yet implemented.");
            }
        }

        /// <summary>
        /// Guess the <see cref="Enums.BandFormat"/> for a <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="pixelFormat"><see cref="PixelFormat"/> to guess for.</param>
        /// <returns>The <see cref="Enums.BandFormat"/>.</returns>
        private static string GuessBandFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return Enums.BandFormat.Uchar;
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return Enums.BandFormat.Ushort;
                default:
                    throw new NotImplementedException($"GuessBands({pixelFormat}) is not yet implemented.");
            }
        }

        /// <summary>
        /// Converts <see cref="Bitmap"/> to <see cref="Image"/>.
        /// </summary>
        /// <param name="src"><see cref="Bitmap"/> to be converted.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public static Image ToVips(this Bitmap src)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            var bands = GuessBands(src.PixelFormat);
            var format = GuessBandFormat(src.PixelFormat);

            var w = src.Width;
            var h = src.Height;

            var rect = new Rectangle(0, 0, w, h);
            BitmapData bd = null;
            var memory = new byte[w * h * bands * SizeofBandFormat[format]];
            try
            {
                bd = src.LockBits(rect, ImageLockMode.ReadOnly, src.PixelFormat);
                Marshal.Copy(bd.Scan0, memory, 0, memory.Length);
            }
            finally
            {
                if (bd != null)
                    src.UnlockBits(bd);
            }

            var dst = Image.NewFromMemory(memory, w, h, bands, format);

            // Switch from BGR to RGB
            if (bands == 3)
            {
                var images = dst.Bandsplit();
                var t = images[0];
                images[0] = images[2];
                images[2] = t;
                dst = (Image)Operation.Call("bandjoin", new object[] { images });
            }

            return dst;
        }

        /// <summary>
        /// Converts <see cref="Image"/> to <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="src"><see cref="Image"/> to be converted.</param>
        /// <returns>A new <see cref="Bitmap"/>.</returns>
        public static Bitmap ToBitmap(this Image src)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            PixelFormat pf;
            switch (src.Bands)
            {
                case 1:
                    pf = PixelFormat.Format8bppIndexed;
                    break;
                case 2:
                    // Note: Format16bppGrayScale appears to be unsupported by GDI+.
                    // See: https://stackoverflow.com/a/19706842/10952119
                    pf = PixelFormat.Format16bppGrayScale;

                    // pf = PixelFormat.Format8bppIndexed;
                    // src = src[0];
                    break;
                case 3:
                    pf = src.Format == Enums.BandFormat.Ushort
                        ? PixelFormat.Format48bppRgb
                        : PixelFormat.Format24bppRgb;

                    // Switch from RGB to BGR
                    var bands = src.Bandsplit();
                    var t = bands[0];
                    bands[0] = bands[2];
                    bands[2] = t;
                    src = (Image)Operation.Call("bandjoin", new object[] { bands });
                    break;
                case 4:
                    pf = src.Format == Enums.BandFormat.Ushort
                        ? PixelFormat.Format64bppArgb
                        : PixelFormat.Format32bppArgb;
                    break;
                default:
                    throw new NotImplementedException(
                        $"Number of bands must be in the range of 1 to 4. Got: {src.Bands}");
            }

            var dst = new Bitmap(src.Width, src.Height, pf);

            // We need to generate a greyscale palette for 8bpp images
            if (pf == PixelFormat.Format8bppIndexed)
            {
                var plt = dst.Palette;
                for (var x = 0; x < 256; x++)
                {
                    plt.Entries[x] = Color.FromArgb(x, x, x);
                }

                dst.Palette = plt;
            }

            var w = src.Width;
            var h = src.Height;
            var rect = new Rectangle(0, 0, w, h);
            BitmapData bd = null;

            try
            {
                bd = dst.LockBits(rect, ImageLockMode.WriteOnly, pf);

                var memory = src.WriteToMemory();
                Marshal.Copy(memory, 0, bd.Scan0, memory.Length);
            }
            finally
            {
                if (bd != null)
                    dst.UnlockBits(bd);
            }

            return dst;
        }
    }
}