using System;
using NetVips.AutoGen;
using NLog;

namespace NetVips
{
    /// <summary>
    /// Manage GObject lifetime.
    /// </summary>
    public class GObject : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public AutoGen.GObject Pointer;

        /// <summary>
        /// Wrap around a pointer.
        /// </summary>
        /// <remarks>
        /// Wraps a GObject instance around an underlying GValue. When the
        /// instance is garbage-collected, the underlying object is unreferenced.
        /// </remarks>
        /// <param name="pointer"></param>
        public GObject(AutoGen.GObject pointer)
        {
            // record the GValue we were given to manage
            Pointer = pointer;
            // logger.Debug($"GObject: GValue = {pointer}");
        }

        ~GObject()
        {
            Dispose(false);
        }

        private void ReleaseUnmanagedResources()
        {
            // on GC, unref
            // logger.Debug($"GObject GC: GValue = {Pointer}");
            gobject.GObjectUnref(Pointer.__Instance);
            // logger.Debug($"GObject GC: GValue = {Pointer}");
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                Pointer?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}