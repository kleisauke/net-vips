extern alias Interop;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using Interop::NetVips.Interop;

namespace NetVips.Internal
{
    internal static class Vips
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_init")]
        internal static extern int VipsInit([MarshalAs(UnmanagedType.LPStr)] string argv0);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_leak_set")]
        internal static extern void VipsLeakSet(int leak);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_version")]
        internal static extern int VipsVersion(int flag);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_enum_nick")]
        internal static extern IntPtr VipsEnumNick(ulong enm, int value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_enum_from_nick")]
        internal static extern int VipsEnumFromNick([MarshalAs(UnmanagedType.LPStr)] string domain, ulong type,
            [MarshalAs(UnmanagedType.LPStr)] string str);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_error_buffer")]
        internal static extern IntPtr VipsErrorBuffer();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_error_clear")]
        internal static extern void VipsErrorClear();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_path_filename7")]
        internal static extern IntPtr VipsPathFilename7(IntPtr path);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_path_mode7")]
        internal static extern IntPtr VipsPathMode7(IntPtr path);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_blend_mode_get_type")]
        internal static extern ulong VipsBlendModeGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_interpretation_get_type")]
        internal static extern ulong VipsInterpretationGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_band_format_get_type")]
        internal static extern ulong VipsBandFormatGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_flags_get_type")]
        internal static extern ulong VipsOperationFlagsGetType();

        internal static string VipsPathFilename7(string path)
        {
            return VipsPathFilename7(path.ToUtf8Ptr()).ToUtf8String();
        }

        internal static string VipsPathMode7(string path)
        {
            return VipsPathMode7(path.ToUtf8Ptr()).ToUtf8String();
        }
    }

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr VipsArgumentMapFn(IntPtr @object, IntPtr pspec, IntPtr argumentClass,
        IntPtr argumentInstance, IntPtr a, IntPtr b);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr VipsTypeMap2Fn(ulong type, IntPtr a, IntPtr b);

    internal unsafe class VipsObject
    {
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        internal struct Fields
        {
            [FieldOffset(0)] internal GObject.Fields ParentInstance;
        }

        internal IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal VipsObject() : this(CopyValue(new Fields()))
        {
        }

        internal VipsObject(Fields native) : this(CopyValue(native))
        {
        }

