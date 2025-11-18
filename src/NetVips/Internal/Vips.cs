using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using NetVips.Interop;

using ArgumentFlags = NetVips.Enums.ArgumentFlags;
using BandFormat = NetVips.Enums.BandFormat;
using OperationFlags = NetVips.Enums.OperationFlags;
using VipsBlobManaged = NetVips.VipsBlob;
using VipsObjectManaged = NetVips.VipsObject;

namespace NetVips.Internal;

internal static class Vips
{
    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate nint TypeMap2Fn(nint type, nint a, nint b);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate nint ArgumentMapFn(nint @object, nint pspec, nint argumentClass,
        nint argumentInstance, nint a, nint b);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int CallbackFn(nint a, nint b);

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
    internal static extern nuint CacheGetMaxMem();

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
    internal static extern nint ErrorBuffer();

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
    internal static extern nint PathFilename7(byte[] path);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_path_mode7")]
    internal static extern nint PathMode7(byte[] path);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_filename_get_filename")]
    internal static extern nint GetFilename(byte[] vipsFilename);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_filename_get_options")]
    internal static extern nint GetOptions(byte[] vipsFilename);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_blend_mode_get_type")]
    internal static extern nint BlendModeGetType();

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_interpretation_get_type")]
    internal static extern nint InterpretationGetType();

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_band_format_get_type")]
    internal static extern nint BandFormatGetType();

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_argument_map")]
    internal static extern nint ArgumentMap(VipsObjectManaged @object, ArgumentMapFn fn, nint a,
        nint b);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_type_map")]
    internal static extern nint TypeMap(nint @base, TypeMap2Fn fn, nint a, nint b);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_type_find")]
    internal static extern nint TypeFind([MarshalAs(UnmanagedType.LPStr)] string basename,
        [MarshalAs(UnmanagedType.LPStr)] string nickname);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_nickname_find")]
    internal static extern nint NicknameFind(nint type);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_format_sizeof")]
    internal static extern ulong FormatSizeof(BandFormat format);

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
    internal static extern int GetArgs(VipsObjectManaged @object, out nint names, out nint flags,
        out int nArgs);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_object_get_argument")]
    internal static extern int GetArgument(VipsObjectManaged @object,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        out nint pspec, out VipsArgumentClass argumentClass,
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
    internal static extern nint GetDescription(VipsObjectManaged @object);
}

[StructLayout(LayoutKind.Sequential)]
internal struct VipsArgumentClass
{
    internal nint Parent;
    internal nint ObjectClass;
    internal ArgumentFlags Flags;

    internal int Priority;
    internal uint Offset;
}

[StructLayout(LayoutKind.Sequential)]
internal struct VipsArgumentInstance
{
    internal nint Parent;
    internal nint ArgumentClass;
    internal nint Object;

    [MarshalAs(UnmanagedType.Bool)]
    internal bool Assigned;
    internal uint CloseId;
    internal uint InvalidateId;
}

internal static class VipsBlob
{
    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_blob_get")]
    internal static extern nint Get(VipsBlobManaged blob, out nuint length);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_blob_copy")]
    internal static extern unsafe nint Copy(void* data, nuint length);
}

internal static class VipsArea
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Struct
    {
        internal nint Data;
        internal nuint Length;

        internal int N;

        // private

        internal int Count;
        internal nint Lock;

        internal Vips.CallbackFn FreeFn;
        internal nint Client;

        internal nint Type;
        internal nuint SizeofType;
    }

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_area_unref")]
    internal static extern nint Unref(nint blob);
}

