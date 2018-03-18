using System;
using System.Runtime.InteropServices;
using System.Security;

namespace NetVips.Internal
{
    public static class GLib
    {
        private struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport(Interop.Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_free")]
            internal static extern void GFree(IntPtr mem);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(Interop.Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_malloc")]
            internal static extern IntPtr GMalloc(ulong nBytes);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(Interop.Libraries.GLib, CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_log_remove_handler")]
            internal static extern void GLogRemoveHandler([MarshalAs(UnmanagedType.LPStr)] string logDomain, uint handlerId);
        }

        public static void GFree(IntPtr mem)
        {
            Internal.GFree(mem);
        }

        public static IntPtr GMalloc(ulong nBytes)
        {
            return Internal.GMalloc(nBytes);
        }

        public static void GLogRemoveHandler(string logDomain, uint handlerId)
        {
            Internal.GLogRemoveHandler(logDomain, handlerId);
        }
    }
}