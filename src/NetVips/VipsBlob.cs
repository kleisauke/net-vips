namespace NetVips
{
    using System;

    /// <summary>
    /// Manage a <see cref="Internal.VipsBlob"/>.
    /// </summary>
    internal class VipsBlob : GObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc cref="GObject"/>
        internal VipsBlob(IntPtr pointer)
            : base(pointer)
        {
            // logger.Debug($"VipsBlob = {pointer}");
        }

        /// <summary>
        /// Get the data from a <see cref="Internal.VipsBlob"/>.
        /// </summary>
        /// <param name="length">return number of bytes of data</param>
        /// <returns>A <see cref="IntPtr"/> containing the data.</returns>
        internal IntPtr GetData(out ulong length)
        {
            return Internal.VipsBlob.Get(this, out length);
        }

        /// <inheritdoc cref="GObject"/>
        protected override void Dispose(bool disposing)
        {
            // Free the VipsArea
            Internal.VipsBlob.Unref(this);

            // Call our base Dispose method
            base.Dispose(disposing);
        }
    }
}