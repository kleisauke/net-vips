using System;
using System.Runtime.InteropServices;
using System.Security;
using NetVips.Interop;

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

    internal static class VipsObject
    {
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        internal struct Fields
        {
            [FieldOffset(0)] internal GObject.Fields ParentInstance;
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

        internal static IntPtr VipsArgumentMap(IntPtr pointer, VipsArgumentMapFn fn, IntPtr a, IntPtr b)
        {
            var funcPtr = fn == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(fn);
            return VipsArgumentMap(pointer, funcPtr, a, b);
        }

        internal static IntPtr VipsTypeMap(ulong @base, VipsTypeMap2Fn fn, IntPtr a, IntPtr b)
        {
            var funcPtr = fn == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(fn);
            return VipsTypeMap(@base, funcPtr, a, b);
        }
    }

    internal static class VipsArgument
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        internal struct Fields
        {
            [FieldOffset(0)] internal IntPtr Pspec;
        }
    }

    internal static class VipsArgumentClass
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
    }

    internal static class VipsArgumentInstance
    {
        [StructLayout(LayoutKind.Explicit, Size = 40)]
        internal struct Fields
        {
            [FieldOffset(0)] internal VipsArgument.Fields Parent;

            // More
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

        internal static void VipsValueSetBlob(IntPtr value, VipsCallbackFn freeFn, IntPtr data, ulong length)
        {
            var funcPtr = freeFn == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(freeFn);
            VipsValueSetBlob(value, funcPtr, data, length);
        }
    }

    internal static class VipsImage
    {
        [StructLayout(LayoutKind.Explicit, Size = 392)]
        internal struct Fields
        {
            [FieldOffset(0)] internal VipsObject.Fields ParentInstance;
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
        internal static extern IntPtr VipsImageNewFromMemory(IntPtr data, ulong size, int width, int height,
            int bands, Enums.VipsBandFormat format);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_matrix_from_array")]
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
            EntryPoint = "vips_value_get_array_image")]
        internal static extern IntPtr VipsValueGetArrayImage(IntPtr value, IntPtr n);

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

        internal static IntPtr VipsImageNewFromMemory(GCHandle data, ulong size, int width, int height, int bands,
            Enums.VipsBandFormat format)
        {
            return VipsImageNewFromMemory(data.AddrOfPinnedObject(), size, width, height, bands, format);
        }

        internal static IntPtr VipsImageNewTempFile(string format)
        {
            return VipsImageNewTempFile(format.ToUtf8Ptr());
        }
    }

    internal static class VipsInterpolate
    {
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        internal struct Fields
        {
            [FieldOffset(0)] internal VipsObject.Fields ParentObject;

            // More
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_interpolate_new")]
        internal static extern IntPtr VipsInterpolateNew([MarshalAs(UnmanagedType.LPStr)] string nickname);
    }

    internal static class VipsOperation
    {
        [StructLayout(LayoutKind.Explicit, Size = 96)]
        internal struct Fields
        {
            [FieldOffset(0)] internal VipsObject.Fields ParentInstance;

            // More
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