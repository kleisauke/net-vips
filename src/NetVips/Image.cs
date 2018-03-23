using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NetVips.Internal;
using SMath = System.Math;

namespace NetVips
{
    /// <summary>
    /// Wrap a <see cref="NetVips.Internal.VipsImage"/> object.
    /// </summary>
    public sealed partial class Image : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        internal VipsImage IntlImage;

        /// <summary>
        /// Secret ref for NewFromMemory
        /// </summary>
#pragma warning disable 414
        private Array _data;
#pragma warning restore 414

        internal Image(VipsImage vImage) : base(vImage.ParentInstance)
        {
            // logger.Debug($"VipsImage = {vImage}");
            IntlImage = vImage;
        }

        #region helpers

        /// <summary>
        /// Handy for the overloadable operators. A vips operator like
        /// 'more', but if the arg is not an image (ie. it's a constant), call
        /// 'more_const' instead.
        /// </summary>
        /// <param name="image">The left-hand argument.</param>
        /// <param name="other">The right-hand argument.</param>
        /// <param name="operationName">The base part of the operation name.</param>
        /// <param name="operation">The operation to call.</param>
        /// <returns></returns>
        public static object CallEnum(object image, object other, string operationName, string operation)
        {
            if (other.IsPixel())
            {
                return Operation.Call(operationName + "_const", image, operation, other);
            }
            else
            {
                return Operation.Call(operationName, image, other, operation);
            }
        }

