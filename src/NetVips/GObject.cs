namespace NetVips
{
    using System;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using global::NetVips.Internal;

    /// <summary>
    /// Manage <see cref="Internal.GObject"/> lifetime.
    /// </summary>
    public class GObject : SafeHandle
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// We have to record all of the <see cref="SignalConnect"/> delegates to
        /// prevent them from being re-located or disposed of by the garbage collector.
        /// </summary>
        private readonly List<GCHandle> _handles = new List<GCHandle>();

        // Handy for debugging
        // public static int NObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="GObject"/> class
        /// with the specified pointer to wrap around.
        /// </summary>
        /// <remarks>
        /// Wraps a GObject instance around an underlying GValue. When the
        /// instance is garbage-collected, the underlying object is unreferenced.
        /// </remarks>
        /// <param name="pointer">The pointer to wrap around.</param>
        internal GObject(IntPtr pointer)
            : base(IntPtr.Zero, true)
        {
            // record the pointer we were given to manage
            SetHandle(pointer);

            // NObjects++;
            // logger.Debug($"GObject = {pointer}");
        }

        /// <summary>
        /// Connects a callback function (<paramref name="callback"/>) to a signal on this object.
        /// </summary>
        /// <remarks>
        /// The callback will be triggered every time this signal is issued on this instance.
        /// </remarks>
        /// <param name="detailedSignal">A string of the form "signal-name::detail".</param>
        /// <param name="callback">The callback to connect.</param>
        /// <param name="data">Data to pass to handler calls.</param>
        /// <returns>The handler id.</returns>
        /// <exception cref="T:System.Exception">If it failed to connect the signal.</exception>
        public uint SignalConnect(string detailedSignal, Delegate callback, IntPtr data = default)
        {
            // prevent the delegate from being re-located or disposed of by the garbage collector
            var delegateHandle = GCHandle.Alloc(callback);
            _handles.Add(delegateHandle);

            // get the pointer for the delegate which can be passed to the native code
            var callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
            var ret = GSignal.ConnectData(this, detailedSignal, callbackPtr, data, null, default);

            if (ret == 0)
            {
                throw new Exception("Failed to connect signal.");
            }

            return ret;
        }

        /// <summary>
        /// Decreases the reference count of object.
        /// When its reference count drops to 0, the object is finalized (i.e. its memory is freed).
        /// </summary>
        /// <returns><see langword="true"/> if the handle is released successfully; otherwise,
        /// in the event of a catastrophic failure, <see langword="false"/>.</returns>
        protected override bool ReleaseHandle()
        {
            // logger.Debug($"Unref: GObject = {handle}");
            if (!IsInvalid)
            {
                Internal.GObject.Unref(handle);

                // release all handles recorded by this object
                foreach (var gcHandle in _handles)
                {
                    gcHandle.Free();
                }

                _handles.Clear();
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
            Internal.GObject.Ref(handle);
        }

        /// <summary>
        /// Gets a value indicating whether the handle is invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the handle is not valid; otherwise, <see langword="false"/>.</returns>
        public override bool IsInvalid => handle == IntPtr.Zero;

        // Do not provide a finalizer - SafeHandle's critical finalizer will
        // call ReleaseHandle for us.
    }
}