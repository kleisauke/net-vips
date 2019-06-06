namespace NetVips
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using global::NetVips.Internal;

    /// <summary>
    /// Basic utility stuff.
    /// </summary>
    public static class NetVips
    {
        /// <summary>
        /// Init() starts up the world of VIPS.
        /// </summary>
        /// <remarks>
        /// This function will be automatically called by <see cref="ModuleInitializer.Initialize"/>
        /// once the assembly is loaded. You should only call this method in your own program if the
        /// <see cref="ModuleInitializer"/> fails to initialize libvips.
        /// </remarks>
        /// <returns><see langword="true"/> if successful started; otherwise, <see langword="false"/>.</returns>
        public static bool Init()
        {
            return Vips.Init("NetVips") == 0;
        }

        /// <summary>
        /// Enable or disable libvips leak checking.
        /// </summary>
        /// <remarks>
        /// With this enabled, libvips will check for object and area leaks on exit.
        /// Enabling this option will make libvips run slightly more slowly.
        /// </remarks>
        /// <param name="leak">Bool indicating if leak checking should be turned on.</param>
        public static void LeakSet(bool leak)
        {
            Vips.LeakSet(leak ? 1 : 0);
        }

        /// <summary>
        /// Enable or disable libvips profile recording.
        /// </summary>
        /// <remarks>
        /// If set, vips will record profiling information, and dump it on program
        /// exit. These profiles can be analysed with the `vipsprofile` program.
        /// </remarks>
        /// <param name="profile">Bool indicating if profile recording should be turned on.</param>
        public static void ProfileSet(bool profile)
        {
            Vips.ProfileSet(profile ? 1 : 0);
        }

        /// <summary>
        /// Set the maximum number of operations libvips will cache.
        /// </summary>
        /// <param name="max">Maximum number of operations.</param>
        public static void CacheSetMax(int max)
        {
            Vips.CacheSetMax(max);
        }

        /// <summary>
        /// Limit the operation cache by memory use.
        /// </summary>
        /// <param name="maxMem">Maximum memory use.</param>
        public static void CacheSetMaxMem(ulong maxMem)
        {
            Vips.CacheSetMaxMem(maxMem);
        }

        /// <summary>
        /// Limit the operation cache by number of open files.
        /// </summary>
        /// <param name="maxFiles">Maximum open files.</param>
        public static void CacheSetMaxFiles(int maxFiles)
        {
            Vips.CacheSetMaxFiles(maxFiles);
        }

        /// <summary>
        /// Turn on libvips cache tracing.
        /// </summary>
        /// <param name="trace">Bool indicating if tracing should be turned on.</param>
        public static void CacheSetTrace(bool trace)
        {
            Vips.CacheSetTrace(trace ? 1 : 0);
        }

        /// <summary>
        /// Set the size of the pools of worker threads vips uses for image
        /// evaluation.
        /// </summary>
        /// <param name="concurrency">The size of the pools of worker threads vips uses
        /// for image evaluation.</param>
        public static void ConcurrencySet(int concurrency)
        {
            Vips.ConcurrencySet(concurrency);
        }

        /// <summary>
        /// Returns the number of worker threads that vips uses for image
        /// evaluation.
        /// </summary>
        /// <returns>The number of worker threads.</returns>
        public static int ConcurrencyGet()
        {
            return Vips.ConcurrencyGet();
        }

        /// <summary>
        /// Enable or disable SIMD and the run-time compiler.
        /// </summary>
        /// <remarks>
        /// This can give a nice speed-up, but can also be unstable on
        /// some systems or with some versions of the run-time compiler.
        /// </remarks>
        /// <param name="enabled">Bool indicating if SIMD and the run-time
        /// compiler should be turned on.</param>
        public static void VectorSet(bool enabled)
        {
            Vips.VectorSet(enabled ? 1 : 0);
        }

        /// <summary>
        /// Returns an array with:
        /// - the number of active allocations.
        /// - the number of bytes currently allocated via `vips_malloc()` and friends.
        /// - the number of open files.
        /// </summary>
        /// <returns>An array with memory stats. Handy for debugging / leak testing.</returns>
        public static int[] MemoryStats()
        {
            return new[]
            {
                Vips.TrackedGetAllocs(),
                Vips.TrackedGetMem(),
                Vips.TrackedGetFiles()
            };
        }

        /// <summary>
        /// Returns the largest number of bytes simultaneously allocated via vips_tracked_malloc().
        /// Handy for estimating max memory requirements for a program.
        /// </summary>
        /// <returns>The largest number of bytes simultaneously allocated.</returns>
        public static ulong MemoryHigh()
        {
            return Vips.TrackedGetMemHighwater();
        }

        /// <summary>
        /// Get the major, minor or micro version number of the libvips library.
        /// </summary>
        /// <param name="flag">Pass 0 to get the major version number, 1 to get minor, 2 to get micro.</param>
        /// <param name="fromModule"><see langword="true"/> to get this value from the pre-initialized
        /// <see cref="ModuleInitializer.Version"/> variable.</param>
        /// <returns>The version number.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="flag"/> is not in range.</exception>
        public static int Version(int flag, bool fromModule = true)
        {
            if (fromModule && ModuleInitializer.Version.HasValue)
            {
                var version = ModuleInitializer.Version.Value;
                switch (flag)
                {
                    case 0:
                        return (version >> 16) & 0xFF;
                    case 1:
                        return (version >> 8) & 0xFF;
                    case 2:
                        return version & 0xFF;
                }
            }

            if (flag < 0 || flag > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(flag), "Flag must be in the range of 0 to 2");
            }

            var value = Vips.Version(flag);
            if (value < 0)
            {
                throw new VipsException("Unable to get library version");
            }

            return value;
        }

        /// <summary>
        /// Is this at least libvips x.y[.z]?
        /// </summary>
        /// <param name="x">Major component.</param>
        /// <param name="y">Minor component.</param>
        /// <param name="z">Micro component.</param>
        /// <returns><see langword="true"/> if at least libvips x.y[.z]; otherwise, <see langword="false"/>.</returns>
        public static bool AtLeastLibvips(int x, int y, int z = 0)
        {
            var major = Version(0);
            var minor = Version(1);
            var micro = Version(2);

            return major > x || major == x && minor >= y && micro >= z;
        }

        /// <summary>
        /// Get a list of all the filename suffixes supported by libvips.
        /// </summary>
        /// <remarks>
        /// At least libvips 8.8 is needed.
        /// </remarks>
        /// <returns>An array of strings or <see langword="null"/>.</returns>
        public static string[] GetSuffixes()
        {
            if (!AtLeastLibvips(8, 8))
            {
                return null;
            }

            var ptrArr = VipsForeign.GetSuffixes();

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

        /// <summary>
        /// Reports leaks (hopefully there are none) it also tracks and reports peak memory use.
        /// </summary>
        internal static void ReportLeak()
        {
            var memStats = MemoryStats();
            var activeAllocs = memStats[0];
            var currentAllocs = memStats[1];
            var files = memStats[2];

            VipsObject.PrintAll();

            Console.WriteLine("memory: {0} allocations, {1} bytes", activeAllocs, currentAllocs);
            Console.WriteLine("files: {0} open", files);

            Console.WriteLine("memory: high-water mark: {0}", MemoryHigh().ToReadableBytes());

            var errorBuffer = Marshal.PtrToStringAnsi(Vips.ErrorBuffer());
            if (!string.IsNullOrEmpty(errorBuffer))
            {
                Console.WriteLine("error buffer: {0}", errorBuffer);
            }
        }

        #region unit test functions

        /// <summary>
        /// For testing only.
        /// </summary>
        /// <param name="path">Path to split.</param>
        /// <returns>The filename part of a vips7 path.</returns>
        public static string PathFilename7(string path)
        {
            return Vips.PathFilename7(path);
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        /// <param name="path">Path to split.</param>
        /// <returns>The mode part of a vips7 path.</returns>
        public static string PathMode7(string path)
        {
            return Vips.PathMode7(path);
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        public static void VipsInterpretationGetType()
        {
            Vips.InterpretationGetType();
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        public static void VipsOperationFlagsGetType()
        {
            VipsOperation.FlagsGetType();
        }

        #endregion

        /// <summary>
        /// Get the GType for a name.
        /// </summary>
        /// <remarks>
        /// Looks up the GType for a nickname. Types below basename in the type
        /// hierarchy are searched.
        /// </remarks>
        /// <param name="basename">Name of base class.</param>
        /// <param name="nickname">Search for a class with this nickname.</param>
        /// <returns>The GType of the class, or <see cref="IntPtr.Zero"/> if the class is not found.</returns>
        public static IntPtr TypeFind(string basename, string nickname)
        {
            return Vips.TypeFind(basename, nickname);
        }

        /// <summary>
        /// Return the name for a GType.
        /// </summary>
        /// <param name="type">Type to return name for.</param>
        /// <returns>Type name.</returns>
        public static string TypeName(IntPtr type)
        {
            return Marshal.PtrToStringAnsi(GType.Name(type));
        }

        /// <summary>
        /// Return the nickname for a GType.
        /// </summary>
        /// <param name="type">Type to return nickname for.</param>
        /// <returns>Nickname.</returns>
        public static string NicknameFind(IntPtr type)
        {
            return Marshal.PtrToStringAnsi(Vips.NicknameFind(type));
        }

        /// <summary>
        /// Map over a type's children. Stop when <paramref name="fn"/> returns
        /// non-<see cref="IntPtr.Zero"/> and return that value.
        /// </summary>
        /// <param name="type">Base type.</param>
        /// <param name="fn">Call this function for every type.</param>
        /// <returns><see cref="IntPtr.Zero"/> if <paramref name="fn"/> returns <see cref="IntPtr.Zero"/> for all arguments,
        /// otherwise the first non-<see cref="IntPtr.Zero"/> value from <paramref name="fn"/>.</returns>
        internal static IntPtr TypeMap(IntPtr type, VipsTypeMap2Fn fn)
        {
            return Vips.TypeMap(type, fn, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Return the GType for a name.
        /// </summary>
        /// <param name="name">Type name to lookup.</param>
        /// <returns>Corresponding type ID or <see cref="IntPtr.Zero"/>.</returns>
        public static IntPtr TypeFromName(string name)
        {
            return GType.FromName(name);
        }
    }
}