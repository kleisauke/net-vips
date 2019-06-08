namespace NetVips
{
    using System;
    using System.Runtime.InteropServices;
    using Internal;

    /// <summary>
    /// Wrap a <see cref="VipsRegion"/> object.
    /// </summary>
    /// <remarks>
    /// A region is a small part of an image. You use regions to read pixels
    /// out of images without storing the entire image in memory.
    /// At least libvips 8.8 is needed.
    /// </remarks>
    public class Region : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        private Region(IntPtr pointer)
            : base(pointer)
        {
            // logger.Debug($"Region = {pointer}");
        }

        /// <summary>
        /// Make a region on an image.
        /// </summary>
        /// <param name="image"><see cref="Image"/> to create this region on.</param>
        /// <returns>A new <see cref="Region"/>.</returns>
        /// <exception cref="VipsException">If unable to make a new region on <paramref name="image"/>.</exception>
        public static Region New(Image image)
        {
            // logger.Debug($"Region.New: image = {image}");
            var vi = VipsRegion.New(image);
            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to make region");
            }

            return new Region(vi);
        }

        /// <summary>
        /// Width of pixels held by region.
        /// </summary>
        public int Width => VipsRegion.Width(this);

        /// <summary>
        /// Height of pixels held by region.
        /// </summary>
        public int Height => VipsRegion.Height(this);

        /// <summary>
        /// Fetch an area of pixels.
        /// </summary>
        /// <param name="left">Left edge of area to fetch.</param>
        /// <param name="top">Top edge of area to fetch.</param>
        /// <param name="width">Width of area to fetch.</param>
        /// <param name="height">Height of area to fetch.</param>
        /// <returns>An array of bytes filled with pixel data.</returns>
        public byte[] Fetch(int left, int top, int width, int height)
        {
            var pointer = VipsRegion.Fetch(this, left, top, width, height, out var size);
            if (pointer == IntPtr.Zero)
            {
                throw new VipsException("unable to fetch from region");
            }

            var managedArray = new byte[size];
            Marshal.Copy(pointer, managedArray, 0, (int)size);

            GLib.GFree(pointer);

            return managedArray;
        }
    }
}