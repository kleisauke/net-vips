using System;
using System.Runtime.InteropServices;
using System.Security;
using NetVips.Interop;

namespace NetVips.Internal
{
    internal static class GObject
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_object_set_property")]
        internal static extern void GObjectSetProperty(NetVips.GObject @object,
            [MarshalAs(UnmanagedType.LPStr)] string propertyName, ref GValue.Struct value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_object_get_property")]
        internal static extern void GObjectGetProperty(NetVips.GObject @object,
            [MarshalAs(UnmanagedType.LPStr)] string propertyName, ref GValue.Struct value);

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
        internal static extern void GValueSetObject(ref GValue.Struct value, NetVips.GObject vObject);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_object")]
        internal static extern IntPtr GValueGetObject(ref GValue.Struct value);
    }

    internal static class GType
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_type_name")]
        internal static extern IntPtr GTypeName(IntPtr type);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_type_from_name")]
        internal static extern IntPtr GTypeFromName([MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_type_fundamental")]
        internal static extern IntPtr GTypeFundamental(IntPtr typeId);
    }

    internal static class GValue
    {
        [StructLayout(LayoutKind.Explicit, Size = 24)]
        internal struct Struct
        {
            [FieldOffset(0)] internal IntPtr GType;

            [FieldOffset(8)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal IntPtr[] data;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g_value_init")]
        internal static extern IntPtr GValueInit(ref Struct value, IntPtr gType);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_unset")]
        internal static extern void GValueUnset(ref Struct value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_boolean")]
        internal static extern void GValueSetBoolean(ref Struct value, int vBoolean);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_boolean")]
        internal static extern int GValueGetBoolean(ref Struct value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_int")]
        internal static extern void GValueSetInt(ref Struct value, int vInt);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_int")]
        internal static extern int GValueGetInt(ref Struct value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_double")]
        internal static extern void GValueSetDouble(ref Struct value, double vDouble);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_double")]
        internal static extern double GValueGetDouble(ref Struct value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_string")]
        internal static extern void GValueSetString(ref Struct value, in byte vString);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_string")]
        internal static extern IntPtr GValueGetString(ref Struct value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_enum")]
        internal static extern void GValueSetEnum(ref Struct value, int vEnum);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_enum")]
        internal static extern int GValueGetEnum(ref Struct value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_set_flags")]
        internal static extern void GValueSetFlags(ref Struct value, uint vFlags);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_value_get_flags")]
        internal static extern uint GValueGetFlags(ref Struct value);
    }

    internal static class GParamSpec
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct GTypeInstance
        {
            internal IntPtr GClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Struct
        {
            internal GTypeInstance GTypeInstance;

            internal IntPtr Name;

            internal Enums.GParamFlags Flags;
            internal IntPtr ValueType;
            internal IntPtr OwnerType;

            public IntPtr Nick;
            public IntPtr Blurb;
            public IntPtr QData;
            public uint RefCount;
            public uint ParamId;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_param_spec_get_blurb")]
        internal static extern IntPtr GParamSpecGetBlurb(ref Struct pspec);
    }
}