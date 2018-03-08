using System;
using NLog;

namespace NetVips
{
    /// <summary>
    /// Manage <see cref="NetVips.Internal.GObject"/> lifetime.
    /// </summary>
    public class GObject : IDisposable
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        public Internal.GObject IntlGObject;

        // Track whether Dispose has been called.
        private bool _disposed;

        /// <summary>
        /// Wrap around a pointer.
        /// </summary>
        /// <remarks>
        /// Wraps a GObject instance around an underlying GValue. When the
        /// instance is garbage-collected, the underlying object is unreferenced.
        /// </remarks>
        /// <param name="gObject"></param>
        protected GObject(Internal.GObject gObject)
        {
            // record the GValue we were given to manage
            IntlGObject = gObject;
            // logger.Debug($"GValue = {gObject}");
        }

        ~GObject()
        {
            // Do not re-create Dispose clean-up code here.
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged resources
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            // logger.Debug($"GC: GObject = {IntlGObject}");
            IntlGObject.Dispose();
            // logger.Debug($"GC: GObject = {IntlGObject}");
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // Dispose unmanaged resources.
                ReleaseUnmanagedResources();

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }
    }
}