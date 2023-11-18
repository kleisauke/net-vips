namespace NetVips
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Internal;

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
        /// Call this to drop caches, close plugins, terminate background threads, and finalize
        /// any internal library testing.
        /// </summary>
        /// <remarks>
        /// Calling this is optional. If you don't call it, your platform will clean up for you.
        /// The only negative consequences are that the leak checker (<see cref="Leak"/>)
        /// and the profiler (<see cref="Profile"/>) will not work.
        /// </remarks>
        public static void Shutdown()
        {
            Vips.Shutdown();
        }

        /// <summary>
        /// Enable or disable libvips leak checking.
        /// </summary>
        /// <remarks>
        /// With this enabled, libvips will check for object and area leaks on <see cref="Shutdown"/>.
        /// Enabling this option will make libvips run slightly more slowly.
        /// </remarks>
        /// <param name="leak">Bool indicating if leak checking should be turned on.</param>
        [Obsolete("NetVips.LeakSet is deprecated, please use the NetVips.Leak setter instead.")]
        public static void LeakSet(bool leak)
        {
            Leak = leak;
        }

        /// <summary>
        /// Enable or disable libvips leak checking.
        /// </summary>
        /// <remarks>
        /// With this enabled, libvips will check for object and area leaks on <see cref="Shutdown"/>.
        /// Enabling this option will make libvips run slightly more slowly.
        /// </remarks>
        public static bool Leak
        {
            set => Vips.LeakSet(value);
        }

        /// <summary>
        /// Enable or disable libvips profile recording.
        /// </summary>
        /// <remarks>
        /// If set, vips will record profiling information, and dump it on <see cref="Shutdown"/>.
        /// These profiles can be analyzed with the `vipsprofile` program.
        /// </remarks>
        /// <param name="profile">Bool indicating if profile recording should be turned on.</param>
        [Obsolete("NetVips.ProfileSet is deprecated, please use the NetVips.Profile setter instead.")]
        public static void ProfileSet(bool profile)
        {
            Profile = profile;
        }

        /// <summary>
        /// Enable or disable libvips profile recording.
        /// </summary>
        /// <remarks>
        /// If set, vips will record profiling information, and dump it on <see cref="Shutdown"/>.
        /// These profiles can be analyzed with the `vipsprofile` program.
        /// </remarks>
        public static bool Profile
        {
            set => Vips.ProfileSet(value);
        }

        /// <summary>
        /// Set the maximum number of operations libvips will cache.
        /// </summary>
        /// <param name="max">Maximum number of operations.</param>
        [Obsolete("NetVips.CacheSetMax is deprecated, please use the Cache.Max setter instead.")]
        public static void CacheSetMax(int max)
        {
            Cache.Max = max;
        }

        /// <summary>
        /// Limit the operation cache by memory use.
        /// </summary>
        /// <param name="maxMem">Maximum memory use.</param>
        [Obsolete("NetVips.CacheSetMaxMem is deprecated, please use the Cache.MaxMem setter instead.")]
        public static void CacheSetMaxMem(ulong maxMem)
        {
            Cache.MaxMem = maxMem;
        }

        /// <summary>
        /// Limit the operation cache by number of open files.
        /// </summary>
        /// <param name="maxFiles">Maximum open files.</param>
        [Obsolete("NetVips.CacheSetMaxFiles is deprecated, please use the Cache.MaxFiles setter instead.")]
        public static void CacheSetMaxFiles(int maxFiles)
        {
            Cache.MaxFiles = maxFiles;
        }

        /// <summary>
        /// Turn on libvips cache tracing.
        /// </summary>
        /// <param name="trace">Bool indicating if tracing should be turned on.</param>
        [Obsolete("NetVips.CacheSetTrace is deprecated, please use the Cache.Trace setter instead.")]
        public static void CacheSetTrace(bool trace)
        {
            Cache.Trace = trace;
        }

        /// <summary>
        /// Set the size of the pools of worker threads vips uses for image evaluation.
        /// </summary>
        /// <param name="concurrency">The size of the pools of worker threads vips uses
        /// for image evaluation.</param>
        [Obsolete("NetVips.ConcurrencySet is deprecated, please use the NetVips.Concurrency setter instead.")]
        public static void ConcurrencySet(int concurrency)
        {
            Concurrency = concurrency;
        }

        /// <summary>
        /// Returns the number of worker threads that vips uses for image evaluation.
        /// </summary>
        /// <returns>The number of worker threads.</returns>
        [Obsolete("NetVips.ConcurrencyGet is deprecated, please use the NetVips.Concurrency getter instead.")]
        public static int ConcurrencyGet()
        {
            return Concurrency;
        }

        /// <summary>
        /// Gets or sets the number of worker threads libvips' should create to process each image.
        /// </summary>
        public static int Concurrency
        {
            get => Vips.ConcurrencyGet();
            set => Vips.ConcurrencySet(value);
        }

        /// <summary>
        /// Enable or disable SIMD.
        /// </summary>
        /// <param name="enabled">Bool indicating if SIMD should be turned on.</param>
        [Obsolete("NetVips.VectorSet is deprecated, please use the NetVips.Vector setter instead.")]
        public static void VectorSet(bool enabled)
        {
            Vector = enabled;
        }

        /// <summary>
        /// Enable or disable SIMD.
        /// </summary>
        public static bool Vector
        {
            get => Vips.VectorIsEnabled();
            set => Vips.VectorSet(value);
        }

        /// <summary>
        /// Set the block state on all untrusted operations.
        /// </summary>
        /// <remarks>
        /// For example:
        /// <code language="lang-csharp">
        /// NetVips.BlockUntrusted = true;
        /// </code>
        /// Will block all untrusted operations from running. Use:
        /// <code language="lang-shell">
        /// $ vips -l
        /// </code>
        /// at the command-line to see the class hierarchy and which
        /// operations are marked as untrusted. Use
        /// <see cref="Operation.Block"/> to set the block state on
        /// specific operations in the libvips class hierarchy.
        ///
        /// At least libvips 8.13 is needed.
        /// </remarks>
        public static bool BlockUntrusted
        {
            set => Vips.BlockUntrustedSet(value);
        }

        /// <summary>
        /// Returns an array with:
        /// - the number of active allocations.
        /// - the number of bytes currently allocated via `vips_malloc()` and friends.
        /// - the number of open files.
        /// </summary>
        /// <returns>An array with memory stats. Handy for debugging / leak testing.</returns>
        [Obsolete("NetVips.MemoryStats is deprecated, please use the Stats class instead.")]
        public static int[] MemoryStats()
        {
            return new[]
            {
                Stats.Allocations,
                Stats.Mem,
                Stats.Files
            };
        }

        /// <summary>
        /// Returns the largest number of bytes simultaneously allocated via vips_tracked_malloc().
        /// Handy for estimating max memory requirements for a program.
        /// </summary>
        /// <returns>The largest number of bytes simultaneously allocated.</returns>
        [Obsolete("NetVips.MemoryHigh is deprecated, please use the Stats.MemHighwater getter instead.")]
        public static ulong MemoryHigh()
        {
            return Stats.MemHighwater;
        }

        /// <summary>
        /// Get the major, minor or patch version number of the libvips library.
        /// </summary>
        /// <param name="flag">Pass 0 to get the major version number, 1 to get minor, 2 to get patch.</param>
        /// <param name="fromModule"><see langword="true"/> to get this value from the pre-initialized
        /// <see cref="ModuleInitializer.Version"/> variable.</param>
        /// <returns>The version number.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">If <paramref name="flag"/> is not in range.</exception>
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
        /// Is this at least libvips major.minor[.patch]?
        /// </summary>
        /// <param name="x">Major component.</param>
        /// <param name="y">Minor component.</param>
        /// <param name="z">Patch component.</param>
        /// <returns><see langword="true"/> if at least libvips major.minor[.patch]; otherwise, <see langword="false"/>.</returns>
        public static bool AtLeastLibvips(int x, int y, int z = 0)
        {
            var major = Version(0);
            var minor = Version(1);
            var patch = Version(2);

            return major > x ||
                   major == x && minor > y ||
                   major == x && minor == y && patch >= z;
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
            VipsObject.PrintAll();

            Console.WriteLine("memory: {0} allocations, {1} bytes", Stats.Allocations, Stats.Mem);
            Console.WriteLine("files: {0} open", Stats.Files);

            Console.WriteLine("memory: high-water mark: {0}", Stats.MemHighwater.ToReadableBytes());

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
        internal static string PathFilename7(string path)
        {
            return Vips.PathFilename7(path);
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        /// <param name="path">Path to split.</param>
        /// <returns>The mode part of a vips7 path.</returns>
        internal static string PathMode7(string path)
        {
            return Vips.PathMode7(path);
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        internal static void VipsInterpretationGetType()
        {
            Vips.InterpretationGetType();
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        internal static void VipsOperationFlagsGetType()
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
        /// Get a list of operations available within the libvips library.
        /// </summary>
        /// <remarks>
        /// This can be useful for documentation generators.
        /// </remarks>
        /// <returns>A list of operations.</returns>
        public static List<string> GetOperations()
        {
            var allNickNames = new List<string>();
            var handle = GCHandle.Alloc(allNickNames);

            IntPtr TypeMap(IntPtr type, IntPtr a, IntPtr b)
            {
                var nickname = NicknameFind(type);

                // exclude base classes, for e.g. 'jpegload_base'
                if (TypeFind("VipsOperation", nickname) != IntPtr.Zero)
                {
                    var list = (List<string>)GCHandle.FromIntPtr(a).Target;
                    list.Add(NicknameFind(type));
                }

                return Vips.TypeMap(type, TypeMap, a, b);
            }

            try
            {
                Vips.TypeMap(TypeFromName("VipsOperation"), TypeMap, GCHandle.ToIntPtr(handle), IntPtr.Zero);
            }
            finally
            {
                handle.Free();
            }

            // Sort
            allNickNames.Sort();

            // Filter duplicates
            allNickNames = allNickNames.Distinct().ToList();

            return allNickNames;
        }

        /// <summary>
        /// Get a list of enums available within the libvips library.
        /// </summary>
        /// <returns>A list of enums.</returns>
        public static List<string> GetEnums()
        {
            var allEnums = new List<string>();
            var handle = GCHandle.Alloc(allEnums);

            IntPtr TypeMap(IntPtr type, IntPtr a, IntPtr b)
            {
                var nickname = TypeName(type);

                var list = (List<string>)GCHandle.FromIntPtr(a).Target;
                list.Add(nickname);

                return Vips.TypeMap(type, TypeMap, a, b);
            }

            try
            {
                Vips.TypeMap(TypeFromName("GEnum"), TypeMap, GCHandle.ToIntPtr(handle), IntPtr.Zero);
            }
            finally
            {
                handle.Free();
            }

            // Sort
            allEnums.Sort();

            return allEnums;
        }

        /// <summary>
        /// Get all values for a enum (GType).
        /// </summary>
        /// <param name="type">Type to return enum values for.</param>
        /// <returns>A list of values.</returns>
        public static Dictionary<string, int> ValuesForEnum(IntPtr type)
        {
            var typeClass = GType.ClassRef(type);
            var enumClass = Marshal.PtrToStructure<GEnumClass>(typeClass);

            var values = new Dictionary<string, int>((int)(enumClass.NValues - 1));

            var ptr = enumClass.Values;

            // -1 since we always have a "last" member
            for (var i = 0; i < enumClass.NValues - 1; i++)
            {
                var enumValue = Marshal.PtrToStructure<GEnumValue>(ptr);
                values[enumValue.ValueNick] = enumValue.Value;

                ptr += Marshal.SizeOf<GEnumValue>();
            }

            return values;
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

        /// <summary>
        /// Extract the fundamental type ID portion.
        /// </summary>
        /// <param name="type">A valid type ID.</param>
        /// <returns>Fundamental type ID.</returns>
        public static IntPtr FundamentalType(IntPtr type)
        {
            return GType.Fundamental(type);
        }

        /// <summary>
        /// Frees the memory pointed to by <paramref name="mem"/>.
        /// </summary>
        /// <remarks>
        /// This is needed for <see cref="Image.WriteToMemory(out ulong)"/>.
        /// </remarks>
        /// <param name="mem">The memory to free.</param>
        public static void Free(IntPtr mem)
        {
            GLib.GFree(mem);
        }
    }
}