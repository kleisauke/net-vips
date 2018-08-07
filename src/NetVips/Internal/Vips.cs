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
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_tracked_get_allocs")]
        internal static extern int VipsTrackedGetAllocs();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_tracked_get_mem")]
        internal static extern int VipsTrackedGetMem();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_tracked_get_files")]
        internal static extern int VipsTrackedGetFiles();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_tracked_get_mem_highwater")]
        internal static extern ulong VipsTrackedGetMemHighwater();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_version")]
        internal static extern int VipsVersion(int flag);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_enum_nick")]
        internal static extern IntPtr VipsEnumNick(IntPtr enm, int value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_enum_from_nick")]
        internal static extern int VipsEnumFromNick([MarshalAs(UnmanagedType.LPStr)] string domain, IntPtr type,
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
        internal static extern IntPtr VipsBlendModeGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_interpretation_get_type")]
        internal static extern IntPtr VipsInterpretationGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_band_format_get_type")]
        internal static extern IntPtr VipsBandFormatGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_flags_get_type")]
        internal static extern IntPtr VipsOperationFlagsGetType();

        internal static string VipsPathFilename7(string path)
        {
            var pointer = path.ToUtf8Ptr();
            var result = VipsPathFilename7(pointer).ToUtf8String();
            GLib.GFree(pointer);

            return result;
        }

        internal static string VipsPathMode7(string path)
        {
            var pointer = path.ToUtf8Ptr();
            var result = VipsPathMode7(pointer).ToUtf8String();
            GLib.GFree(pointer);

            return result;
        }
    }

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr VipsArgumentMapFn(IntPtr @object, IntPtr pspec, IntPtr argumentClass,
        IntPtr argumentInstance, IntPtr a, IntPtr b);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr VipsTypeMap2Fn(IntPtr type, IntPtr a, IntPtr b);

    internal static class VipsObject
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_argument_map")]
        internal static extern IntPtr VipsArgumentMap(NetVips.VipsObject @object, VipsArgumentMapFn fn, IntPtr a,
            IntPtr b);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_get_argument")]
        internal static extern int VipsObjectGetArgument(NetVips.VipsObject @object,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            ref GParamSpec.Struct pspec, ref VipsArgumentClass.Struct argumentClass,
            ref VipsArgumentInstance.Struct argumentInstance);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_set_from_string")]
        internal static extern int VipsObjectSetFromString(NetVips.VipsObject @object,
            [MarshalAs(UnmanagedType.LPStr)] string @string);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_type_map")]
        internal static extern IntPtr VipsTypeMap(IntPtr @base, VipsTypeMap2Fn fn, IntPtr a, IntPtr b);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_type_find")]
        internal static extern IntPtr VipsTypeFind([MarshalAs(UnmanagedType.LPStr)] string basename,
            [MarshalAs(UnmanagedType.LPStr)] string nickname);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_nickname_find")]
        internal static extern IntPtr VipsNicknameFind(IntPtr type);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_print_all")]
        internal static extern void VipsObjectPrintAll();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_unref_outputs")]
        internal static extern void VipsObjectUnrefOutputs(NetVips.VipsObject @object);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_get_description")]
        internal static extern IntPtr VipsObjectGetDescription(NetVips.VipsObject @object);
    }

    internal static class VipsArgument
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Struct
        {
            internal IntPtr Pspec;
        }
    }

    internal static class VipsArgumentClass
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Struct
        {
            internal VipsArgument.Struct Parent;
            internal IntPtr ObjectClass;
            internal Enums.VipsArgumentFlags Flags;

            internal int Priority;
            internal uint Offset;
        }
    }

    internal static class VipsArgumentInstance
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Struct
        {
            internal VipsArgument.Struct Parent;
            internal IntPtr ArgumentClass;
            internal IntPtr Object;

            internal int Assigned;
            internal uint CloseId;
            internal uint InvalidateId;
        }
    }

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int VipsCallbackFn(IntPtr a, IntPtr b);

    internal static class VipsType
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_ref_string")]
        internal static extern IntPtr VipsValueGetRefString(ref GValue.Struct value, out ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_ref_string")]
        internal static extern void VipsValueSetRefString(ref GValue.Struct value, IntPtr str);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_blob")]
        internal static extern IntPtr VipsValueGetBlob(ref GValue.Struct value, out ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_blob")]
        internal static extern void VipsValueSetBlob(ref GValue.Struct value, VipsCallbackFn freeFn, IntPtr data,
            UIntPtr length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_blob_free")]
        internal static extern void VipsValueSetBlobFree(ref GValue.Struct value, IntPtr data, UIntPtr length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_double")]
        internal static extern IntPtr VipsValueGetArrayDouble(ref GValue.Struct value, out int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_array_double")]
        internal static extern void VipsValueSetArrayDouble(ref GValue.Struct value, double[] array, int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_int")]
        internal static extern IntPtr VipsValueGetArrayInt(ref GValue.Struct value, out int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_array_int")]
        internal static extern void VipsValueSetArrayInt(ref GValue.Struct value, int[] array, int n);
    }

    internal static class VipsImage
    {
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
        internal static extern IntPtr VipsImageNewFromMemory(IntPtr data, UIntPtr size, int width, int height,
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
        internal static extern int VipsImageWrite(Image image, Image @out);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_write_to_memory")]
        internal static extern IntPtr VipsImageWriteToMemory(Image @in, out ulong size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_copy_memory")]
        internal static extern IntPtr VipsImageCopyMemory(Image image);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_image")]
        internal static extern IntPtr VipsValueGetArrayImage(ref GValue.Struct value, out int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_image")]
        internal static extern IntPtr VipsValueGetArrayImage(ref GValue.Struct value, IntPtr n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_array_image")]
        internal static extern void VipsValueSetArrayImage(ref GValue.Struct value, int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_image_set")]
        internal static extern void VipsImageSet(Image image, [MarshalAs(UnmanagedType.LPStr)] string name,
            ref GValue.Struct value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_image_get")]
        internal static extern int VipsImageGet(Image image, [MarshalAs(UnmanagedType.LPStr)] string name,
            ref GValue.Struct valueCopy);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_get_typeof")]
        internal static extern IntPtr VipsImageGetTypeof(Image image, [MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_remove")]
        internal static extern int VipsImageRemove(Image image, [MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_get_fields")]
        internal static extern IntPtr VipsImageGetFields(Image image);

        internal static IntPtr VipsImageNewTempFile(string format)
        {
            var pointer = format.ToUtf8Ptr();
            var result = VipsImageNewTempFile(pointer);
            GLib.GFree(pointer);

            return result;
        }
    }

    internal static class VipsInterpolate
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_interpolate_new")]
        internal static extern IntPtr VipsInterpolateNew([MarshalAs(UnmanagedType.LPStr)] string nickname);
    }

    internal static class VipsOperation
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_get_flags")]
        internal static extern Enums.VipsOperationFlags VipsOperationGetFlags(Operation operation);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_new")]
        internal static extern IntPtr VipsOperationNew([MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_operation_build")]
        internal static extern IntPtr VipsCacheOperationBuild(Operation operation);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max")]
        internal static extern void VipsCacheSetMax(int max);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max_mem")]
        internal static extern void VipsCacheSetMaxMem(UIntPtr maxMem);

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
        internal static extern IntPtr VipsForeignFindLoadBuffer(IntPtr data, UIntPtr size);

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