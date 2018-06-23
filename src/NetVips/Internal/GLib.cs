using System;
using System.Runtime.InteropServices;
using System.Security;
using NetVips.Interop;

namespace NetVips.Internal
{
    internal static class GLib
    {
        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void LogFuncNative(IntPtr logDomain, NetVips.Enums.LogLevelFlags flags, IntPtr message,
            IntPtr userData);

        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void PrintFuncNative(IntPtr message);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_free")]
        internal static extern void GFree(IntPtr mem);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_malloc")]
        internal static extern IntPtr GMalloc(UIntPtr nBytes);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_logv")]
        internal static extern void GLogv([MarshalAs(UnmanagedType.LPStr)] string logDomain,
            NetVips.Enums.LogLevelFlags flags,
            IntPtr message);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_log_set_handler")]
        internal static extern uint GLogSetHandler([MarshalAs(UnmanagedType.LPStr)] string logDomain,
            NetVips.Enums.LogLevelFlags flags,
            LogFuncNative logFunc, IntPtr userData);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_log_remove_handler")]
        internal static extern void
            GLogRemoveHandler([MarshalAs(UnmanagedType.LPStr)] string logDomain, uint handlerId);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_log_set_default_handler")]
        internal static extern LogFuncNative GLogSetDefaultHandler(LogFuncNative logFunc, IntPtr userData);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_log_default_handler")]
        internal static extern void GLogDefaultHandler([MarshalAs(UnmanagedType.LPStr)] string logDomain,
            NetVips.Enums.LogLevelFlags logLevel, IntPtr message, IntPtr unusedData);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_set_print_handler")]
        internal static extern PrintFuncNative GSetPrintHandler(PrintFuncNative handler);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_set_printerr_handler")]
        internal static extern PrintFuncNative GSetPrinterrHandler(PrintFuncNative handler);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_log_set_always_fatal")]
        internal static extern NetVips.Enums.LogLevelFlags GLogSetAlwaysFatal(NetVips.Enums.LogLevelFlags fatalMask);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_log_set_fatal_mask")]
        internal static extern NetVips.Enums.LogLevelFlags GLogSetFatalMask(
            [MarshalAs(UnmanagedType.LPStr)] string logDomain,
            NetVips.Enums.LogLevelFlags fatalMask);
    }
}