using System.Runtime.InteropServices;
using System.Security;
using NetVips.Interop;

using LogLevelFlags = NetVips.Enums.LogLevelFlags;

namespace NetVips.Internal;

internal static class GLib
{
    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void LogFuncNative([MarshalAs(UnmanagedType.LPStr)] string logDomain,
        LogLevelFlags flags, nint message, nint userData);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "g_free")]
    internal static extern void GFree(nint mem);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "g_malloc")]
    internal static extern nint GMalloc(ulong nBytes);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "g_log_set_handler")]
    internal static extern uint GLogSetHandler([MarshalAs(UnmanagedType.LPStr)] string logDomain,
        LogLevelFlags flags, LogFuncNative logFunc, nint userData);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "g_log_remove_handler")]
    internal static extern void
        GLogRemoveHandler([MarshalAs(UnmanagedType.LPStr)] string logDomain, uint handlerId);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "g_log_set_always_fatal")]
    internal static extern LogLevelFlags GLogSetAlwaysFatal(LogLevelFlags fatalMask);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "g_log_set_fatal_mask")]
    internal static extern LogLevelFlags GLogSetFatalMask(
        [MarshalAs(UnmanagedType.LPStr)] string logDomain, LogLevelFlags fatalMask);
}