        internal VipsObject(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsObject(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_argument_map")]
        internal static extern IntPtr VipsArgumentMap(IntPtr @object, IntPtr fn, IntPtr a, IntPtr b);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_get_argument")]
        internal static extern int VipsObjectGetArgument(IntPtr @object, [MarshalAs(UnmanagedType.LPStr)] string name,
            IntPtr pspec, IntPtr argumentClass, IntPtr argumentInstance);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_set_from_string")]
        internal static extern int VipsObjectSetFromString(IntPtr @object,
            [MarshalAs(UnmanagedType.LPStr)] string @string);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_type_map")]
        internal static extern IntPtr VipsTypeMap(ulong @base, IntPtr fn, IntPtr a, IntPtr b);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_type_find")]
        internal static extern ulong VipsTypeFind([MarshalAs(UnmanagedType.LPStr)] string basename,
            [MarshalAs(UnmanagedType.LPStr)] string nickname);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_nickname_find")]
        internal static extern IntPtr VipsNicknameFind(ulong type);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_print_all")]
        internal static extern void VipsObjectPrintAll();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_unref_outputs")]
        internal static extern void VipsObjectUnrefOutputs(IntPtr @object);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_get_description")]
        internal static extern IntPtr VipsObjectGetDescription(IntPtr @object);

        internal static IntPtr VipsArgumentMap(VipsObject @object, VipsArgumentMapFn fn, IntPtr a, IntPtr b)
        {
            var funcPtr = fn == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(fn);
            return VipsArgumentMap(@object.Pointer, funcPtr, a, b);
        }

        internal static int VipsObjectGetArgument(VipsObject @object, string name, GParamSpec pspec,
            VipsArgumentClass argumentClass,
            VipsArgumentInstance argumentInstance)
        {
            return VipsObjectGetArgument(@object.Pointer, name, pspec.Pointer, argumentClass.Pointer,
                argumentInstance.Pointer);
        }

        internal static int VipsObjectSetFromString(VipsObject @object, string @string)
        {
            return VipsObjectSetFromString(@object.Pointer, @string);
        }

        internal static IntPtr VipsTypeMap(ulong @base, VipsTypeMap2Fn fn, IntPtr a, IntPtr b)
        {
            var funcPtr = fn == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(fn);
            return VipsTypeMap(@base, funcPtr, a, b);
        }

        internal static void VipsObjectUnrefOutputs(VipsObject @object)
        {
            VipsObjectUnrefOutputs(@object.Pointer);
        }

        internal static string VipsObjectGetDescription(VipsObject @object)
        {
            return Marshal.PtrToStringAnsi(VipsObjectGetDescription(@object.Pointer));
        }

        internal GObject ParentInstance => new GObject(new IntPtr(&((Fields*) Pointer)->ParentInstance));
    }

    internal unsafe class VipsArgument
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        internal struct Fields
        {
            [FieldOffset(0)] internal IntPtr Pspec;
        }

        internal IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal VipsArgument() : this(CopyValue(new Fields()))
        {
        }

        internal VipsArgument(Fields native) : this(CopyValue(native))
        {
        }

        internal VipsArgument(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsArgument(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        internal GParamSpec Pspec => new GParamSpec(((Fields*) Pointer)->Pspec);
    }

    internal unsafe class VipsArgumentClass
    {
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        internal struct Fields
        {
            [FieldOffset(0)] internal VipsArgument.Fields Parent;

            [FieldOffset(8)] internal IntPtr ObjectClass;

            [FieldOffset(16)] internal Enums.VipsArgumentFlags Flags;

            [FieldOffset(20)] internal int Priority;

            [FieldOffset(24)] internal uint Offset;
        }

        internal IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal VipsArgumentClass() : this(CopyValue(new Fields()))
        {
        }

        internal VipsArgumentClass(Fields native) : this(CopyValue(native))
        {
        }

        internal VipsArgumentClass(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsArgumentClass(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        internal Enums.VipsArgumentFlags Flags => ((Fields*) Pointer)->Flags;
    }

    internal unsafe class VipsArgumentInstance
    {
        [StructLayout(LayoutKind.Explicit, Size = 40)]
        internal struct Fields
        {
            [FieldOffset(0)] internal VipsArgument.Fields Parent;

            // More
        }

        internal IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal VipsArgumentInstance() : this(CopyValue(new Fields()))
        {
        }

        internal VipsArgumentInstance(Fields native) : this(CopyValue(native))
        {
        }

        internal VipsArgumentInstance(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsArgumentInstance(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }
    }

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int VipsCallbackFn(IntPtr a, IntPtr b);

    internal static class VipsType
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_ref_string")]
        internal static extern IntPtr VipsValueGetRefString(IntPtr value, ref ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_ref_string")]
        internal static extern void VipsValueSetRefString(IntPtr value, IntPtr str);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_blob")]
        internal static extern IntPtr VipsValueGetBlob(IntPtr value, ref ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_blob")]
        internal static extern void VipsValueSetBlob(IntPtr value, IntPtr freeFn, IntPtr data, ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_blob_free")]
        internal static extern void VipsValueSetBlobFree(IntPtr value, IntPtr data, ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_double")]
        internal static extern IntPtr VipsValueGetArrayDouble(IntPtr value, ref int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_array_double")]
        internal static extern void VipsValueSetArrayDouble(IntPtr value, double[] array, int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_int")]
        internal static extern IntPtr VipsValueGetArrayInt(IntPtr value, ref int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_array_int")]
        internal static extern void VipsValueSetArrayInt(IntPtr value, int[] array, int n);

        internal static string VipsValueGetRefString(GValue value, ref ulong length)
        {
            return VipsValueGetRefString(value.Pointer, ref length).ToUtf8String();
        }

        internal static void VipsValueSetRefString(GValue value, string str)
        {
            VipsValueSetRefString(value.Pointer, str.ToUtf8Ptr());
        }

        internal static IntPtr VipsValueGetBlob(GValue value, ref ulong length)
        {
            return VipsValueGetBlob(value.Pointer, ref length);
        }

        internal static void VipsValueSetBlob(GValue value, VipsCallbackFn freeFn, IntPtr data, ulong length)
        {
            var funcPtr = freeFn == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(freeFn);
            VipsValueSetBlob(value.Pointer, funcPtr, data, length);
        }

        internal static void VipsValueSetBlobFree(GValue value, IntPtr data, ulong length)
        {
            VipsValueSetBlobFree(value.Pointer, data, length);
        }

        internal static IntPtr VipsValueGetArrayDouble(GValue value, ref int n)
        {
            return VipsValueGetArrayDouble(value.Pointer, ref n);
        }

        internal static void VipsValueSetArrayDouble(GValue value, double[] array, int n)
        {
            VipsValueSetArrayDouble(value.Pointer, array, n);
        }

        internal static IntPtr VipsValueGetArrayInt(GValue value, ref int n)
        {
            return VipsValueGetArrayInt(value.Pointer, ref n);
        }

        internal static void VipsValueSetArrayInt(GValue value, int[] array, int n)
        {
            VipsValueSetArrayInt(value.Pointer, array, n);
        }
    }

    internal unsafe class VipsImage
    {
        [StructLayout(LayoutKind.Explicit, Size = 392)]
        internal struct Fields
        {
            [FieldOffset(0)] internal VipsObject.Fields ParentInstance;
        }

        internal IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal VipsImage() : this(CopyValue(new Fields()))
        {
        }

        internal VipsImage(Fields native) : this(CopyValue(native))
        {
        }

        internal VipsImage(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsImage(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_filename_get_filename")]
        internal static extern IntPtr VipsFilenameGetFilename(IntPtr vipsFilename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_filename_get_options")]
        internal static extern IntPtr VipsFilenameGetOptions(IntPtr vipsFilename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_from_memory")]
        internal static extern IntPtr VipsImageNewFromMemory(IntPtr data, ulong size, int width, int height, int bands, Enums.VipsBandFormat format);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_image_new_matrix_from_array")]
        internal static extern IntPtr VipsImageNewMatrixFromArray(int width, int height, double[] array, int size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_temp_file")]
        internal static extern IntPtr VipsImageNewTempFile(IntPtr format);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_write")]
        internal static extern int VipsImageWrite(IntPtr image, IntPtr @out);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_write_to_memory")]
        internal static extern IntPtr VipsImageWriteToMemory(IntPtr @in, ref ulong size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_copy_memory")]
        internal static extern IntPtr VipsImageCopyMemory(IntPtr image);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_image")]
        internal static extern IntPtr VipsValueGetArrayImage(IntPtr value, ref int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_array_image")]
        internal static extern void VipsValueSetArrayImage(IntPtr value, int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_image_set")]
        internal static extern void VipsImageSet(IntPtr image, [MarshalAs(UnmanagedType.LPStr)] string name,
            IntPtr value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_image_get")]
        internal static extern int VipsImageGet(IntPtr image, [MarshalAs(UnmanagedType.LPStr)] string name,
            IntPtr valueCopy);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_get_typeof")]
        internal static extern ulong VipsImageGetTypeof(IntPtr image, [MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_remove")]
        internal static extern int VipsImageRemove(IntPtr image, [MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_get_fields")]
        internal static extern IntPtr VipsImageGetFields(IntPtr image);

        internal static VipsImage VipsImageNewFromMemory(GCHandle data, ulong size, int width, int height, int bands, Enums.VipsBandFormat format)
        {
            return new VipsImage(VipsImageNewFromMemory(data.AddrOfPinnedObject(), size, width, height, bands, format));
        }

        internal static VipsImage VipsImageNewTempFile(string format)
        {
            return new VipsImage(VipsImageNewTempFile(format.ToUtf8Ptr()));
        }

        internal static int VipsImageWrite(VipsImage image, VipsImage @out)
        {
            return VipsImageWrite(image.Pointer, @out.Pointer);
        }

        internal static IntPtr VipsImageWriteToMemory(VipsImage @in, ref ulong size)
        {
            return VipsImageWriteToMemory(@in.Pointer, ref size);
        }

        internal static VipsImage VipsImageCopyMemory(VipsImage image)
        {
            return new VipsImage(VipsImageCopyMemory(image.Pointer));
        }

        internal static IntPtr VipsValueGetArrayImage(GValue value, ref int n)
        {
            return VipsValueGetArrayImage(value.Pointer, ref n);
        }

        internal static void VipsValueSetArrayImage(GValue value, int n)
        {
            VipsValueSetArrayImage(value.Pointer, n);
        }

        internal static void VipsImageSet(VipsImage image, string name, GValue value)
        {
            VipsImageSet(image.Pointer, name, value.Pointer);
        }

        internal static int VipsImageGet(VipsImage image, string name, GValue valueCopy)
        {
            return VipsImageGet(image.Pointer, name, valueCopy.Pointer);
        }

        internal static ulong VipsImageGetTypeof(VipsImage image, string name)
        {
            return VipsImageGetTypeof(image.Pointer, name);
        }

        internal static int VipsImageRemove(VipsImage image, string name)
        {
            return VipsImageRemove(image.Pointer, name);
        }

        internal static string[] VipsImageGetFields(VipsImage image)
        {
            var ptrArr = VipsImageGetFields(image.Pointer);

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

        internal VipsObject ParentInstance => new VipsObject(new IntPtr(&((Fields*) Pointer)->ParentInstance));
    }

    internal unsafe class VipsInterpolate
    {
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        internal struct Fields
        {
            [FieldOffset(0)] internal VipsObject.Fields ParentObject;

            // More
        }

        internal IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal VipsInterpolate() : this(CopyValue(new Fields()))
        {
        }

        internal VipsInterpolate(Fields native) : this(CopyValue(native))
        {
        }

        internal VipsInterpolate(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsInterpolate(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_interpolate_new")]
        internal static extern IntPtr VipsInterpolateNew([MarshalAs(UnmanagedType.LPStr)] string nickname);

        internal VipsObject ParentObject => new VipsObject(new IntPtr(&((Fields*) Pointer)->ParentObject));
    }

    internal unsafe class VipsOperation
    {
        [StructLayout(LayoutKind.Explicit, Size = 96)]
        internal struct Fields
        {
            [FieldOffset(0)] internal VipsObject.Fields ParentInstance;

            // More
        }

        internal IntPtr Pointer { get; }

        private static void* CopyValue(Fields native)
        {
            var ret = GLib.GMalloc((ulong) sizeof(Fields));
            *(Fields*) ret = native;
            return ret.ToPointer();
        }

        internal VipsOperation() : this(CopyValue(new Fields()))
        {
        }

        internal VipsOperation(Fields native) : this(CopyValue(native))
        {
        }

        internal VipsOperation(IntPtr native) : this(native.ToPointer())
        {
        }

        protected VipsOperation(void* ptr)
        {
            Pointer = new IntPtr(ptr);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_get_flags")]
        internal static extern Enums.VipsOperationFlags VipsOperationGetFlags(IntPtr operation);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_new")]
        internal static extern IntPtr VipsOperationNew([MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_operation_build")]
        internal static extern IntPtr VipsCacheOperationBuild(IntPtr operation);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max")]
        internal static extern void VipsCacheSetMax(int max);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max_mem")]
        internal static extern void VipsCacheSetMaxMem(ulong maxMem);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max_files")]
        internal static extern void VipsCacheSetMaxFiles(int maxFiles);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_trace")]
        internal static extern void VipsCacheSetTrace(int trace);

        internal static Enums.VipsOperationFlags VipsOperationGetFlags(VipsOperation operation)
        {
            return VipsOperationGetFlags(operation.Pointer);
        }

        internal static IntPtr VipsCacheOperationBuild(VipsOperation operation)
        {
            return VipsCacheOperationBuild(operation.Pointer);
        }

        internal VipsObject ParentInstance => new VipsObject(new IntPtr(&((Fields*) Pointer)->ParentInstance));
    }

    internal static class VipsForeign
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_load")]
        internal static extern IntPtr VipsForeignFindLoad(IntPtr filename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_load_buffer")]
        internal static extern IntPtr VipsForeignFindLoadBuffer(IntPtr data, ulong size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_save")]
        internal static extern IntPtr VipsForeignFindSave(IntPtr filename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_save_buffer")]
        internal static extern IntPtr VipsForeignFindSaveBuffer(IntPtr suffix);
    }
}