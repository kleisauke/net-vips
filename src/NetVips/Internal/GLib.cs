namespace NetVips.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using NetVips.Interop;

    internal static class GLib
    {
        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void LogFuncNative(IntPtr logDomain, NetVips.Enums.LogLevelFlags flags, IntPtr message,
            IntPtr userData);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_free")]
        internal static extern void GFree(IntPtr mem);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_malloc")]
        internal static extern IntPtr GMalloc(ulong nBytes);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_log_set_handler")]
        internal static extern uint GLogSetHandler([MarshalAs(UnmanagedType.LPStr)] string logDomain,
            NetVips.Enums.LogLevelFlags flags, LogFuncNative logFunc, IntPtr userData);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_log_remove_handler")]
        internal static extern void
            GLogRemoveHandler([MarshalAs(UnmanagedType.LPStr)] string logDomain, uint handlerId);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_log_set_always_fatal")]
        internal static extern NetVips.Enums.LogLevelFlags GLogSetAlwaysFatal(NetVips.Enums.LogLevelFlags fatalMask);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_log_set_fatal_mask")]
        internal static extern NetVips.Enums.LogLevelFlags GLogSetFatalMask(
            [MarshalAs(UnmanagedType.LPStr)] string logDomain, NetVips.Enums.LogLevelFlags fatalMask);
    }
}