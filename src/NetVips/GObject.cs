namespace NetVips
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Internal;
    using GSignalMatchType = Internal.Enums.GSignalMatchType;

    /// <summary>
    /// Manage <see cref="Internal.GObject"/> lifetime.
    /// </summary>
    public class GObject : SafeHandle
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// We have to record all of the <see cref="SignalConnect{T}"/> delegates to
        /// prevent them from being re-located or disposed of by the garbage collector.
        /// </summary>
        /// <remarks>
        /// All recorded delegates are freed in <see cref="ReleaseDelegates"/>.
        /// </remarks>
        private readonly ICollection<GCHandle> _handles = new List<GCHandle>();

        /// <summary>
        /// Hint of how much native memory is actually occupied by the object.
        /// </summary>
        internal long MemoryPressure;

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
        /// <typeparam name="T">The type of the callback to connect.</typeparam>
        /// <param name="detailedSignal">A string of the form "signal-name::detail".</param>
        /// <param name="callback">The callback to connect.</param>
        /// <param name="data">Data to pass to handler calls.</param>
        /// <returns>The handler id.</returns>
        /// <exception cref="T:System.ArgumentException">If it failed to connect the signal.</exception>
        public ulong SignalConnect<T>(string detailedSignal, T callback, IntPtr data = default)
            where T : notnull
        {
            // add a weak reference callback to ensure all handles are released on finalization
            if (_handles.Count == 0)
            {
                GWeakNotify notify = ReleaseDelegates;
                var notifyHandle = GCHandle.Alloc(notify);

                Internal.GObject.WeakRef(this, notify, GCHandle.ToIntPtr(notifyHandle));
            }

            // prevent the delegate from being re-located or disposed of by the garbage collector
            var delegateHandle = GCHandle.Alloc(callback);
            _handles.Add(delegateHandle);

            var cHandler = Marshal.GetFunctionPointerForDelegate(callback);
            var ret = GSignal.ConnectData(this, detailedSignal, cHandler, data, null, default);
            if (ret == 0)
            {
                throw new ArgumentException("Failed to connect signal " + detailedSignal);
            }

            return ret;
        }

        /// <summary>
        /// Disconnects a handler from this object.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="handlerId"/> is 0 then this function does nothing.
        /// </remarks>
        /// <param name="handlerId">Handler id of the handler to be disconnected.</param>
        public void SignalHandlerDisconnect(ulong handlerId)
        {
            if (handlerId != 0)
            {
                GSignal.HandlerDisconnect(this, handlerId);
            }
        }

        /// <summary>
        /// Disconnects all handlers from this object that match <paramref name="func"/> and
        /// <paramref name="data"/>.
        /// </summary>
        /// <typeparam name="T">The type of the func.</typeparam>
        /// <param name="func">The func of the handlers.</param>
        /// <param name="data">The data of the handlers.</param>
        /// <returns>The number of handlers that matched.</returns>
        public uint SignalHandlersDisconnectByFunc<T>(T func, IntPtr data = default)
            where T : notnull
        {
            var funcPtr = Marshal.GetFunctionPointerForDelegate(func);
            return GSignal.HandlersDisconnectMatched(this,
                GSignalMatchType.G_SIGNAL_MATCH_FUNC | GSignalMatchType.G_SIGNAL_MATCH_DATA,
                0, 0, IntPtr.Zero, funcPtr, data);
        }

        /// <summary>
        /// Disconnects all handlers from this object that match <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data of the handlers.</param>
        /// <returns>The number of handlers that matched.</returns>
        public uint SignalHandlersDisconnectByData(IntPtr data)
        {
            return GSignal.HandlersDisconnectMatched(this,
                GSignalMatchType.G_SIGNAL_MATCH_DATA,
                0, 0, IntPtr.Zero, IntPtr.Zero, data);
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
            }
            // NObjects--;

            return true;
        }

        /// <summary>
        /// Release all the <see cref="SignalConnect{T}"/> delegates by this object on finalization.
        /// </summary>
        /// <remarks>
        /// This function is only called when <see cref="SignalConnect{T}"/> was used on this object.
        /// </remarks>
        /// <param name="data">Data that was provided when the weak reference was established.</param>
        /// <param name="objectPointer">The object being disposed.</param>
        internal void ReleaseDelegates(IntPtr data, IntPtr objectPointer)
        {
            foreach (var gcHandle in _handles)
            {
                if (gcHandle.IsAllocated)
                {
                    gcHandle.Free();
                }
            }

            // All GCHandles are free'd. Clear the list to prevent inadvertent use.
            _handles.Clear();

            // Free the GCHandle used by this GWeakNotify
            var notifyHandle = GCHandle.FromIntPtr(data);
            if (notifyHandle.IsAllocated)
            {
                notifyHandle.Free();
            }
        }

        /// <summary>
        /// Increases the reference count of object.
        /// </summary>
        internal IntPtr ObjectRef()
        {
            // logger.Debug($"Ref: GObject = {handle}");
            return Internal.GObject.Ref(handle);
        }

        /// <summary>
        /// Gets a value indicating whether the handle is invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the handle is not valid; otherwise, <see langword="false"/>.</returns>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Get the reference count of object. Handy for debugging.
        /// </summary>
        internal uint RefCount => Marshal.PtrToStructure<Internal.GObject.Struct>(handle).RefCount;

        // Do not provide a finalizer - SafeHandle's critical finalizer will
        // call ReleaseHandle for us.
    }
}