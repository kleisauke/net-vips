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
        /// VipsInit() starts up the world of VIPS. You should call this on
        /// program startup before using any other VIPS operations. 
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
        /// <returns></returns>
        public static void LeakSet(int leak)
        {
            Vips.VipsLeakSet(leak);
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
        /// <returns></returns>
        public static void VipsInterpretationGetType()
        {
            Vips.VipsInterpretationGetType();
        }

        /// <summary>
        /// For testing only.
        /// </summary>
        /// <returns></returns>
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
        public static ulong TypeFind(string basename, string nickname)
        {
            return Internal.VipsObject.VipsTypeFind(basename, nickname);
        }

        /// <summary>
        /// Return the name for a GType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TypeName(ulong type)
        {
            return Marshal.PtrToStringAnsi(GType.GTypeName(type));
        }

        /// <summary>
        /// Return the nickname for a GType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string NicknameFind(ulong type)
        {
            return Marshal.PtrToStringAnsi(Internal.VipsObject.VipsNicknameFind(type));
        }

        /// <summary>
        /// Map fn over all child types of gtype.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        internal static IntPtr TypeMap(ulong type, VipsTypeMap2Fn fn)
        {
            return Internal.VipsObject.VipsTypeMap(type, fn, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Return the GType for a name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ulong TypeFromName(string name)
        {
            return GType.GTypeFromName(name);
        }
    }
}