namespace NetVips.Extensions
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using Image = Image;

    /// <summary>
    /// Static class which provides conversion between <see cref="Bitmap"/> and <see cref="Image"/>.
    /// </summary>
    public static class BitmapConverter
    {
        /// <summary>
        /// Guess the number of bands for a <see cref="PixelFormat"/>.
        /// </summary>
        /// <remarks>
        /// GDI+ does not support 16bpp images very well (even though they are still in the enumeration).
        /// </remarks>
        /// <param name="pixelFormat"><see cref="PixelFormat"/> to guess for.</param>
        /// <returns>The number of bands.</returns>
        private static int GuessBands(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    /* Note: Maplut below will create a 3-band image */
                    return 1;
                /*case PixelFormat.Format16bppGrayScale:
                    return 2;*/
                /*case PixelFormat.Format1bppIndexed:*/
                /*case PixelFormat.Format4bppIndexed:*/
                /*case PixelFormat.Format16bppRgb555:*/
                /*case PixelFormat.Format16bppRgb565:*/
                case PixelFormat.Format24bppRgb:
                /*case PixelFormat.Format32bppRgb:*/
                case PixelFormat.Format48bppRgb:
                    return 3;
                /*case PixelFormat.Format16bppArgb1555:*/
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
        /// <remarks>
        /// GDI+ does not support 16bpp images very well (even though they are still in the enumeration).
        /// </remarks>
        /// <param name="pixelFormat"><see cref="PixelFormat"/> to guess for.</param>
        /// <returns>The <see cref="Enums.BandFormat"/>.</returns>
        private static Enums.BandFormat GuessBandFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                /*case PixelFormat.Format1bppIndexed:*/
                /*case PixelFormat.Format4bppIndexed:*/
                case PixelFormat.Format8bppIndexed:
                /*case PixelFormat.Format16bppGrayScale:*/
                /*case PixelFormat.Format16bppRgb555:*/
                /*case PixelFormat.Format16bppRgb565:*/
                case PixelFormat.Format24bppRgb:
                /*case PixelFormat.Format32bppRgb:*/
                /*case PixelFormat.Format16bppArgb1555:*/
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return Enums.BandFormat.Uchar;
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return Enums.BandFormat.Ushort;
                default:
                    throw new NotImplementedException($"GuessBandFormat({pixelFormat}) is not yet implemented.");
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

            // Let LockBits convert the pixel data to Format24bppRgb for indexed
            // (excluding Format8bppIndexed) and Format32bppRgb (the remaining
            // 8 bits are not used anyway) images. This is faster than the pixel
            // loops commented below and simplifies the code considerably.
            var pf =
                src.PixelFormat == PixelFormat.Format1bppIndexed ||
                src.PixelFormat == PixelFormat.Format4bppIndexed ||
                src.PixelFormat == PixelFormat.Format32bppRgb
                    ? PixelFormat.Format24bppRgb
                    : src.PixelFormat;

            var bands = GuessBands(pf);
            var format = GuessBandFormat(pf);
            var sizeofFormat = format == Enums.BandFormat.Uchar ? sizeof(byte) : sizeof(ushort);

            var w = src.Width;
            var h = src.Height;
            var stride = w * bands * sizeofFormat;
            var size = stride * h;

            var rect = new Rectangle(0, 0, w, h);
            BitmapData bd = null;
            Image dst;
            try
            {
                bd = src.LockBits(rect, ImageLockMode.ReadOnly, pf);

                switch (pf)
                {
                    /*case PixelFormat.Format1bppIndexed:
                    {
                        var buffer = new byte[size];

                        for (var y = 0; y < h; y++)
                        {
                            for (int bytePos = 0, x = 0; bytePos < bd.Stride; bytePos++)
                            {
                                if (x >= w)
                                    continue;

                                var b = Marshal.ReadByte(bd.Scan0, y * bd.Stride + bytePos);
                                for (var i = 0; i < 8; i++, x++)
                                {
                                    if (x >= w)
                                        break;

                                    var colorIndex = (b & 0x80) == 0x80 ? 1 : 0;
                                    buffer[y * stride + x * 3 + 0] = src.Palette.Entries[colorIndex].R;
                                    buffer[y * stride + x * 3 + 1] = src.Palette.Entries[colorIndex].G;
                                    buffer[y * stride + x * 3 + 2] = src.Palette.Entries[colorIndex].B;
                                    b <<= 1;
                                }
                            }
                        }

                        return Image.NewFromMemory(buffer, w, h, bands, format);
                    }
                    case PixelFormat.Format4bppIndexed:
                    {
                        var buffer = new byte[size];

                        for (var y = 0; y < h; y++)
                        {
                            for (var x = 0; x < w; x++)
                            {
                                var b = Marshal.ReadByte(bd.Scan0, y * bd.Stride + (x >> 1));

                                var colorIndex = (x & 1) == 0 ? b >> 4 : b & 0x0F;
                                buffer[y * stride + x * 3 + 0] = src.Palette.Entries[colorIndex].R;
                                buffer[y * stride + x * 3 + 1] = src.Palette.Entries[colorIndex].G;
                                buffer[y * stride + x * 3 + 2] = src.Palette.Entries[colorIndex].B;
                            }
                        }

                        return Image.NewFromMemory(buffer, w, h, bands, format);
                    }
                    case PixelFormat.Format32bppRgb:
                    {
                        var buffer = new byte[size];

                        for (var y = 0; y < h; y++)
                        {
                            for (var x = 0; x < w; x++)
                            {
                                // The remaining 8 bits are not used
                                Marshal.Copy(bd.Scan0 + y * bd.Stride + x * 4, buffer, y * stride + x * 3, 3);
                            }
                        }

                        dst = Image.NewFromMemory(buffer, w, h, bands, format);
                        break;
                    }*/
                    case PixelFormat.Format24bppRgb when bd.Stride == stride:
                    case PixelFormat.Format8bppIndexed when bd.Stride == stride:
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                    case PixelFormat.Format48bppRgb when bd.Stride == stride:
                    case PixelFormat.Format64bppArgb:
                    case PixelFormat.Format64bppPArgb:
                        // bd.Stride is aligned to a multiple of 4
                        dst = Image.NewFromMemoryCopy(bd.Scan0, (ulong)size, w, h, bands, format);
                        break;
                    default:
                    {
                        var buffer = new byte[size];

                        // Copy the bytes from src to the managed array for each scanline
                        for (var y = 0; y < h; y++)
                        {
                            Marshal.Copy(bd.Scan0 + y * bd.Stride, buffer, y * stride, stride);
                        }

                        dst = Image.NewFromMemory(buffer, w, h, bands, format);
                        break;
                    }
                }
            }
            finally
            {
                if (bd != null)
                    src.UnlockBits(bd);
            }

            if (pf == PixelFormat.Format8bppIndexed)
            {
                var palette = new byte[src.Palette.Entries.Length * 3];
                for (var i = 0; i < src.Palette.Entries.Length; i++)
                {
                    palette[i * 3 + 0] = src.Palette.Entries[i].R;
                    palette[i * 3 + 1] = src.Palette.Entries[i].G;
                    palette[i * 3 + 2] = src.Palette.Entries[i].B;
                }

                var lut = Image.NewFromMemory(palette, src.Palette.Entries.Length, 1, 3, Enums.BandFormat.Uchar);
                return dst.Maplut(lut);
            }

            switch (bands)
            {
                case 3:
                    // Switch from BGR to RGB
                    var bgr = dst.Bandsplit();
                    return bgr[2].Bandjoin(bgr[1], bgr[0]);
                case 4:
                    // Switch from BGRA to RGBA
                    var bgra = dst.Bandsplit();
                    return bgra[2].Bandjoin(bgra[1], bgra[0], bgra[3]);
                default:
                    return dst;
            }
        }

        /// <summary>
        /// Converts <see cref="System.Drawing.Image"/> to <see cref="Image"/>.
        /// </summary>
        /// <param name="src"><see cref="System.Drawing.Image"/> to be converted.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public static Image ToVips(this System.Drawing.Image src)
        {
            return ToVips((Bitmap)src);
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

            if (src.Interpretation == Enums.Interpretation.Bw)
            {
                // Convert to sRGB, since Format16bppGrayScale appears to be unsupported by GDI+.
                // See: https://stackoverflow.com/a/19706842/10952119
                src = src.Colourspace(Enums.Interpretation.Srgb);
            }
            else if (src.Bands == 2)
            {
                // Add an additional band
                src = src.Bandjoin(255);
            }

            if (src.Bands == 1 || src.Format != Enums.BandFormat.Ushort)
            {
                // Ensure image is casted to uint8 (unsigned char)
                src = src.Cast(Enums.BandFormat.Uchar);
            }

            PixelFormat pf;
            switch (src.Bands)
            {
                case 1:
                    pf = PixelFormat.Format8bppIndexed;
                    break;
                case 3:
                    pf = src.Format == Enums.BandFormat.Ushort
                        ? PixelFormat.Format48bppRgb
                        : PixelFormat.Format24bppRgb;

                    // Switch from RGB to BGR
                    var rgb = src.Bandsplit();
                    src = rgb[2].Bandjoin(rgb[1], rgb[0]);
                    break;
                case 4:
                    pf = src.Format == Enums.BandFormat.Ushort
                        ? PixelFormat.Format64bppArgb
                        : PixelFormat.Format32bppArgb;

                    // Switch from RGBA to BGRA
                    var rgba = src.Bandsplit();
                    src = rgba[2].Bandjoin(rgba[1], rgba[0], rgba[3]);
                    break;
                default:
                    throw new NotImplementedException(
                        $"Number of bands must be 1 or in the in the range of 3 to 4. Got: {src.Bands}");
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
                var dstSize = (ulong)(bd.Stride * h);
                var memory = src.WriteToMemory(out var srcSize);

                // bd.Stride is aligned to a multiple of 4
                if (dstSize == srcSize)
                {
                    unsafe
                    {
                        Buffer.MemoryCopy(memory.ToPointer(), bd.Scan0.ToPointer(), srcSize, srcSize);
                    }
                }
                else
                {
                    var offset = w * src.Bands;

                    // Copy the bytes from src to dst for each scanline
                    for (var y = 0; y < h; y++)
                    {
                        var pSrc = memory + y * offset;
                        var pDst = bd.Scan0 + y * bd.Stride;

                        unsafe
                        {
                            Buffer.MemoryCopy(pSrc.ToPointer(), pDst.ToPointer(), offset, offset);
                        }
                    }
                }

                NetVips.Free(memory);
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