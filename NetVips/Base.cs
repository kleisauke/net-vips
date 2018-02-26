using System;
using NetVips.AutoGen;
using System.Runtime.InteropServices;

namespace NetVips
{
    public static class Base
    {
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
            vips.VipsLeakSet(leak);
        }

        /// <summary>
        /// Get the major, minor or micro version number of the libvips library.
        /// </summary>
        /// <param name="flag">Pass 0 to get the major version number, 1 to get minor, 2 to get micro.</param>
        /// <returns>The version number</returns>
        public static int Version(int flag)
        {
            var value = vips.VipsVersion(flag);
            if (value < 0)
            {
                throw new Exception("Unable to get library version");
            }

            return value;
        }

        /// <summary>
        /// Is this at least libvips x.y?
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool AtLeastLibvips(int x, int y)
        {
            var major = Version(0);
            var minor = Version(1);
            return major > x || major == x && minor >= y;
        }

        public static unsafe string PathFilename7(string filename)
        {
            return Marshal.PtrToStringAnsi((IntPtr) basic.VipsPathFilename7(filename));
        }

        public static unsafe string PathMode7(string filename)
        {
            return Marshal.PtrToStringAnsi((IntPtr) basic.VipsPathMode7(filename));
        }

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
            return @object.VipsTypeFind(basename, nickname);
        }

        /// <summary>
        /// Return the name for a GType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TypeName(ulong type)
        {
            return gtype.GTypeName(type);
        }

        /// <summary>
        /// Return the nickname for a GType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string NicknameFind(ulong type)
        {
            return @object.VipsNicknameFind(type);
        }

        /// <summary>
        /// Map fn over all child types of gtype.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static IntPtr TypeMap(ulong type, VipsTypeMap2Fn fn)
        {
            return @object.VipsTypeMap(type, fn, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Return the GType for a name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ulong TypeFromName(string name)
        {
            return gtype.GTypeFromName(name);
        }
    }
}