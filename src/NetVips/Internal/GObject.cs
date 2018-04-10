using System;
using System.Runtime.InteropServices;
using System.Security;
using NetVips.Interop;

namespace NetVips.Internal
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal struct GTypeInstance
    {
        [FieldOffset(0)] internal IntPtr GClass;
    }

    internal static class GObject
    {
        [StructLayout(LayoutKind.Explicit, Size = 24)]
        internal struct Fields
        {
            [FieldOffset(0)] internal GTypeInstance GTypeInstance;

            [FieldOffset(8)] internal uint RefCount;

            [FieldOffset(16)] internal IntPtr Qdata;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_object_set_property")]
        internal static extern void GObjectSetProperty(IntPtr @object,
            [MarshalAs(UnmanagedType.LPStr)] string propertyName, IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_object_get_property")]
        internal static extern void GObjectGetProperty(IntPtr @object,
            [MarshalAs(UnmanagedType.LPStr)] string propertyName, IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_object_ref")]
        internal static extern IntPtr GObjectRef(IntPtr @object);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_object_unref")]
        internal static extern void GObjectUnref(IntPtr @object);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_object")]
        internal static extern void GValueSetObject(IntPtr value, IntPtr vObject);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_object")]
        internal static extern IntPtr GValueGetObject(IntPtr value);
    }

    internal static class GType
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_type_name")]
        internal static extern IntPtr GTypeName(ulong type);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_type_from_name")]
        internal static extern ulong GTypeFromName([MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_type_fundamental")]
        internal static extern ulong GTypeFundamental(ulong typeId);
    }

    internal static class GValue
    {
        [StructLayout(LayoutKind.Explicit, Size = 24)]
        internal struct Fields
        {
            [FieldOffset(0)] internal ulong GType;

            [FieldOffset(8)] [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal Padding[] data;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Padding
        {
            [FieldOffset(0)] int vInt;

            [FieldOffset(0)] uint vUInt;

            [FieldOffset(0)] int vLong;

            [FieldOffset(0)] uint vULong;

            [FieldOffset(0)] long vInt64;

            [FieldOffset(0)] ulong vUInt64;

            [FieldOffset(0)] float vFloat;

            [FieldOffset(0)] double vDouble;

            [FieldOffset(0)] IntPtr vPointer;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_value_init")]
        internal static extern IntPtr GValueInit(IntPtr value, ulong gType);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_unset")]
        internal static extern void GValueUnset(IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_boolean")]
        internal static extern void GValueSetBoolean(IntPtr value, int vBoolean);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_boolean")]
        internal static extern int GValueGetBoolean(IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_int")]
        internal static extern void GValueSetInt(IntPtr value, int vInt);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_int")]
        internal static extern int GValueGetInt(IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_double")]
        internal static extern void GValueSetDouble(IntPtr value, double vDouble);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_double")]
        internal static extern double GValueGetDouble(IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_string")]
        internal static extern void GValueSetString(IntPtr value, IntPtr vString);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_string")]
        internal static extern IntPtr GValueGetString(IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_enum")]
        internal static extern void GValueSetEnum(IntPtr value, int vEnum);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_enum")]
        internal static extern int GValueGetEnum(IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_flags")]
        internal static extern void GValueSetFlags(IntPtr value, uint vFlags);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_flags")]
        internal static extern uint GValueGetFlags(IntPtr value);
    }

    internal static class GParamSpec
    {
        [StructLayout(LayoutKind.Explicit, Size = 72)]
        internal struct Fields
        {
            [FieldOffset(0)] internal GTypeInstance GTypeInstance;

            [FieldOffset(8)] internal IntPtr Name;

            [FieldOffset(16)] internal Enums.GParamFlags Flags;

            [FieldOffset(24)] internal ulong ValueType;

            [FieldOffset(32)] internal ulong OwnerType;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_param_spec_get_blurb")]
        internal static extern IntPtr GParamSpecGetBlurb(IntPtr pspec);
    }
}