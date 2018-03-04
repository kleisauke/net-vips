using System;
using System.Runtime.InteropServices;
using System.Security;

namespace NetVips.Internal
{
    public class GTypeInstance
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct Fields
        {
            [FieldOffset(0)] internal IntPtr GClass;
        }
    }

    public unsafe class GObject : IDisposable
    {
        [StructLayout(LayoutKind.Explicit, Size = 24)]
        public struct Fields
        {
            [FieldOffset(0)] internal GTypeInstance.Fields GTypeInstance;

            [FieldOffset(8)] internal uint RefCount;

            [FieldOffset(16)] internal IntPtr Qdata;
        }

        public struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_object_set_property")]
            internal static extern void GObjectSetProperty(IntPtr @object,
                [MarshalAs(UnmanagedType.LPStr)] string propertyName, IntPtr value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_object_get_property")]
            internal static extern void GObjectGetProperty(IntPtr @object,
                [MarshalAs(UnmanagedType.LPStr)] string propertyName, IntPtr value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_object_ref")]
            internal static extern IntPtr GObjectRef(IntPtr @object);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_object_unref")]
            internal static extern void GObjectUnref(IntPtr @object);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_set_object")]
            internal static extern void GValueSetObject(IntPtr value, IntPtr vObject);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_get_object")]
            internal static extern IntPtr GValueGetObject(IntPtr value);
        }

        public IntPtr Pointer { get; protected set; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public GObject() : this(CopyValue(new Fields()))
        {
        }

        public GObject(Fields native) : this(CopyValue(native))
        {
        }

        public GObject(IntPtr native) : this(native.ToPointer())
        {
        }

        protected GObject(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        public static void GObjectSetProperty(GObject @object, string propertyName, GValue value)
        {
            Internal.GObjectSetProperty(@object.Pointer, propertyName, value.Pointer);
        }

        public static void GObjectGetProperty(GObject @object, string propertyName, GValue value)
        {
            Internal.GObjectGetProperty(@object.Pointer, propertyName, value.Pointer);
        }

        public static IntPtr GObjectRef(IntPtr @object)
        {
            return Internal.GObjectRef(@object);
        }

        public static void GObjectUnref(IntPtr @object)
        {
            Internal.GObjectUnref(@object);
        }

        public static void GValueSetObject(GValue value, IntPtr vObject)
        {
            Internal.GValueSetObject(value.Pointer, vObject);
        }

        public static IntPtr GValueGetObject(GValue value)
        {
            return Internal.GValueGetObject(value.Pointer);
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

        public void Dispose(bool disposing)
        {
            if (Pointer == IntPtr.Zero)
            {
                return;
            }

            // on GC, unref
            Internal.GObjectUnref(Pointer);
            Pointer = IntPtr.Zero;
        }
    }

    public class GType
    {
        public struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_type_name")]
            internal static extern IntPtr GTypeName(ulong type);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_type_from_name")]
            internal static extern ulong GTypeFromName([MarshalAs(UnmanagedType.LPStr)] string name);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_type_fundamental")]
            internal static extern ulong GTypeFundamental(ulong typeId);
        }

        public static string GTypeName(ulong type)
        {
            return Marshal.PtrToStringAnsi(Internal.GTypeName(type));
        }

        public static ulong GTypeFromName(string name)
        {
            return Internal.GTypeFromName(name);
        }

        public static ulong GTypeFundamental(ulong typeId)
        {
            return Internal.GTypeFundamental(typeId);
        }
    }

    public unsafe class GValue : IDisposable
    {
        [StructLayout(LayoutKind.Explicit, Size = 24)]
        public struct Fields
        {
            [FieldOffset(0)] internal ulong GType;

            [FieldOffset(8)] internal fixed byte Data[16];
        }

        public struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_init")]
            internal static extern IntPtr GValueInit(IntPtr value, ulong gType);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_unset")]
            internal static extern void GValueUnset(IntPtr value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_set_boolean")]
            internal static extern void GValueSetBoolean(IntPtr value, int vBoolean);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_get_boolean")]
            internal static extern int GValueGetBoolean(IntPtr value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_set_int")]
            internal static extern void GValueSetInt(IntPtr value, int vInt);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_get_int")]
            internal static extern int GValueGetInt(IntPtr value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_set_double")]
            internal static extern void GValueSetDouble(IntPtr value, double vDouble);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_get_double")]
            internal static extern double GValueGetDouble(IntPtr value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_set_string")]
            internal static extern void GValueSetString(IntPtr value, IntPtr vString);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_get_string")]
            internal static extern IntPtr GValueGetString(IntPtr value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_set_enum")]
            internal static extern void GValueSetEnum(IntPtr value, int vEnum);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_get_enum")]
            internal static extern int GValueGetEnum(IntPtr value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_set_flags")]
            internal static extern void GValueSetFlags(IntPtr value, uint vFlags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_value_get_flags")]
            internal static extern uint GValueGetFlags(IntPtr value);
        }

        public IntPtr Pointer { get; protected set; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public GValue() : this(CopyValue(new Fields()))
        {
        }

        public GValue(Fields native) : this(CopyValue(native))
        {
        }

        public GValue(IntPtr native) : this(native.ToPointer())
        {
        }

        protected GValue(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        public static void GValueInit(GValue value, ulong gType)
        {
            Internal.GValueInit(value.Pointer, gType);
        }

        public static void GValueUnset(GValue value)
        {
            Internal.GValueUnset(value.Pointer);
        }

        public static void GValueSetBoolean(GValue value, int vBoolean)
        {
            Internal.GValueSetBoolean(value.Pointer, vBoolean);
        }

        public static int GValueGetBoolean(GValue value)
        {
            return Internal.GValueGetBoolean(value.Pointer);
        }

        public static void GValueSetInt(GValue value, int vInt)
        {
            Internal.GValueSetInt(value.Pointer, vInt);
        }

        public static int GValueGetInt(GValue value)
        {
            return Internal.GValueGetInt(value.Pointer);
        }

        public static void GValueSetDouble(GValue value, double vDouble)
        {
            Internal.GValueSetDouble(value.Pointer, vDouble);
        }

        public static double GValueGetDouble(GValue value)
        {
            return Internal.GValueGetDouble(value.Pointer);
        }

        public static void GValueSetString(GValue value, string vString)
        {
            Internal.GValueSetString(value.Pointer, vString.ToUtf8Ptr());
        }

        public static string GValueGetString(GValue value)
        {
            return Internal.GValueGetString(value.Pointer).ToUtf8String(true);
        }

        public static void GValueSetEnum(GValue value, int vEnum)
        {
            Internal.GValueSetEnum(value.Pointer, vEnum);
        }

        public static int GValueGetEnum(GValue value)
        {
            return Internal.GValueGetEnum(value.Pointer);
        }

        public static void GValueSetFlags(GValue value, uint vFlags)
        {
            Internal.GValueSetFlags(value.Pointer, vFlags);
        }

        public static uint GValueGetFlags(GValue value)
        {
            return Internal.GValueGetFlags(value.Pointer);
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

        public void Dispose(bool disposing)
        {
            if (Pointer == IntPtr.Zero)
            {
                return;
            }

            // and tag it to be unset on GC as well
            Internal.GValueUnset(Pointer);
            Pointer = IntPtr.Zero;
        }

        public ulong GType => ((Fields*)Pointer)->GType;
    }

    public unsafe class GParamSpec
    {
        [StructLayout(LayoutKind.Explicit, Size = 72)]
        public struct Fields
        {
            [FieldOffset(0)] internal GTypeInstance.Fields GTypeInstance;

            [FieldOffset(8)] internal IntPtr Name;

            [FieldOffset(16)] internal Enums.GParamFlags Flags;

            [FieldOffset(24)] internal ulong ValueType;

            [FieldOffset(32)] internal ulong OwnerType;
        }

        public struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libgobject-2.0-0.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "g_param_spec_get_blurb")]
            internal static extern IntPtr GParamSpecGetBlurb(IntPtr pspec);
        }

        public IntPtr Pointer { get; protected set; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public GParamSpec() : this(CopyValue(new Fields()))
        {
        }

        public GParamSpec(Fields native) : this(CopyValue(native))
        {
        }

        public GParamSpec(IntPtr native) : this(native.ToPointer())
        {
        }

        protected GParamSpec(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        public static string GParamSpecGetBlurb(GParamSpec pspec)
        {
            return Marshal.PtrToStringAnsi(Internal.GParamSpecGetBlurb(pspec.Pointer));
        }

        public string Name => Marshal.PtrToStringAnsi(Marshal.PtrToStructure<Fields>(Pointer).Name);

        public ulong ValueType => ((Fields*) Pointer)->ValueType;
    }
}