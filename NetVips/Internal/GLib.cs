using System;
using System.Runtime.InteropServices;
using System.Security;

namespace NetVips.Internal
{
    public class GLib
    {
        public struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_free")]
            internal static extern void GFree(IntPtr mem);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_malloc")]
            internal static extern IntPtr GMalloc(ulong nBytes);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
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