namespace NetVips
{
    using System;
    using System.Buffers;
    using System.Runtime.InteropServices;
    using System.Text;
    using Internal;

    /// <summary>
    /// Useful extension methods that we use in our codebase.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Removes the element with the specified key from the <see cref="VOption"/>
        /// and retrieves the value to <paramref name="target"/>.
        /// </summary>
        /// <param name="self">The <see cref="VOption"/> to remove from.</param>
        /// <param name="key">>The key of the element to remove.</param>
        /// <param name="target">The target to retrieve the value to.</param>
        /// <returns><see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>.</returns>
        internal static bool Remove(this VOption self, string key, out object target)
        {
            self.TryGetValue(key, out target);
            return self.Remove(key);
        }

        /// <summary>
        /// Merges 2 <see cref="VOption"/>s.
        /// </summary>
        /// <param name="self">The <see cref="VOption"/> to merge into.</param>
        /// <param name="merge">The <see cref="VOption"/> to merge from.</param>
        internal static void Merge(this VOption self, VOption merge)
        {
            foreach (var item in merge)
            {
                self[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image">A <see cref="Image"/> used as guide.</param>
        /// <param name="operationName">Operation name.</param>
        /// <returns>A new object.</returns>
        internal static object Call(this Image image, string operationName) =>
            Operation.Call(operationName, null, image);

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image">A <see cref="Image"/> used as guide.</param>
        /// <param name="operationName">Operation name.</param>
        /// <param name="args">An arbitrary number and variety of arguments.</param>
        /// <returns>A new object.</returns>
        internal static object Call(this Image image, string operationName, params object[] args) =>
            Operation.Call(operationName, null, image, args);

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image">A <see cref="Image"/> used as guide.</param>
        /// <param name="operationName">Operation name.</param>
        /// <param name="kwargs">Optional arguments.</param>
        /// <returns>A new object.</returns>
        internal static object Call(this Image image, string operationName, VOption kwargs) =>
            Operation.Call(operationName, kwargs, image);

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <param name="image">A <see cref="Image"/> used as guide.</param>
        /// <param name="operationName">Operation name.</param>
        /// <param name="kwargs">Optional arguments.</param>
        /// <param name="args">An arbitrary number and variety of arguments.</param>
        /// <returns>A new object.</returns>
        internal static object Call(this Image image, string operationName, VOption kwargs, params object[] args) =>
            Operation.Call(operationName, kwargs, image, args);

        /// <summary>
        /// Prepends <paramref name="image"/> to <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="Image"/> array.</param>
        /// <param name="image">The <see cref="Image"/> to prepend to <paramref name="args"/>.</param>
        /// <returns>A new object array.</returns>
        internal static object[] PrependImage<T>(this T[] args, Image image)
        {
            if (args == null)
            {
                return new object[] { image };
            }

            var newValues = new object[args.Length + 1];
            newValues[0] = image;
            Array.Copy(args, 0, newValues, 1, args.Length);
            return newValues;
        }

        /// <summary>
        /// Marshals a GLib UTF8 char* to a managed string.
        /// </summary>
        /// <param name="utf8Str">Pointer to the GLib string.</param>
        /// <param name="freePtr">If set to <see langword="true"/>, free the GLib string.</param>
        /// <param name="size">Size of the GLib string, use 0 to read until the null character.</param>
        /// <returns>The managed string.</returns>
        internal static string ToUtf8String(this IntPtr utf8Str, bool freePtr = false, int size = 0)
        {
            if (utf8Str == IntPtr.Zero)
            {
                return null;
            }

            if (size == 0)
            {
                while (Marshal.ReadByte(utf8Str, size) != 0)
                {
                    ++size;
                }
            }

            if (size == 0)
            {
                if (freePtr)
                {
                    GLib.GFree(utf8Str);
                }

                return string.Empty;
            }

            var bytes = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                Marshal.Copy(utf8Str, bytes, 0, size);
                return Encoding.UTF8.GetString(bytes, 0, size);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
                if (freePtr)
                {
                    GLib.GFree(utf8Str);
                }
            }
        }

        /// <summary>
        /// Convert bytes to human readable format.
        /// </summary>
        /// <param name="value">The number of bytes.</param>
        /// <returns>The readable format of the bytes.</returns>
        internal static string ToReadableBytes(this ulong value)
        {
            string[] sizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            var i = 0;
            decimal dValue = value;
            while (Math.Round(dValue, 2) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return $"{dValue:n2} {sizeSuffixes[i]}";
        }

        /// <summary>
        /// Negate all elements in an array.
        /// </summary>
        /// <param name="array">An array of doubles.</param>
        /// <returns>The negated array.</returns>
        internal static double[] Negate(this double[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] *= -1;
            }

            return array;
        }

        /// <summary>
        /// Negate all elements in an array.
        /// </summary>
        /// <remarks>
        /// It will output an array of doubles instead of integers.
        /// </remarks>
        /// <param name="array">An array of integers.</param>
        /// <returns>The negated array.</returns>
        internal static double[] Negate(this int[] array)
        {
            var doubles = new double[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                ref var value = ref doubles[i];
                value = array[i] * -1;
            }

            return doubles;
        }

        /// <summary>
        /// Invert all elements in an array.
        /// </summary>
        /// <param name="array">An array of doubles.</param>
        /// <returns>The inverted array.</returns>
        internal static double[] Invert(this double[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = 1.0 / array[i];
            }

            return array;
        }

        /// <summary>
        /// Invert all elements in an array.
        /// </summary>
        /// <remarks>
        /// It will output an array of doubles instead of integers.
        /// </remarks>
        /// <param name="array">An array of integers.</param>
        /// <returns>The inverted array.</returns>
        internal static double[] Invert(this int[] array)
        {
            var doubles = new double[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                ref var value = ref doubles[i];
                value = 1.0 / array[i];
            }

            return doubles;
        }

        /// <summary>
        /// Compatibility method to call loaders with the <see cref="Enums.FailOn"/> enum.
        /// </summary>
        /// <param name="options">The optional arguments for the loader.</param>
        /// <param name="failOn">The optional <see cref="Enums.FailOn"/> parameter.</param>
        internal static void AddFailOn(this VOption options, Enums.FailOn? failOn = null)
        {
            if (!failOn.HasValue)
            {
                return;
            }

            if (NetVips.AtLeastLibvips(8, 12))
            {
                options.Add("fail_on", failOn);
            }
            else
            {
                // The deprecated "fail" param was at the highest sensitivity (>= warning),
                // but for compat it would be more correct to set this to true only when
                // a non-permissive enum is given (> none).
                options.Add("fail", failOn > Enums.FailOn.None);
            }
        }

        /// <summary>
        /// Compatibility method to call savers with the <see cref="Enums.ForeignKeep"/> enum.
        /// </summary>
        /// <param name="options">The optional arguments for the saver.</param>
        /// <param name="keep">The optional <see cref="Enums.ForeignKeep"/> parameter.</param>
        /// <param name="isDzsave">Whether this operation is <see cref="Image.Dzsave"/>-like.</param>
        internal static void AddForeignKeep(this VOption options, Enums.ForeignKeep? keep = null, bool isDzsave = false)
        {
            if (!keep.HasValue)
            {
                return;
            }

            if (NetVips.AtLeastLibvips(8, 15))
            {
                options.Add(nameof(keep), keep);
            }
            else if (isDzsave)
            {
                options.Add("no_strip", keep != Enums.ForeignKeep.None);
            }
            else
            {
                options.Add("strip", keep == Enums.ForeignKeep.None);
            }
        }
    }
}