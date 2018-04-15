using System;

namespace NetVips
{
    /// <summary>
    /// Manage <see cref="Internal.GObject"/> lifetime.
    /// </summary>
    public class GObject : IDisposable
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        internal IntPtr Pointer;

        // Handy for debugging
        // public static int NObjects;

        /// <summary>
        /// Wrap around a pointer.
        /// </summary>
        /// <remarks>
        /// Wraps a GObject instance around an underlying GValue. When the
        /// instance is garbage-collected, the underlying object is unreferenced.
        /// </remarks>
        /// <param name="pointer"></param>
        internal GObject(IntPtr pointer)
        {
            // record the pointer we were given to manage
            Pointer = pointer;
            // NObjects++;
            // logger.Debug($"GObject = {pointer}");
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup 
        /// operations before it is reclaimed by garbage collection.
        /// </summary>
        ~GObject()
        {
            // Do not re-create Dispose clean-up code here.
            Dispose(false);
        }

        /// <summary>
        /// Increases the reference count of object.
        /// </summary>
        internal void ObjectRef()
        {
            // logger.Debug($"Ref: GObject = {Pointer}");
            Internal.GObject.GObjectRef(Pointer);
        }

        /// <summary>
        /// Decreases the reference count of object. 
        /// When its reference count drops to 0, the object is finalized (i.e. its memory is freed).
        /// </summary>
        internal void ObjectUnref()
        {
            // logger.Debug($"Unref: GObject = {Pointer}");
            Internal.GObject.GObjectUnref(Pointer);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // logger.Debug($"GC: GObject = {Pointer}");
            if (Pointer != IntPtr.Zero)
            {
                // on GC, unref
                ObjectUnref();

                Pointer = IntPtr.Zero;
            }

            // NObjects--;
            // logger.Debug($"GC: GObject = {Pointer}");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, 
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }
    }
}