using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using NetVips.Interop;

namespace NetVips;

/// <summary>
/// All code inside the <see cref="Initialize"/> method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Is vips initialized?
    /// </summary>
    public static bool VipsInitialized;

    /// <summary>
    /// Contains the exception when initialization of libvips fails.
    /// </summary>
    public static Exception Exception;

    /// <summary>
    /// Could contain the version number of libvips in an 3-bytes integer.
    /// </summary>
    public static int? Version;

#if NET6_0_OR_GREATER
    /// <summary>
    /// Windows specific: is GLib statically-linked in `libvips-42.dll`?
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static bool _gLibStaticallyLinked = true;

    /// <summary>
    /// A cache for <see cref="DllImportResolver"/>.
    /// </summary>
    internal static readonly Dictionary<string, nint> DllImportCache = [];

    internal static string RemapLibraryName(string libraryName)
    {
        // For Windows, we try to locate the GLib symbols within
        // `libvips-42.dll` first. If these symbols cannot be found there,
        // we proceed to locate them within `libglib-2.0-0.dll` and
        // `libgobject-2.0-0.dll`. Note that this is only possible when
        // targeting .NET 6 or higher. As a result, we always ship at least
        // 3 DLLs in a separate package for .NET Framework.
        if (OperatingSystem.IsWindows())
        {
            return _gLibStaticallyLinked ? Libraries.Vips : libraryName;
        }

        // FIXME: Switch to `OperatingSystem.IsApplePlatform()` once public.
        // See: https://github.com/dotnet/runtime/issues/113262
        var isApplePlatform = OperatingSystem.IsMacOS() || OperatingSystem.IsIOS() ||
                              OperatingSystem.IsTvOS() || OperatingSystem.IsWatchOS();

        // We can safely remap the library names to `libvips.so.42` on *nix
        // and `libvips.42.dylib` on macOS since DLLImport uses dlsym() there.
        // This function also searches for named symbols in the dependencies
        // of the shared library. Therefore, we can provide libvips as a
        // single shared library with all dependencies statically linked
        // without breaking compatibility with the shared builds
        // (i.e. what is usually installed via package managers).
        return isApplePlatform ? "libvips.42.dylib" : "libvips.so.42";
    }

    internal static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        libraryName = RemapLibraryName(libraryName);
        if (DllImportCache.TryGetValue(libraryName, out var cachedHandle))
        {
            return cachedHandle;
        }

        var handle = NativeLibrary.Load(libraryName, assembly, searchPath);
        DllImportCache[libraryName] = handle;

        return handle;
    }
#endif

    /// <summary>
    /// Initializes the module.
    /// </summary>
#pragma warning disable CA2255
    [System.Runtime.CompilerServices.ModuleInitializer]
#pragma warning restore CA2255
    public static void Initialize()
    {
#if NET6_0_OR_GREATER
        NativeLibrary.SetDllImportResolver(typeof(ModuleInitializer).Assembly, DllImportResolver);
#endif

        try
        {
            VipsInitialized = NetVips.Init();
            if (VipsInitialized)
            {
                Version = NetVips.Version(0, false);
                Version = (Version << 8) + NetVips.Version(1, false);
                Version = (Version << 8) + NetVips.Version(2, false);

#if NET6_0_OR_GREATER
                if (!OperatingSystem.IsWindows())
                {
                    return;
                }

                try
                {
                    _gLibStaticallyLinked = NetVips.TypeFromName("VipsImage") != IntPtr.Zero;
                }
                catch
                {
                    _gLibStaticallyLinked = false;
                }
#endif
            }
            else
            {
                Exception = new VipsException("unable to initialize libvips");
            }
        }
        catch (Exception e)
        {
            VipsInitialized = false;
            Exception = e;
        }
    }
}