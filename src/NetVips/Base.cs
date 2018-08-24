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
        /// VipsInit() starts up the world of VIPS. You should not call
        /// this method in your own program. It is already called by
        /// <see cref="ModuleInitializer.Initialize"/> once the assembly
        /// is loaded.
        /// </summary>
        /// <returns><see langword="true" /> if successful started; otherwise, <see langword="false" /></returns>
        public static bool VipsInit()
        {
            return Vips.VipsInit("NetVips") == 0;
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
            Vips.VipsLeakSet(leak);
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
                Vips.VipsTrackedGetAllocs(),
                Vips.VipsTrackedGetMem(),
                Vips.VipsTrackedGetFiles()
            };
        }

        /// <summary>
        /// Returns the largest number of bytes simultaneously allocated via vips_tracked_malloc().
        /// Handy for estimating max memory requirements for a program.
        /// </summary>
        /// <returns>The largest number of bytes simultaneously allocated.</returns>
        public static ulong MemoryHigh()
        {
            return Vips.VipsTrackedGetMemHighwater();
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

            var errorBuffer = Marshal.PtrToStringAnsi(Vips.VipsErrorBuffer());
            if (!string.IsNullOrEmpty(errorBuffer))
            {
                Console.WriteLine("error buffer: {0}", errorBuffer);
            }
        }

        /// <summary>
        /// Get the major, minor or micro version number of the libvips library.
        /// </summary>
        /// <param name="flag">Pass 0 to get the major version number, 1 to get minor, 2 to get micro.</param>
        /// <returns>The version number</returns>
        public static int Version(int flag)
        {
            var value = Vips.VipsVersion(flag);
            if (value < 0)
            {
                throw new VipsException("Unable to get library version");
            }

            return value;
        }

        /// <summary>
        /// Is this at least libvips x.y?
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns><see langword="true" /> if at least libvips x.y; otherwise, <see langword="false" /></returns>
        public static bool AtLeastLibvips(int x, int y)
        {
            var major = Version(0);
            var minor = Version(1);
            return major > x || major == x && minor >= y;
        }

        #region unit test functions

        /// <summary>
        /// For testing only.
        /// </summary>
        /// <param name="path">path to split</param>
        /// <returns>The filename part of a vips7 path</returns>
        public static string PathFilename7(string path)
        {
            return Vips.VipsPathFilename7(path);
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        /// <param name="path">path to split</param>
        /// <returns>The mode part of a vips7 path</returns>
        public static string PathMode7(string path)
        {
            return Vips.VipsPathMode7(path);
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        public static void VipsInterpretationGetType()
        {
            Vips.VipsInterpretationGetType();
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        public static void VipsOperationFlagsGetType()
        {
            Vips.VipsOperationFlagsGetType();
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
            return Internal.VipsObject.VipsTypeFind(basename, nickname);
        }

        /// <summary>
        /// Return the name for a GType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TypeName(IntPtr type)
        {
            return Marshal.PtrToStringAnsi(GType.GTypeName(type));
        }

        /// <summary>
        /// Return the nickname for a GType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string NicknameFind(IntPtr type)
        {
            return Marshal.PtrToStringAnsi(Internal.VipsObject.VipsNicknameFind(type));
        }

        /// <summary>
        /// Map fn over all child types of gtype.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        internal static IntPtr TypeMap(IntPtr type, VipsTypeMap2Fn fn)
        {
            return Internal.VipsObject.VipsTypeMap(type, fn, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Return the GType for a name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IntPtr TypeFromName(string name)
        {
            return GType.GTypeFromName(name);
        }
    }
}