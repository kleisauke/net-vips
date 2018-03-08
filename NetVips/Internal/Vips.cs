using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace NetVips.Internal
{
    public static class Vips
    {
        private struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_init")]
            internal static extern int VipsInit([MarshalAs(UnmanagedType.LPStr)] string argv0);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_leak_set")]
            internal static extern void VipsLeakSet(int leak);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_version")]
            internal static extern int VipsVersion(int flag);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_enum_nick")]
            internal static extern IntPtr VipsEnumNick(ulong enm, int value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_enum_from_nick")]
            internal static extern int VipsEnumFromNick([MarshalAs(UnmanagedType.LPStr)] string domain, ulong type,
                [MarshalAs(UnmanagedType.LPStr)] string str);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_error_buffer")]
            internal static extern IntPtr VipsErrorBuffer();

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_error_clear")]
            internal static extern void VipsErrorClear();

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_path_filename7")]
            internal static extern IntPtr VipsPathFilename7(IntPtr path);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_path_mode7")]
            internal static extern IntPtr VipsPathMode7(IntPtr path);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_blend_mode_get_type")]
            internal static extern ulong VipsBlendModeGetType();

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_interpretation_get_type")]
            internal static extern ulong VipsInterpretationGetType();

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_band_format_get_type")]
            internal static extern ulong VipsBandFormatGetType();

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_operation_flags_get_type")]
            internal static extern ulong VipsOperationFlagsGetType();
        }

        public static int VipsInit(string argv0)
        {
            return Internal.VipsInit(argv0);
        }

        public static void VipsLeakSet(int leak)
        {
            Internal.VipsLeakSet(leak);
        }

        public static int VipsVersion(int flag)
        {
            return Internal.VipsVersion(flag);
        }

        public static string VipsEnumNick(ulong enm, int value)
        {
            return Marshal.PtrToStringAnsi(Internal.VipsEnumNick(enm, value));
        }

        public static int VipsEnumFromNick(string domain, ulong type, string str)
        {
            return Internal.VipsEnumFromNick(domain, type, str);
        }

        public static string VipsErrorBuffer()
        {
            return Marshal.PtrToStringAnsi(Internal.VipsErrorBuffer());
        }

        public static void VipsErrorClear()
        {
            Internal.VipsErrorClear();
        }

        public static string VipsPathFilename7(IntPtr path)
        {
            return Internal.VipsPathFilename7(path).ToUtf8String(true);
        }

        public static string VipsPathMode7(IntPtr path)
        {
            return Internal.VipsPathMode7(path).ToUtf8String(true);
        }

        public static ulong VipsBlendModeGetType()
        {
            return Internal.VipsBlendModeGetType();
        }

        public static ulong VipsInterpretationGetType()
        {
            return Internal.VipsInterpretationGetType();
        }

        public static ulong VipsBandFormatGetType()
        {
            return Internal.VipsBandFormatGetType();
        }

        public static ulong VipsOperationFlagsGetType()
        {
            return Internal.VipsOperationFlagsGetType();
        }
    }

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr VipsArgumentMapFn(IntPtr @object, IntPtr pspec, IntPtr argumentClass,
        IntPtr argumentInstance, IntPtr a, IntPtr b);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr VipsTypeMap2Fn(ulong type, IntPtr a, IntPtr b);

    public unsafe class VipsObject
    {
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct Fields
        {
            [FieldOffset(0)] internal GObject.Fields ParentInstance;
        }

        private struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_argument_map")]
            internal static extern IntPtr VipsArgumentMap(IntPtr @object, IntPtr fn, IntPtr a, IntPtr b);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_object_get_argument")]
            internal static extern int VipsObjectGetArgument(IntPtr @object,
                [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr pspec, IntPtr argumentClass,
                IntPtr argumentInstance);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_object_set_from_string")]
            internal static extern int VipsObjectSetFromString(IntPtr @object,
                [MarshalAs(UnmanagedType.LPStr)] string @string);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_type_map")]
            internal static extern IntPtr VipsTypeMap(ulong @base, IntPtr fn, IntPtr a, IntPtr b);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_type_find")]
            internal static extern ulong VipsTypeFind([MarshalAs(UnmanagedType.LPStr)] string basename,
                [MarshalAs(UnmanagedType.LPStr)] string nickname);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_nickname_find")]
            internal static extern IntPtr VipsNicknameFind(ulong type);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_object_print_all")]
            internal static extern void VipsObjectPrintAll();

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_object_unref_outputs")]
            internal static extern void VipsObjectUnrefOutputs(IntPtr @object);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_object_get_description")]
            internal static extern IntPtr VipsObjectGetDescription(IntPtr @object);
        }

        public IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public VipsObject() : this(CopyValue(new Fields()))
        {
        }

        public VipsObject(Fields native) : this(CopyValue(native))
        {
        }

        public VipsObject(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsObject(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        public static IntPtr VipsArgumentMap(VipsObject @object, VipsArgumentMapFn fn, IntPtr a, IntPtr b)
        {
            var funcPtr = fn == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(fn);
            return Internal.VipsArgumentMap(@object.Pointer, funcPtr, a, b);
        }

        public static int VipsObjectGetArgument(VipsObject @object, string name, GParamSpec pspec,
            VipsArgumentClass argumentClass,
            VipsArgumentInstance argumentInstance)
        {
            return Internal.VipsObjectGetArgument(@object.Pointer, name, pspec.Pointer, argumentClass.Pointer,
                argumentInstance.Pointer);
        }

        public static int VipsObjectSetFromString(VipsObject @object, string @string)
        {
            return Internal.VipsObjectSetFromString(@object.Pointer, @string);
        }

        public static IntPtr VipsTypeMap(ulong @base, VipsTypeMap2Fn fn, IntPtr a, IntPtr b)
        {
            var funcPtr = fn == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(fn);
            return Internal.VipsTypeMap(@base, funcPtr, a, b);
        }

        public static ulong VipsTypeFind(string basename, string nickname)
        {
            return Internal.VipsTypeFind(basename, nickname);
        }

        public static string VipsNicknameFind(ulong type)
        {
            return Marshal.PtrToStringAnsi(Internal.VipsNicknameFind(type));
        }

        public static void VipsObjectPrintAll()
        {
            Internal.VipsObjectPrintAll();
        }

        public static void VipsObjectUnrefOutputs(VipsObject @object)
        {
            Internal.VipsObjectUnrefOutputs(@object.Pointer);
        }

        public static string VipsObjectGetDescription(VipsObject @object)
        {
            return Marshal.PtrToStringAnsi(Internal.VipsObjectGetDescription(@object.Pointer));
        }

        public GObject ParentInstance => new GObject(new IntPtr(&((Fields*) Pointer)->ParentInstance));
    }

    public unsafe class VipsArgument
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct Fields
        {
            [FieldOffset(0)] internal IntPtr Pspec;
        }

        public IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public VipsArgument() : this(CopyValue(new Fields()))
        {
        }

        public VipsArgument(Fields native) : this(CopyValue(native))
        {
        }

        public VipsArgument(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsArgument(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        public GParamSpec Pspec => new GParamSpec(((Fields*) Pointer)->Pspec);
    }

    public unsafe class VipsArgumentClass
    {
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        public struct Fields
        {
            [FieldOffset(0)] internal VipsArgument.Fields Parent;

            [FieldOffset(8)] internal IntPtr ObjectClass;

            [FieldOffset(16)] internal Enums.VipsArgumentFlags Flags;

            [FieldOffset(20)] internal int Priority;

            [FieldOffset(24)] internal uint Offset;
        }

        public IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public VipsArgumentClass() : this(CopyValue(new Fields()))
        {
        }

        public VipsArgumentClass(Fields native) : this(CopyValue(native))
        {
        }

        public VipsArgumentClass(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsArgumentClass(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        public Enums.VipsArgumentFlags Flags => ((Fields*) Pointer)->Flags;
    }

    public unsafe class VipsArgumentInstance
    {
        [StructLayout(LayoutKind.Explicit, Size = 40)]
        public struct Fields
        {
            [FieldOffset(0)] internal VipsArgument.Fields Parent;

            // More
        }

        public IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public VipsArgumentInstance() : this(CopyValue(new Fields()))
        {
        }

        public VipsArgumentInstance(Fields native) : this(CopyValue(native))
        {
        }

        public VipsArgumentInstance(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsArgumentInstance(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }
    }

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int VipsCallbackFn(IntPtr a, IntPtr b);

    public static class VipsType
    {
        private struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_get_ref_string")]
            internal static extern IntPtr VipsValueGetRefString(IntPtr value, ref ulong length);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_set_ref_string")]
            internal static extern void VipsValueSetRefString(IntPtr value, IntPtr str);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_get_blob")]
            internal static extern IntPtr VipsValueGetBlob(IntPtr value, ref ulong length);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_set_blob")]
            internal static extern void VipsValueSetBlob(IntPtr value, IntPtr freeFn, IntPtr data, ulong length);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_set_blob_free")]
            internal static extern void VipsValueSetBlobFree(IntPtr value, IntPtr data, ulong length);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_get_array_double")]
            internal static extern IntPtr VipsValueGetArrayDouble(IntPtr value, ref int n);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_set_array_double")]
            internal static extern void VipsValueSetArrayDouble(IntPtr value, double[] array, int n);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_get_array_int")]
            internal static extern IntPtr VipsValueGetArrayInt(IntPtr value, ref int n);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_set_array_int")]
            internal static extern void VipsValueSetArrayInt(IntPtr value, int[] array, int n);
        }

        public static string VipsValueGetRefString(GValue value, ref ulong length)
        {
            return Internal.VipsValueGetRefString(value.Pointer, ref length).ToUtf8String(true);
        }

        public static void VipsValueSetRefString(GValue value, string str)
        {
            Internal.VipsValueSetRefString(value.Pointer, str.ToUtf8Ptr());
        }

        public static IntPtr VipsValueGetBlob(GValue value, ref ulong length)
        {
            return Internal.VipsValueGetBlob(value.Pointer, ref length);
        }

        public static void VipsValueSetBlob(GValue value, VipsCallbackFn freeFn, IntPtr data, ulong length)
        {
            var funcPtr = freeFn == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(freeFn);
            Internal.VipsValueSetBlob(value.Pointer, funcPtr, data, length);
        }

        public static void VipsValueSetBlobFree(GValue value, IntPtr data, ulong length)
        {
            Internal.VipsValueSetBlobFree(value.Pointer, data, length);
        }

        public static IntPtr VipsValueGetArrayDouble(GValue value, ref int n)
        {
            return Internal.VipsValueGetArrayDouble(value.Pointer, ref n);
        }

        public static void VipsValueSetArrayDouble(GValue value, double[] array, int n)
        {
            Internal.VipsValueSetArrayDouble(value.Pointer, array, n);
        }

        public static IntPtr VipsValueGetArrayInt(GValue value, ref int n)
        {
            return Internal.VipsValueGetArrayInt(value.Pointer, ref n);
        }

        public static void VipsValueSetArrayInt(GValue value, int[] array, int n)
        {
            Internal.VipsValueSetArrayInt(value.Pointer, array, n);
        }
    }

    public unsafe class VipsImage
    {
        [StructLayout(LayoutKind.Explicit, Size = 392)]
        public struct Fields
        {
            [FieldOffset(0)] internal VipsObject.Fields ParentInstance;
        }

        private struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_filename_get_filename")]
            internal static extern IntPtr VipsFilenameGetFilename(IntPtr vipsFilename);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_filename_get_options")]
            internal static extern IntPtr VipsFilenameGetOptions(IntPtr vipsFilename);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_new_from_memory")]
            internal static extern IntPtr VipsImageNewFromMemory(IntPtr data, ulong size, int width, int height,
                int bands, Enums.VipsBandFormat format);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_new_matrix_from_array")]
            internal static extern IntPtr VipsImageNewMatrixFromArray(int width, int height, double[] array, int size);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_new_temp_file")]
            internal static extern IntPtr VipsImageNewTempFile(IntPtr format);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_write")]
            internal static extern int VipsImageWrite(IntPtr image, IntPtr @out);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_write_to_memory")]
            internal static extern IntPtr VipsImageWriteToMemory(IntPtr @in, ref ulong size);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_copy_memory")]
            internal static extern IntPtr VipsImageCopyMemory(IntPtr image);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_get_array_image")]
            internal static extern IntPtr VipsValueGetArrayImage(IntPtr value, ref int n);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_value_set_array_image")]
            internal static extern void VipsValueSetArrayImage(IntPtr value, int n);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_set")]
            internal static extern void VipsImageSet(IntPtr image, [MarshalAs(UnmanagedType.LPStr)] string name,
                IntPtr value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_get")]
            internal static extern int VipsImageGet(IntPtr image, [MarshalAs(UnmanagedType.LPStr)] string name,
                IntPtr valueCopy);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_get_typeof")]
            internal static extern ulong VipsImageGetTypeof(IntPtr image, [MarshalAs(UnmanagedType.LPStr)] string name);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_remove")]
            internal static extern int VipsImageRemove(IntPtr image, [MarshalAs(UnmanagedType.LPStr)] string name);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_image_get_fields")]
            internal static extern IntPtr VipsImageGetFields(IntPtr image);
        }

        public IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public VipsImage() : this(CopyValue(new Fields()))
        {
        }

        public VipsImage(Fields native) : this(CopyValue(native))
        {
        }

        public VipsImage(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsImage(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        public static string VipsFilenameGetFilename(IntPtr vipsFilename)
        {
            return Internal.VipsFilenameGetFilename(vipsFilename).ToUtf8String(true);
        }

        public static string VipsFilenameGetOptions(IntPtr vipsFilename)
        {
            // ToUtf8String() isn't needed here.
            return Marshal.PtrToStringAnsi(Internal.VipsFilenameGetOptions(vipsFilename));
        }

        public static VipsImage VipsImageNewFromMemory(IntPtr data, ulong size, int width, int height, int bands,
            Enums.VipsBandFormat format)
        {
            return new VipsImage(Internal.VipsImageNewFromMemory(data, size, width, height, bands, format));
        }

        public static VipsImage VipsImageNewMatrixFromArray(int width, int height, double[] array, int size)
        {
            return new VipsImage(Internal.VipsImageNewMatrixFromArray(width, height, array, size));
        }

        public static VipsImage VipsImageNewTempFile(IntPtr format)
        {
            return new VipsImage(Internal.VipsImageNewTempFile(format));
        }

        public static int VipsImageWrite(VipsImage image, VipsImage @out)
        {
            return Internal.VipsImageWrite(image.Pointer, @out.Pointer);
        }

        public static IntPtr VipsImageWriteToMemory(VipsImage @in, ref ulong size)
        {
            return Internal.VipsImageWriteToMemory(@in.Pointer, ref size);
        }

        public static VipsImage VipsImageCopyMemory(VipsImage image)
        {
            return new VipsImage(Internal.VipsImageCopyMemory(image.Pointer));
        }

        public static IntPtr VipsValueGetArrayImage(GValue value, ref int n)
        {
            return Internal.VipsValueGetArrayImage(value.Pointer, ref n);
        }

        public static void VipsValueSetArrayImage(GValue value, int n)
        {
            Internal.VipsValueSetArrayImage(value.Pointer, n);
        }

        public static void VipsImageSet(VipsImage image, string name, GValue value)
        {
            Internal.VipsImageSet(image.Pointer, name, value.Pointer);
        }

        public static int VipsImageGet(VipsImage image, string name, GValue valueCopy)
        {
            return Internal.VipsImageGet(image.Pointer, name, valueCopy.Pointer);
        }

        public static ulong VipsImageGetTypeof(VipsImage image, string name)
        {
            return Internal.VipsImageGetTypeof(image.Pointer, name);
        }

        public static int VipsImageRemove(VipsImage image, string name)
        {
            return Internal.VipsImageRemove(image.Pointer, name);
        }

        public static string[] VipsImageGetFields(VipsImage image)
        {
            var ptrArr = Internal.VipsImageGetFields(image.Pointer);

            var names = new List<string>();

            var count = 0;
            IntPtr strPtr;
            while ((strPtr = Marshal.ReadIntPtr(ptrArr, count * IntPtr.Size)) != IntPtr.Zero)
            {
                var name = Marshal.PtrToStringAnsi(strPtr);
                names.Add(name);
                GLib.GFree(strPtr);
                ++count;
            }

            GLib.GFree(ptrArr);

            return names.ToArray();
        }

        public VipsObject ParentInstance => new VipsObject(new IntPtr(&((Fields*) Pointer)->ParentInstance));
    }

    public unsafe class VipsInterpolate
    {
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct Fields
        {
            [FieldOffset(0)] internal VipsObject.Fields ParentObject;

            // More
        }

        public struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_interpolate_new")]
            internal static extern IntPtr VipsInterpolateNew([MarshalAs(UnmanagedType.LPStr)] string nickname);
        }

        public IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public VipsInterpolate() : this(CopyValue(new Fields()))
        {
        }

        public VipsInterpolate(Fields native) : this(CopyValue(native))
        {
        }

        public VipsInterpolate(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsInterpolate(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        public static IntPtr VipsInterpolateNew(string nickname)
        {
            return Internal.VipsInterpolateNew(nickname);
        }

        public VipsObject ParentObject => new VipsObject(new IntPtr(&((Fields*)Pointer)->ParentObject));
    }

    public unsafe class VipsOperation
    {
        [StructLayout(LayoutKind.Explicit, Size = 96)]
        public struct Fields
        {
            [FieldOffset(0)] internal VipsObject.Fields ParentInstance;

            // More
        }

        private struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_operation_get_flags")]
            internal static extern Enums.VipsOperationFlags VipsOperationGetFlags(IntPtr operation);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_operation_new")]
            internal static extern IntPtr VipsOperationNew([MarshalAs(UnmanagedType.LPStr)] string name);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_cache_operation_build")]
            internal static extern IntPtr VipsCacheOperationBuild(IntPtr operation);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_cache_set_max")]
            internal static extern void VipsCacheSetMax(int max);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_cache_set_max_mem")]
            internal static extern void VipsCacheSetMaxMem(ulong maxMem);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_cache_set_max_files")]
            internal static extern void VipsCacheSetMaxFiles(int maxFiles);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_cache_set_trace")]
            internal static extern void VipsCacheSetTrace(int trace);
        }

        public IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        public VipsOperation() : this(CopyValue(new Fields()))
        {
        }

        public VipsOperation(Fields native) : this(CopyValue(native))
        {
        }

        public VipsOperation(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsOperation(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        public static Enums.VipsOperationFlags VipsOperationGetFlags(VipsOperation operation)
        {
            return Internal.VipsOperationGetFlags(operation.Pointer);
        }

        public static IntPtr VipsOperationNew(string name)
        {
            return Internal.VipsOperationNew(name);
        }

        public static IntPtr VipsCacheOperationBuild(VipsOperation operation)
        {
            return Internal.VipsCacheOperationBuild(operation.Pointer);
        }

        public static void VipsCacheSetMax(int max)
        {
            Internal.VipsCacheSetMax(max);
        }

        public static void VipsCacheSetMaxMem(ulong maxMem)
        {
            Internal.VipsCacheSetMaxMem(maxMem);
        }

        public static void VipsCacheSetMaxFiles(int maxFiles)
        {
            Internal.VipsCacheSetMaxFiles(maxFiles);
        }

        public static void VipsCacheSetTrace(int trace)
        {
            Internal.VipsCacheSetTrace(trace);
        }

        public VipsObject ParentInstance => new VipsObject(new IntPtr(&((Fields*) Pointer)->ParentInstance));
    }

    public static class VipsForeign
    {
        private struct Internal
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_foreign_find_load")]
            internal static extern IntPtr VipsForeignFindLoad(IntPtr filename);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_foreign_find_load_buffer")]
            internal static extern IntPtr VipsForeignFindLoadBuffer(IntPtr data, ulong size);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_foreign_find_save")]
            internal static extern IntPtr VipsForeignFindSave(IntPtr filename);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("libvips-42.dll", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "vips_foreign_find_save_buffer")]
            internal static extern IntPtr VipsForeignFindSaveBuffer([MarshalAs(UnmanagedType.LPStr)] string suffix);
        }

        public static string VipsForeignFindLoad(IntPtr filename)
        {
            return Marshal.PtrToStringAnsi(Internal.VipsForeignFindLoad(filename));
        }

        public static string VipsForeignFindLoadBuffer(IntPtr data, ulong size)
        {
            return Marshal.PtrToStringAnsi(Internal.VipsForeignFindLoadBuffer(data, size));
        }

        public static string VipsForeignFindSave(IntPtr filename)
        {
            return Marshal.PtrToStringAnsi(Internal.VipsForeignFindSave(filename));
        }

        public static string VipsForeignFindSaveBuffer(string suffix)
        {
            return Marshal.PtrToStringAnsi(Internal.VipsForeignFindSaveBuffer(suffix));
        }
    }
}