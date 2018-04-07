extern alias Interop;

using System;
using System.Runtime.InteropServices;
using System.Security;
using Interop::NetVips.Interop;

namespace NetVips.Internal
{
    internal static class GLib
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_free")]
        internal static extern void GFree(IntPtr mem);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_malloc")]
        internal static extern IntPtr GMalloc(ulong nBytes);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_log_remove_handler")]
        internal static extern void
            GLogRemoveHandler([MarshalAs(UnmanagedType.LPStr)] string logDomain, uint handlerId);
    }
}