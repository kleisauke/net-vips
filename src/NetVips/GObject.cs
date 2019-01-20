using System;
using System.Runtime.InteropServices;

namespace NetVips
{
    /// <summary>
    /// Manage <see cref="Internal.GObject"/> lifetime.
    /// </summary>
    public class GObject : SafeHandle
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

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
        internal GObject(IntPtr pointer) : base(IntPtr.Zero, true)
        {
            // record the pointer we were given to manage
            SetHandle(pointer);

            // NObjects++;
            // logger.Debug($"GObject = {pointer}");
        }

        /// <summary>
        /// Decreases the reference count of object.
        /// When its reference count drops to 0, the object is finalized (i.e. its memory is freed).
        /// </summary>
        /// <returns><see langword="true" /> if the handle is released successfully; otherwise,
        /// in the event of a catastrophic failure, <see langword="false" />.</returns>
        protected override bool ReleaseHandle()
        {
            // logger.Debug($"Unref: GObject = {handle}");
            if (!IsInvalid)
            {
                Internal.GObject.GObjectUnref(handle);
            }
            // NObjects--;

            return true;
        }

        /// <summary>
        /// Increases the reference count of object.
        /// </summary>
        internal void ObjectRef()
        {
            // logger.Debug($"Ref: GObject = {handle}");
            Internal.GObject.GObjectRef(handle);
        }

        /// <summary>
        /// Gets a value that indicates whether the handle is invalid.
        /// </summary>
        /// <returns><see langword="true" /> if the handle is not valid; otherwise, <see langword="false" />.</returns>
        public override bool IsInvalid => handle == IntPtr.Zero;

        // Do not provide a finalizer - SafeHandle's critical finalizer will
        // call ReleaseHandle for us.
    }
}