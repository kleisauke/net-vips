namespace NetVips.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using NetVips.Interop;

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr VipsArgumentMapFn(IntPtr @object, IntPtr pspec, IntPtr argumentClass,
        IntPtr argumentInstance, IntPtr a, IntPtr b);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr VipsTypeMap2Fn(IntPtr type, IntPtr a, IntPtr b);

    internal static class Vips
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_init")]
        internal static extern int Init([MarshalAs(UnmanagedType.LPStr)] string argv0);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_leak_set")]
        internal static extern void LeakSet(int leak);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_tracked_get_allocs")]
        internal static extern int TrackedGetAllocs();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_tracked_get_mem")]
        internal static extern int TrackedGetMem();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_tracked_get_files")]
        internal static extern int TrackedGetFiles();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_tracked_get_mem_highwater")]
        internal static extern ulong TrackedGetMemHighwater();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_version")]
        internal static extern int Version(int flag);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_enum_nick")]
        internal static extern IntPtr EnumNick(IntPtr enm, int value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_enum_from_nick")]
        internal static extern int EnumFromNick([MarshalAs(UnmanagedType.LPStr)] string domain, IntPtr type,
            [MarshalAs(UnmanagedType.LPStr)] string str);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_error_buffer")]
        internal static extern IntPtr ErrorBuffer();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_error_clear")]
        internal static extern void ErrorClear();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_path_filename7")]
        internal static extern IntPtr PathFilename7(in byte path);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_path_mode7")]
        internal static extern IntPtr PathMode7(in byte path);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_filename_get_filename")]
        internal static extern IntPtr GetFilename(in byte vipsFilename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_filename_get_options")]
        internal static extern IntPtr GetOptions(in byte vipsFilename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_blend_mode_get_type")]
        internal static extern IntPtr BlendModeGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_interpretation_get_type")]
        internal static extern IntPtr InterpretationGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_band_format_get_type")]
        internal static extern IntPtr BandFormatGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_argument_map")]
        internal static extern IntPtr ArgumentMap(NetVips.VipsObject @object, VipsArgumentMapFn fn, IntPtr a,
            IntPtr b);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_type_map")]
        internal static extern IntPtr TypeMap(IntPtr @base, VipsTypeMap2Fn fn, IntPtr a, IntPtr b);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_type_find")]
        internal static extern IntPtr TypeFind([MarshalAs(UnmanagedType.LPStr)] string basename,
            [MarshalAs(UnmanagedType.LPStr)] string nickname);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_nickname_find")]
        internal static extern IntPtr NicknameFind(IntPtr type);

        internal static string PathFilename7(string path)
        {
            ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(path);
            return PathFilename7(MemoryMarshal.GetReference(span)).ToUtf8String();
        }

        internal static string PathMode7(string path)
        {
            ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(path);
            return PathMode7(MemoryMarshal.GetReference(span)).ToUtf8String();
        }
    }

    internal static class VipsProgress
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Struct
        {
            internal IntPtr Im;

            internal int Run;

            internal int Eta;

            internal long TPels;

            internal long NPels;

            internal int Percent;

            internal IntPtr Start;
        }
    }

    internal static class VipsObject
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_get_args")]
        internal static extern int GetArgs(NetVips.VipsObject @object, out IntPtr names, out IntPtr flags,
            out int nArgs);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_get_argument")]
        internal static extern int GetArgument(NetVips.VipsObject @object,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            out IntPtr pspec, out VipsArgumentClass.Struct argumentClass,
            out VipsArgumentInstance.Struct argumentInstance);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_set_from_string")]
        internal static extern int SetFromString(NetVips.VipsObject @object,
            [MarshalAs(UnmanagedType.LPStr)] string @string);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_print_all")]
        internal static extern void PrintAll();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_unref_outputs")]
        internal static extern void UnrefOutputs(NetVips.VipsObject @object);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_get_description")]
        internal static extern IntPtr GetDescription(NetVips.VipsObject @object);
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

    internal static class VipsValue
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_ref_string")]
        internal static extern IntPtr GetRefString(in GValue.Struct value, out ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_ref_string")]
        internal static extern void SetRefString(ref GValue.Struct value, in byte str);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_blob")]
        internal static extern IntPtr GetBlob(in GValue.Struct value, out ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_blob")]
        internal static extern void SetBlob(ref GValue.Struct value, VipsCallbackFn freeFn,
            IntPtr data, ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_blob_free")]
        internal static extern void SetBlobFree(ref GValue.Struct value, IntPtr data,
            ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_double")]
        internal static extern IntPtr GetArrayDouble(in GValue.Struct value, out int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_array_double")]
        internal static extern void SetArrayDouble(ref GValue.Struct value,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            double[] array, int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_int")]
        internal static extern IntPtr GetArrayInt(in GValue.Struct value, out int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_array_int")]
        internal static extern void SetArrayInt(ref GValue.Struct value,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            int[] array, int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_array_image")]
        internal static extern IntPtr GetArrayImage(in GValue.Struct value, out int n);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_array_image")]
        internal static extern void SetArrayImage(ref GValue.Struct value, int n);
    }

    internal static class VipsImage
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_set_progress")]
        internal static extern void SetProgress(Image image, int progress);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_from_memory")]
        internal static extern IntPtr NewFromMemory(IntPtr data, UIntPtr size, int width, int height,
            int bands, Enums.VipsBandFormat format);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_matrix_from_array")]
        internal static extern IntPtr NewMatrixFromArray(int width, int height, double[] array, int size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_temp_file")]
        internal static extern IntPtr NewTempFile(in byte format);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_write")]
        internal static extern int Write(Image image, Image @out);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_write_to_memory")]
        internal static extern IntPtr WriteToMemory(Image @in, out ulong size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_copy_memory")]
        internal static extern IntPtr CopyMemory(Image image);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_image_set")]
        internal static extern void Set(Image image, [MarshalAs(UnmanagedType.LPStr)] string name,
            in GValue.Struct value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_image_get")]
        internal static extern int Get(Image image, [MarshalAs(UnmanagedType.LPStr)] string name,
            out GValue.Struct valueCopy);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_get_typeof")]
        internal static extern IntPtr GetTypeof(Image image, [MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_remove")]
        internal static extern int Remove(Image image, [MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_get_fields")]
        internal static extern IntPtr GetFields(Image image);

        internal static IntPtr NewTempFile(string format)
        {
            ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(format);
            return NewTempFile(MemoryMarshal.GetReference(span));
        }
    }

    internal static class VipsInterpolate
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_interpolate_new")]
        internal static extern IntPtr New([MarshalAs(UnmanagedType.LPStr)] string nickname);
    }

    internal static class VipsCache
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_operation_build")]
        internal static extern IntPtr OperationBuild(Operation operation);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max")]
        internal static extern void SetMax(int max);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max_mem")]
        internal static extern void SetMaxMem(ulong maxMem);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max_files")]
        internal static extern void SetMaxFiles(int maxFiles);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_trace")]
        internal static extern void SetTrace(int trace);
    }

    internal static class VipsOperation
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_get_flags")]
        internal static extern Enums.VipsOperationFlags GetFlags(Operation operation);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_new")]
        internal static extern IntPtr New([MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_flags_get_type")]
        internal static extern IntPtr FlagsGetType();
    }

    internal static class VipsForeign
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_load")]
        internal static extern IntPtr FindLoad(IntPtr filename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_load_buffer")]
        internal static extern IntPtr FindLoadBuffer(in byte data, ulong size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_save")]
        internal static extern IntPtr FindSave(IntPtr filename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_save_buffer")]
        internal static extern IntPtr FindSaveBuffer(in byte suffix);
    }
}