internal static class VipsValue
{
    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_get_ref_string")]
    internal static extern nint GetRefString(in GValue.Struct value, out ulong length);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_set_ref_string")]
    internal static extern void SetRefString(ref GValue.Struct value, byte[] str);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_get_blob")]
    internal static extern nint GetBlob(in GValue.Struct value, out ulong length);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_set_blob")]
    internal static extern void SetBlob(ref GValue.Struct value, Vips.CallbackFn freeFn,
        nint data, ulong length);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_set_blob_free")]
    internal static extern void SetBlobFree(ref GValue.Struct value, nint data,
        ulong length);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_get_array_double")]
    internal static extern nint GetArrayDouble(in GValue.Struct value, out int n);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_set_array_double")]
    internal static extern void SetArrayDouble(ref GValue.Struct value,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
        double[] array, int n);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_get_array_int")]
    internal static extern nint GetArrayInt(in GValue.Struct value, out int n);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_set_array_int")]
    internal static extern void SetArrayInt(ref GValue.Struct value,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
        int[] array, int n);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_value_get_array_image")]
    internal static extern nint GetArrayImage(in GValue.Struct value, out int n);

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
    internal static extern nint NewFromMemory(nint data, nuint size, int width, int height,
        int bands, BandFormat format);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_new_from_memory")]
    internal static extern unsafe nint NewFromMemory(void* data, nuint size, int width, int height,
        int bands, BandFormat format);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_new_from_memory_copy")]
    internal static extern nint NewFromMemoryCopy(nint data, nuint size, int width, int height,
        int bands, BandFormat format);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_new_from_memory_copy")]
    internal static extern unsafe nint NewFromMemoryCopy(void* data, nuint size, int width, int height,
        int bands, BandFormat format);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_new_matrix_from_array")]
    internal static extern nint NewMatrixFromArray(int width, int height, double[] array, int size);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_new_temp_file")]
    internal static extern nint NewTempFile(byte[] format);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_write")]
    internal static extern int Write(Image image, Image @out);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_write_to_memory")]
    internal static extern nint WriteToMemory(Image @in, out ulong size);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_hasalpha")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool HasAlpha(Image image);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_addalpha")]
    internal static extern int AddAlpha(Image image, out nint @out, nint sentinel = default);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_copy_memory")]
    internal static extern nint CopyMemory(Image image);

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
    internal static extern nint GetTypeof(Image image, [MarshalAs(UnmanagedType.LPStr)] string name);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_remove")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool Remove(Image image, [MarshalAs(UnmanagedType.LPStr)] string name);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_image_get_fields")]
    internal static extern nint GetFields(Image image);
}

internal static class VipsInterpolate
{
    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_interpolate_new")]
    internal static extern nint New([MarshalAs(UnmanagedType.LPStr)] string nickname);
}

internal static class VipsRegion
{
    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_region_new")]
    internal static extern nint New(Image image);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_region_fetch")]
    internal static extern nint Fetch(Region region, int left, int top, int width, int height, out ulong length);

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
    internal static extern nint New([MarshalAs(UnmanagedType.LPStr)] string name);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_cache_operation_build")]
    internal static extern nint Build(Operation operation);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_operation_flags_get_type")]
    internal static extern nint FlagsGetType();

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
    internal static extern nint FindLoad(nint filename);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_foreign_find_load")]
    internal static extern nint FindLoad(byte[] filename);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_foreign_find_load_buffer")]
    internal static extern nint FindLoadBuffer(byte[] data, ulong size);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_foreign_find_load_buffer")]
    internal static extern nint FindLoadBuffer(nint data, ulong size);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_foreign_find_load_buffer")]
    internal static extern unsafe nint FindLoadBuffer(void* data, ulong size);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_foreign_find_load_source")]
    internal static extern nint FindLoadSource(Source stream);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_foreign_find_save")]
    internal static extern nint FindSave(nint filename);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_foreign_find_save_buffer")]
    internal static extern nint FindSaveBuffer(byte[] name);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_foreign_find_save_target")]
    internal static extern nint FindSaveTarget(byte[] name);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_foreign_get_suffixes")]
    internal static extern nint GetSuffixes();
}

internal static class VipsConnection
{
    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_connection_filename")]
    internal static extern nint FileName(Connection connection);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_connection_nick")]
    internal static extern nint Nick(Connection connection);
}

internal static class VipsSource
{
    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_source_new_from_descriptor")]
    internal static extern nint NewFromDescriptor(int descriptor);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_source_new_from_file")]
    internal static extern nint NewFromFile(byte[] filename);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_source_new_from_blob")]
    internal static extern nint NewFromBlob(VipsBlobManaged blob);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_source_new_from_memory")]
    internal static extern nint NewFromMemory(nint data, nuint size);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vips_source_map_blob")]
    internal static extern nint MapBlob(Source source);
}

internal static class VipsSourceCustom
{
    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_source_custom_new")]
    internal static extern nint New();

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long ReadSignal(nint sourcePtr, nint buffer, long length, nint userDataPtr);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long SeekSignal(nint sourcePtr, long offset, int whence, nint userDataPtr);
}

internal static class VipsTarget
{
    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_target_new_to_descriptor")]
    internal static extern nint NewToDescriptor(int descriptor);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_target_new_to_file")]
    internal static extern nint NewToFile(byte[] filename);

    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_target_new_to_memory")]
    internal static extern nint NewToMemory();
}

internal static class VipsTargetCustom
{
    [SuppressUnmanagedCodeSecurity]
    [DllImport(Libraries.Vips, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "vips_target_custom_new")]
    internal static extern nint New();

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long WriteSignal(nint targetPtr,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
        byte[] buffer, int length, nint userDataPtr);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long ReadSignal(nint targetPtr, nint buffer, long length, nint userDataPtr);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long SeekSignal(nint targetPtr, long offset, int whence, nint userDataPtr);

    [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int EndSignal(nint targetPtr, nint userDataPtr);
}