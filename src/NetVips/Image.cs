namespace NetVips
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;
    using Internal;

    /// <summary>
    /// Wrap a <see cref="VipsImage"/> object.
    /// </summary>
    public partial class Image : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A evaluation delegate that can be used on the
        /// <see cref="Enums.Signals.PreEval"/>, <see cref="Enums.Signals.Eval"/> and
        /// <see cref="Enums.Signals.PostEval"/> signals.
        /// </summary>
        /// <remarks>
        /// Use <see cref="O:SetProgress"/> to enable progress reporting on an image.
        /// </remarks>
        public delegate void EvalDelegate(Image image, VipsProgress progressStruct);

        /// <summary>
        /// Internal marshaller delegate for <see cref="EvalDelegate"/>.
        /// </summary>
        [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void EvalMarshalDelegate(IntPtr imagePtr, IntPtr progressPtr, IntPtr userDataPtr);

        /// <inheritdoc cref="VipsObject"/>
        internal Image(IntPtr pointer)
            : base(pointer)
        {
            // logger.Debug($"VipsImage = {pointer}");
        }

        #region helpers

        /// <summary>
        /// Run a complex function on a non-complex image.
        /// </summary>
        /// <remarks>
        /// The image needs to be complex, or have an even number of bands. The input
        /// can be int, the output is always float or double.
        /// </remarks>
        /// <param name="func">A complex function.</param>
        /// <param name="image">A non-complex image.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="T:System.ArgumentException">If image doesn't have an even number of bands.</exception>
        private static Image RunCmplx(Func<Image, Image> func, Image image)
        {
            var originalFormat = image.Format;
            if (image.Format != Enums.BandFormat.Complex && image.Format != Enums.BandFormat.Dpcomplex)
            {
                if (image.Bands % 2 != 0)
                {
                    throw new ArgumentException("not an even number of bands");
                }

                if (image.Format != Enums.BandFormat.Float && image.Format != Enums.BandFormat.Double)
                {
                    using (image)
                    {
                        image = image.Cast(Enums.BandFormat.Float);
                    }
                }

                var newFormat = image.Format == Enums.BandFormat.Double
                    ? Enums.BandFormat.Dpcomplex
                    : Enums.BandFormat.Complex;

                using (image)
                {
                    image = image.Copy(format: newFormat, bands: image.Bands / 2);
                }
            }

            using (image)
            {
                image = func(image);
            }

            if (originalFormat != Enums.BandFormat.Complex && originalFormat != Enums.BandFormat.Dpcomplex)
            {
                var newFormat = image.Format == Enums.BandFormat.Dpcomplex
                    ? Enums.BandFormat.Double
                    : Enums.BandFormat.Float;

                using (image)
                {
                    image = image.Copy(format: newFormat, bands: image.Bands * 2);
                }
            }

            return image;
        }

        /// <summary>
        /// Turn a constant (eg. 1, "12", new[] {1, 2, 3}, new[] {new[] {1}}) into an image using
        /// <paramref name="matchImage"/> as a guide.
        /// </summary>
        /// <param name="matchImage">Image guide.</param>
        /// <param name="value">A constant.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public static Image Imageize(Image matchImage, object value)
        {
            // logger.Debug($"Imageize: value = {value}");
            // careful! this can be None if value is a 2D array
            switch (value)
            {
                case Image image:
                    return image;
                case double[,] doubleArray:
                    return NewFromArray(doubleArray);
                case int[,] intArray:
                    return NewFromArray(intArray);
                case double[] doubles:
                    return matchImage.NewFromImage(doubles);
                case int[] ints:
                    return matchImage.NewFromImage(ints);
                case double doubleValue:
                    return matchImage.NewFromImage(doubleValue);
                case int intValue:
                    return matchImage.NewFromImage(intValue);
                default:
                    throw new ArgumentException(
                        $"unsupported value type {value.GetType()} for Imageize");
            }
        }

        /// <summary>
        /// Find the name of the load operation vips will use to load a file.
        /// </summary>
        /// <remarks>
        /// For example "VipsForeignLoadJpegFile". You can use this to work out what
        /// options to pass to <see cref="NewFromFile"/>.
        /// </remarks>
        /// <param name="filename">The file to test.</param>
        /// <returns>The name of the load operation, or <see langword="null"/>.</returns>
        public static string FindLoad(string filename)
        {
            var bytes = Encoding.UTF8.GetBytes(filename + char.MinValue); // Ensure null-terminated string
            return Marshal.PtrToStringAnsi(VipsForeign.FindLoad(bytes));
        }

        /// <summary>
        /// Find the name of the load operation vips will use to load a buffer.
        /// </summary>
        /// <remarks>
        /// For example "VipsForeignLoadJpegBuffer". You can use this to work out what
        /// options to pass to <see cref="NewFromBuffer(byte[], string, Enums.Access?, Enums.FailOn?, VOption)"/>.
        /// </remarks>
        /// <param name="data">The buffer to test.</param>
        /// <returns>The name of the load operation, or <see langword="null"/>.</returns>
        public static string FindLoadBuffer(byte[] data) =>
            Marshal.PtrToStringAnsi(VipsForeign.FindLoadBuffer(data, (ulong)data.Length));

        /// <summary>
        /// Find the name of the load operation vips will use to load a buffer.
        /// </summary>
        /// <remarks>
        /// For example "VipsForeignLoadJpegBuffer". You can use this to work out what
        /// options to pass to <see cref="NewFromBuffer(string, string, Enums.Access?, Enums.FailOn?, VOption)"/>.
        /// </remarks>
        /// <param name="data">The buffer to test.</param>
        /// <returns>The name of the load operation, or <see langword="null"/>.</returns>
        public static string FindLoadBuffer(string data) => FindLoadBuffer(Encoding.UTF8.GetBytes(data));

        /// <summary>
        /// Find the name of the load operation vips will use to load a buffer.
        /// </summary>
        /// <remarks>
        /// For example "VipsForeignLoadJpegBuffer". You can use this to work out what
        /// options to pass to <see cref="NewFromBuffer(char[], string, Enums.Access?, Enums.FailOn?, VOption)"/>.
        /// </remarks>
        /// <param name="data">The buffer to test.</param>
        /// <returns>The name of the load operation, or <see langword="null"/>.</returns>
        public static string FindLoadBuffer(char[] data) => FindLoadBuffer(Encoding.UTF8.GetBytes(data));

        /// <summary>
        /// Find the name of the load operation vips will use to load a source.
        /// </summary>
        /// <remarks>
        /// For example "VipsForeignLoadJpegSource". You can use this to work out what
        /// options to pass to <see cref="NewFromSource(Source, string, Enums.Access?, Enums.FailOn?, VOption)"/>.
        /// </remarks>
        /// <param name="source">The source to test.</param>
        /// <returns>The name of the load operation, or <see langword="null"/>.</returns>
        public static string FindLoadSource(Source source) =>
            Marshal.PtrToStringAnsi(VipsForeign.FindLoadSource(source));

        /// <summary>
        /// Find the name of the load operation vips will use to load a stream.
        /// </summary>
        /// <remarks>
        /// For example "VipsForeignLoadJpegSource". You can use this to work out what
        /// options to pass to <see cref="NewFromStream(Stream, string, Enums.Access?, Enums.FailOn?, VOption)"/>.
        /// </remarks>
        /// <param name="stream">The stream to test.</param>
        /// <returns>The name of the load operation, or <see langword="null"/>.</returns>
        public static string FindLoadStream(Stream stream)
        {
            using var source = SourceStream.NewFromStream(stream);
            return FindLoadSource(source);
        }

        #endregion

        #region constructors

        /// <summary>
        /// Load an image from a file.
        /// </summary>
        /// <remarks>
        /// This method can load images in any format supported by vips. The
        /// filename can include load options, for example:
        /// <code language="lang-csharp">
        /// using var image = Image.NewFromFile("fred.jpg[shrink=2]");
        /// </code>
        /// You can also supply options as keyword arguments, for example:
        /// <code language="lang-csharp">
        /// using var image = Image.NewFromFile("fred.jpg", new VOption
        /// {
        ///     {"shrink", 2}
        /// });
        /// </code>
        /// The full set of options available depend upon the load operation that
        /// will be executed. Try something like:
        /// <code language="lang-shell">
        /// $ vips jpegload
        /// </code>
        /// at the command-line to see a summary of the available options for the
        /// JPEG loader.
        ///
        /// Loading is fast: only enough of the image is loaded to be able to fill
        /// out the header. Pixels will only be decompressed when they are needed.
        /// </remarks>
        /// <param name="vipsFilename">The disc file to load the image from, with
        /// optional appended arguments.</param>
        /// <param name="memory">If set to <see langword="true"/>, load the image
        /// via memory rather than via a temporary disc file. See <see cref="NewTempFile"/>
        /// for notes on where temporary files are created. Small images are loaded via memory
        /// by default, use `VIPS_DISC_THRESHOLD` to set the definition of small.</param>
        /// <param name="access">Hint the expected access pattern for the image.</param>
        /// <param name="failOn">The type of error that will cause load to fail. By
        /// default, loaders are permissive, that is, <see cref="Enums.FailOn.None"/>.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="vipsFilename"/>.</exception>
        public static Image NewFromFile(
            string vipsFilename,
            bool? memory = null,
            Enums.Access? access = null,
            Enums.FailOn? failOn = null,
            VOption kwargs = null)
        {
            var bytes = Encoding.UTF8.GetBytes(vipsFilename + char.MinValue); // Ensure null-terminated string
            var filename = Vips.GetFilename(bytes);
            var fileOptions = Vips.GetOptions(bytes).ToUtf8String(true);

            var operationName = Marshal.PtrToStringAnsi(VipsForeign.FindLoad(filename));
            if (operationName == null)
            {
                throw new VipsException($"unable to load from file {vipsFilename}");
            }

            var options = new VOption();
            if (kwargs != null)
            {
                options.Merge(kwargs);
            }

            options.AddIfPresent(nameof(memory), memory);
            options.AddIfPresent(nameof(access), access);
            options.AddFailOn(failOn);

            options.Add("string_options", fileOptions);

            return Operation.Call(operationName, options, filename.ToUtf8String(true)) as Image;
        }

        /// <summary>
        /// Load a formatted image from memory.
        /// </summary>
        /// <remarks>
        /// This behaves exactly as <see cref="NewFromFile"/>, but the image is
        /// loaded from the memory object rather than from a file. The memory
        /// object can be a string or buffer.
        /// </remarks>
        /// <param name="data">The memory object to load the image from.</param>
        /// <param name="strOptions">Load options as a string. Use <see cref="string.Empty"/> for no options.</param>
        /// <param name="access">Hint the expected access pattern for the image.</param>
        /// <param name="failOn">The type of error that will cause load to fail. By
        /// default, loaders are permissive, that is, <see cref="Enums.FailOn.None"/>.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="data"/>.</exception>
        public static Image NewFromBuffer(
            byte[] data,
            string strOptions = "",
            Enums.Access? access = null,
            Enums.FailOn? failOn = null,
            VOption kwargs = null)
        {
            var operationName = FindLoadBuffer(data);
            if (operationName == null)
            {
                throw new VipsException("unable to load from buffer");
            }

            var options = new VOption();
            if (kwargs != null)
            {
                options.Merge(kwargs);
            }

            options.AddIfPresent(nameof(access), access);
            options.AddFailOn(failOn);

            options.Add("string_options", strOptions);

            return Operation.Call(operationName, options, data) as Image;
        }

        /// <summary>
        /// Load a formatted image from memory.
        /// </summary>
        /// <remarks>
        /// This behaves exactly as <see cref="NewFromFile"/>, but the image is
        /// loaded from the memory object rather than from a file. The memory
        /// object can be a string or buffer.
        /// </remarks>
        /// <param name="data">The memory object to load the image from.</param>
        /// <param name="strOptions">Load options as a string. Use <see cref="string.Empty"/> for no options.</param>
        /// <param name="access">Hint the expected access pattern for the image.</param>
        /// <param name="failOn">The type of error that will cause load to fail. By
        /// default, loaders are permissive, that is, <see cref="Enums.FailOn.None"/>.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="data"/>.</exception>
        public static Image NewFromBuffer(
            string data,
            string strOptions = "",
            Enums.Access? access = null,
            Enums.FailOn? failOn = null,
            VOption kwargs = null) => NewFromBuffer(Encoding.UTF8.GetBytes(data), strOptions, access, failOn, kwargs);

        /// <summary>
        /// Load a formatted image from memory.
        /// </summary>
        /// <remarks>
        /// This behaves exactly as <see cref="NewFromFile"/>, but the image is
        /// loaded from the memory object rather than from a file. The memory
        /// object can be a string or buffer.
        /// </remarks>
        /// <param name="data">The memory object to load the image from.</param>
        /// <param name="strOptions">Load options as a string. Use <see cref="string.Empty"/> for no options.</param>
        /// <param name="access">Hint the expected access pattern for the image.</param>
        /// <param name="failOn">The type of error that will cause load to fail. By
        /// default, loaders are permissive, that is, <see cref="Enums.FailOn.None"/>.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="data"/>.</exception>
        public static Image NewFromBuffer(
            char[] data,
            string strOptions = "",
            Enums.Access? access = null,
            Enums.FailOn? failOn = null,
            VOption kwargs = null) => NewFromBuffer(Encoding.UTF8.GetBytes(data), strOptions, access, failOn, kwargs);

        /// <summary>
        /// Load a formatted image from a source.
        /// </summary>
        /// <remarks>
        /// This behaves exactly as <see cref="NewFromFile"/>, but the image is
        /// loaded from a source rather than from a file.
        /// At least libvips 8.9 is needed.
        /// </remarks>
        /// <param name="source">The source to load the image from.</param>
        /// <param name="strOptions">Load options as a string. Use <see cref="string.Empty"/> for no options.</param>
        /// <param name="access">Hint the expected access pattern for the image.</param>
        /// <param name="failOn">The type of error that will cause load to fail. By
        /// default, loaders are permissive, that is, <see cref="Enums.FailOn.None"/>.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="source"/>.</exception>
        public static Image NewFromSource(
            Source source,
            string strOptions = "",
            Enums.Access? access = null,
            Enums.FailOn? failOn = null,
            VOption kwargs = null)
        {
            // Load with the new source API if we can. Fall back to the older
            // mechanism in case the loader we need has not been converted yet.
            // We need to hide any errors from this first phase.
            Vips.ErrorFreeze();
            var operationName = FindLoadSource(source);
            Vips.ErrorThaw();

            var options = new VOption();
            if (kwargs != null)
            {
                options.Merge(kwargs);
            }

            options.AddIfPresent(nameof(access), access);
            options.AddFailOn(failOn);

            options.Add("string_options", strOptions);

            if (operationName != null)
            {
                return Operation.Call(operationName, options, source) as Image;
            }

            #region fallback mechanism

            var filename = VipsConnection.FileName(source);
            if (filename != IntPtr.Zero)
            {
                // Try with the old file-based loaders.
                operationName = Marshal.PtrToStringAnsi(VipsForeign.FindLoad(filename));
                if (operationName == null)
                {
                    throw new VipsException("unable to load from source");
                }

                return Operation.Call(operationName, options, filename.ToUtf8String()) as Image;
            }

            // Try with the old buffer-based loaders.
            // TODO:
            // Do we need to check if the source can be efficiently mapped into
            // memory with `vips_source_is_mappable`?
            // This implicitly means that it will not work with network streams
            // (`is_pipe` streams).

            var ptr = VipsSource.MapBlob(source);
            if (ptr == IntPtr.Zero)
            {
                throw new VipsException("unable to load from source");
            }

            using var blob = new VipsBlob(ptr);
            var buf = blob.GetData(out var length);

            operationName = Marshal.PtrToStringAnsi(VipsForeign.FindLoadBuffer(buf, (ulong)length));
            if (operationName == null)
            {
                throw new VipsException("unable to load from source");
            }

            return Operation.Call(operationName, options, blob) as Image;

            #endregion
        }

        /// <summary>
        /// Load a formatted image from a stream.
        /// </summary>
        /// <remarks>
        /// This behaves exactly as <see cref="NewFromSource"/>, but the image is
        /// loaded from a stream rather than from a source.
        /// At least libvips 8.9 is needed.
        /// </remarks>
        /// <param name="stream">The stream to load the image from.</param>
        /// <param name="strOptions">Load options as a string. Use <see cref="string.Empty"/> for no options.</param>
        /// <param name="access">Hint the expected access pattern for the image.</param>
        /// <param name="failOn">The type of error that will cause load to fail. By
        /// default, loaders are permissive, that is, <see cref="Enums.FailOn.None"/>.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="stream"/>.</exception>
        public static Image NewFromStream(
            Stream stream,
            string strOptions = "",
            Enums.Access? access = null,
            Enums.FailOn? failOn = null,
            VOption kwargs = null)
        {
            var source = SourceStream.NewFromStream(stream);
            var image = NewFromSource(source, strOptions, access, failOn, kwargs);

            // Need to dispose the SourceStream when the image is closed.
            image.OnPostClose += () => source.Dispose();

            return image;
        }


        /// <summary>
        /// Create an image from a 2D array.
        /// </summary>
        /// <remarks>
        /// A new one-band image with <see cref="Enums.BandFormat.Double"/> pixels is
        /// created from the array. These images are useful with the libvips
        /// convolution operator <see cref="Conv"/>.
        /// </remarks>
        /// <param name="array">Create the image from these values.</param>
        /// <param name="scale">Default to 1.0. What to divide each pixel by after
        /// convolution. Useful for integer convolution masks.</param>
        /// <param name="offset">Default to 0.0. What to subtract from each pixel
        /// after convolution. Useful for integer convolution masks.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to make image from <paramref name="array"/>.</exception>
        public static Image NewFromArray<T>(T[,] array, double scale = 1.0, double offset = 0.0)
            where T : struct, IEquatable<T>
        {
            var height = array.GetLength(0);
            var width = array.GetLength(1);
            var n = width * height;

            var a = new double[n];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    ref var value = ref a[x + (y * width)];
                    value = Convert.ToDouble(array[y, x]);
                }
            }

            var vi = VipsImage.NewMatrixFromArray(width, height, a, n);

            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to make image from matrix");
            }

            using var image = new Image(vi);
            return image.Mutate(mutable =>
            {
                // be careful to set them as double
                mutable.Set(GValue.GDoubleType, nameof(scale), scale);
                mutable.Set(GValue.GDoubleType, nameof(offset), offset);
            });
        }

        /// <summary>
        /// Create an image from a 2D array.
        /// </summary>
        /// <remarks>
        /// A new one-band image with <see cref="Enums.BandFormat.Double"/> pixels is
        /// created from the array. These images are useful with the libvips
        /// convolution operator <see cref="Conv"/>.
        /// </remarks>
        /// <param name="array">Create the image from these values.</param>
        /// <param name="scale">Default to 1.0. What to divide each pixel by after
        /// convolution. Useful for integer convolution masks.</param>
        /// <param name="offset">Default to 0.0. What to subtract from each pixel
        /// after convolution. Useful for integer convolution masks.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to make image from <paramref name="array"/>.</exception>
        public static Image NewFromArray<T>(T[][] array, double scale = 1.0, double offset = 0.0)
            where T : struct, IEquatable<T>
        {
            var height = array.Length;
            var width = array[0].Length;
            var n = width * height;

            var a = new double[n];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    ref var value = ref a[x + (y * width)];
                    value = Convert.ToDouble(array[y][x]);
                }
            }

            var vi = VipsImage.NewMatrixFromArray(width, height, a, n);

            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to make image from matrix");
            }

            using var image = new Image(vi);
            return image.Mutate(mutable =>
            {
                // be careful to set them as double
                mutable.Set(GValue.GDoubleType, nameof(scale), scale);
                mutable.Set(GValue.GDoubleType, nameof(offset), offset);
            });
        }

        /// <summary>
        /// Create an image from a 1D array.
        /// </summary>
        /// <remarks>
        /// A new one-band image with <see cref="Enums.BandFormat.Double"/> pixels is
        /// created from the array. These images are useful with the libvips
        /// convolution operator <see cref="Conv"/>.
        /// </remarks>
        /// <param name="array">Create the image from these values.
        /// 1D arrays become a single row of pixels.</param>
        /// <param name="scale">Default to 1.0. What to divide each pixel by after
        /// convolution. Useful for integer convolution masks.</param>
        /// <param name="offset">Default to 0.0. What to subtract from each pixel
        /// after convolution. Useful for integer convolution masks.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to make image from <paramref name="array"/>.</exception>
        public static Image NewFromArray<T>(T[] array, double scale = 1.0, double offset = 0.0)
            where T : struct, IEquatable<T>
        {
            var height = array.Length;
            var a = new double[height];
            for (var y = 0; y < height; y++)
            {
                ref var value = ref a[y];
                value = Convert.ToDouble(array[y]);
            }

            var vi = VipsImage.NewMatrixFromArray(1, height, a, height);

            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to make image from matrix");
            }

            using var image = new Image(vi);
            return image.Mutate(mutable =>
            {
                // be careful to set them as double
                mutable.Set(GValue.GDoubleType, nameof(scale), scale);
                mutable.Set(GValue.GDoubleType, nameof(offset), offset);
            });
        }

        /// <summary>
        /// Wrap an image around a memory array.
        /// </summary>
        /// <remarks>
        /// Wraps an image around a C-style memory array. For example, if the
        /// <paramref name="data"/> memory array contains four bytes with the
        /// values 1, 2, 3, 4, you can make a one-band, 2x2 uchar image from
        /// it like this:
        /// <code language="lang-csharp">
        /// using var image = Image.NewFromMemory(data, 2, 2, 1, Enums.BandFormat.Uchar);
        /// </code>
        /// A reference is kept to the data object, so it will not be
        /// garbage-collected until the returned image is garbage-collected.
        ///
        /// This method is useful for efficiently transferring images from GDI+
        /// into libvips.
        ///
        /// See <see cref="WriteToMemory()"/> for the opposite operation.
        ///
        /// Use <see cref="Copy"/> to set other image attributes.
        /// </remarks>
        /// <param name="data">A memory object.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="bands">Number of bands.</param>
        /// <param name="format">Band format.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to make image from <paramref name="data"/>.</exception>
        public static Image NewFromMemory(
            Array data,
            int width,
            int height,
            int bands,
            Enums.BandFormat format)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var vi = VipsImage.NewFromMemory(handle.AddrOfPinnedObject(), (UIntPtr)data.Length, width, height, bands,
                format);

            if (vi == IntPtr.Zero)
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }

                throw new VipsException("unable to make image from memory");
            }

            var image = new Image(vi);

            // Need to release the pinned GCHandle when the image is closed.
            image.OnPostClose += () =>
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            };

            return image;
        }

        /// <summary>
        /// Wrap an image around a memory area.
        /// </summary>
        /// <remarks>
        /// Because libvips is "borrowing" <paramref name="data"/> from the caller, this function
        /// is extremely dangerous. Unless you are very careful, you will get crashes or memory
        /// corruption. Use <see cref="NewFromMemoryCopy"/> instead if you are at all unsure.
        /// </remarks>
        /// <param name="data">A unmanaged block of memory.</param>
        /// <param name="size">Length of memory.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="bands">Number of bands.</param>
        /// <param name="format">Band format.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to make image from <paramref name="data"/>.</exception>
        public static Image NewFromMemory(
            IntPtr data,
            ulong size,
            int width,
            int height,
            int bands,
            Enums.BandFormat format)
        {
            var vi = VipsImage.NewFromMemory(data, (UIntPtr)size, width, height, bands, format);

            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to make image from memory");
            }

            return new Image(vi) { MemoryPressure = (long)size };
        }

        /// <summary>
        /// Like <see cref="NewFromMemory(IntPtr, ulong, int, int, int, Enums.BandFormat)"/>, but libvips
        /// will make a copy of the memory area. This means more memory use and an extra copy
        /// operation, but is much simpler and safer.
        /// </summary>
        /// <param name="data">A unmanaged block of memory.</param>
        /// <param name="size">Length of memory.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="bands">Number of bands.</param>
        /// <param name="format">Band format.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to make image from <paramref name="data"/>.</exception>
        public static Image NewFromMemoryCopy(
            IntPtr data,
            ulong size,
            int width,
            int height,
            int bands,
            Enums.BandFormat format)
        {
            var vi = VipsImage.NewFromMemoryCopy(data, (UIntPtr)size, width, height, bands, format);

            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to make image from memory");
            }

            return new Image(vi) { MemoryPressure = (long)size };
        }

        /// <summary>
        /// Make a new temporary image.
        /// </summary>
        /// <remarks>
        /// Returns an image backed by a temporary file. When written to with
        /// <see cref="Write"/>, a temporary file will be created on disc in the
        /// specified format. When the image is closed, the file will be deleted
        /// automatically.
        ///
        /// The file is created in the temporary directory. This is set with
        /// the environment variable `TMPDIR`. If this is not set, then on
        /// Unix systems, vips will default to `/tmp`. On Windows, vips uses
        /// `GetTempPath()` to find the temporary directory.
        ///
        /// vips uses `g_mkstemp()` to make the temporary filename. They
        /// generally look something like `vips-12-EJKJFGH.v`.
        /// </remarks>
        /// <param name="format">The format for the temp file, for example
        /// `%s.v` for a vips format file. The `%s` is
        /// substituted by the file path.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to make temp file from <paramref name="format"/>.</exception>
        public static Image NewTempFile(string format)
        {
            var vi = VipsImage.NewTempFile(Encoding.UTF8.GetBytes(format));
            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to make temp file");
            }

            return new Image(vi);
        }

        /// <summary>
        /// Make a new image from an existing one.
        /// </summary>
        /// <remarks>
        /// A new image is created which has the same size, format, interpretation
        /// and resolution as `this`, but with every pixel set to `value`.
        /// </remarks>
        /// <param name="value">The value for the pixels. Use a
        /// single number to make a one-band image; use an array constant
        /// to make a many-band image.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image NewFromImage(Image value)
        {
            using var black = Black(1, 1);
            using var pixel = black + value;
            using var cast = pixel.Cast(Format);
            using var image = cast.Embed(0, 0, Width, Height, extend: Enums.Extend.Copy);
            return image.Copy(interpretation: Interpretation, xres: Xres, yres: Yres, xoffset: Xoffset,
                yoffset: Yoffset);
        }

        /// <summary>
        /// Make a new image from an existing one.
        /// </summary>
        /// <remarks>
        /// A new image is created which has the same size, format, interpretation
        /// and resolution as `this`, but with every pixel set to `value`.
        /// </remarks>
        /// <param name="doubles">The value for the pixels. Use a
        /// single number to make a one-band image; use an array constant
        /// to make a many-band image.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image NewFromImage(params double[] doubles)
        {
            using var black = Black(1, 1);
            using var pixel = black + doubles;
            using var cast = pixel.Cast(Format);
            using var image = cast.Embed(0, 0, Width, Height, extend: Enums.Extend.Copy);
            return image.Copy(interpretation: Interpretation, xres: Xres, yres: Yres, xoffset: Xoffset,
                yoffset: Yoffset);
        }

        /// <summary>
        /// Make a new image from an existing one.
        /// </summary>
        /// <remarks>
        /// A new image is created which has the same size, format, interpretation
        /// and resolution as `this`, but with every pixel set to `value`.
        /// </remarks>
        /// <param name="ints">The value for the pixels. Use a
        /// single number to make a one-band image; use an array constant
        /// to make a many-band image.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image NewFromImage(params int[] ints) =>
            NewFromImage(Array.ConvertAll(ints, Convert.ToDouble));

        /// <summary>
        /// Copy an image to memory.
        /// </summary>
        /// <remarks>
        /// A large area of memory is allocated, the image is rendered to that
        /// memory area, and a new image is returned which wraps that large memory
        /// area.
        /// </remarks>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to copy to memory.</exception>
        public Image CopyMemory()
        {
            var vi = VipsImage.CopyMemory(this);
            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to copy to memory");
            }

            return new Image(vi) { MemoryPressure = MemoryPressure };
        }

        #endregion

        #region writers

        /// <summary>
        /// Write an image to a file on disc.
        /// </summary>
        /// <remarks>
        /// This method can save images in any format supported by vips. The format
        /// is selected from the filename suffix. The filename can include embedded
        /// save options, see <see cref="NewFromFile"/>.
        ///
        /// For example:
        /// <code language="lang-csharp">
        /// image.WriteToFile("fred.jpg[Q=95]");
        /// </code>
        /// You can also supply options as keyword arguments, for example:
        /// <code language="lang-csharp">
        /// image.WriteToFile("fred.jpg", new VOption
        /// {
        ///     {"Q", 95}
        /// });
        /// </code>
        /// The full set of options available depend upon the save operation that
        /// will be executed. Try something like:
        /// <code language="lang-shell">
        /// $ vips jpegsave
        /// </code>
        /// at the command-line to see a summary of the available options for the
        /// JPEG saver.
        /// </remarks>
        /// <param name="vipsFilename">The disc file to save the image to, with
        /// optional appended arguments.</param>
        /// <param name="kwargs">Optional options that depend on the save operation.</param>
        /// <exception cref="VipsException">If unable to write to <paramref name="vipsFilename"/>.</exception>
        public void WriteToFile(string vipsFilename, VOption kwargs = null)
        {
            var bytes = Encoding.UTF8.GetBytes(vipsFilename + char.MinValue); // Ensure null-terminated string
            var filename = Vips.GetFilename(bytes);
            var fileOptions = Vips.GetOptions(bytes).ToUtf8String(true);
            var operationName = Marshal.PtrToStringAnsi(VipsForeign.FindSave(filename));

            if (operationName == null)
            {
                throw new VipsException($"unable to write to file {vipsFilename}");
            }

            var stringOptions = new VOption
            {
                {"string_options", fileOptions}
            };

            if (kwargs != null)
            {
                kwargs.Merge(stringOptions);
            }
            else
            {
                kwargs = stringOptions;
            }

            this.Call(operationName, kwargs, filename.ToUtf8String(true));
        }

        /// <summary>
        /// Write an image to a formatted string.
        /// </summary>
        /// <remarks>
        /// This method can save images in any format supported by vips. The format
        /// is selected from the suffix in the format string. This can include
        /// embedded save options, see <see cref="NewFromFile"/>.
        ///
        /// For example:
        /// <code language="lang-csharp">
        /// var data = image.WriteToBuffer(".jpg[Q=95]");
        /// </code>
        /// You can also supply options as keyword arguments, for example:
        /// <code language="lang-csharp">
        /// var data = image.WriteToBuffer(".jpg", new VOption
        /// {
        ///     {"Q", 95}
        /// });
        /// </code>
        /// The full set of options available depend upon the load operation that
        /// will be executed. Try something like:
        /// <code language="lang-shell">
        /// $ vips jpegsave_buffer
        /// </code>
        /// at the command-line to see a summary of the available options for the
        /// JPEG saver.
        /// </remarks>
        /// <param name="formatString">The suffix, plus any string-form arguments.</param>
        /// <param name="kwargs">Optional options that depend on the save operation.</param>
        /// <returns>An array of bytes.</returns>
        /// <exception cref="VipsException">If unable to write to buffer.</exception>
        public byte[] WriteToBuffer(string formatString, VOption kwargs = null)
        {
            var bytes = Encoding.UTF8.GetBytes(formatString + char.MinValue); // Ensure null-terminated string
            var bufferOptions = Vips.GetOptions(bytes).ToUtf8String(true);
            string operationName = null;

            // Save with the new target API if we can. Fall back to the older
            // mechanism in case the saver we need has not been converted yet.
            // We need to hide any errors from this first phase.
            if (NetVips.AtLeastLibvips(8, 9))
            {
                Vips.ErrorFreeze();
                operationName = Marshal.PtrToStringAnsi(VipsForeign.FindSaveTarget(bytes));
                Vips.ErrorThaw();
            }

            var stringOptions = new VOption
            {
                {"string_options", bufferOptions}
            };

            if (kwargs != null)
            {
                kwargs.Merge(stringOptions);
            }
            else
            {
                kwargs = stringOptions;
            }

            if (operationName != null)
            {
                using var target = Target.NewToMemory();
                this.Call(operationName, kwargs, target);
                return target.Blob;
            }

            #region fallback mechanism

            operationName = Marshal.PtrToStringAnsi(VipsForeign.FindSaveBuffer(bytes));
            if (operationName == null)
            {
                throw new VipsException($"unable to write to buffer");
            }

            return this.Call(operationName, kwargs) as byte[];

            #endregion
        }

        /// <summary>
        /// Write an image to a target.
        /// </summary>
        /// <remarks>
        /// This behaves exactly as <see cref="WriteToFile"/>, but the image is
        /// written to a target rather than a file.
        /// At least libvips 8.9 is needed.
        /// </remarks>
        /// <param name="target">Write to this target.</param>
        /// <param name="formatString">The suffix, plus any string-form arguments.</param>
        /// <param name="kwargs">Optional options that depend on the save operation.</param>
        /// <exception cref="VipsException">If unable to write to target.</exception>
        public void WriteToTarget(Target target, string formatString, VOption kwargs = null)
        {
            var bytes = Encoding.UTF8.GetBytes(formatString + char.MinValue); // Ensure null-terminated string
            var bufferOptions = Vips.GetOptions(bytes).ToUtf8String(true);
            var operationName = Marshal.PtrToStringAnsi(VipsForeign.FindSaveTarget(bytes));

            if (operationName == null)
            {
                throw new VipsException("unable to write to target");
            }

            var stringOptions = new VOption
            {
                {"string_options", bufferOptions}
            };

            if (kwargs != null)
            {
                kwargs.Merge(stringOptions);
            }
            else
            {
                kwargs = stringOptions;
            }

            this.Call(operationName, kwargs, target);
        }

        /// <summary>
        /// Write an image to a stream.
        /// </summary>
        /// <remarks>
        /// This behaves exactly as <see cref="WriteToTarget"/>, but the image is
        /// written to a stream rather than a target.
        /// At least libvips 8.9 is needed.
        /// </remarks>
        /// <param name="stream">Write to this stream.</param>
        /// <param name="formatString">The suffix, plus any string-form arguments.</param>
        /// <param name="kwargs">Optional options that depend on the save operation.</param>
        /// <exception cref="VipsException">If unable to write to stream.</exception>
        public void WriteToStream(Stream stream, string formatString, VOption kwargs = null)
        {
            using var target = TargetStream.NewFromStream(stream);
            WriteToTarget(target, formatString, kwargs);
        }


        /// <summary>
        /// Write the image to memory as a simple, unformatted C-style array.
        /// </summary>
        /// <remarks>
        /// The caller is responsible for freeing this memory with <see cref="NetVips.Free"/>.
        /// </remarks>
        /// <param name="size">Output buffer length.</param>
        /// <returns>A <see cref="IntPtr"/> pointing to an unformatted C-style array.</returns>
        /// <exception cref="VipsException">If unable to write to memory.</exception>
        public IntPtr WriteToMemory(out ulong size)
        {
            var pointer = VipsImage.WriteToMemory(this, out size);
            if (pointer == IntPtr.Zero)
            {
                throw new VipsException("unable to write to memory");
            }

            return pointer;
        }

        /// <summary>
        /// Write the image to a large memory array.
        /// </summary>
        /// <remarks>
        /// A large area of memory is allocated, the image is rendered to that
        /// memory array, and the array is returned as a buffer.
        ///
        /// For example, if you have a 2x2 uchar image containing the bytes 1, 2,
        /// 3, 4, read left-to-right, top-to-bottom, then:
        /// <code language="lang-csharp">
        /// var buf = image.WriteToMemory();
        /// </code>
        /// will return a four byte buffer containing the values 1, 2, 3, 4.
        /// </remarks>
        /// <returns>An array of bytes.</returns>
        /// <exception cref="VipsException">If unable to write to memory.</exception>
        public byte[] WriteToMemory()
        {
            var pointer = WriteToMemory(out var size);

            var managedArray = new byte[size];
            Marshal.Copy(pointer, managedArray, 0, (int)size);

            GLib.GFree(pointer);

            return managedArray;
        }

        /// <summary>
        /// Write an image to another image.
        /// </summary>
        /// <remarks>
        /// This function writes `this` to another image. Use something like
        /// <see cref="NewTempFile"/> to make an image that can be written to.
        /// </remarks>
        /// <param name="other">The <see cref="Image"/> to write to.</param>
        /// <exception cref="VipsException">If unable to write to image.</exception>
        public void Write(Image other)
        {
            var result = VipsImage.Write(this, other);
            if (result != 0)
            {
                throw new VipsException("unable to write to image");
            }
        }

        #endregion

        #region get metadata

        /// <summary>
        /// Get the GType of an item of metadata.
        /// </summary>
        /// <remarks>
        /// Fetch the GType of a piece of metadata, or <see cref="IntPtr.Zero"/> if the named
        /// item does not exist. See <see cref="GValue"/>.
        /// </remarks>
        /// <param name="name">The name of the piece of metadata to get the type of.</param>
        /// <returns>A new instance of <see cref="IntPtr"/> initialized to the GType or
        /// <see cref="IntPtr.Zero"/> if the property does not exist.</returns>
        public new IntPtr GetTypeOf(string name)
        {
            // on libvips before 8.5, property types must be fetched separately,
            // since built-in enums were reported as ints
            if (!NetVips.AtLeastLibvips(8, 5))
            {
                var gtype = base.GetTypeOf(name);
                if (gtype != IntPtr.Zero)
                {
                    return gtype;
                }
            }

            return VipsImage.GetTypeof(this, name);
        }

        /// <summary>
        /// Check if the underlying image contains an property of metadata.
        /// </summary>
        /// <param name="name">The name of the piece of metadata to check for.</param>
        /// <returns><see langword="true"/> if the metadata exits; otherwise, <see langword="false"/>.</returns>
        public bool Contains(string name)
        {
            return GetTypeOf(name) != IntPtr.Zero;
        }

        /// <summary>
        /// Get an item of metadata.
        /// </summary>
        /// <remarks>
        /// Fetches an item of metadata as a C# value. For example:
        /// <code language="lang-csharp">
        /// var orientation = image.Get("orientation");
        /// </code>
        /// would fetch the image orientation.
        /// </remarks>
        /// <param name="name">The name of the piece of metadata to get.</param>
        /// <returns>The metadata item as a C# value.</returns>
        /// <exception cref="VipsException">If unable to get <paramref name="name"/>.</exception>
        public new object Get(string name)
        {
            switch (name)
            {
                // scale and offset have default values
                case "scale" when !Contains("scale"):
                    return 1.0;
                case "offset" when !Contains("offset"):
                    return 0.0;
            }

            // with old libvips, we must fetch properties (as opposed to
            // metadata) via VipsObject
            if (!NetVips.AtLeastLibvips(8, 5))
            {
                var gtype = base.GetTypeOf(name);
                if (gtype != IntPtr.Zero)
                {
                    return base.Get(name);
                }
            }

            var result = VipsImage.Get(this, name, out var gvCopy);
            if (result != 0)
            {
                throw new VipsException($"unable to get {name}");
            }

            using var gv = new GValue(gvCopy);
            return gv.Get();
        }

        /// <summary>
        /// Get a list of all the metadata fields on an image.
        /// </summary>
        /// <remarks>
        /// At least libvips 8.5 is needed.
        /// </remarks>
        /// <returns>An array of strings or <see langword="null"/>.</returns>
        public string[] GetFields()
        {
            if (!NetVips.AtLeastLibvips(8, 5))
            {
                return null;
            }

            var ptrArr = VipsImage.GetFields(this);

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
        /// Returns a string that represents the current image.
        /// </summary>
        /// <returns>A string that represents the current image.</returns>
        public override string ToString()
        {
            return $"<NetVips.Image {Width}x{Height} {Format}, {Bands} bands, {Interpretation}>";
        }

        #endregion

        #region handwritten functions

        /// <summary>
        /// Mutate an image with a delegate. Inside the delegate, you can call methods
        /// which modify the image, such as setting or removing metadata, or
        /// modifying pixels.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using var mutated = image.Mutate(mutable =>
        /// {
        ///     for (var i = 0; i &lt;= 100; i++)
        ///     {
        ///         var j = i / 100.0;
        ///         mutable.DrawLine(new[] { 255.0 }, (int)(mutable.Width * j), 0, 0, (int)(mutable.Height * (1 - j)));
        ///     }
        /// });
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/>.</returns>
        public virtual Image Mutate(Action<MutableImage> action)
        {
            // We take a copy of the regular Image to ensure we have an unshared (unique) object.
            using var mutable = new MutableImage(Copy());
            action.Invoke(mutable);
            return mutable.Image;
        }

        /// <summary>
        /// Scale an image to 0 - 255.
        /// </summary>
        /// <remarks>
        /// This is the libvips `scale` operation, renamed to avoid a clash with
        /// the `scale` for convolution masks.
        /// </remarks>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = in.Scale(exp: double, log: bool);
        /// </code>
        /// </example>
        /// <param name="exp">Exponent for log scale.</param>
        /// <param name="log">Log scale.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image ScaleImage(double? exp = null, bool? log = null)
        {
            var options = new VOption();

            options.AddIfPresent(nameof(exp), exp);
            options.AddIfPresent(nameof(log), log);

            return this.Call("scale", options) as Image;
        }

        /// <summary>
        /// Ifthenelse an image.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = cond.Ifthenelse(in1, in2, blend: bool);
        /// </code>
        /// </example>
        /// <param name="in1">Source for TRUE pixels.</param>
        /// <param name="in2">Source for FALSE pixels.</param>
        /// <param name="blend">Blend smoothly between then and else parts.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Ifthenelse(object in1, object in2, bool? blend = null)
        {
            Image matchImage;
            if (in1 is Image th)
            {
                matchImage = th;
            }
            else if (in2 is Image el)
            {
                matchImage = el;
            }
            else
            {
                matchImage = this;
            }

            using var im1 = in1 is Image ? null : Imageize(matchImage, in1);
            using var im2 = in2 is Image ? null : Imageize(matchImage, in2);

            var options = new VOption();

            options.AddIfPresent(nameof(blend), blend);

            return this.Call("ifthenelse", options, im1 ?? in1, im2 ?? in2) as Image;
        }

        /// <summary>
        /// Use pixel values to pick cases from an array of constants.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = index.Case(10.5, 20.5);
        /// </code>
        /// </example>
        /// <param name="doubles">Array of constants.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Case(params double[] doubles) =>
            this.Call("case", doubles) as Image;

        /// <summary>
        /// Use pixel values to pick cases from an array of constants.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = index.Case(10, 20);
        /// </code>
        /// </example>
        /// <param name="ints">Array of constants.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Case(params int[] ints) =>
            Case(Array.ConvertAll(ints, Convert.ToDouble));

        /// <summary>
        /// Use pixel values to pick cases from an array of images.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = index.Case(images);
        /// </code>
        /// </example>
        /// <param name="images">Array of case images.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Case(params Image[] images) =>
            this.Call("case", new object[] { images }) as Image;

        /// <summary>
        /// Use pixel values to pick cases from an a set of mixed images and constants.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = index.Case(image, 10);
        /// </code>
        /// </example>
        /// <param name="objects">Array of mixed images and constants.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Case(params object[] objects) =>
            this.Call("case", new object[] { objects }) as Image;

        /// <summary>
        /// Append a set of constants bandwise.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Bandjoin(127.5, 255.0);
        /// </code>
        /// </example>
        /// <param name="doubles">Array of constants.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandjoin(params double[] doubles) =>
            BandjoinConst(doubles);

        /// <summary>
        /// Append a set of constants bandwise.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Bandjoin(255, 128);
        /// </code>
        /// </example>
        /// <param name="ints">Array of constants.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandjoin(params int[] ints) =>
            BandjoinConst(Array.ConvertAll(ints, Convert.ToDouble));

        /// <summary>
        /// Append a set of images bandwise.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Bandjoin(image2, image3);
        /// </code>
        /// </example>
        /// <param name="images">Array of images.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandjoin(params Image[] images) =>
            this.Call("bandjoin", new object[] { images.PrependImage(this) }) as Image;

        /// <summary>
        /// Append a set of mixed images and constants bandwise.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Bandjoin(image2, 255);
        /// </code>
        /// </example>
        /// <param name="objects">Array of mixed images and constants.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandjoin(params object[] objects) =>
            this.Call("bandjoin", new object[] { objects.PrependImage(this) }) as Image;

        /// <summary>
        /// Band-wise rank a set of constants.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Bandrank(other, index: int);
        /// </code>
        /// </example>
        /// <param name="doubles">Array of constants.</param>
        /// <param name="index">Select this band element from sorted list.</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandrank(double[] doubles, int? index = null)
        {
            var options = new VOption();

            options.AddIfPresent(nameof(index), index);

            return this.Call("bandrank", options, doubles) as Image;
        }

        /// <summary>
        /// Band-wise rank a set of constants.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Bandrank(other, index: int);
        /// </code>
        /// </example>
        /// <param name="ints">Array of constants.</param>
        /// <param name="index">Select this band element from sorted list.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandrank(int[] ints, int? index = null) =>
            Bandrank(Array.ConvertAll(ints, Convert.ToDouble), index);

        /// <summary>
        /// Band-wise rank a set of images.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Bandrank(other, index: int);
        /// </code>
        /// </example>
        /// <param name="images">Array of input images.</param>
        /// <param name="index">Select this band element from sorted list.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandrank(Image[] images, int? index = null)
        {
            var options = new VOption();

            options.AddIfPresent(nameof(index), index);

            return this.Call("bandrank", options, new object[] { images.PrependImage(this) }) as Image;
        }

        /// <summary>
        /// Band-wise rank a image.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Bandrank(other, index: int);
        /// </code>
        /// </example>
        /// <param name="other">Input image.</param>
        /// <param name="index">Select this band element from sorted list.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandrank(Image other, int? index = null) =>
            Bandrank(new[] { other }, index);

        /// <summary>
        /// Band-wise rank a set of mixed images and constants.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Bandrank(new object[] { image2, 255 }, index: int);
        /// </code>
        /// </example>
        /// <param name="objects">Array of mixed images and constants.</param>
        /// <param name="index">Select this band element from sorted list.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandrank(object[] objects, int? index = null)
        {
            var options = new VOption();

            options.AddIfPresent(nameof(index), index);

            return this.Call("bandrank", options, new object[] { objects.PrependImage(this) }) as Image;
        }

        /// <summary>
        /// Blend an array of images with an array of blend modes.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = image.Composite(images, modes, x: int[], y: int[], compositingSpace: Enums.Interpretation, premultiplied: bool);
        /// </code>
        /// </example>
        /// <param name="images">Array of input images.</param>
        /// <param name="modes">Array of VipsBlendMode to join with.</param>
        /// <param name="x">Array of x coordinates to join at.</param>
        /// <param name="y">Array of y coordinates to join at.</param>
        /// <param name="compositingSpace">Composite images in this colour space.</param>
        /// <param name="premultiplied">Images have premultiplied alpha.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Composite(Image[] images, Enums.BlendMode[] modes, int[] x = null, int[] y = null,
            Enums.Interpretation? compositingSpace = null, bool? premultiplied = null)
        {
            var options = new VOption();

            options.AddIfPresent(nameof(x), x);
            options.AddIfPresent(nameof(y), y);
            options.AddIfPresent("compositing_space", compositingSpace);
            options.AddIfPresent(nameof(premultiplied), premultiplied);

            return this.Call("composite", options, images.PrependImage(this), modes) as Image;
        }

        /// <summary>
        /// A synonym for <see cref="Composite2"/>.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = base.Composite(overlay, mode, x: int, y: int, compositingSpace: Enums.Interpretation, premultiplied: bool);
        /// </code>
        /// </example>
        /// <param name="overlay">Overlay image.</param>
        /// <param name="mode">VipsBlendMode to join with.</param>
        /// <param name="x">x position of overlay.</param>
        /// <param name="y">y position of overlay.</param>
        /// <param name="compositingSpace">Composite images in this colour space.</param>
        /// <param name="premultiplied">Images have premultiplied alpha.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Composite(Image overlay, Enums.BlendMode mode, int? x = null, int? y = null,
            Enums.Interpretation? compositingSpace = null, bool? premultiplied = null) =>
            Composite2(overlay, mode, x, y, compositingSpace, premultiplied);

        /// <summary>
        /// A synonym for <see cref="ExtractArea"/>.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using Image @out = input.Crop(left, top, width, height);
        /// </code>
        /// </example>
        /// <param name="left">Left edge of extract area.</param>
        /// <param name="top">Top edge of extract area.</param>
        /// <param name="width">Width of extract area.</param>
        /// <param name="height">Height of extract area.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Crop(int left, int top, int width, int height) =>
            ExtractArea(left, top, width, height);

        /// <summary>
        /// Return the coordinates of the image maximum.
        /// </summary>
        /// <returns>An array of doubles.</returns>
        public double[] MaxPos()
        {
            var v = Max(out var x, out var y);
            return new[] { v, x, y };
        }

        /// <summary>
        /// Return the coordinates of the image minimum.
        /// </summary>
        /// <returns>An array of doubles.</returns>
        public double[] MinPos()
        {
            var v = Min(out var x, out var y);
            return new[] { v, x, y };
        }

        /// <summary>
        /// Return the real part of a complex image.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Real() => Complexget(Enums.OperationComplexget.Real);

        /// <summary>
        /// Return the imaginary part of a complex image.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Imag() => Complexget(Enums.OperationComplexget.Imag);

        /// <summary>
        ///  Return an image converted to polar coordinates.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Polar() => RunCmplx(x => x.Complex(Enums.OperationComplex.Polar), this);

        /// <summary>
        /// Return an image converted to rectangular coordinates.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Rect() => RunCmplx(x => x.Complex(Enums.OperationComplex.Rect), this);

        /// <summary>
        /// Return the complex conjugate of an image.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Conj() => Complex(Enums.OperationComplex.Conj);

        /// <summary>
        /// Return the sine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Sin() => Math(Enums.OperationMath.Sin);

        /// <summary>
        /// Return the cosine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Cos() => Math(Enums.OperationMath.Cos);

        /// <summary>
        /// Return the tangent of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Tan() => Math(Enums.OperationMath.Tan);

        /// <summary>
        /// Return the inverse sine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Asin() => Math(Enums.OperationMath.Asin);

        /// <summary>
        /// Return the inverse cosine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Acos() => Math(Enums.OperationMath.Acos);

        /// <summary>
        /// Return the inverse tangent of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Atan() => Math(Enums.OperationMath.Atan);

        /// <summary>
        /// Return the hyperbolic sine of an image in radians.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Sinh() => Math(Enums.OperationMath.Sinh);

        /// <summary>
        /// Return the hyperbolic cosine of an image in radians.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Cosh() => Math(Enums.OperationMath.Cosh);

        /// <summary>
        /// Return the hyperbolic tangent of an image in radians.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Tanh() => Math(Enums.OperationMath.Tanh);

        /// <summary>
        /// Return the inverse hyperbolic sine of an image in radians.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Asinh() => Math(Enums.OperationMath.Asinh);

        /// <summary>
        /// Return the inverse hyperbolic cosine of an image in radians.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Acosh() => Math(Enums.OperationMath.Acosh);

        /// <summary>
        /// Return the inverse hyperbolic tangent of an image in radians.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Atanh() => Math(Enums.OperationMath.Atanh);

        /// <summary>
        /// Return the natural log of an image.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Log() => Math(Enums.OperationMath.Log);

        /// <summary>
        /// Return the log base 10 of an image.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Log10() => Math(Enums.OperationMath.Log10);

        /// <summary>
        /// Return e ** pixel.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Exp() => Math(Enums.OperationMath.Exp);

        /// <summary>
        /// Return 10 ** pixel.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Exp10() => Math(Enums.OperationMath.Exp10);

        /// <summary>
        /// Raise to power of an image.
        /// </summary>
        /// <param name="exp">To the power of this.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Pow(Image exp) => Math2(exp, Enums.OperationMath2.Pow);

        /// <summary>
        /// Raise to power of an constant.
        /// </summary>
        /// <param name="exp">To the power of this.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Pow(double exp) => Math2Const(Enums.OperationMath2.Pow, new[] { exp });

        /// <summary>
        /// Raise to power of an array.
        /// </summary>
        /// <param name="exp">To the power of this.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Pow(double[] exp) => Math2Const(Enums.OperationMath2.Pow, exp);

        /// <summary>
        /// Raise to power of an array.
        /// </summary>
        /// <param name="exp">To the power of this.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Pow(int[] exp) =>
            Math2Const(Enums.OperationMath2.Pow, Array.ConvertAll(exp, Convert.ToDouble));

        /// <summary>
        /// Raise to power of an image, but with the arguments reversed.
        /// </summary>
        /// <param name="base">To the base of this.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Wop(Image @base) => Math2(@base, Enums.OperationMath2.Wop);

        /// <summary>
        /// Raise to power of an constant, but with the arguments reversed.
        /// </summary>
        /// <param name="base">To the base of this.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Wop(double @base) => Math2Const(Enums.OperationMath2.Wop, new[] { @base });

        /// <summary>
        /// Raise to power of an array, but with the arguments reversed.
        /// </summary>
        /// <param name="base">To the base of this.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Wop(double[] @base) => Math2Const(Enums.OperationMath2.Wop, @base);

        /// <summary>
        /// Raise to power of an array, but with the arguments reversed.
        /// </summary>
        /// <param name="base">To the base of this.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Wop(int[] @base) =>
            Math2Const(Enums.OperationMath2.Wop, Array.ConvertAll(@base, Convert.ToDouble));

        /// <summary>
        /// Arc tangent of an image in degrees.
        /// </summary>
        /// <param name="x">Arc tangent of y / <paramref name="x"/>.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Atan2(Image x) => Math2(x, Enums.OperationMath2.Atan2);

        /// <summary>
        /// Arc tangent of an constant in degrees.
        /// </summary>
        /// <param name="x">Arc tangent of y / <paramref name="x"/>.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Atan2(double x) => Math2Const(Enums.OperationMath2.Atan2, new[] { x });

        /// <summary>
        /// Arc tangent of an array in degrees.
        /// </summary>
        /// <param name="x">Arc tangent of y / <paramref name="x"/>.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Atan2(double[] x) => Math2Const(Enums.OperationMath2.Atan2, x);

        /// <summary>
        /// Arc tangent of an array in degrees.
        /// </summary>
        /// <param name="x">Arc tangent of y / <paramref name="x"/>.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Atan2(int[] x) =>
            Math2Const(Enums.OperationMath2.Atan2, Array.ConvertAll(x, Convert.ToDouble));

        /// <summary>
        /// Erode with a structuring element.
        /// </summary>
        /// <param name="mask">The structuring element.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Erode(Image mask) => Morph(mask, Enums.OperationMorphology.Erode);

        /// <summary>
        /// Dilate with a structuring element.
        /// </summary>
        /// <param name="mask">The structuring element.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Dilate(Image mask) => Morph(mask, Enums.OperationMorphology.Dilate);

        /// <summary>
        /// size x size median filter.
        /// </summary>
        /// <param name="size">The median filter.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Median(int size) => Rank(size, size, size * size / 2);

        /// <summary>
        /// Flip horizontally.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image FlipHor() => Flip(Enums.Direction.Horizontal);

        /// <summary>
        /// Flip vertically.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image FlipVer() => Flip(Enums.Direction.Vertical);

        /// <summary>
        /// Rotate 90 degrees clockwise.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Rot90() => Rot(Enums.Angle.D90);

        /// <summary>
        /// Rotate 180 degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Rot180() => Rot(Enums.Angle.D180);

        /// <summary>
        /// Rotate 270 degrees clockwise.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Rot270() => Rot(Enums.Angle.D270);

        /// <summary>
        /// Return the largest integral value not greater than the argument.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Floor() => this.Call("round", Enums.OperationRound.Floor) as Image;

        /// <summary>
        /// Return the largest integral value not greater than the argument.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Ceil() => this.Call("round", Enums.OperationRound.Ceil) as Image;

        /// <summary>
        /// Return the nearest integral value.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Rint() => this.Call("round", Enums.OperationRound.Rint) as Image;

        /// <summary>
        /// AND image bands together.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image BandAnd() => this.Call("bandbool", Enums.OperationBoolean.And) as Image;

        /// <summary>
        /// OR image bands together.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image BandOr() => this.Call("bandbool", Enums.OperationBoolean.Or) as Image;

        /// <summary>
        /// EOR image bands together.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image BandEor() => this.Call("bandbool", Enums.OperationBoolean.Eor) as Image;

        /// <summary>
        /// This operation compares two images on equality.
        /// </summary>
        /// <param name="right">A <see cref="Image"/> to compare.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Equal(Image right) =>
            this.Call("relational", right, Enums.OperationRelational.Equal) as Image;

        /// <summary>
        /// This operation compares two images on equality.
        /// </summary>
        /// <param name="right">A double array to compare.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Equal(double[] right) =>
            this.Call("relational_const", Enums.OperationRelational.Equal, right) as Image;

        /// <summary>
        /// This operation compares two images on equality.
        /// </summary>
        /// <param name="right">A integer array to compare.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Equal(int[] right) =>
            this.Call("relational_const", Enums.OperationRelational.Equal, right) as Image;

        /// <summary>
        /// This operation compares two images on equality.
        /// </summary>
        /// <param name="right">A double constant to compare.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Equal(double right) =>
            this.Call("relational_const", Enums.OperationRelational.Equal, right) as Image;

        /// <summary>
        /// This operation compares two images on inequality.
        /// </summary>
        /// <param name="right">A <see cref="Image"/> to compare.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image NotEqual(Image right) =>
            this.Call("relational", right, Enums.OperationRelational.Noteq) as Image;

        /// <summary>
        /// This operation compares two images on inequality.
        /// </summary>
        /// <param name="right">A double constant  to compare.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image NotEqual(double right) =>
            this.Call("relational_const", Enums.OperationRelational.Noteq, right) as Image;

        /// <summary>
        /// This operation compares two images on inequality.
        /// </summary>
        /// <param name="right">A double array to compare.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image NotEqual(double[] right) =>
            this.Call("relational_const", Enums.OperationRelational.Noteq, right) as Image;

        /// <summary>
        /// This operation compares two images on inequality.
        /// </summary>
        /// <param name="right">A integer array to compare.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image NotEqual(int[] right) =>
            this.Call("relational_const", Enums.OperationRelational.Noteq, right) as Image;

        /// <summary>
        /// Does this image have an alpha channel?
        /// </summary>
        /// <remarks>
        /// Uses colour space interpretation with number of channels to guess
        /// this.
        /// </remarks>
        /// <returns><see langword="true"/> if this image has an alpha channel;
        /// otherwise, <see langword="false"/>.</returns>
        public bool HasAlpha()
        {
            // use `vips_image_hasalpha` on libvips >= 8.5.
            if (NetVips.AtLeastLibvips(8, 5))
            {
                return VipsImage.HasAlpha(this);
            }

            return Bands == 2 ||
                   (Bands == 4 && Interpretation != Enums.Interpretation.Cmyk) ||
                   Bands > 4;
        }

        /// <summary>
        /// Append an alpha channel to an image.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// using rgba = rgb.AddAlpha();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image AddAlpha()
        {
            // use `vips_addalpha` on libvips >= 8.6.
            if (NetVips.AtLeastLibvips(8, 6))
            {
                var result = VipsImage.AddAlpha(this, out var vi, IntPtr.Zero);
                if (result != 0)
                {
                    throw new VipsException("unable to append alpha channel to image.");
                }

                return new Image(vi);
            }

            var maxAlpha = Interpretation == Enums.Interpretation.Grey16 || Interpretation == Enums.Interpretation.Rgb16
                ? 65535
                : 255;
            return Bandjoin(maxAlpha);
        }

        /// <summary>
        /// If image has been killed (see <see cref="SetKill"/>), set an error message,
        /// clear the `kill` flag and return <see langword="true"/>.
        /// Otherwise return <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// Handy for loops which need to run sets of threads which can fail.
        /// At least libvips 8.8 is needed. If this version requirement is not met,
        /// it will always return <see langword="false"/>.
        /// </remarks>
        /// <returns><see langword="true"/> if image has been killed;
        /// otherwise, <see langword="false"/>.</returns>
        public bool IsKilled()
        {
            if (!NetVips.AtLeastLibvips(8, 8))
            {
                return false;
            }

            return VipsImage.IsKilled(this);
        }

        /// <summary>
        /// Set the `kill` flag on an image. Handy for stopping sets of threads.
        /// </summary>
        /// <remarks>
        /// At least libvips 8.8 is needed.
        /// </remarks>
        /// <param name="kill">The kill state.</param>
        public void SetKill(bool kill)
        {
            if (!NetVips.AtLeastLibvips(8, 8))
            {
                return;
            }

            VipsImage.SetKill(this, kill);
        }

        /// <summary>
        /// Connects a callback function (<paramref name="callback"/>) to a signal on this image.
        /// </summary>
        /// <remarks>
        /// The callback will be triggered every time this signal is issued on this image.
        /// </remarks>
        /// <param name="signal">A signal to be used on this image. See <see cref="Enums.Signals"/>.</param>
        /// <param name="callback">The callback to connect.</param>
        /// <param name="data">Data to pass to handler calls.</param>
        /// <returns>The handler id.</returns>
        /// <exception cref="T:System.ArgumentException">If it failed to connect the signal.</exception>
        public ulong SignalConnect(Enums.Signals signal, EvalDelegate callback, IntPtr data = default)
        {
            void EvalMarshal(IntPtr imagePtr, IntPtr progressPtr, IntPtr userDataPtr)
            {
                if (imagePtr == IntPtr.Zero || progressPtr == IntPtr.Zero)
                {
                    return;
                }

                using var image = new Image(imagePtr);
                image.ObjectRef();

                var progress = Marshal.PtrToStructure<VipsProgress>(progressPtr);

                callback.Invoke(image, progress);
            }

            switch (signal)
            {
                case Enums.Signals.PreEval:
                    return SignalConnect<EvalMarshalDelegate>("preeval", EvalMarshal, data);
                case Enums.Signals.Eval:
                    return SignalConnect<EvalMarshalDelegate>("eval", EvalMarshal, data);
                case Enums.Signals.PostEval:
                    return SignalConnect<EvalMarshalDelegate>("posteval", EvalMarshal, data);
                default:
                    throw new ArgumentOutOfRangeException(nameof(signal), signal,
                        $"The value of argument '{nameof(signal)}' ({signal}) is invalid for enum type '{nameof(Enums.Signals)}'.");
            }
        }

        /// <summary>
        /// Drop caches on an image, and any downstream images.
        /// </summary>
        /// <remarks>
        /// This method drops all pixel caches on an image and on all downstream
        /// images. Any operations which depend on this image, directly or
        /// indirectly, are also dropped from the libvips operation cache.
        ///
        /// This method can be useful if you wrap a libvips image around an array
        /// with <see cref="NewFromMemory(Array, int, int, int, Enums.BandFormat)"/>
        /// and then change some bytes without libvips knowing.
        /// </remarks>
        public void Invalidate()
        {
            VipsImage.InvalidateAll(this);
        }

        /// <summary>
        /// Enable progress reporting on an image.
        /// </summary>
        /// <remarks>
        /// When progress reporting is enabled, evaluation of the most downstream
        /// image from this image will report progress using the <see cref="Enums.Signals.PreEval"/>,
        /// <see cref="Enums.Signals.Eval"/> and <see cref="Enums.Signals.PostEval"/> signals.
        /// </remarks>
        /// <param name="progress"><see langword="true"/> to enable progress reporting;
        /// otherwise, <see langword="false"/>.</param>
        public void SetProgress(bool progress)
        {
            VipsImage.SetProgress(this, progress);
        }

        /// <summary>
        /// Attach progress feedback, if required.
        /// </summary>
        /// <remarks>
        /// You can use this function to update user-interfaces with
        /// progress feedback, for example:
        /// <code language="lang-csharp">
        /// using var image = Image.NewFromFile("huge.jpg", access: Enums.Access.Sequential);
        ///
        /// var progress = new Progress&lt;int&gt;(percent =>
        /// {
        ///     Console.Write($"\r{percent}% complete");
        /// });
        /// image.SetProgress(progress);
        ///
        /// image.Dzsave("image-pyramid");
        /// </code>
        ///
        /// If a cancellation has been requested for this token (see <paramref name="token"/>)
        /// it will block the evaluation of this image on libvips >= 8.8 (see <see cref="SetKill"/>).
        /// If this version requirement is not met, it will only stop updating the progress.
        /// </remarks>
        /// <param name="progress">A provider for progress updates.</param>
        /// <param name="token">Cancellation token to block evaluation on this image.</param>
        public void SetProgress(IProgress<int> progress, CancellationToken token = default)
        {
            SetProgress(progress != null);
            if (progress == null)
            {
                return;
            }

            var lastPercent = 0;
            var isKilled = false;

            void EvalCallback(Image image, VipsProgress progressStruct)
            {
                // Block evaluation on this image if a cancellation
                // has been requested for this token.
                if (token.IsCancellationRequested)
                {
                    if (!isKilled)
                    {
                        image.SetKill(true);
                        isKilled = true;
                    }

                    return;
                }

                if (progressStruct.Percent != lastPercent)
                {
                    progress.Report(progressStruct.Percent);
                    lastPercent = progressStruct.Percent;
                }
            }

            SignalConnect(Enums.Signals.Eval, EvalCallback);
        }

        #endregion

        #region handwritten properties

        /// <summary>
        /// Multi-page images can have a page height.
        /// If page-height is not set, it defaults to the image height.
        /// </summary>
        /// <remarks>
        /// At least libvips 8.8 is needed.
        /// </remarks>
        public int PageHeight => VipsImage.GetPageHeight(this);

        #endregion

        #region support with in the most trivial way

        /// <summary>
        /// Does band exist in image.
        /// </summary>
        /// <param name="i">The index to fetch.</param>
        /// <returns>true if the index exists.</returns>
        public bool BandExists(int i)
        {
            return i >= 0 && i <= Bands - 1;
        }

        /// <summary>
        /// Overload `[]`.
        /// </summary>
        /// <remarks>
        /// Use `[]` to pull out band elements from an image. For example:
        /// <code language="lang-csharp">
        /// using var green = rgbImage[1];
        /// </code>
        /// Will make a new one-band image from band 1 (the middle band).
        /// </remarks>
        /// <param name="i">The band element to pull out.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image this[int i] => BandExists(i) ? ExtractBand(i) : null;

        /// <summary>
        /// A synonym for <see cref="Getpoint"/>.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// double[] outArray = in[x, y];
        /// </code>
        /// </example>
        /// <param name="x">Point to read.</param>
        /// <param name="y">Point to read.</param>
        /// <returns>An array of doubles.</returns>
        public double[] this[int x, int y] => Getpoint(x, y);

        /// <summary>
        /// Split an n-band image into n separate images.
        /// </summary>
        /// <returns>An array of <see cref="Image"/>.</returns>
        public Image[] Bandsplit()
        {
            var images = new Image[Bands];
            for (var i = 0; i < Bands; i++)
            {
                ref var image = ref images[i];
                image = this[i];
            }

            return images;
        }

        /// <summary>
        /// Compares the hashcode of two images.
        /// </summary>
        /// <param name="other">The <see cref="Image"/> to compare.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Image other)
        {
            return Equals(GetHashCode(), other.GetHashCode());
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current image.
        /// </summary>
        /// <param name="obj">The object to compare with the current image.</param>
        /// <returns><see langword="true"/> if the specified object is equal
        /// to the current image; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Image)obj);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current image.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion
    }
}