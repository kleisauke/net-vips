namespace NetVips
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using global::NetVips.Internal;
    using SMath = System.Math;

    /// <summary>
    /// Wrap a <see cref="VipsImage"/> object.
    /// </summary>
    public sealed partial class Image : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Secret ref for <see cref="NewFromMemory"/>.
        /// </summary>
        private GCHandle _dataHandle;

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
        /// <exception cref="T:System.Exception">If image doesn't have an even number of bands.</exception>
        private static Image RunCmplx(Func<Image, Image> func, Image image)
        {
            var originalFormat = image.Format;
            if (image.Format != Enums.BandFormat.Complex && image.Format != Enums.BandFormat.Dpcomplex)
            {
                if (image.Bands % 2 != 0)
                {
                    throw new Exception("not an even number of bands");
                }

                if (image.Format != Enums.BandFormat.Float && image.Format != Enums.BandFormat.Double)
                {
                    image = image.Cast(Enums.BandFormat.Float);
                }

                var newFormat = image.Format == Enums.BandFormat.Double
                    ? Enums.BandFormat.Dpcomplex
                    : Enums.BandFormat.Complex;

                image = image.Copy(format: newFormat, bands: image.Bands / 2);
            }

            image = func(image);
            if (originalFormat != Enums.BandFormat.Complex && originalFormat != Enums.BandFormat.Dpcomplex)
            {
                var newFormat = image.Format == Enums.BandFormat.Dpcomplex
                    ? Enums.BandFormat.Double
                    : Enums.BandFormat.Float;

                image = image.Copy(format: newFormat, bands: image.Bands * 2);
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
                case Array array when array.Is2D():
                    return NewFromArray(array);
                case double[] doubles:
                    return matchImage.NewFromImage(doubles);
                case double doubleValue:
                    return matchImage.NewFromImage(doubleValue);
                case int[] ints:
                    return matchImage.NewFromImage(ints);
                case int intValue:
                    return matchImage.NewFromImage(intValue);
                default:
                    throw new ArgumentException(
                        $"unsupported value type {value.GetType()} for Imageize");
            }
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
        /// var image = Image.NewFromFile("fred.jpg[shrink=2]");
        /// </code>
        /// You can also supply options as keyword arguments, for example:
        /// <code language="lang-csharp">
        /// var image = Image.NewFromFile("fred.jpg", new VOption
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
        /// <param name="fail">If set to <see langword="true"/>, the loader will fail
        /// with an error on the first serious error in the file. By default, libvips
        /// will attempt to read everything it can from a damaged image.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="vipsFilename"/>.</exception>
        public static Image NewFromFile(
            string vipsFilename,
            bool? memory = null,
            string access = null,
            bool? fail = null,
            VOption kwargs = null)
        {
            ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(vipsFilename);
            ref var filenameRef = ref MemoryMarshal.GetReference(span);

            var filename = Vips.GetFilename(filenameRef);
            var fileOptions = Vips.GetOptions(filenameRef).ToUtf8String(true);

            var name = Marshal.PtrToStringAnsi(VipsForeign.FindLoad(filename));
            if (name == null)
            {
                throw new VipsException($"unable to load from file {vipsFilename}");
            }

            var options = new VOption();
            if (kwargs != null)
            {
                options.Merge(kwargs);
            }

            if (memory.HasValue)
            {
                options.Add(nameof(memory), memory);
            }

            if (access != null)
            {
                options.Add(nameof(access), access);
            }

            if (fail.HasValue)
            {
                options.Add(nameof(fail), fail);
            }

            options.Add("string_options", fileOptions);

            return Operation.Call(name, options, filename.ToUtf8String(true)) as Image;
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
        /// <param name="access">Hint the expected access pattern for the image. See <see cref="Enums.Access"/>.</param>
        /// <param name="fail">If set True, the loader will fail with an error on
        /// the first serious error in the file. By default, libvips
        /// will attempt to read everything it can from a damaged image.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="data"/>.</exception>
        public static Image NewFromBuffer(
            byte[] data,
            string strOptions = "",
            string access = null,
            bool? fail = null,
            VOption kwargs = null)
        {
            var name = Marshal.PtrToStringAnsi(
                VipsForeign.FindLoadBuffer(MemoryMarshal.GetReference(data.AsSpan()), (ulong)data.Length));

            if (name == null)
            {
                throw new VipsException("unable to load from buffer");
            }

            var options = new VOption();
            if (kwargs != null)
            {
                options.Merge(kwargs);
            }

            if (access != null)
            {
                options.Add(nameof(access), access);
            }

            if (fail.HasValue)
            {
                options.Add(nameof(fail), fail);
            }

            options.Add("string_options", strOptions);

            return Operation.Call(name, options, data) as Image;
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
        /// <param name="fail">If set True, the loader will fail with an error on
        /// the first serious error in the file. By default, libvips
        /// will attempt to read everything it can from a damaged image.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="data"/>.</exception>
        public static Image NewFromBuffer(
            string data,
            string strOptions = "",
            string access = null,
            bool? fail = null,
            VOption kwargs = null) => NewFromBuffer(Encoding.UTF8.GetBytes(data), strOptions, access, fail, kwargs);

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
        /// <param name="fail">If set True, the loader will fail with an error on
        /// the first serious error in the file. By default, libvips
        /// will attempt to read everything it can from a damaged image.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="data"/>.</exception>
        public static Image NewFromBuffer(
            char[] data,
            string strOptions = "",
            string access = null,
            bool? fail = null,
            VOption kwargs = null) => NewFromBuffer(Encoding.UTF8.GetBytes(data), strOptions, access, fail, kwargs);

        /// <summary>
        /// Load a formatted image from stream.
        /// </summary>
        /// <remarks>
        /// True streaming is not yet supported within libvips. There has been
        /// quite a bit of talk of adding this, and there's a branch that adds 
        /// this, but it's never been merged for various reasons. See:
        /// https://github.com/lovell/sharp/issues/30#issuecomment-46960443
        ///
        /// So this simply copies the stream to a byte array and loads it with
        /// <see cref="NewFromBuffer(byte[], string, string, bool?, VOption)"/>.
        /// </remarks>
        /// <param name="stream">The stream object to load the image from.</param>
        /// <param name="strOptions">Load options as a string. Use <see cref="string.Empty"/> for no options.</param>
        /// <param name="access">Hint the expected access pattern for the image.</param>
        /// <param name="fail">If set True, the loader will fail with an error on
        /// the first serious error in the file. By default, libvips
        /// will attempt to read everything it can from a damaged image.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="stream"/>.</exception>
        public static Image NewFromStream(
            Stream stream,
            string strOptions = "",
            string access = null,
            bool? fail = null,
            VOption kwargs = null) => NewFromBuffer(stream.ToByteArray(), strOptions, access, fail, kwargs);

        /// <summary>
        /// Create an image from a 1D or 2D array.
        /// </summary>
        /// <remarks>
        /// A new one-band image with <see cref="Enums.BandFormat.Double"/> pixels is
        /// created from the array. These image are useful with the libvips
        /// convolution operator <see cref="Conv"/>.
        /// </remarks>
        /// <param name="array">Create the image from these values.
        /// 1D arrays become a single row of pixels.</param>
        /// <param name="scale">Default to 1.0. What to divide each pixel by after
        /// convolution.  Useful for integer convolution masks.</param>
        /// <param name="offset">Default to 0.0. What to subtract from each pixel
        /// after convolution.  Useful for integer convolution masks.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        /// <exception cref="VipsException">If unable to make image from <paramref name="array"/>.</exception>
        public static Image NewFromArray(Array array, double scale = 1.0, double offset = 0.0)
        {
            var is2D = array.Rank == 2;

            var height = is2D ? array.GetLength(0) : array.Length;
            var width = is2D ? array.GetLength(1) : (array.GetValue(0) is Array arrWidth ? arrWidth.Length : 1);
            var n = width * height;

            var a = new double[n];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    object value;
                    if (is2D)
                    {
                        value = array.GetValue(y, x);
                    }
                    else
                    {
                        var yValue = array.GetValue(y);
                        value = yValue is Array yArray ? (yArray.Length <= x ? 0 : yArray.GetValue(x)) : yValue;
                    }

                    a[x + (y * width)] = Convert.ToDouble(value);
                }
            }

            var vi = VipsImage.NewMatrixFromArray(width, height, a, n);

            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to make image from matrix");
            }

            var image = new Image(vi);
            image.SetType(GValue.GDoubleType, nameof(scale), scale);
            image.SetType(GValue.GDoubleType, nameof(offset), offset);
            return image;
        }

        /// <summary>
        /// Wrap an image around a memory array.
        /// </summary>
        /// <remarks>
        /// Wraps an Image around an area of memory containing a C-style array. For
        /// example, if the `data` memory array contains four bytes with the
        /// values 1, 2, 3, 4, you can make a one-band, 2x2 uchar image from
        /// it like this:
        /// <code language="lang-csharp">
        /// var image = Image.NewFromMemory(data, 2, 2, 1, "uchar");
        /// </code>
        /// A reference is kept to the data object, so it will not be
        /// garbage-collected until the returned image is garbage-collected.
        ///
        /// This method is useful for efficiently transferring images from GDI+
        /// into libvips.
        ///
        /// See <see cref="WriteToMemory"/> for the opposite operation.
        ///
        /// Use <see cref="Copy"/> to set other image attributes.
        /// </remarks>
        /// <param name="data">A memoryview or buffer object.</param>
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
            string format)
        {
            var formatValue = GValue.ToEnum(GValue.BandFormatType, format);

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var vi = VipsImage.NewFromMemory(handle.AddrOfPinnedObject(), new UIntPtr((ulong)data.Length),
                width, height, bands, (Internal.Enums.VipsBandFormat)formatValue);

            if (vi == IntPtr.Zero)
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }

                throw new VipsException("unable to make image from memory");
            }

            return new Image(vi) { _dataHandle = handle };
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
            var vi = VipsImage.NewTempFile(format);
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
            var pixel = (Black(1, 1) + value).Cast(Format);
            var image = pixel.Embed(0, 0, Width, Height, extend: "copy");
            image = image.Copy(interpretation: Interpretation, xres: Xres, yres: Yres, xoffset: Xoffset,
                yoffset: Yoffset);
            return image;
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
            var pixel = (Black(1, 1) + doubles).Cast(Format);
            var image = pixel.Embed(0, 0, Width, Height, extend: "copy");
            image = image.Copy(interpretation: Interpretation, xres: Xres, yres: Yres, xoffset: Xoffset,
                yoffset: Yoffset);
            return image;
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

            return new Image(vi);
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
            ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(vipsFilename);
            ref var filenameRef = ref MemoryMarshal.GetReference(span);

            var filename = Vips.GetFilename(filenameRef);
            var fileOptions = Vips.GetOptions(filenameRef).ToUtf8String(true);

            var name = Marshal.PtrToStringAnsi(VipsForeign.FindSave(filename));
            if (name == null)
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

            this.Call(name, kwargs, filename.ToUtf8String(true));
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
            ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(formatString);
            ref var formatRef = ref MemoryMarshal.GetReference(span);

            var bufferOptions = Vips.GetOptions(formatRef).ToUtf8String(true);
            var name = Marshal.PtrToStringAnsi(VipsForeign.FindSaveBuffer(formatRef));

            if (name == null)
            {
                throw new VipsException("unable to write to buffer");
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

            return this.Call(name, kwargs) as byte[];
        }

        /// <summary>
        /// Write an image to a stream.
        /// </summary>
        /// <remarks>
        /// True streaming is not yet supported within libvips. There has been
        /// quite a bit of talk of adding this, and there's a branch that adds 
        /// this, but it's never been merged for various reasons. See:
        /// https://github.com/lovell/sharp/issues/30#issuecomment-46960443
        ///
        /// So this simply creates a new non-resizable instance of the
        /// <see cref="MemoryStream"/> class based on the byte array that is
        /// returned from <see cref="WriteToBuffer"/>.
        /// </remarks>
        /// <param name="formatString">The suffix, plus any string-form arguments.</param>
        /// <param name="kwargs">Optional options that depend on the save operation.</param>
        /// <returns>A non-resizable <see cref="MemoryStream"/>.</returns>
        /// <exception cref="VipsException">If unable to write to stream.</exception>
        public Stream WriteToStream(string formatString, VOption kwargs = null) => 
            new MemoryStream(WriteToBuffer(formatString, kwargs));

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
        public byte[] WriteToMemory()
        {
            var pointer = VipsImage.WriteToMemory(this, out var psize);

            var managedArray = new byte[psize];
            Marshal.Copy(pointer, managedArray, 0, (int)psize);

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

        #region get/set metadata

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
        public override IntPtr GetTypeOf(string name)
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
        public override object Get(string name)
        {
            // scale and offset have default values
            if (name == "scale" && !Contains("scale"))
            {
                return 1.0;
            }

            if (name == "offset" && !Contains("offset"))
            {
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

            var result = VipsImage.Get(this, name, out var gv);
            if (result != 0)
            {
                throw new VipsException($"unable to get {name}");
            }

            return new GValue(gv).Get();
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
        /// Set the type and value of an item of metadata.
        /// </summary>
        /// <remarks>
        /// Sets the type and value of an item of metadata. Any old item of the
        /// same name is removed. See <see cref="GValue"/> for types.
        /// </remarks>
        /// <param name="gtype">The GType of the metadata item to create.</param>
        /// <param name="name">The name of the piece of metadata to create.</param>
        /// <param name="value">The value to set as a C# value. It is
        /// converted to the GType, if possible.</param>
        public void SetType(IntPtr gtype, string name, object value)
        {
            var gv = new GValue();
            gv.SetType(gtype);
            gv.Set(value);
            VipsImage.Set(this, name, in gv.Struct);
        }

        /// <summary>
        /// Set the value of an item of metadata.
        /// </summary>
        /// <remarks>
        /// Sets the value of an item of metadata. The metadata item must already
        /// exist.
        /// </remarks>
        /// <param name="name">The name of the piece of metadata to set the value of.</param>
        /// <param name="value">The value to set as a C# value. It is
        /// converted to the type of the metadata item, if possible.</param>
        /// <exception cref="T:System.Exception">If metadata item <paramref name="name"/> does not exist.</exception>
        public override void Set(string name, object value)
        {
            var gtype = GetTypeOf(name);
            if (gtype == IntPtr.Zero)
            {
                throw new Exception($"metadata item {name} does not exist - use SetType() to create and set");
            }

            SetType(gtype, name, value);
        }

        /// <summary>
        /// Remove an item of metadata.
        /// </summary>
        /// <remarks>
        /// The named metadata item is removed.
        /// </remarks>
        /// <param name="name">The name of the piece of metadata to remove.</param>
        /// <returns><see langword="true"/> if the metadata is successfully removed; 
        /// otherwise, <see langword="false"/>.</returns>
        public bool Remove(string name)
        {
            return VipsImage.Remove(this, name) != 0;
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
        /// Scale an image to 0 - 255.
        /// </summary>
        /// <remarks>
        /// This is the libvips `scale` operation, renamed to avoid a clash with
        /// the `scale` for convolution masks.
        /// </remarks>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = in.Scale(exp: double, log: bool);
        /// </code>
        /// </example>
        /// <param name="exp">Exponent for log scale.</param>
        /// <param name="log">Log scale.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image ScaleImage(double? exp = null, bool? log = null)
        {
            var options = new VOption();

            if (exp.HasValue)
            {
                options.Add(nameof(exp), exp);
            }

            if (log.HasValue)
            {
                options.Add(nameof(log), log);
            }

            return this.Call("scale", options) as Image;
        }

        /// <summary>
        /// Ifthenelse an image.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = cond.Ifthenelse(in1, in2, blend: bool);
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

            if (!(in1 is Image))
            {
                in1 = Imageize(matchImage, in1);
            }

            if (!(in2 is Image))
            {
                in2 = Imageize(matchImage, in2);
            }

            var options = new VOption();

            if (blend.HasValue)
            {
                options.Add(nameof(blend), blend);
            }

            return this.Call("ifthenelse", options, in1, in2) as Image;
        }

        /// <summary>
        /// Append a set of constants bandwise.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = image.Bandjoin(doubles);
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
        /// Image @out = image.Bandjoin(ints);
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
        /// Image @out = image.Bandjoin(images);
        /// </code>
        /// </example>
        /// <param name="images">Array of images.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandjoin(params Image[] images) =>
            this.Call("bandjoin", new object[] { images.PrependImage(this) }) as Image;

        /// <summary>
        /// Band-wise rank a set of constants.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = image.Bandrank(other, index: int);
        /// </code>
        /// </example>
        /// <param name="doubles">Array of constants.</param>
        /// <param name="index">Select this band element from sorted list.</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandrank(double[] doubles, int? index = null)
        {
            var options = new VOption();

            if (index.HasValue)
            {
                options.Add(nameof(index), index);
            }

            return this.Call("bandrank", options, new object[] { doubles }) as Image;
        }

        /// <summary>
        /// Band-wise rank a set of constants.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = image.Bandrank(other, index: int);
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
        /// Image @out = image.Bandrank(other, index: int);
        /// </code>
        /// </example>
        /// <param name="images">Array of input images.</param>
        /// <param name="index">Select this band element from sorted list.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandrank(Image[] images, int? index = null)
        {
            var options = new VOption();

            if (index.HasValue)
            {
                options.Add(nameof(index), index);
            }

            return this.Call("bandrank", options, new object[] { images.PrependImage(this) }) as Image;
        }

        /// <summary>
        /// Band-wise rank a image.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = image.Bandrank(other, index: int);
        /// </code>
        /// </example>
        /// <param name="other">Input image.</param>
        /// <param name="index">Select this band element from sorted list.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Bandrank(Image other, int? index = null) =>
            Bandrank(new[] { other }, index);

        /// <summary>
        /// Blend an array of images with an array of blend modes.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = image.Composite(other, modes, x: int, y: int, compositingSpace: string, premultiplied: bool);
        /// </code>
        /// </example>
        /// <param name="images">Array of input images.</param>
        /// <param name="modes">Array of VipsBlendMode to join with.</param>
        /// <param name="x">x position of overlay.</param>
        /// <param name="y">y position of overlay.</param>
        /// <param name="compositingSpace">Composite images in this colour space.</param>
        /// <param name="premultiplied">Images have premultiplied alpha.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Composite(Image[] images, int[] modes, int? x = null, int? y = null,
            string compositingSpace = null, bool? premultiplied = null)
        {
            var options = new VOption();

            if (x.HasValue)
            {
                options.Add(nameof(x), x);
            }

            if (y.HasValue)
            {
                options.Add(nameof(y), y);
            }

            if (compositingSpace != null)
            {
                options.Add("compositing_space", compositingSpace);
            }

            if (premultiplied.HasValue)
            {
                options.Add(nameof(premultiplied), premultiplied);
            }

            return this.Call("composite", options, images.PrependImage(this), modes) as Image;
        }

        /// <summary>
        /// Blend an array of images with an array of blend modes.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = image.Composite(other, modes, x: int, y: int, compositingSpace: string, premultiplied: bool);
        /// </code>
        /// </example>
        /// <param name="images">Array of input images.</param>
        /// <param name="modes">Array of VipsBlendMode to join with.</param>
        /// <param name="x">x position of overlay.</param>
        /// <param name="y">y position of overlay.</param>
        /// <param name="compositingSpace">Composite images in this colour space.</param>
        /// <param name="premultiplied">Images have premultiplied alpha.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Composite(Image[] images, string[] modes, int? x = null, int? y = null,
            string compositingSpace = null, bool? premultiplied = null) =>
            Composite(images, modes.Select(m => GValue.ToEnum(GValue.BlendModeType, m)).ToArray(), x, y,
                compositingSpace, premultiplied);

        /// <summary>
        /// A synonym for <see cref="Composite2"/>.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = base.Composite(overlay, mode, x: int, y: int, compositingSpace: string, premultiplied: bool);
        /// </code>
        /// </example>
        /// <param name="overlay">Overlay image.</param>
        /// <param name="mode">VipsBlendMode to join with.</param>
        /// <param name="x">x position of overlay.</param>
        /// <param name="y">y position of overlay.</param>
        /// <param name="compositingSpace">Composite images in this colour space.</param>
        /// <param name="premultiplied">Images have premultiplied alpha.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Composite(Image overlay, string mode, int? x = null, int? y = null, string compositingSpace = null,
            bool? premultiplied = null) =>
            Composite2(overlay, mode, x, y, compositingSpace, premultiplied);

        /// <summary>
        /// A synonym for <see cref="ExtractArea"/>.
        /// </summary>
        /// <example>
        /// <code language="lang-csharp">
        /// Image @out = input.Crop(left, top, width, height);
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
        public Image Real() => Complexget("real");

        /// <summary>
        /// Return the imaginary part of a complex image.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Imag() => Complexget("imag");

        /// <summary>
        ///  Return an image converted to polar coordinates.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Polar() => RunCmplx(x => x.Complex("polar"), this);

        /// <summary>
        /// Return an image converted to rectangular coordinates.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Rect() => RunCmplx(x => x.Complex("rect"), this);

        /// <summary>
        /// Return the complex conjugate of an image.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Conj() => Complex("conj");

        /// <summary>
        /// Return the sine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Sin() => Math("sin");

        /// <summary>
        /// Return the cosine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Cos() => Math("cos");

        /// <summary>
        /// Return the tangent of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Tan() => Math("tan");

        /// <summary>
        /// Return the inverse sine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Asin() => Math("asin");

        /// <summary>
        /// Return the inverse cosine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Acos() => Math("acos");

        /// <summary>
        /// Return the inverse tangent of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Atan() => Math("atan");

        /// <summary>
        /// Return the natural log of an image.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Log() => Math("log");

        /// <summary>
        /// Return the log base 10 of an image.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Log10() => Math("log10");

        /// <summary>
        /// Return e ** pixel.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Exp() => Math("exp");

        /// <summary>
        /// Return 10 ** pixel.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Exp10() => Math("exp10");

        /// <summary>
        /// Erode with a structuring element.
        /// </summary>
        /// <param name="mask">The structuring element.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Erode(Image mask) => Morph(mask, "erode");

        /// <summary>
        /// Dilate with a structuring element.
        /// </summary>
        /// <param name="mask">The structuring element.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Dilate(Image mask) => Morph(mask, "dilate");

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
        public Image Floor() => this.Call("round", "floor") as Image;

        /// <summary>
        /// Return the largest integral value not greater than the argument.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Ceil() => this.Call("round", "ceil") as Image;

        /// <summary>
        /// Return the nearest integral value.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Rint() => this.Call("round", "rint") as Image;

        /// <summary>
        /// AND image bands together.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image BandAnd() => this.Call("bandbool", "and") as Image;

        /// <summary>
        /// OR image bands together.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image BandOr() => this.Call("bandbool", "or") as Image;

        /// <summary>
        /// EOR image bands together.
        /// </summary>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image BandEor() => this.Call("bandbool", "eor") as Image;

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
                return VipsImage.HasAlpha(this) == 1;
            }

            return Bands == 2 ||
                   (Bands == 4 && Interpretation != Enums.Interpretation.Cmyk) ||
                   Bands > 4;
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

            return VipsImage.IsKilled(this) == 1;
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

            VipsImage.SetKill(this, kill ? 1 : 0);
        }

        /// <summary>
        /// Attach progress feedback, if required.
        /// </summary>
        /// <remarks>
        /// You can use this function to update user-interfaces with
        /// progress feedback, for example:
        /// <code language="lang-csharp">
        /// var image = Image.NewFromFile("huge.jpg", access: Enums.Access.Sequential);
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
            VipsImage.SetProgress(this, progress == null ? 0 : 1);
            if (progress == null)
            {
                return;
            }

            var lastPercent = 0;
            var isKilled = false;

            EvalCallback evalCallback = (imagePtr, progressPtr, userDataPtr) =>
            {
                // Block evaluation on this image if a cancellation
                // has been requested for this token.
                if (token.IsCancellationRequested)
                {
                    if (!isKilled)
                    {
                        SetKill(true);
                        isKilled = true;
                    }

                    return;
                }

                if (progressPtr == IntPtr.Zero)
                {
                    return;
                }

                var progressStruct = progressPtr.Dereference<VipsProgress.Struct>();
                if (progressStruct.Percent != lastPercent)
                {
                    progress.Report(progressStruct.Percent);
                    lastPercent = progressStruct.Percent;
                }
            };

            SignalConnect(Internal.Enums.VipsEvaluation.Eval, evalCallback);
        }

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
        /// var green = rgbImage[1];
        /// </code>
        /// Will make a new one-band image from band 1 (the middle band).
        /// </remarks>
        /// <param name="i">The band element to pull out.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image this[int i]
        {
            get => BandExists(i) ? this.Call("extract_band", i) as Image : null;
            set
            {
                // number of bands to the left and right of value
                var nLeft = SMath.Min(Bands, SMath.Max(0, i));
                var nRight = SMath.Min(Bands, SMath.Max(0, Bands - 1 - i));
                var offset = Bands - nRight;
                var componentsList = new List<Image>();
                if (nLeft > 0)
                {
                    var image = this.Call("extract_band", new VOption
                    {
                        {"n", nLeft}
                    }, 0) as Image;
                    componentsList.Add(image);
                }

                componentsList.Add(value);

                if (nRight > 0)
                {
                    var image = this.Call("extract_band", new VOption
                    {
                        {"n", nRight}
                    }, offset) as Image;
                    componentsList.Add(image);
                }

                var head = componentsList[0];

                var components = new object[componentsList.Count - 1];
                for (var index = 1; index < componentsList.Count; index++)
                {
                    components[index - 1] = componentsList[index];
                }

                if (this.Call("bandjoin", head, components) is Image bandImage)
                {
                    SetHandle(bandImage.handle);
                }
            }
        }

        /// <summary>
        /// Split an n-band image into n separate images.
        /// </summary>
        /// <returns>An array of <see cref="Image"/>.</returns>
        public Image[] Bandsplit()
        {
            var images = new Image[Bands];
            for (var i = 0; i < Bands; i++)
            {
                images[i] = this[i];
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

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // release reference to our secret ref
            if (_dataHandle.IsAllocated)
            {
                _dataHandle.Free();
            }

            // Call our base Dispose method
            base.Dispose(disposing);
        }
    }
}