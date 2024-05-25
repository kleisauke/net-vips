namespace NetVips.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Interop;
    using VipsObjectManaged = global::NetVips.VipsObject;
    using VipsBlobManaged = global::NetVips.VipsBlob;
    using OperationFlags = global::NetVips.Enums.OperationFlags;
    using ArgumentFlags = global::NetVips.Enums.ArgumentFlags;
    using BandFormat = global::NetVips.Enums.BandFormat;

    internal static class Vips
    {
        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr TypeMap2Fn(IntPtr type, IntPtr a, IntPtr b);

        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr ArgumentMapFn(IntPtr @object, IntPtr pspec, IntPtr argumentClass,
            IntPtr argumentInstance, IntPtr a, IntPtr b);

        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int CallbackFn(IntPtr a, IntPtr b);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_init")]
        internal static extern int Init([MarshalAs(UnmanagedType.LPStr)] string argv0);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_shutdown")]
        internal static extern void Shutdown();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_leak_set")]
        internal static extern void LeakSet([MarshalAs(UnmanagedType.Bool)] bool leak);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_profile_set")]
        internal static extern void ProfileSet([MarshalAs(UnmanagedType.Bool)] bool profile);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_get_max")]
        internal static extern int CacheGetMax();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max")]
        internal static extern void CacheSetMax(int max);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_get_max_mem")]
        internal static extern UIntPtr CacheGetMaxMem();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max_mem")]
        internal static extern void CacheSetMaxMem(ulong maxMem);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_get_max_files")]
        internal static extern int CacheGetMaxFiles();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_max_files")]
        internal static extern void CacheSetMaxFiles(int maxFiles);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_get_size")]
        internal static extern int CacheGetSize();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_set_trace")]
        internal static extern void CacheSetTrace([MarshalAs(UnmanagedType.Bool)] bool trace);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_concurrency_set")]
        internal static extern void ConcurrencySet(int concurrency);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_concurrency_get")]
        internal static extern int ConcurrencyGet();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_vector_isenabled")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VectorIsEnabled();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_vector_set_enabled")]
        internal static extern void VectorSet([MarshalAs(UnmanagedType.Bool)] bool enabled);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_block_untrusted_set")]
        internal static extern void BlockUntrustedSet([MarshalAs(UnmanagedType.Bool)] bool state);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_tracked_get_allocs")]
        internal static extern int TrackedGetAllocs();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_tracked_get_mem")]
        internal static extern int TrackedGetMem();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_tracked_get_files")]
        internal static extern int TrackedGetFiles();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_tracked_get_mem_highwater")]
        internal static extern ulong TrackedGetMemHighwater();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_version")]
        internal static extern int Version(int flag);

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
            EntryPoint = "vips_error_freeze")]
        internal static extern void ErrorFreeze();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_error_thaw")]
        internal static extern void ErrorThaw();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_path_filename7")]
        internal static extern IntPtr PathFilename7(byte[] path);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_path_mode7")]
        internal static extern IntPtr PathMode7(byte[] path);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_filename_get_filename")]
        internal static extern IntPtr GetFilename(byte[] vipsFilename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_filename_get_options")]
        internal static extern IntPtr GetOptions(byte[] vipsFilename);

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
        internal static extern IntPtr ArgumentMap(VipsObjectManaged @object, ArgumentMapFn fn, IntPtr a,
            IntPtr b);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_type_map")]
        internal static extern IntPtr TypeMap(IntPtr @base, TypeMap2Fn fn, IntPtr a, IntPtr b);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_type_find")]
        internal static extern IntPtr TypeFind([MarshalAs(UnmanagedType.LPStr)] string basename,
            [MarshalAs(UnmanagedType.LPStr)] string nickname);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_nickname_find")]
        internal static extern IntPtr NicknameFind(IntPtr type);

        internal static string PathFilename7(string path)
        {
            var bytes = Encoding.UTF8.GetBytes(path + char.MinValue); // Ensure null-terminated string
            return PathFilename7(bytes).ToUtf8String();
        }

        internal static string PathMode7(string path)
        {
            var bytes = Encoding.UTF8.GetBytes(path + char.MinValue); // Ensure null-terminated string
            return PathMode7(bytes).ToUtf8String();
        }
    }

    internal static class VipsObject
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_object_get_args")]
        internal static extern int GetArgs(VipsObjectManaged @object, out IntPtr names, out IntPtr flags,
            out int nArgs);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_get_argument")]
        internal static extern int GetArgument(VipsObjectManaged @object,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            out IntPtr pspec, out VipsArgumentClass argumentClass,
            out VipsArgumentInstance argumentInstance);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_set_from_string")]
        internal static extern int SetFromString(VipsObjectManaged @object,
            [MarshalAs(UnmanagedType.LPStr)] string @string);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_object_print_all")]
        internal static extern void PrintAll();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_unref_outputs")]
        internal static extern void UnrefOutputs(VipsObjectManaged @object);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_object_get_description")]
        internal static extern IntPtr GetDescription(VipsObjectManaged @object);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct VipsArgumentClass
    {
        internal IntPtr Parent;
        internal IntPtr ObjectClass;
        internal ArgumentFlags Flags;

        internal int Priority;
        internal uint Offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct VipsArgumentInstance
    {
        internal IntPtr Parent;
        internal IntPtr ArgumentClass;
        internal IntPtr Object;

        [MarshalAs(UnmanagedType.Bool)]
        internal bool Assigned;
        internal uint CloseId;
        internal uint InvalidateId;
    }

    internal static class VipsBlob
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_blob_get")]
        internal static extern IntPtr Get(VipsBlobManaged blob, out UIntPtr length);
    }

    internal static class VipsArea
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Struct
        {
            internal IntPtr Data;
            internal UIntPtr Length;

            internal int N;

            // private

            internal int Count;
            internal IntPtr Lock;

            internal Vips.CallbackFn FreeFn;
            internal IntPtr Client;

            internal IntPtr Type;
            internal UIntPtr SizeofType;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_area_unref")]
        internal static extern IntPtr Unref(IntPtr blob);
    }

    internal static class VipsValue
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_ref_string")]
        internal static extern IntPtr GetRefString(in GValue.Struct value, out ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_ref_string")]
        internal static extern void SetRefString(ref GValue.Struct value, byte[] str);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_get_blob")]
        internal static extern IntPtr GetBlob(in GValue.Struct value, out ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_value_set_blob")]
        internal static extern void SetBlob(ref GValue.Struct value, Vips.CallbackFn freeFn,
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
            EntryPoint = "vips_image_get_page_height")]
        internal static extern int GetPageHeight(Image image);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_invalidate_all")]
        internal static extern void InvalidateAll(Image image);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_set_progress")]
        internal static extern void SetProgress(Image image, [MarshalAs(UnmanagedType.Bool)] bool progress);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_iskilled")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsKilled(Image image);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_set_kill")]
        internal static extern void SetKill(Image image, [MarshalAs(UnmanagedType.Bool)] bool kill);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_from_memory")]
        internal static extern IntPtr NewFromMemory(IntPtr data, UIntPtr size, int width, int height,
            int bands, BandFormat format);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_from_memory_copy")]
        internal static extern IntPtr NewFromMemoryCopy(IntPtr data, UIntPtr size, int width, int height,
            int bands, BandFormat format);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_matrix_from_array")]
        internal static extern IntPtr NewMatrixFromArray(int width, int height, double[] array, int size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_new_temp_file")]
        internal static extern IntPtr NewTempFile(byte[] format);

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
            EntryPoint = "vips_image_hasalpha")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HasAlpha(Image image);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_addalpha")]
        internal static extern int AddAlpha(Image image, out IntPtr @out, IntPtr args);

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
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool Remove(Image image, [MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_image_get_fields")]
        internal static extern IntPtr GetFields(Image image);
    }

    internal static class VipsInterpolate
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_interpolate_new")]
        internal static extern IntPtr New([MarshalAs(UnmanagedType.LPStr)] string nickname);
    }

    internal static class VipsRegion
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_region_new")]
        internal static extern IntPtr New(Image image);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_region_fetch")]
        internal static extern IntPtr Fetch(Region region, int left, int top, int width, int height, out ulong length);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_region_width")]
        internal static extern int Width(Region region);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_region_height")]
        internal static extern int Height(Region region);
    }

    internal static class VipsOperation
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_get_flags")]
        internal static extern OperationFlags GetFlags(Operation operation);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_new")]
        internal static extern IntPtr New([MarshalAs(UnmanagedType.LPStr)] string name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_cache_operation_build")]
        internal static extern IntPtr Build(Operation operation);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_flags_get_type")]
        internal static extern IntPtr FlagsGetType();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_operation_block_set")]
        internal static extern void BlockSet([MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.Bool)] bool state);
    }

    internal static class VipsForeign
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_load")]
        internal static extern IntPtr FindLoad(IntPtr filename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_load")]
        internal static extern IntPtr FindLoad(byte[] filename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_load_buffer")]
        internal static extern IntPtr FindLoadBuffer(byte[] data, ulong size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_load_buffer")]
        internal static extern IntPtr FindLoadBuffer(IntPtr data, ulong size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_load_source")]
        internal static extern IntPtr FindLoadSource(Source stream);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_save")]
        internal static extern IntPtr FindSave(IntPtr filename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_save_buffer")]
        internal static extern IntPtr FindSaveBuffer(byte[] name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_find_save_target")]
        internal static extern IntPtr FindSaveTarget(byte[] name);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_foreign_get_suffixes")]
        internal static extern IntPtr GetSuffixes();
    }

    internal static class VipsConnection
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_connection_filename")]
        internal static extern IntPtr FileName(Connection connection);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_connection_nick")]
        internal static extern IntPtr Nick(Connection connection);
    }

    internal static class VipsSource
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_source_new_from_descriptor")]
        internal static extern IntPtr NewFromDescriptor(int descriptor);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_source_new_from_file")]
        internal static extern IntPtr NewFromFile(byte[] filename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_source_new_from_memory")]
        internal static extern IntPtr NewFromMemory(IntPtr data, UIntPtr size);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_source_map_blob")]
        internal static extern IntPtr MapBlob(Source source);
    }

    internal static class VipsSourceCustom
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_source_custom_new")]
        internal static extern IntPtr New();

        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate long ReadSignal(IntPtr sourcePtr, IntPtr buffer, long length, IntPtr userDataPtr);

        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate long SeekSignal(IntPtr sourcePtr, long offset, int whence, IntPtr userDataPtr);
    }

    internal static class VipsTarget
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_target_new_to_descriptor")]
        internal static extern IntPtr NewToDescriptor(int descriptor);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_target_new_to_file")]
        internal static extern IntPtr NewToFile(byte[] filename);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_target_new_to_memory")]
        internal static extern IntPtr NewToMemory();
    }

    internal static class VipsTargetCustom
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_target_custom_new")]
        internal static extern IntPtr New();

        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate long WriteSignal(IntPtr targetPtr,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            byte[] buffer, int length, IntPtr userDataPtr);

        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate long ReadSignal(IntPtr targetPtr, IntPtr buffer, long length, IntPtr userDataPtr);

        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate long SeekSignal(IntPtr targetPtr, long offset, int whence, IntPtr userDataPtr);

        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int EndSignal(IntPtr targetPtr, IntPtr userDataPtr);
    }
}