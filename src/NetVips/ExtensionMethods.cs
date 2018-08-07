using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NetVips.Internal;

namespace NetVips
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// All numeric types. Used by <see cref="IsNumeric(Type)"/>.
        /// </summary>
        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(double),
            typeof(decimal),
            typeof(long),
            typeof(short),
            typeof(sbyte),
            typeof(byte),
            typeof(ulong),
            typeof(ushort),
            typeof(uint),
            typeof(float)
        };

        /// <summary>
        /// Removes the element with the specified key from the <see cref="VOption" />
        /// and retrieves the value to <paramref name="target" />.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key">>The key of the element to remove.</param>
        /// <param name="target">The target to retrieve the value to.</param>
        /// <returns><see langword="true" /> if the element is successfully removed; otherwise, <see langword="false" /></returns>
        public static bool Remove(this VOption self, string key, out object target)
        {
            self.TryGetValue(key, out target);
            return self.Remove(key);
        }

        /// <summary>
        /// Merges 2 <see cref="VOption" />s
        /// </summary>
        /// <param name="self"></param>
        /// <param name="merge"></param>
        public static void Merge(this VOption self, VOption merge)
        {
            foreach (var item in merge)
            {
                self[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// Check whether the object is a pixel.
        /// </summary>
        /// <param name="value"></param>
        /// <returns><see langword="true" /> if the object is a pixel; otherwise, <see langword="false" /></returns>
        public static bool IsPixel(this object value)
        {
            var valueType = value.GetType();
            return valueType.IsNumeric() ||
                   value is Array array && array.Length > 0 && !(valueType == typeof(Image));
        }

        /// <summary>
        /// Test for rectangular array of something
        /// </summary>
        /// <param name="value"></param>
        /// <returns><see langword="true" /> if the object is a rectangular array; otherwise, <see langword="false" /></returns>
        public static bool Is2D(this object value)
        {
            return value is Array array &&
                   array.Length > 0 &&
                   (array.Rank == 2 || array.GetValue(0) is Array jaggedArray &&
                    jaggedArray.Length == array.Length);
        }

        /// <summary>
        /// apply a function to a thing, or map over a list
        /// we often need to do something like (1.0 / other) and need to work for lists
        /// as well as scalars
        /// </summary>
        /// <param name="values"></param>
        /// <param name="func"></param>
        public static T[] Smap<T>(this IEnumerable<object> values, Func<object, T> func)
        {
            return values.Select(func).ToArray();
        }

        /// <summary>
        /// apply a function to a thing, or map over a list
        /// we often need to do something like (1.0 / other) and need to work for lists
        /// as well as scalars
        /// </summary>
        /// <param name="x"></param>
        /// <param name="func"></param>
        public static object Smap<T>(this T x, Func<T, T> func)
        {
            if (x is IEnumerable enumerable)
            {
                return enumerable.Cast<T>().Select(func).ToArray();
            }

            return func(x);
        }

        /// <summary>
        /// Extension method, call for any object, eg "if (x.IsNumeric())..."
        /// </summary>
        /// <param name="x"></param>
        /// <returns><see langword="true" /> if the object is numeric; otherwise, <see langword="false" /></returns>
        public static bool IsNumeric(this object x)
        {
            return x != null && x.GetType().IsNumeric();
        }

        /// <summary>
        /// Checks if the given <paramref name="myType" /> is numeric.
        /// </summary>
        /// <param name="myType"></param>
        /// <returns><see langword="true" /> if the type is numeric; otherwise, <see langword="false" /></returns>
        public static bool IsNumeric(this Type myType)
        {
            return NumericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);
        }

        /// <summary>
        /// Dereferences data from an unmanaged block of memory 
        /// to a newly allocated managed object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to be created. This object
        /// must represent a formatted class or a structure.</typeparam>
        /// <param name="ptr">A pointer to an unmanaged block of memory.</param>
        /// <returns>A newly allocated managed object of the specified type.</returns>
        public static T Dereference<T>(this IntPtr ptr)
        {
            return (T) Marshal.PtrToStructure(ptr, typeof(T));
        }

        /// <summary>
        /// Creates a new pointer to an unmanaged block of memory 
        /// from an structure of the specified type.
        /// </summary>
        /// <remarks>
        /// The returned pointer should be freed by calling <see cref="GLib.GFree"/>.
        /// </remarks>
        /// <typeparam name="T">The type of structure to be created.</typeparam>
        /// <param name="structure">A managed object that holds the data to be marshaled. 
        /// This object must be a structure or an instance of a formatted class.</param>
        /// <returns>A pointer to an pre-allocated block of memory of the specified type.</returns>
        public static IntPtr ToIntPtr<T>(this object structure) where T : struct
        {
            // Initialize unmanged memory to hold the struct.
            var ptr = GLib.GMalloc(new UIntPtr((ulong) Marshal.SizeOf(typeof(T))));

            // Copy the struct to unmanaged memory.
            Marshal.StructureToPtr(structure, ptr, false);

            return ptr;
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="operationName"></param>
        /// <returns></returns>
        public static object Call(this Image image, string operationName)
        {
            return Operation.Call(operationName, null, image);
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="operationName"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static object Call(this Image image, string operationName, object arg)
        {
            return Operation.Call(operationName, null, image, arg);
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="operationName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Call(this Image image, string operationName, params object[] args)
        {
            return Operation.Call(operationName, null, args.PrependImage(image));
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="operationName"></param>
        /// <param name="kwargs"></param>
        /// <returns></returns>
        public static object Call(this Image image, string operationName, VOption kwargs)
        {
            return Operation.Call(operationName, kwargs, image);
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="operationName"></param>
        /// <param name="kwargs"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static object Call(this Image image, string operationName, VOption kwargs, object arg)
        {
            return Operation.Call(operationName, kwargs, image, arg);
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="operationName"></param>
        /// <param name="kwargs"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Call(this Image image, string operationName, VOption kwargs, params object[] args)
        {
            return Operation.Call(operationName, kwargs, args.PrependImage(image));
        }

        /// <summary>
        /// Make first letter of a string upper case
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToUpper(str[0]) + str.Substring(1);
            }

            return str.ToUpper();
        }

        /// <summary>
        /// Make first letter of a string lower case
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstLetterToLower(this string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToLower(str[0]) + str.Substring(1);
            }

            return str.ToLower();
        }

        /// <summary>
        /// Convert snake case (my_string) to camel case (MyString).
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string str)
        {
            return str.Split(new[] {"_"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
                .Aggregate(string.Empty, (s1, s2) => s1 + s2);
        }

        /// <summary>
        /// Prepends <paramref name="image" /> to <paramref name="args" />.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="image"></param>
        /// <returns>A new object array.</returns>
        public static object[] PrependImage(this IEnumerable args, Image image)
        {
            if (args == null)
            {
                return new object[] {image};
            }

            var enumerable = args as object[] ?? args.Cast<object>().ToArray();

            var newValues = new object[enumerable.Length + 1];
            newValues[0] = image;
            Array.Copy(enumerable, 0, newValues, 1, enumerable.Length);
            return newValues;
        }

        /// <summary>
        /// Marshals a GLib UTF8 char* to a managed string.
        /// </summary>
        /// <returns>The managed string string.</returns>
        /// <param name="ptr">Pointer to the GLib string.</param>
        public static string ToUtf8String(this IntPtr ptr)
        {
            return ptr == IntPtr.Zero ? null : Encoding.UTF8.GetString(ptr.ToByteString());
        }

        /// <summary>
        /// Marshals a managed string to a GLib UTF8 char*.
        /// </summary>
        /// <remarks>
        /// The returned pointer should be freed by calling <see cref="GLib.GFree"/>.
        /// </remarks>
        /// <param name="str">The managed string.</param>
        /// <returns>The to pointer to the GLib string.</returns>
        public static IntPtr ToUtf8Ptr(this string str)
        {
            return str == null ? IntPtr.Zero : Encoding.UTF8.GetBytes(str).ToPtr();
        }

        /// <summary>
        /// Marshals a managed byte array to a C string.
        /// </summary>
        /// <remarks>
        /// The returned pointer should be freed by calling <see cref="GLib.GFree"/>.
        /// The byte array should not include the null terminator. It will be
        /// added automatically.
        /// </remarks>
        /// <param name="bytes">The managed byte array.</param>
        /// <returns>A pointer to the unmanaged string.</returns>
        public static IntPtr ToPtr(this byte[] bytes)
        {
            if (bytes == null)
            {
                return IntPtr.Zero;
            }

            var ptr = GLib.GMalloc(new UIntPtr((ulong) bytes.Length + 1));
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            return ptr;
        }

        /// <summary>
        /// Marshals a C string pointer to a byte array.
        /// </summary>
        /// <remarks>
        /// Since encoding is not specified, the string is returned as a byte array.
        /// The byte array does not include the null terminator.
        /// </remarks>
        /// <param name="ptr">Pointer to the unmanaged string.</param>
        /// <returns>The string as a byte array.</returns>
        public static byte[] ToByteString(this IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            var bytes = new List<byte>();
            var offset = 0;

            byte b;
            while ((b = Marshal.ReadByte(ptr, offset++)) != 0)
            {
                bytes.Add(b);
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Convert bytes to human readable format.
        /// </summary>
        /// <param name="value">The number of bytes.</param>
        /// <returns>The readable format of the bytes.</returns>
        internal static string ToReadableBytes(this ulong value)
        {
            string[] sizeSuffixes = {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

            var i = 0;
            decimal dValue = value;
            while (Math.Round(dValue, 2) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return $"{dValue:n2} {sizeSuffixes[i]}";
        }
    }
}