        /// <summary>
        /// Run a complex function on a non-complex image.
        /// </summary>
        /// <remarks>
        /// The image needs to be complex, or have an even number of bands. The input
        /// can be int, the output is always float or double.
        /// </remarks>
        /// <param name="func"></param>
        /// <param name="image"></param>
        /// <returns>A new <see cref="Image"/></returns>
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
        /// Turn a constant (eg. 1, '12', new []{1, 2, 3}, {new []{1}}) into an image using
        /// <paramref name="matchImage" /> as a guide.
        /// </summary>
        /// <param name="matchImage"></param>
        /// <param name="value"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Imageize(Image matchImage, object value)
        {
            // logger.Debug($"Imageize: value = {value}");
            // careful! this can be None if value is a 2D array
            if (value is Image image)
            {
                return image;
            }
            else if (value.Is2D())
            {
                return NewFromArray(value);
            }
            else
            {
                return matchImage.NewFromImage(value);
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
        ///
        ///     image = NetVips.Image.NewFromFile('fred.jpg[shrink=2]')
        ///
        /// You can also supply options as keyword arguments, for example:
        ///
        ///     var image = NetVips.Image.NewFromFile('fred.jpg', new VOption
        ///     {
        ///         {"shrink", 2}
        ///     });
        ///
        /// The full set of options available depend upon the load operation that
        /// will be executed. Try something like:
        /// 
        ///     $ vips jpegload
        ///
        /// at the command-line to see a summary of the available options for the
        /// JPEG loader.
        /// 
        /// Loading is fast: only enough of the image is loaded to be able to fill
        /// out the header. Pixels will only be decompressed when they are needed.
        /// </remarks>
        /// <param name="vipsFilename">The disc file to load the image from, with
        /// optional appended arguments.</param>
        /// <param name="memory">If set True, load the image via memory rather than
        /// via a temporary disc file. See <see cref="NewTempFile"/> for
        /// notes on where temporary files are created. Small images are
        /// loaded via memory by default, use ``VIPS_DISC_THRESHOLD`` to
        /// set the definition of small.</param>
        /// <param name="access">Hint the expected access pattern for the image.</param>
        /// <param name="fail">If set True, the loader will fail with an error on
        /// the first serious error in the file. By default, libvips
        /// will attempt to read everything it can from a damanged image.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/></returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="vipsFilename" />.</exception>
        public static Image NewFromFile(string vipsFilename, bool? memory = null, string access = null,
            bool? fail = null, VOption kwargs = null)
        {
            var fileNamePtr = vipsFilename.ToUtf8Ptr();
            var filename = VipsImage.VipsFilenameGetFilename(fileNamePtr);
            var fileOptions = Marshal.PtrToStringAnsi(VipsImage.VipsFilenameGetOptions(fileNamePtr));

            var name = Marshal.PtrToStringAnsi(VipsForeign.VipsForeignFindLoad(filename));
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
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            options.Add("string_options", fileOptions);

            return Operation.Call(name, options, filename.ToUtf8String()) as Image;
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
        /// <param name="strOptions">Load options as a string. Use ``""`` for no options.</param>
        /// <param name="access">Hint the expected access pattern for the image.</param>
        /// <param name="fail">If set True, the loader will fail with an error on
        /// the first serious error in the file. By default, libvips
        /// will attempt to read everything it can from a damanged image.</param>
        /// <param name="kwargs">Optional options that depend on the load operation.</param>
        /// <returns>A new <see cref="Image"/></returns>
        /// <exception cref="VipsException">If unable to load from <paramref name="data" />.</exception>
        public static Image NewFromBuffer(object data, string strOptions = "", string access = null, bool? fail = null,
            VOption kwargs = null)
        {
            int length;
            IntPtr memory;
            switch (data)
            {
                case string strValue:
                    length = Encoding.UTF8.GetByteCount(strValue);
                    memory = strValue.ToUtf8Ptr();
                    break;
                case byte[] byteArrValue:
                    length = byteArrValue.Length;
                    memory = byteArrValue.ToPtr();
                    break;
                case char[] charArrValue:
                    length = Encoding.UTF8.GetByteCount(charArrValue);
                    memory = Encoding.UTF8.GetBytes(charArrValue).ToPtr();
                    break;
                default:
                    throw new Exception(
                        $"unsupported value type {data.GetType()} for NewFromBuffer"
                    );
            }

            var name = Marshal.PtrToStringAnsi(VipsForeign.VipsForeignFindLoadBuffer(memory, (ulong) length));
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
                options.Add("access", access);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            options.Add("string_options", strOptions);

            return Operation.Call(name, options, data) as Image;
        }

        /// <summary>
        /// Create an image from a 1D or 2D array.
        /// </summary>
        /// <remarks>
        /// A new one-band image with <see cref="Enums.BandFormat.Double"/> pixels is
        /// created from the array. These image are useful with the libvips
        /// convolution operator <see cref="Conv"/> 
        /// </remarks>
        /// <param name="array">Create the image from these values. 
        /// 1D arrays become a single row of pixels.</param>
        /// <param name="scale">Default to 1.0. What to divide each pixel by after
        /// convolution.  Useful for integer convolution masks.</param>
        /// <param name="offset">Default to 0.0. What to subtract from each pixel
        /// after convolution.  Useful for integer convolution masks.</param>
        /// <returns>A new <see cref="Image"/></returns>
        /// <exception cref="VipsException">If unable to make image from <paramref name="array" />.</exception>
        public static Image NewFromArray(object array, double scale = 1.0, double offset = 0.0)
        {
            if (!array.Is2D())
            {
                array = array as Array;
            }

            if (!(array is Array arr))
            {
                throw new ArgumentException("can't create image from unknown object");
            }

            var is2D = arr.Rank == 2;

            var height = is2D ? arr.GetLength(0) : arr.Length;
            var width = is2D ? arr.GetLength(1) : (arr.GetValue(0) is Array arrWidth ? arrWidth.Length : 1);
            var n = width * height;

            var a = new double[n];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    object value;
                    if (is2D)
                    {
                        value = arr.GetValue(y, x);
                    }
                    else
                    {
                        var yValue = arr.GetValue(y);
                        value = yValue is Array yArray ? (yArray.Length <= x ? 0 : yArray.GetValue(x)) : yValue;
                    }

                    a[x + y * width] = Convert.ToDouble(value);
                }
            }

            var vi = VipsImage.VipsImageNewMatrixFromArray(width, height, a, n);

            if (vi == null)
            {
                throw new VipsException("unable to make image from matrix");
            }

            var image = new Image(new VipsImage(vi));
            image.SetType(GValue.GDoubleType, "scale", scale);
            image.SetType(GValue.GDoubleType, "offset", offset);
            return image;
        }

        /// <summary>
        /// Wrap an image around a memory array.
        /// </summary>
        /// <remarks>
        /// Wraps an Image around an area of memory containing a C-style array. For
        /// example, if the ``data`` memory array contains four bytes with the
        /// values 1, 2, 3, 4, you can make a one-band, 2x2 uchar image from
        /// it like this: 
        /// 
        ///     var image = NetVips.Image.NewFromMemory(data, 2, 2, 1, "uchar")
        /// 
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
        /// <param name="bands">Number of bands</param>
        /// <param name="format">Band format.</param>
        /// <returns>A new <see cref="Image"/></returns>
        /// <exception cref="VipsException">If unable to make image from <paramref name="data" />.</exception>
        public static Image NewFromMemory(
            Array data,
            int width,
            int height,
            int bands,
            string format)
        {
            var formatValue = GValue.ToEnum(GValue.BandFormatType, format);

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            VipsImage vi;
            try
            {
                vi = VipsImage.VipsImageNewFromMemory(handle, (ulong) data.Length, width, height, bands, (Internal.Enums.VipsBandFormat) formatValue);
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }

            if (vi == null)
            {
                throw new VipsException("unable to make image from memory");
            }

            var image = new Image(vi)
            {
                // keep a secret ref to the underlying object
                _data = data
            };

            return image;
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
        /// the environment variable ``TMPDIR``. If this is not set, then on
        /// Unix systems, vips will default to ``/tmp``. On Windows, vips uses
        /// ``GetTempPath()`` to find the temporary directory.
        /// 
        /// vips uses ``g_mkstemp()`` to make the temporary filename. They
        /// generally look something like ``"vips-12-EJKJFGH.v"``.
        /// </remarks>
        /// <param name="format">The format for the temp file, for example
        /// ``"%s.v"`` for a vips format file. The ``%s`` is
        /// substituted by the file path.</param>
        /// <returns>A new <see cref="Image"/></returns>
        /// <exception cref="VipsException">If unable to make temp file from <paramref name="format" />.</exception>
        public static Image NewTempFile(string format)
        {
            var vi = VipsImage.VipsImageNewTempFile(format);
            if (vi == null)
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
        /// and resolution as ``this``, but with every pixel set to ``value``.
        /// </remarks>
        /// <param name="value">The value for the pixels. Use a
        /// single number to make a one-band image; use an array constant
        /// to make a many-band image.</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image NewFromImage(object value)
        {
            var pixel = (Black(1, 1) + value).Cast(Format);
            var image = pixel.Embed(0, 0, Width, Height, extend: "copy");
            image = image.Copy(interpretation: Interpretation, xres: Xres, yres: Yres, xoffset: Xoffset,
                yoffset: Yoffset);
            return image;
        }

        /// <summary>
        /// Copy an image to memory.
        /// </summary>
        /// <remarks>
        /// A large area of memory is allocated, the image is rendered to that
        /// memory area, and a new image is returned which wraps that large memory
        /// area.
        /// </remarks>
        /// <returns>A new <see cref="Image"/></returns>
        /// <exception cref="VipsException">If unable to copy to memory.</exception>
        public Image CopyMemory()
        {
            var vi = VipsImage.VipsImageCopyMemory(IntlImage);
            if (vi == null)
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
        /// 
        ///     image.WriteToFile('fred.jpg[Q=95]')
        /// 
        /// You can also supply options as keyword arguments, for example: 
        ///
        ///     image.WriteToFile('fred.jpg', new VOption
        ///     {
        ///         {"Q", 95}
        ///     });
        ///
        /// The full set of options available depend upon the save operation that
        /// will be executed. Try something like: 
        /// 
        ///     $ vips jpegsave
        /// 
        /// at the command-line to see a summary of the available options for the
        /// JPEG saver.
        /// </remarks>
        /// <param name="vipsFilename">The disc file to save the image to, with
        /// optional appended arguments.</param>
        /// <param name="kwargs">Optional options that depend on the save operation.</param>
        /// <returns>None</returns>
        /// <exception cref="VipsException">If unable to write to <paramref name="vipsFilename" />.</exception>
        public void WriteToFile(string vipsFilename, VOption kwargs = null)
        {
            var fileNamePtr = vipsFilename.ToUtf8Ptr();
            var filename = VipsImage.VipsFilenameGetFilename(fileNamePtr);
            var options = Marshal.PtrToStringAnsi(VipsImage.VipsFilenameGetOptions(fileNamePtr));

            var name = Marshal.PtrToStringAnsi(VipsForeign.VipsForeignFindSave(filename));
            if (name == null)
            {
                throw new VipsException($"unable to write to file {vipsFilename}");
            }

            var stringOptions = new VOption
            {
                {"string_options", options}
            };

            if (kwargs != null)
            {
                kwargs.Merge(stringOptions);
            }
            else
            {
                kwargs = stringOptions;
            }

            Operation.Call(name, kwargs, this, filename.ToUtf8String());
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
        /// 
        ///     var data = image.WriteToBuffer('.jpg[Q=95]')
        /// 
        /// You can also supply options as keyword arguments, for example: 
        ///
        ///     var data = image.WriteToBuffer('.jpg', new VOption
        ///     {
        ///         {"Q", 95}
        ///     });
        ///
        /// The full set of options available depend upon the load operation that
        /// will be executed. Try something like: 
        /// 
        ///     $ vips jpegsave_buffer
        /// 
        /// at the command-line to see a summary of the available options for the
        /// JPEG saver.
        /// </remarks>
        /// <param name="formatString">The suffix, plus any string-form arguments.</param>
        /// <param name="kwargs">Optional options that depend on the save operation.</param>
        /// <returns>An array of bytes</returns>
        /// <exception cref="VipsException">If unable to write to buffer.</exception>
        public byte[] WriteToBuffer(string formatString, VOption kwargs = null)
        {
            var formatStrPtr = formatString.ToUtf8Ptr();
            var options = Marshal.PtrToStringAnsi(VipsImage.VipsFilenameGetOptions(formatStrPtr));

            var name = Marshal.PtrToStringAnsi(VipsForeign.VipsForeignFindSaveBuffer(formatStrPtr));
            if (name == null)
            {
                throw new VipsException("unable to write to buffer");
            }

            var stringOptions = new VOption
            {
                {"string_options", options}
            };

            if (kwargs != null)
            {
                kwargs.Merge(stringOptions);
            }
            else
            {
                kwargs = stringOptions;
            }

            return Operation.Call(name, kwargs, this) as byte[];
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
        /// 
        ///     var buf = image.WriteToMemory()
        /// 
        /// will return a four byte buffer containing the values 1, 2, 3, 4.
        /// </remarks>
        /// <returns>An array of bytes</returns>
        public byte[] WriteToMemory()
        {
            ulong psize = 0;
            var pointer = VipsImage.VipsImageWriteToMemory(IntlImage, ref psize);

            var managedArray = new byte[psize];
            Marshal.Copy(pointer, managedArray, 0, (int) psize);

            GLib.GFree(pointer);

            return managedArray;
        }

        /// <summary>
        /// Write an image to another image.
        /// </summary>
        /// <remarks>
        /// This function writes ``this`` to another image. Use something like
        /// <see cref="NewTempFile"/> to make an image that can be written to.
        /// </remarks>
        /// <param name="other">The <see cref="Image"/> to write to.</param>
        /// <returns></returns>
        /// <exception cref="VipsException">If unable to write to image.</exception>
        public void Write(Image other)
        {
            var result = VipsImage.VipsImageWrite(IntlImage, other.IntlImage);
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
        /// Fetch the GType of a piece of metadata, or 0 if the named item does not
        /// exist. See <see cref="GValue"/>.
        /// </remarks>
        /// <param name="name">The name of the piece of metadata to get the type of.</param>
        /// <returns>The ``GType``, or 0</returns>
        public override ulong GetTypeOf(string name)
        {
            // on libvips before 8.5, property types must be fetched separately,
            // since built-in enums were reported as ints
            if (!Base.AtLeastLibvips(8, 5))
            {
                var gtype = base.GetTypeOf(name);
                if (gtype != 0)
                {
                    return gtype;
                }
            }

            return VipsImage.VipsImageGetTypeof(IntlImage, name);
        }

        /// <summary>
        /// Get an item of metadata.
        /// </summary>
        /// Fetches an item of metadata as a C# value. For example:
        /// 
        ///     orientation = image.get("orientation")
        /// 
        /// would fetch the image orientation.
        /// <param name="name">The name of the piece of metadata to get.</param>
        /// <returns>The metadata item as a C# value</returns>
        /// <exception cref="VipsException">If unable to get <paramref name="name" />.</exception>
        public override object Get(string name)
        {
            // scale and offset have default values
            if (name == "scale" && GetTypeOf("scale") == 0)
            {
                return 1.0;
            }

            if (name == "offset" && GetTypeOf("offset") == 0)
            {
                return 0.0;
            }

            // with old libvips, we must fetch properties (as opposed to
            // metadata) via VipsObject
            if (!Base.AtLeastLibvips(8, 5))
            {
                var gtype = base.GetTypeOf(name);
                if (gtype != 0)
                {
                    return base.Get(name);
                }
            }

            var gv = new GValue();
            var result = VipsImage.VipsImageGet(IntlImage, name, gv.IntlGValue);
            if (result != 0)
            {
                throw new VipsException($"unable to get {name}");
            }

            return gv.Get();
        }

        /// <summary>
        /// Get a list of all the metadata fields on an image.
        /// </summary>
        /// <remarks>
        /// At least libvips 8.5 is needed
        /// </remarks>
        /// <returns>string[] or null</returns>
        public string[] GetFields()
        {
            if (!Base.AtLeastLibvips(8, 5))
            {
                return null;
            }

            return VipsImage.VipsImageGetFields(IntlImage);
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
        /// converted to the ``gtype``, if possible.</param>
        /// <returns></returns>
        public void SetType(ulong gtype, string name, object value)
        {
            var gv = new GValue();
            gv.SetType(gtype);
            gv.Set(value);
            VipsImage.VipsImageSet(IntlImage, name, gv.IntlGValue);
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
        /// <returns></returns>
        /// <exception cref="T:System.Exception">If metadata item <paramref name="name" /> does not exist.</exception>
        public override void Set(string name, object value)
        {
            var gtype = GetTypeOf(name);
            if (gtype == 0)
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
        /// <returns></returns>
        public bool Remove(string name)
        {
            return VipsImage.VipsImageRemove(IntlImage, name) != 0;
        }

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
        /// This is the libvips ``scale`` operation, renamed to avoid a clash with
        /// the ``scale`` for convolution masks.
        /// </remarks>
        /// <example>
        /// <code>
        /// Image @out = in.Scale(exp: double, log: bool);
        /// </code>
        /// </example>
        /// <param name="exp">Exponent for log scale</param>
        /// <param name="log">Log scale</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ScaleImage(double? exp = null, bool? log = null)
        {
            var options = new VOption();

            if (exp.HasValue)
            {
                options.Add("exp", exp);
            }

            if (log.HasValue)
            {
                options.Add("log", log);
            }

            return this.Call("scale", options) as Image;
        }

        /// <summary>
        /// Ifthenelse an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = cond.Ifthenelse(in1, in2, blend: bool);
        /// </code>
        /// </example>
        /// <param name="in1">Source for TRUE pixels</param>
        /// <param name="in2">Source for FALSE pixels</param>
        /// <param name="blend">Blend smoothly between then and else parts</param>
        /// <returns>A new <see cref="Image"/></returns>
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
                options.Add("blend", blend);
            }

            return this.Call("ifthenelse", options, in1, in2) as Image;
        }

        /// <summary>
        /// Append a set of images or constants bandwise.
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Bandjoin(other);
        /// </code>
        /// </example>
        /// <param name="other">Array of input images</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandjoin(object other)
        {
            if (!(other is IEnumerable))
            {
                other = new[] {other};
            }

            // if [other] is all numbers, we can use BandjoinConst
            switch (other)
            {
                case double[] doubles:
                    return BandjoinConst(doubles);
                case int[] ints:
                    return BandjoinConst(Array.ConvertAll(ints, Convert.ToDouble));
                case object[] objects when objects.All(x => x.IsNumeric()):
                    return BandjoinConst(Array.ConvertAll(objects, Convert.ToDouble));
                case IEnumerable objects:
                    return Operation.Call("bandjoin", null, new object[] {objects.PrependImage(this)}) as Image;
                default:
                    throw new ArgumentException(
                        $"unsupported value type {other.GetType()} for Bandjoin"
                    );
            }
        }

        /// <summary>
        /// Band-wise rank of a set of images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Bandrank(other, index: int);
        /// </code>
        /// </example>
        /// <param name="other">Array of input images</param>
        /// <param name="index">Select this band element from sorted list</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandrank(object other, int? index = null)
        {
            if (!(other is IEnumerable))
            {
                other = new[] {other};
            }

            var options = new VOption();

            if (index.HasValue)
            {
                options.Add("index", index);
            }

            return Operation.Call("bandrank", options,
                new object[] {((IEnumerable) other).PrependImage(this)}) as Image;
        }

        /// <summary>
        /// Blend an array of images with an array of blend modes
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Composite(other, mode, compositingSpace: string, premultiplied: bool);
        /// </code>
        /// </example>
        /// <param name="other">Array of input images</param>
        /// <param name="mode">Array of VipsBlendMode to join with</param>
        /// <param name="compositingSpace">Composite images in this colour space</param>
        /// <param name="premultiplied">Images have premultiplied alpha</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Composite(object other, object mode, string compositingSpace = null, bool? premultiplied = null)
        {
            if (!(other is IEnumerable))
            {
                other = new[] {other};
            }

            if (!(other is object[] images))
            {
                return null;
            }

            if (!(mode is IEnumerable))
            {
                mode = new[] {mode};
            }

            // modes are VipsBlendMode enums, but we have to pass as array of int --
            // we need to map str->int by hand
            int[] blendModes;
            switch (mode)
            {
                case string[] strModes:
                    blendModes = strModes.Select(x => GValue.ToEnum(GValue.BlendModeType, x)).ToArray();
                    break;
                case int[] intModes:
                    blendModes = intModes;
                    break;
                default:
                    // Use Enums.BlendMode.Over if a non-existent value is given.
                    blendModes = new[] {GValue.ToEnum(GValue.BlendModeType, Enums.BlendMode.Over)};
                    break;
            }

            var options = new VOption();

            if (compositingSpace != null)
            {
                options.Add("compositing_space", compositingSpace);
            }

            if (premultiplied.HasValue)
            {
                options.Add("premultiplied", premultiplied);
            }

            return Operation.Call("composite", options, images.PrependImage(this), blendModes) as Image;
        }

        /// <summary>
        /// A synonym for <see cref="ExtractArea"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = input.Crop(left, top, width, height);
        /// </code>
        /// </example>
        /// <param name="left">Left edge of extract area</param>
        /// <param name="top">Top edge of extract area</param>
        /// <param name="width">Width of extract area</param>
        /// <param name="height">Height of extract area</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Crop(int left, int top, int width, int height)
        {
            return this.Call("extract_area", left, top, width, height) as Image;
        }

        /// <summary>
        /// Return the coordinates of the image maximum.
        /// </summary>
        /// <returns>An array of doubles</returns>
        public double[] MaxPos()
        {
            var v = Max(out var x, out var y);
            return new[] {v, x, y};
        }

        /// <summary>
        /// Return the coordinates of the image minimum.
        /// </summary>
        /// <returns>An array of doubles</returns>
        public double[] MinPos()
        {
            var v = Min(out var x, out var y);
            return new[] {v, x, y};
        }

        /// <summary>
        /// Return the real part of a complex image.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Real()
        {
            return Complexget("real");
        }

        /// <summary>
        /// Return the imaginary part of a complex image.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Imag()
        {
            return Complexget("imag");
        }

        /// <summary>
        ///  Return an image converted to polar coordinates.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Polar()
        {
            return RunCmplx(x => x.Complex("polar"), this);
        }

        /// <summary>
        /// Return an image converted to rectangular coordinates.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rect()
        {
            return RunCmplx(x => x.Complex("rect"), this);
        }

        /// <summary>
        /// Return the complex conjugate of an image.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Conj()
        {
            return Complex("conj");
        }

        /// <summary>
        /// Return the sine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Sin()
        {
            return Math("sin");
        }

        /// <summary>
        /// Return the cosine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Cos()
        {
            return Math("cos");
        }

        /// <summary>
        /// Return the tangent of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Tan()
        {
            return Math("tan");
        }

        /// <summary>
        /// Return the inverse sine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Asin()
        {
            return Math("asin");
        }

        /// <summary>
        /// Return the inverse cosine of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Acos()
        {
            return Math("acos");
        }

        /// <summary>
        /// Return the inverse tangent of an image in degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Atan()
        {
            return Math("atan");
        }

        /// <summary>
        /// Return the natural log of an image.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Log()
        {
            return Math("log");
        }

        /// <summary>
        /// Return the log base 10 of an image.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Log10()
        {
            return Math("log10");
        }

        /// <summary>
        /// Return e ** pixel.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Exp()
        {
            return Math("exp");
        }

        /// <summary>
        /// Return 10 ** pixel.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Exp10()
        {
            return Math("exp10");
        }

        /// <summary>
        /// Erode with a structuring element.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Erode(Image mask)
        {
            return Morph(mask, "erode");
        }

        /// <summary>
        /// Dilate with a structuring element.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Dilate(Image mask)
        {
            return Morph(mask, "dilate");
        }

        /// <summary>
        /// size x size median filter.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Median(int size)
        {
            return Rank(size, size, size * size / 2);
        }

        /// <summary>
        /// Flip horizontally.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image FlipHor()
        {
            return Flip(Enums.Direction.Horizontal);
        }

        /// <summary>
        /// Flip vertically.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image FlipVer()
        {
            return Flip(Enums.Direction.Vertical);
        }

        /// <summary>
        /// Rotate 90 degrees clockwise.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rot90()
        {
            return Rot(Enums.Angle.D90);
        }

        /// <summary>
        /// Rotate 180 degrees.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rot180()
        {
            return Rot(Enums.Angle.D180);
        }

        /// <summary>
        /// Rotate 270 degrees clockwise.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rot270()
        {
            return Rot(Enums.Angle.D270);
        }

        /// <summary>
        /// Return the largest integral value not greater than the argument.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Floor()
        {
            return this.Call("round", "floor") as Image;
        }

        /// <summary>
        /// Return the largest integral value not greater than the argument.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Ceil()
        {
            return this.Call("round", "ceil") as Image;
        }

        /// <summary>
        /// Return the nearest integral value.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rint()
        {
            return this.Call("round", "rint") as Image;
        }

        /// <summary>
        /// AND image bands together.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image BandAnd()
        {
            return this.Call("bandbool", "and") as Image;
        }

        /// <summary>
        /// OR image bands together.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image BandOr()
        {
            return this.Call("bandbool", "or") as Image;
        }

        /// <summary>
        /// EOR image bands together.
        /// </summary>
        /// <returns>A new <see cref="Image"/></returns>
        public Image BandEor()
        {
            return this.Call("bandbool", "eor") as Image;
        }

        /// <summary>
        /// Does this image have an alpha channel?
        /// </summary>
        /// <remarks>
        /// Uses colour space interpretation with number of channels to guess
        /// this.
        /// </remarks>
        /// <returns><see langword="true" /> if this image has an alpha channel; 
        /// otherwise, <see langword="false" /></returns>
        public bool HasAlpha()
        {
            return Bands == 2 ||
                   (Bands == 4 && Interpretation != Enums.Interpretation.Cmyk) ||
                   Bands > 4;
        }

        #endregion

        #region overloadable operators

        public static Image operator +(Image left, object right)
        {
            if (right is Image image)
            {
                return left.Call("add", image) as Image;
            }
            else
            {
                return left.Call("linear", 1, right) as Image;
            }
        }

        public static Image operator -(Image left, object right)
        {
            if (right is Image image)
            {
                return left.Call("subtract", image) as Image;
            }
            else
            {
                return left.Call("linear", 1, right.Smap<dynamic>(x => x * -1)) as Image;
            }
        }

        public static Image operator *(Image left, object right)
        {
            if (right is Image image)
            {
                return left.Call("multiply", image) as Image;
            }
            else
            {
                return left.Call("linear", right, 0) as Image;
            }
        }

        public static Image operator /(Image left, object right)
        {
            if (right is Image image)
            {
                return left.Call("divide", image) as Image;
            }
            else
            {
                return left.Call("linear", right.Smap<dynamic>(x => 1.0 / x), 0) as Image;
            }
        }

        public static Image operator %(Image left, object right)
        {
            if (right is Image image)
            {
                return left.Call("remainder", image) as Image;
            }
            else
            {
                return left.Call("remainder_const", right) as Image;
            }
        }

        public static Image operator &(Image left, object right)
        {
            return CallEnum(left, right, "boolean", "and") as Image;
        }

        public static Image operator |(Image left, object right)
        {
            return CallEnum(left, right, "boolean", "or") as Image;
        }

        public static Image operator ^(Image left, object right)
        {
            return CallEnum(left, right, "boolean", "eor") as Image;
        }

        public static Image operator <<(Image left, int right)
        {
            return CallEnum(left, right, "boolean", "lshift") as Image;
        }

        public static Image operator >>(Image left, int right)
        {
            return CallEnum(left, right, "boolean", "rshift") as Image;
        }

        public static object operator ==(Image left, object right)
        {
            // == version allows comparison to null
            if (right == null)
            {
                return false;
            }

            return CallEnum(left, right, "relational", "equal") as Image;
        }

        public static object operator !=(Image left, object right)
        {
            // == version allows comparison to null
            if (right == null)
            {
                return true;
            }

            return CallEnum(left, right, "relational", "noteq") as Image;
        }

        public static Image operator <(Image left, object right)
        {
            return CallEnum(left, right, "relational", "less") as Image;
        }

        public static Image operator >(Image left, object right)
        {
            return CallEnum(left, right, "relational", "more") as Image;
        }

        public static Image operator <=(Image left, object right)
        {
            return CallEnum(left, right, "relational", "lesseq") as Image;
        }

        public static Image operator >=(Image left, object right)
        {
            return CallEnum(left, right, "relational", "moreeq") as Image;
        }

        #endregion

        #region support with in the most trivial way

        /// <summary>
        /// Does band exist in image.
        /// </summary>
        /// <param name="i">The index to fetch.</param>
        /// <returns>true if the index exists</returns>
        public bool BandExists(int i)
        {
            return i >= 0 && i <= Bands - 1;
        }

        /// <summary>
        /// Overload []
        /// </summary>
        /// <remarks>
        /// Use [] to pull out band elements from an image. For example:
        ///
        ///     green = rgbImage[1]
        ///
        /// Will make a new one-band image from band 1 (the middle band).
        /// </remarks>
        /// <param name="i"></param>
        /// <returns>A new <see cref="Image"/></returns>
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
                    IntlImage = bandImage.IntlImage;
                    IntlVipsObject = bandImage.IntlVipsObject;
                    IntlGObject = bandImage.IntlGObject;
                }
            }
        }

        /// <summary>
        /// Split an n-band image into n separate images.
        /// </summary>
        /// <returns>An array of <see cref="Image"/></returns>
        public Image[] Bandsplit()
        {
            var images = new Image[Bands];
            for (var i = 0; i < Bands; i++)
            {
                images[i] = this[i];
            }

            return images;
        }

        public bool Equals(Image other)
        {
            return Equals(GetHashCode(), other.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Image) obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion
    }
}