using System;
using System.Runtime.InteropServices;
using NetVips.Internal;

namespace NetVips
{
    /// <summary>
    /// Basic utility stuff.
    /// </summary>
    public static class Base
    {
        /// <summary>
        /// VipsInit() starts up the world of VIPS.
        /// </summary>
        /// <remarks>
        /// This function will be automatically called by <see cref="ModuleInitializer.Initialize"/>
        /// once the assembly is loaded. You should only call this method in your own program if the
        /// <see cref="ModuleInitializer"/> fails to initialize libvips.
        /// </remarks>
        /// <returns><see langword="true" /> if successful started; otherwise, <see langword="false" /></returns>
        public static bool VipsInit()
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
        /// <param name="leak"></param>
        public static void LeakSet(int leak)
        {
            Vips.LeakSet(leak);
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

        /// <summary>
        /// Get the major, minor or micro version number of the libvips library.
        /// </summary>
        /// <param name="flag">Pass 0 to get the major version number, 1 to get minor, 2 to get micro.</param>
        /// <param name="fromModule"><see langword="true" /> to get this value from the pre-initialized
        /// <see cref="ModuleInitializer.Version"/> variable.</param>
        /// <returns>The version number</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="flag" /> is not in range.</exception>
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
        /// <param name="x">major</param>
        /// <param name="y">minor</param>
        /// <param name="z">micro</param>
        /// <returns><see langword="true" /> if at least libvips x.y[.z]; otherwise, <see langword="false" /></returns>
        public static bool AtLeastLibvips(int x, int y, int z = 0)
        {
            var major = Version(0);
            var minor = Version(1);
            var micro = Version(2);

            return major > x || major == x && minor >= y && micro >= z;
        }

        #region unit test functions

        /// <summary>
        /// For testing only.
        /// </summary>
        /// <param name="path">path to split</param>
        /// <returns>The filename part of a vips7 path</returns>
        public static string PathFilename7(string path)
        {
            return Vips.PathFilename7(path);
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        /// <param name="path">path to split</param>
        /// <returns>The mode part of a vips7 path</returns>
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
        /// <param name="basename"></param>
        /// <param name="nickname"></param>
        /// <returns></returns>
        public static IntPtr TypeFind(string basename, string nickname)
        {
            return Vips.TypeFind(basename, nickname);
        }

        /// <summary>
        /// Return the name for a GType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TypeName(IntPtr type)
        {
            return Marshal.PtrToStringAnsi(GType.Name(type));
        }

        /// <summary>
        /// Return the nickname for a GType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string NicknameFind(IntPtr type)
        {
            return Marshal.PtrToStringAnsi(Vips.NicknameFind(type));
        }

        /// <summary>
        /// Map fn over all child types of gtype.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        internal static IntPtr TypeMap(IntPtr type, VipsTypeMap2Fn fn)
        {
            return Vips.TypeMap(type, fn, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Return the GType for a name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IntPtr TypeFromName(string name)
        {
            return GType.FromName(name);
        }
    }
}