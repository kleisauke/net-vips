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

    internal unsafe class GObject : IDisposable
    {
        [StructLayout(LayoutKind.Explicit, Size = 24)]
        internal struct Fields
        {
            [FieldOffset(0)] internal GTypeInstance GTypeInstance;

            [FieldOffset(8)] internal uint RefCount;

            [FieldOffset(16)] internal IntPtr Qdata;
        }

        internal IntPtr Pointer { get; private set; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal GObject() : this(CopyValue(new Fields()))
        {
        }

        internal GObject(Fields native) : this(CopyValue(native))
        {
        }

        internal GObject(IntPtr native) : this(native.ToPointer())
        {
        }

        protected GObject(void* ptr)
        {
            Pointer = new IntPtr(ptr);
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

        internal static void GObjectSetProperty(GObject @object, string propertyName, GValue value)
        {
            GObjectSetProperty(@object.Pointer, propertyName, value.Pointer);
        }

        internal static void GObjectGetProperty(GObject @object, string propertyName, GValue value)
        {
            GObjectGetProperty(@object.Pointer, propertyName, value.Pointer);
        }

        internal static void GValueSetObject(GValue value, GObject vObject)
        {
            GValueSetObject(value.Pointer, vObject.Pointer);
        }

        internal static IntPtr GValueGetObject(GValue value)
        {
            return GValueGetObject(value.Pointer);
        }

        ~GObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (Pointer == IntPtr.Zero)
            {
                return;
            }

            // on GC, unref
            GObjectUnref(Pointer);
            Pointer = IntPtr.Zero;
        }
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

    internal unsafe class GValue : IDisposable
    {
        [StructLayout(LayoutKind.Explicit, Size = 24)]
        internal struct Fields
        {
            [FieldOffset(0)] internal ulong GType;

            [FieldOffset(8)] internal fixed byte Data[16];
        }

        internal IntPtr Pointer { get; private set; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal GValue() : this(CopyValue(new Fields()))
        {
        }

        internal GValue(Fields native) : this(CopyValue(native))
        {
        }

        internal GValue(IntPtr native) : this(native.ToPointer())
        {
        }

        protected GValue(void* ptr)
        {
            Pointer = new IntPtr(ptr);
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

        internal static void GValueInit(GValue value, ulong gType)
        {
            GValueInit(value.Pointer, gType);
        }

        internal static void GValueUnset(GValue value)
        {
            GValueUnset(value.Pointer);
        }

        internal static void GValueSetBoolean(GValue value, int vBoolean)
        {
            GValueSetBoolean(value.Pointer, vBoolean);
        }

        internal static int GValueGetBoolean(GValue value)
        {
            return GValueGetBoolean(value.Pointer);
        }

        internal static void GValueSetInt(GValue value, int vInt)
        {
            GValueSetInt(value.Pointer, vInt);
        }

        internal static int GValueGetInt(GValue value)
        {
            return GValueGetInt(value.Pointer);
        }

        internal static void GValueSetDouble(GValue value, double vDouble)
        {
            GValueSetDouble(value.Pointer, vDouble);
        }

        internal static double GValueGetDouble(GValue value)
        {
            return GValueGetDouble(value.Pointer);
        }

        internal static void GValueSetString(GValue value, string vString)
        {
            GValueSetString(value.Pointer, vString.ToUtf8Ptr());
        }

        internal static string GValueGetString(GValue value)
        {
            return GValueGetString(value.Pointer).ToUtf8String();
        }

        internal static void GValueSetEnum(GValue value, int vEnum)
        {
            GValueSetEnum(value.Pointer, vEnum);
        }

        internal static int GValueGetEnum(GValue value)
        {
            return GValueGetEnum(value.Pointer);
        }

        internal static void GValueSetFlags(GValue value, uint vFlags)
        {
            GValueSetFlags(value.Pointer, vFlags);
        }

        internal static uint GValueGetFlags(GValue value)
        {
            return GValueGetFlags(value.Pointer);
        }

        ~GValue()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (Pointer == IntPtr.Zero)
            {
                return;
            }

            // and tag it to be unset on GC as well
            GValueUnset(Pointer);
            Pointer = IntPtr.Zero;
        }

        internal ulong GType => ((Fields*) Pointer)->GType;
    }

    internal unsafe class GParamSpec
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

        private struct Internal
        {
        }

        internal IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal GParamSpec() : this(CopyValue(new Fields()))
        {
        }

        internal GParamSpec(Fields native) : this(CopyValue(native))
        {
        }

        internal GParamSpec(IntPtr native) : this(native.ToPointer())
        {
        }

        protected GParamSpec(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.GObject, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "g_param_spec_get_blurb")]
        internal static extern IntPtr GParamSpecGetBlurb(IntPtr pspec);

        internal static string GParamSpecGetBlurb(GParamSpec pspec)
        {
            return Marshal.PtrToStringAnsi(GParamSpecGetBlurb(pspec.Pointer));
        }

        internal string Name => Marshal.PtrToStringAnsi(((Fields*) Pointer)->Name);

        internal ulong ValueType => ((Fields*) Pointer)->ValueType;
    }
}