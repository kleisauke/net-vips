using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace NetVips
{
    public static class ExtensionMethods
    {
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

        public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, out TValue target)
        {
            self.TryGetValue(key, out target);
            return self.Remove(key);
        }

        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> self, IDictionary<TKey, TValue> merge)
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
        /// <returns></returns>
        public static bool IsPixel(this object value)
        {
            Type valueType = value.GetType();
            return valueType.IsNumeric() ||
                   value is Array array && array.Length > 0 && !(valueType == typeof(Image));
        }


        /// <summary>
        /// Test for rectangular array of something
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Is2D(this object value)
        {
            return value is object[][] jaggedArr &&
                   jaggedArr.Length > 0 &&
                   jaggedArr.Rank == 2 &&
                   jaggedArr.All(x => x.Length == jaggedArr[0].Length);
        }

        /// <summary>
        /// apply a function to a thing, or map over a list
        /// we often need to do something like (1.0 / other) and need to work for lists
        /// as well as scalars
        /// </summary>
        /// <param name="func"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static T[] Smap<T>(this IEnumerable<T> x, Func<T, T> func)
        {
            return x.ToList().Select(func).ToArray();
        }

        /// <summary>
        /// apply a function to a thing, or map over a list
        /// we often need to do something like (1.0 / other) and need to work for lists
        /// as well as scalars
        /// </summary>
        /// <param name="func"></param>
        /// <param name="x"></param>
        public static T Smap<T>(this T x, Func<T, T> func)
        {
            return func(x);
        }

        /// <summary>
        ///  Extension method, call for any object, eg "if (x.IsNumeric())..."
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static bool IsNumeric(this object x)
        {
            return x != null && IsNumeric(x.GetType());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myType"></param>
        /// <returns></returns>
        public static bool IsNumeric(this Type myType)
        {
            return NumericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static T Dereference<T>(this IntPtr ptr)
        {
            return (T) Marshal.PtrToStructure(ptr, typeof(T));
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
            return Operation.Call(operationName, image, arg);
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
            return Operation.Call(operationName, args.PrependImage(image));
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="operationName"></param>
        /// <param name="kwargs"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static object Call(this Image image, string operationName, IDictionary<string, object> kwargs,
            object arg)
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
        public static object Call(this Image image, string operationName, IDictionary<string, object> kwargs,
            params object[] args)
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

        public static string ToCamelCase(this string str)
        {
            return str.Split(new[] {"_"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
                .Aggregate(string.Empty, (s1, s2) => s1 + s2);
        }

        public static object[] PrependImage(this object[] args, object image)
        {
            if (args == null)
            {
                return new[] {image};
            }

            var newValues = new object[args.Length + 1];
            newValues[0] = image;
            Array.Copy(args, 0, newValues, 1, args.Length);
            return newValues;
        }
    }
}