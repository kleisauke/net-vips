using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NetVips.Internal;
using SMath = System.Math;
using NLog;

namespace NetVips
{
    /// <summary>
    /// Wrap a <see cref="NetVips.Internal.VipsImage"/> object.
    /// </summary>
    public class Image : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        public VipsImage IntlImage;

        /// <summary>
        /// Secret ref for NewFromMemory
        /// </summary>
#pragma warning disable 414
        private Array _data;
#pragma warning restore 414

        public Image(VipsImage vImage) : base(vImage.ParentInstance)
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
        /// <see cref="matchImage"></see> as a guide.
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
        /// <exception cref="T:System.Exception">If unable to load from <paramref name="vipsFilename" />.</exception>
        public static Image NewFromFile(string vipsFilename, bool? memory = null, string access = null,
            bool? fail = null, VOption kwargs = null)
        {
            var fileNamePtr = vipsFilename.ToUtf8Ptr();
            var filename = VipsImage.VipsFilenameGetFilename(fileNamePtr);
            var fileOptions = VipsImage.VipsFilenameGetOptions(fileNamePtr);

            var name = VipsForeign.VipsForeignFindLoad(filename.ToUtf8Ptr());
            if (name == null)
            {
                throw new Exception($"unable to load from file {vipsFilename}");
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

            return Operation.Call(name, options, filename) as Image;
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
        /// <exception cref="T:System.Exception">If unable to load from <paramref name="data" />.</exception>
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

            var name = VipsForeign.VipsForeignFindLoadBuffer(memory, (ulong) length);
            if (name == null)
            {
                throw new Exception("unable to load from buffer");
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
        /// <exception cref="T:System.Exception">If unable to make image from <paramref name="array" />.</exception>
        public static Image NewFromArray(object array, double scale = 1.0, double offset = 0.0)
        {
            if (!array.Is2D())
            {
                array = array as Array;
            }

            if (!(array is Array arr))
            {
                throw new Exception("can't create image from unknown object");
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
                throw new Exception("unable to make image from matrix");
            }

            var image = new Image(vi);
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
        /// <exception cref="T:System.Exception">If unable to make image from <paramref name="data" />.</exception>
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
                var pointer = handle.AddrOfPinnedObject();
                vi = VipsImage.VipsImageNewFromMemory(pointer, (ulong) data.Length,
                    width, height, bands, (Internal.Enums.VipsBandFormat) formatValue);
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
                throw new Exception("unable to make image from memory");
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
        /// <exception cref="T:System.Exception">If unable to make temp file from <paramref name="format" />.</exception>
        public static Image NewTempFile(string format)
        {
            var vi = VipsImage.VipsImageNewTempFile(format.ToUtf8Ptr());
            if (vi == null)
            {
                throw new Exception("unable to make temp file");
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
        /// <exception cref="T:System.Exception">If unable to copy to memory.</exception>
        public Image CopyMemory()
        {
            var vi = VipsImage.VipsImageCopyMemory(IntlImage);
            if (vi == null)
            {
                throw new Exception("unable to copy to memory");
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
        /// <exception cref="T:System.Exception">If unable to write to <paramref name="vipsFilename" />.</exception>
        public void WriteToFile(string vipsFilename, VOption kwargs = null)
        {
            var fileNamePtr = vipsFilename.ToUtf8Ptr();
            var filename = VipsImage.VipsFilenameGetFilename(fileNamePtr);
            var options = VipsImage.VipsFilenameGetOptions(fileNamePtr);

            var name = VipsForeign.VipsForeignFindSave(filename.ToUtf8Ptr());
            if (name == null)
            {
                throw new Exception($"unable to write to file {vipsFilename}");
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

            Operation.Call(name, kwargs, this, filename);
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
        /// <exception cref="T:System.Exception">If unable to write to buffer.</exception>
        public byte[] WriteToBuffer(string formatString, VOption kwargs = null)
        {
            var formatStrPtr = formatString.ToUtf8Ptr();
            var options = VipsImage.VipsFilenameGetOptions(formatStrPtr);

            var name = VipsForeign.VipsForeignFindSaveBuffer(formatString);
            if (name == null)
            {
                throw new Exception("unable to write to buffer");
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
        /// <exception cref="T:System.Exception">If unable to write to image.</exception>
        public void Write(Image other)
        {
            var result = VipsImage.VipsImageWrite(IntlImage, other.IntlImage);
            if (result != 0)
            {
                throw new Exception("unable to write to image");
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
        /// <exception cref="T:System.Exception">If unable to get <paramref name="name" />.</exception>
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
                throw new Exception($"unable to get {name}");
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

        // TODO Should we define these in a separate file?

        #region auto-generated functions

        /// <summary>
        /// Absolute value of an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Abs();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Abs()
        {
            return this.Call("abs") as Image;
        }

        /// <summary>
        /// Add two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Add(right);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Add(Image right)
        {
            return this.Call("add", right) as Image;
        }

        /// <summary>
        /// Affine transform of an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Affine(matrix, interpolate: GObject, oarea: int[], odx: double, ody: double, idx: double, idy: double, background: double[], extend: string);
        /// </code>
        /// </example>
        /// <param name="matrix">Transformation matrix</param>
        /// <param name="interpolate">Interpolate pixels with this</param>
        /// <param name="oarea">Area of output to generate</param>
        /// <param name="odx">Horizontal output displacement</param>
        /// <param name="ody">Vertical output displacement</param>
        /// <param name="idx">Horizontal input displacement</param>
        /// <param name="idy">Vertical input displacement</param>
        /// <param name="background">Background value</param>
        /// <param name="extend">How to generate the extra pixels</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Affine(double[] matrix, GObject interpolate = null, int[] oarea = null, double? odx = null,
            double? ody = null, double? idx = null, double? idy = null, double[] background = null,
            string extend = null)
        {
            var options = new VOption();

            if (interpolate != null)
            {
                options.Add("interpolate", interpolate);
            }

            if (oarea != null && oarea.Length > 0)
            {
                options.Add("oarea", oarea);
            }

            if (odx.HasValue)
            {
                options.Add("odx", odx);
            }

            if (ody.HasValue)
            {
                options.Add("ody", ody);
            }

            if (idx.HasValue)
            {
                options.Add("idx", idx);
            }

            if (idy.HasValue)
            {
                options.Add("idy", idy);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            if (extend != null)
            {
                options.Add("extend", extend);
            }

            return this.Call("affine", options, matrix) as Image;
        }

        /// <summary>
        /// Load an Analyze6 image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Analyzeload(filename, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Analyzeload(string filename, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

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

            return Operation.Call("analyzeload", options, filename) as Image;
        }

        /// <summary>
        /// Load an Analyze6 image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Analyzeload(filename, out var flags, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Analyzeload(string filename, out int flags, bool? memory = null, string access = null,
            bool? fail = null)
        {
            var options = new VOption();

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

            options.Add("flags", true);

            var results = Operation.Call("analyzeload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Join an array of images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Arrayjoin(@in, across: int, shim: int, background: double[], halign: string, valign: string, hspacing: int, vspacing: int);
        /// </code>
        /// </example>
        /// <param name="in">Array of input images</param>
        /// <param name="across">Number of images across grid</param>
        /// <param name="shim">Pixels between images</param>
        /// <param name="background">Colour for new pixels</param>
        /// <param name="halign">Align on the left, centre or right</param>
        /// <param name="valign">Align on the top, centre or bottom</param>
        /// <param name="hspacing">Horizontal spacing between images</param>
        /// <param name="vspacing">Vertical spacing between images</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Arrayjoin(Image[] @in, int? across = null, int? shim = null, double[] background = null,
            string halign = null, string valign = null, int? hspacing = null, int? vspacing = null)
        {
            var options = new VOption();

            if (across.HasValue)
            {
                options.Add("across", across);
            }

            if (shim.HasValue)
            {
                options.Add("shim", shim);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            if (halign != null)
            {
                options.Add("halign", halign);
            }

            if (valign != null)
            {
                options.Add("valign", valign);
            }

            if (hspacing.HasValue)
            {
                options.Add("hspacing", hspacing);
            }

            if (vspacing.HasValue)
            {
                options.Add("vspacing", vspacing);
            }

            return Operation.Call("arrayjoin", options, new object[] {@in}) as Image;
        }

        /// <summary>
        /// Autorotate image by exif tag
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Autorot();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Autorot()
        {
            return this.Call("autorot") as Image;
        }

        /// <summary>
        /// Autorotate image by exif tag
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Autorot(out var angle);
        /// </code>
        /// </example>
        /// <param name="angle">Angle image was rotated by</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Autorot(out string angle)
        {
            var optionalOutput = new VOption
            {
                {"angle", true}
            };

            var results = this.Call("autorot", optionalOutput) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            angle = opts?["angle"] is string out1 ? out1 : null;

            return finalResult;
        }

        /// <summary>
        /// Find image average
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Avg();
        /// </code>
        /// </example>
        /// <returns>A double</returns>
        public double Avg()
        {
            return this.Call("avg") is double result ? result : 0;
        }

        /// <summary>
        /// Boolean operation across image bands
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Bandbool(boolean);
        /// </code>
        /// </example>
        /// <param name="boolean">boolean to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandbool(string boolean)
        {
            return this.Call("bandbool", boolean) as Image;
        }

        /// <summary>
        /// Fold up x axis into bands
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Bandfold(factor: int);
        /// </code>
        /// </example>
        /// <param name="factor">Fold by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandfold(int? factor = null)
        {
            var options = new VOption();

            if (factor.HasValue)
            {
                options.Add("factor", factor);
            }

            return this.Call("bandfold", options) as Image;
        }

        /// <summary>
        /// Append a constant band to an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.BandjoinConst(c);
        /// </code>
        /// </example>
        /// <param name="c">Array of constants to add</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image BandjoinConst(double[] c)
        {
            return this.Call("bandjoin_const", c) as Image;
        }

        /// <summary>
        /// Band-wise average
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Bandmean();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandmean()
        {
            return this.Call("bandmean") as Image;
        }

        /// <summary>
        /// Unfold image bands into x axis
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Bandunfold(factor: int);
        /// </code>
        /// </example>
        /// <param name="factor">Unfold by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandunfold(int? factor = null)
        {
            var options = new VOption();

            if (factor.HasValue)
            {
                options.Add("factor", factor);
            }

            return this.Call("bandunfold", options) as Image;
        }

        /// <summary>
        /// Make a black image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Black(width, height, bands: int);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="bands">Number of bands in image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Black(int width, int height, int? bands = null)
        {
            var options = new VOption();

            if (bands.HasValue)
            {
                options.Add("bands", bands);
            }

            return Operation.Call("black", options, width, height) as Image;
        }

        /// <summary>
        /// Boolean operation on two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Boolean(right, boolean);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <param name="boolean">boolean to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Boolean(Image right, string boolean)
        {
            return this.Call("boolean", right, boolean) as Image;
        }

        /// <summary>
        /// Boolean operations against a constant
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.BooleanConst(boolean, c);
        /// </code>
        /// </example>
        /// <param name="boolean">boolean to perform</param>
        /// <param name="c">Array of constants</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image BooleanConst(string boolean, double[] c)
        {
            return this.Call("boolean_const", boolean, c) as Image;
        }

        /// <summary>
        /// Build a look-up table
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Buildlut();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Buildlut()
        {
            return this.Call("buildlut") as Image;
        }

        /// <summary>
        /// Byteswap an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Byteswap();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Byteswap()
        {
            return this.Call("byteswap") as Image;
        }

        /// <summary>
        /// Cache an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Cache(maxTiles: int, tileHeight: int, tileWidth: int);
        /// </code>
        /// </example>
        /// <param name="maxTiles">Maximum number of tiles to cache</param>
        /// <param name="tileHeight">Tile height in pixels</param>
        /// <param name="tileWidth">Tile width in pixels</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Cache(int? maxTiles = null, int? tileHeight = null, int? tileWidth = null)
        {
            var options = new VOption();

            if (maxTiles.HasValue)
            {
                options.Add("max_tiles", maxTiles);
            }

            if (tileHeight.HasValue)
            {
                options.Add("tile_height", tileHeight);
            }

            if (tileWidth.HasValue)
            {
                options.Add("tile_width", tileWidth);
            }

            return this.Call("cache", options) as Image;
        }

        /// <summary>
        /// Cast an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Cast(format, shift: bool);
        /// </code>
        /// </example>
        /// <param name="format">Format to cast to</param>
        /// <param name="shift">Shift integer values up and down</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Cast(string format, bool? shift = null)
        {
            var options = new VOption();

            if (shift.HasValue)
            {
                options.Add("shift", shift);
            }

            return this.Call("cast", options, format) as Image;
        }

        /// <summary>
        /// Transform LCh to CMC
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.CMC2LCh();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image CMC2LCh()
        {
            return this.Call("CMC2LCh") as Image;
        }

        /// <summary>
        /// Convert to a new colorspace
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Colourspace(space, sourceSpace: string);
        /// </code>
        /// </example>
        /// <param name="space">Destination color space</param>
        /// <param name="sourceSpace">Source color space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Colourspace(string space, string sourceSpace = null)
        {
            var options = new VOption();

            if (sourceSpace != null)
            {
                options.Add("source_space", sourceSpace);
            }

            return this.Call("colourspace", options, space) as Image;
        }

        /// <summary>
        /// Convolve with rotating mask
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Compass(mask, times: int, angle: string, combine: string, precision: string, layers: int, cluster: int);
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="times">Rotate and convolve this many times</param>
        /// <param name="angle">Rotate mask by this much between convolutions</param>
        /// <param name="combine">Combine convolution results like this</param>
        /// <param name="precision">Convolve with this precision</param>
        /// <param name="layers">Use this many layers in approximation</param>
        /// <param name="cluster">Cluster lines closer than this in approximation</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Compass(Image mask, int? times = null, string angle = null, string combine = null,
            string precision = null, int? layers = null, int? cluster = null)
        {
            var options = new VOption();

            if (times.HasValue)
            {
                options.Add("times", times);
            }

            if (angle != null)
            {
                options.Add("angle", angle);
            }

            if (combine != null)
            {
                options.Add("combine", combine);
            }

            if (precision != null)
            {
                options.Add("precision", precision);
            }

            if (layers.HasValue)
            {
                options.Add("layers", layers);
            }

            if (cluster.HasValue)
            {
                options.Add("cluster", cluster);
            }

            return this.Call("compass", options, mask) as Image;
        }

        /// <summary>
        /// Perform a complex operation on an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Complex(cmplx);
        /// </code>
        /// </example>
        /// <param name="cmplx">complex to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Complex(string cmplx)
        {
            return this.Call("complex", cmplx) as Image;
        }

        /// <summary>
        /// Complex binary operations on two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Complex2(right, cmplx);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <param name="cmplx">binary complex operation to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Complex2(Image right, string cmplx)
        {
            return this.Call("complex2", right, cmplx) as Image;
        }

        /// <summary>
        /// Form a complex image from two real images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Complexform(right);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Complexform(Image right)
        {
            return this.Call("complexform", right) as Image;
        }

        /// <summary>
        /// Get a component from a complex image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Complexget(get);
        /// </code>
        /// </example>
        /// <param name="get">complex to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Complexget(string get)
        {
            return this.Call("complexget", get) as Image;
        }

        /// <summary>
        /// Blend a pair of images with a blend mode
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = base.Composite2(overlay, mode, compositingSpace: string, premultiplied: bool);
        /// </code>
        /// </example>
        /// <param name="overlay">Overlay image</param>
        /// <param name="mode">VipsBlendMode to join with</param>
        /// <param name="compositingSpace">Composite images in this colour space</param>
        /// <param name="premultiplied">Images have premultiplied alpha</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Composite2(Image overlay, string mode, string compositingSpace = null, bool? premultiplied = null)
        {
            var options = new VOption();

            if (compositingSpace != null)
            {
                options.Add("compositing_space", compositingSpace);
            }

            if (premultiplied.HasValue)
            {
                options.Add("premultiplied", premultiplied);
            }

            return this.Call("composite2", options, overlay, mode) as Image;
        }

        /// <summary>
        /// Convolution operation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Conv(mask, precision: string, layers: int, cluster: int);
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="precision">Convolve with this precision</param>
        /// <param name="layers">Use this many layers in approximation</param>
        /// <param name="cluster">Cluster lines closer than this in approximation</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Conv(Image mask, string precision = null, int? layers = null, int? cluster = null)
        {
            var options = new VOption();

            if (precision != null)
            {
                options.Add("precision", precision);
            }

            if (layers.HasValue)
            {
                options.Add("layers", layers);
            }

            if (cluster.HasValue)
            {
                options.Add("cluster", cluster);
            }

            return this.Call("conv", options, mask) as Image;
        }

        /// <summary>
        /// Approximate integer convolution
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Conva(mask, layers: int, cluster: int);
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="layers">Use this many layers in approximation</param>
        /// <param name="cluster">Cluster lines closer than this in approximation</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Conva(Image mask, int? layers = null, int? cluster = null)
        {
            var options = new VOption();

            if (layers.HasValue)
            {
                options.Add("layers", layers);
            }

            if (cluster.HasValue)
            {
                options.Add("cluster", cluster);
            }

            return this.Call("conva", options, mask) as Image;
        }

        /// <summary>
        /// Approximate separable integer convolution
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Convasep(mask, layers: int);
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="layers">Use this many layers in approximation</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Convasep(Image mask, int? layers = null)
        {
            var options = new VOption();

            if (layers.HasValue)
            {
                options.Add("layers", layers);
            }

            return this.Call("convasep", options, mask) as Image;
        }

        /// <summary>
        /// Float convolution operation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Convf(mask);
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Convf(Image mask)
        {
            return this.Call("convf", mask) as Image;
        }

        /// <summary>
        /// Int convolution operation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Convi(mask);
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Convi(Image mask)
        {
            return this.Call("convi", mask) as Image;
        }

        /// <summary>
        /// Seperable convolution operation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Convsep(mask, precision: string, layers: int, cluster: int);
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="precision">Convolve with this precision</param>
        /// <param name="layers">Use this many layers in approximation</param>
        /// <param name="cluster">Cluster lines closer than this in approximation</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Convsep(Image mask, string precision = null, int? layers = null, int? cluster = null)
        {
            var options = new VOption();

            if (precision != null)
            {
                options.Add("precision", precision);
            }

            if (layers.HasValue)
            {
                options.Add("layers", layers);
            }

            if (cluster.HasValue)
            {
                options.Add("cluster", cluster);
            }

            return this.Call("convsep", options, mask) as Image;
        }

        /// <summary>
        /// Copy an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Copy(width: int, height: int, bands: int, format: string, coding: string, interpretation: string, xres: double, yres: double, xoffset: int, yoffset: int);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="bands">Number of bands in image</param>
        /// <param name="format">Pixel format in image</param>
        /// <param name="coding">Pixel coding</param>
        /// <param name="interpretation">Pixel interpretation</param>
        /// <param name="xres">Horizontal resolution in pixels/mm</param>
        /// <param name="yres">Vertical resolution in pixels/mm</param>
        /// <param name="xoffset">Horizontal offset of origin</param>
        /// <param name="yoffset">Vertical offset of origin</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Copy(int? width = null, int? height = null, int? bands = null, string format = null,
            string coding = null, string interpretation = null, double? xres = null, double? yres = null,
            int? xoffset = null, int? yoffset = null)
        {
            var options = new VOption();

            if (width.HasValue)
            {
                options.Add("width", width);
            }

            if (height.HasValue)
            {
                options.Add("height", height);
            }

            if (bands.HasValue)
            {
                options.Add("bands", bands);
            }

            if (format != null)
            {
                options.Add("format", format);
            }

            if (coding != null)
            {
                options.Add("coding", coding);
            }

            if (interpretation != null)
            {
                options.Add("interpretation", interpretation);
            }

            if (xres.HasValue)
            {
                options.Add("xres", xres);
            }

            if (yres.HasValue)
            {
                options.Add("yres", yres);
            }

            if (xoffset.HasValue)
            {
                options.Add("xoffset", xoffset);
            }

            if (yoffset.HasValue)
            {
                options.Add("yoffset", yoffset);
            }

            return this.Call("copy", options) as Image;
        }

        /// <summary>
        /// Count lines in an image
        /// </summary>
        /// <example>
        /// <code>
        /// double nolines = in.Countlines(direction);
        /// </code>
        /// </example>
        /// <param name="direction">Countlines left-right or up-down</param>
        /// <returns>A double</returns>
        public double Countlines(string direction)
        {
            return this.Call("countlines", direction) is double result ? result : 0;
        }

        /// <summary>
        /// Load csv from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Csvload(filename, memory: bool, access: string, skip: int, lines: int, fail: bool, whitespace: string, separator: string);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="skip">Skip this many lines at the start of the file</param>
        /// <param name="lines">Read this many lines from the file</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="whitespace">Set of whitespace characters</param>
        /// <param name="separator">Set of separator characters</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Csvload(string filename, bool? memory = null, string access = null, int? skip = null,
            int? lines = null, bool? fail = null, string whitespace = null, string separator = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (skip.HasValue)
            {
                options.Add("skip", skip);
            }

            if (lines.HasValue)
            {
                options.Add("lines", lines);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (whitespace != null)
            {
                options.Add("whitespace", whitespace);
            }

            if (separator != null)
            {
                options.Add("separator", separator);
            }

            return Operation.Call("csvload", options, filename) as Image;
        }

        /// <summary>
        /// Load csv from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Csvload(filename, out var flags, memory: bool, access: string, skip: int, lines: int, fail: bool, whitespace: string, separator: string);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="skip">Skip this many lines at the start of the file</param>
        /// <param name="lines">Read this many lines from the file</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="whitespace">Set of whitespace characters</param>
        /// <param name="separator">Set of separator characters</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Csvload(string filename, out int flags, bool? memory = null, string access = null,
            int? skip = null, int? lines = null, bool? fail = null, string whitespace = null, string separator = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (skip.HasValue)
            {
                options.Add("skip", skip);
            }

            if (lines.HasValue)
            {
                options.Add("lines", lines);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (whitespace != null)
            {
                options.Add("whitespace", whitespace);
            }

            if (separator != null)
            {
                options.Add("separator", separator);
            }

            options.Add("flags", true);

            var results = Operation.Call("csvload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Save image to csv file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Csvsave(filename, pageHeight: int, separator: string, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="separator">Separator characters</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Csvsave(string filename, int? pageHeight = null, string separator = null, bool? strip = null,
            double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (separator != null)
            {
                options.Add("separator", separator);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("csvsave", options, filename);
        }

        /// <summary>
        /// Calculate dE00
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.DE00(right);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand input image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DE00(Image right)
        {
            return this.Call("dE00", right) as Image;
        }

        /// <summary>
        /// Calculate dE76
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.DE76(right);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand input image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DE76(Image right)
        {
            return this.Call("dE76", right) as Image;
        }

        /// <summary>
        /// Calculate dECMC
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.DECMC(right);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand input image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DECMC(Image right)
        {
            return this.Call("dECMC", right) as Image;
        }

        /// <summary>
        /// Find image standard deviation
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Deviate();
        /// </code>
        /// </example>
        /// <returns>A double</returns>
        public double Deviate()
        {
            return this.Call("deviate") is double result ? result : 0;
        }

        /// <summary>
        /// Divide two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Divide(right);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Divide(Image right)
        {
            return this.Call("divide", right) as Image;
        }

        /// <summary>
        /// Draw a circle on an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawCircle(ink, cx, cy, radius, fill: bool);
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="cx">Centre of draw_circle</param>
        /// <param name="cy">Centre of draw_circle</param>
        /// <param name="radius">Radius in pixels</param>
        /// <param name="fill">Draw a solid object</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawCircle(double[] ink, int cx, int cy, int radius, bool? fill = null)
        {
            var options = new VOption();

            if (fill.HasValue)
            {
                options.Add("fill", fill);
            }

            return this.Call("draw_circle", options, ink, cx, cy, radius) as Image;
        }

        /// <summary>
        /// Flood-fill an area
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawFlood(ink, x, y, test: Image, equal: bool);
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="x">DrawFlood start point</param>
        /// <param name="y">DrawFlood start point</param>
        /// <param name="test">Test pixels in this image</param>
        /// <param name="equal">DrawFlood while equal to edge</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawFlood(double[] ink, int x, int y, Image test = null, bool? equal = null)
        {
            var options = new VOption();

            if (!(test is null))
            {
                options.Add("test", test);
            }

            if (equal.HasValue)
            {
                options.Add("equal", equal);
            }

            return this.Call("draw_flood", options, ink, x, y) as Image;
        }

        /// <summary>
        /// Flood-fill an area
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawFlood(ink, x, y, out var left, test: Image, equal: bool);
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="x">DrawFlood start point</param>
        /// <param name="y">DrawFlood start point</param>
        /// <param name="left">Left edge of modified area</param>
        /// <param name="test">Test pixels in this image</param>
        /// <param name="equal">DrawFlood while equal to edge</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawFlood(double[] ink, int x, int y, out int left, Image test = null, bool? equal = null)
        {
            var options = new VOption();

            if (!(test is null))
            {
                options.Add("test", test);
            }

            if (equal.HasValue)
            {
                options.Add("equal", equal);
            }

            options.Add("left", true);

            var results = this.Call("draw_flood", options, ink, x, y) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            left = opts?["left"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Flood-fill an area
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawFlood(ink, x, y, out var left, out var top, test: Image, equal: bool);
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="x">DrawFlood start point</param>
        /// <param name="y">DrawFlood start point</param>
        /// <param name="left">Left edge of modified area</param>
        /// <param name="top">top edge of modified area</param>
        /// <param name="test">Test pixels in this image</param>
        /// <param name="equal">DrawFlood while equal to edge</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawFlood(double[] ink, int x, int y, out int left, out int top, Image test = null,
            bool? equal = null)
        {
            var options = new VOption();

            if (!(test is null))
            {
                options.Add("test", test);
            }

            if (equal.HasValue)
            {
                options.Add("equal", equal);
            }

            options.Add("left", true);
            options.Add("top", true);

            var results = this.Call("draw_flood", options, ink, x, y) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            left = opts?["left"] is int out1 ? out1 : 0;
            top = opts?["top"] is int out2 ? out2 : 0;

            return finalResult;
        }

        /// <summary>
        /// Flood-fill an area
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawFlood(ink, x, y, out var left, out var top, out var width, test: Image, equal: bool);
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="x">DrawFlood start point</param>
        /// <param name="y">DrawFlood start point</param>
        /// <param name="left">Left edge of modified area</param>
        /// <param name="top">top edge of modified area</param>
        /// <param name="width">width of modified area</param>
        /// <param name="test">Test pixels in this image</param>
        /// <param name="equal">DrawFlood while equal to edge</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawFlood(double[] ink, int x, int y, out int left, out int top, out int width, Image test = null,
            bool? equal = null)
        {
            var options = new VOption();

            if (!(test is null))
            {
                options.Add("test", test);
            }

            if (equal.HasValue)
            {
                options.Add("equal", equal);
            }

            options.Add("left", true);
            options.Add("top", true);
            options.Add("width", true);

            var results = this.Call("draw_flood", options, ink, x, y) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            left = opts?["left"] is int out1 ? out1 : 0;
            top = opts?["top"] is int out2 ? out2 : 0;
            width = opts?["width"] is int out3 ? out3 : 0;

            return finalResult;
        }

        /// <summary>
        /// Flood-fill an area
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawFlood(ink, x, y, out var left, out var top, out var width, out var height, test: Image, equal: bool);
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="x">DrawFlood start point</param>
        /// <param name="y">DrawFlood start point</param>
        /// <param name="left">Left edge of modified area</param>
        /// <param name="top">top edge of modified area</param>
        /// <param name="width">width of modified area</param>
        /// <param name="height">height of modified area</param>
        /// <param name="test">Test pixels in this image</param>
        /// <param name="equal">DrawFlood while equal to edge</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawFlood(double[] ink, int x, int y, out int left, out int top, out int width, out int height,
            Image test = null, bool? equal = null)
        {
            var options = new VOption();

            if (!(test is null))
            {
                options.Add("test", test);
            }

            if (equal.HasValue)
            {
                options.Add("equal", equal);
            }

            options.Add("left", true);
            options.Add("top", true);
            options.Add("width", true);
            options.Add("height", true);

            var results = this.Call("draw_flood", options, ink, x, y) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            left = opts?["left"] is int out1 ? out1 : 0;
            top = opts?["top"] is int out2 ? out2 : 0;
            width = opts?["width"] is int out3 ? out3 : 0;
            height = opts?["height"] is int out4 ? out4 : 0;

            return finalResult;
        }

        /// <summary>
        /// Paint an image into another image
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawImage(sub, x, y, mode: string);
        /// </code>
        /// </example>
        /// <param name="sub">Sub-image to insert into main image</param>
        /// <param name="x">Draw image here</param>
        /// <param name="y">Draw image here</param>
        /// <param name="mode">Combining mode</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawImage(Image sub, int x, int y, string mode = null)
        {
            var options = new VOption();

            if (mode != null)
            {
                options.Add("mode", mode);
            }

            return this.Call("draw_image", options, sub, x, y) as Image;
        }

        /// <summary>
        /// Draw a line on an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawLine(ink, x1, y1, x2, y2);
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="x1">Start of draw_line</param>
        /// <param name="y1">Start of draw_line</param>
        /// <param name="x2">End of draw_line</param>
        /// <param name="y2">End of draw_line</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawLine(double[] ink, int x1, int y1, int x2, int y2)
        {
            return this.Call("draw_line", ink, x1, y1, x2, y2) as Image;
        }

        /// <summary>
        /// Draw a mask on an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawMask(ink, mask, x, y);
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="mask">Mask of pixels to draw</param>
        /// <param name="x">Draw mask here</param>
        /// <param name="y">Draw mask here</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawMask(double[] ink, Image mask, int x, int y)
        {
            return this.Call("draw_mask", ink, mask, x, y) as Image;
        }

        /// <summary>
        /// Paint a rectangle on an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawRect(ink, left, top, width, height, fill: bool);
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="left">Rect to fill</param>
        /// <param name="top">Rect to fill</param>
        /// <param name="width">Rect to fill</param>
        /// <param name="height">Rect to fill</param>
        /// <param name="fill">Draw a solid object</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawRect(double[] ink, int left, int top, int width, int height, bool? fill = null)
        {
            var options = new VOption();

            if (fill.HasValue)
            {
                options.Add("fill", fill);
            }

            return this.Call("draw_rect", options, ink, left, top, width, height) as Image;
        }

        /// <summary>
        /// Blur a rectangle on an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image image = image.DrawSmudge(left, top, width, height);
        /// </code>
        /// </example>
        /// <param name="left">Rect to fill</param>
        /// <param name="top">Rect to fill</param>
        /// <param name="width">Rect to fill</param>
        /// <param name="height">Rect to fill</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawSmudge(int left, int top, int width, int height)
        {
            return this.Call("draw_smudge", left, top, width, height) as Image;
        }

        /// <summary>
        /// Save image to deepzoom file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Dzsave(filename, basename: string, layout: string, pageHeight: int, suffix: string, overlap: int, tileSize: int, centre: bool, depth: string, angle: string, container: string, properties: bool, compression: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="basename">Base name to save to</param>
        /// <param name="layout">Directory layout</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="suffix">Filename suffix for tiles</param>
        /// <param name="overlap">Tile overlap in pixels</param>
        /// <param name="tileSize">Tile size in pixels</param>
        /// <param name="centre">Center image in tile</param>
        /// <param name="depth">Pyramid depth</param>
        /// <param name="angle">Rotate image during save</param>
        /// <param name="container">Pyramid container type</param>
        /// <param name="properties">Write a properties file to the output directory</param>
        /// <param name="compression">ZIP deflate compression level</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Dzsave(string filename, string basename = null, string layout = null, int? pageHeight = null,
            string suffix = null, int? overlap = null, int? tileSize = null, bool? centre = null, string depth = null,
            string angle = null, string container = null, bool? properties = null, int? compression = null,
            bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (basename != null)
            {
                options.Add("basename", basename);
            }

            if (layout != null)
            {
                options.Add("layout", layout);
            }

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (suffix != null)
            {
                options.Add("suffix", suffix);
            }

            if (overlap.HasValue)
            {
                options.Add("overlap", overlap);
            }

            if (tileSize.HasValue)
            {
                options.Add("tile_size", tileSize);
            }

            if (centre.HasValue)
            {
                options.Add("centre", centre);
            }

            if (depth != null)
            {
                options.Add("depth", depth);
            }

            if (angle != null)
            {
                options.Add("angle", angle);
            }

            if (container != null)
            {
                options.Add("container", container);
            }

            if (properties.HasValue)
            {
                options.Add("properties", properties);
            }

            if (compression.HasValue)
            {
                options.Add("compression", compression);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("dzsave", options, filename);
        }

        /// <summary>
        /// Save image to dz buffer
        /// </summary>
        /// <example>
        /// <code>
        /// byte[] buffer = in.DzsaveBuffer(basename: string, layout: string, pageHeight: int, suffix: string, overlap: int, tileSize: int, centre: bool, depth: string, angle: string, container: string, properties: bool, compression: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="basename">Base name to save to</param>
        /// <param name="layout">Directory layout</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="suffix">Filename suffix for tiles</param>
        /// <param name="overlap">Tile overlap in pixels</param>
        /// <param name="tileSize">Tile size in pixels</param>
        /// <param name="centre">Center image in tile</param>
        /// <param name="depth">Pyramid depth</param>
        /// <param name="angle">Rotate image during save</param>
        /// <param name="container">Pyramid container type</param>
        /// <param name="properties">Write a properties file to the output directory</param>
        /// <param name="compression">ZIP deflate compression level</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>An array of bytes</returns>
        public byte[] DzsaveBuffer(string basename = null, string layout = null, int? pageHeight = null,
            string suffix = null, int? overlap = null, int? tileSize = null, bool? centre = null, string depth = null,
            string angle = null, string container = null, bool? properties = null, int? compression = null,
            bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (basename != null)
            {
                options.Add("basename", basename);
            }

            if (layout != null)
            {
                options.Add("layout", layout);
            }

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (suffix != null)
            {
                options.Add("suffix", suffix);
            }

            if (overlap.HasValue)
            {
                options.Add("overlap", overlap);
            }

            if (tileSize.HasValue)
            {
                options.Add("tile_size", tileSize);
            }

            if (centre.HasValue)
            {
                options.Add("centre", centre);
            }

            if (depth != null)
            {
                options.Add("depth", depth);
            }

            if (angle != null)
            {
                options.Add("angle", angle);
            }

            if (container != null)
            {
                options.Add("container", container);
            }

            if (properties.HasValue)
            {
                options.Add("properties", properties);
            }

            if (compression.HasValue)
            {
                options.Add("compression", compression);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("dzsave_buffer", options) as byte[];
        }

        /// <summary>
        /// Embed an image in a larger image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Embed(x, y, width, height, extend: string, background: double[]);
        /// </code>
        /// </example>
        /// <param name="x">Left edge of input in output</param>
        /// <param name="y">Top edge of input in output</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="extend">How to generate the extra pixels</param>
        /// <param name="background">Color for background pixels</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Embed(int x, int y, int width, int height, string extend = null, double[] background = null)
        {
            var options = new VOption();

            if (extend != null)
            {
                options.Add("extend", extend);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("embed", options, x, y, width, height) as Image;
        }

        /// <summary>
        /// Extract an area from an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = input.ExtractArea(left, top, width, height);
        /// </code>
        /// </example>
        /// <param name="left">Left edge of extract area</param>
        /// <param name="top">Top edge of extract area</param>
        /// <param name="width">Width of extract area</param>
        /// <param name="height">Height of extract area</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ExtractArea(int left, int top, int width, int height)
        {
            return this.Call("extract_area", left, top, width, height) as Image;
        }

        /// <summary>
        /// Extract band from an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.ExtractBand(band, n: int);
        /// </code>
        /// </example>
        /// <param name="band">Band to extract</param>
        /// <param name="n">Number of bands to extract</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ExtractBand(int band, int? n = null)
        {
            var options = new VOption();

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            return this.Call("extract_band", options, band) as Image;
        }

        /// <summary>
        /// Make an image showing the eye's spatial response
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Eye(width, height, uchar: bool, factor: double);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="factor">Maximum spatial frequency</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Eye(int width, int height, bool? uchar = null, double? factor = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (factor.HasValue)
            {
                options.Add("factor", factor);
            }

            return Operation.Call("eye", options, width, height) as Image;
        }

        /// <summary>
        /// False-color an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Falsecolour();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Falsecolour()
        {
            return this.Call("falsecolour") as Image;
        }

        /// <summary>
        /// Fast correlation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Fastcor(@ref);
        /// </code>
        /// </example>
        /// <param name="ref">Input reference image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Fastcor(Image @ref)
        {
            return this.Call("fastcor", @ref) as Image;
        }

        /// <summary>
        /// Fill image zeros with nearest non-zero pixel
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.FillNearest();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image FillNearest()
        {
            return this.Call("fill_nearest") as Image;
        }

        /// <summary>
        /// Fill image zeros with nearest non-zero pixel
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.FillNearest(out var distance);
        /// </code>
        /// </example>
        /// <param name="distance">Distance to nearest non-zero pixel</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image FillNearest(out Image distance)
        {
            var optionalOutput = new VOption
            {
                {"distance", true}
            };

            var results = this.Call("fill_nearest", optionalOutput) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            distance = opts?["distance"] as Image;

            return finalResult;
        }

        /// <summary>
        /// Search an image for non-edge areas
        /// </summary>
        /// <example>
        /// <code>
        /// var output = in.FindTrim(threshold: double, background: double[]);
        /// </code>
        /// </example>
        /// <param name="threshold">Object threshold</param>
        /// <param name="background">Color for background pixels</param>
        /// <returns>An array of objects</returns>
        public object[] FindTrim(double? threshold = null, double[] background = null)
        {
            var options = new VOption();

            if (threshold.HasValue)
            {
                options.Add("threshold", threshold);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("find_trim", options) as object[];
        }

        /// <summary>
        /// Flatten alpha out of an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Flatten(background: double[], maxAlpha: double);
        /// </code>
        /// </example>
        /// <param name="background">Background value</param>
        /// <param name="maxAlpha">Maximum value of alpha channel</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Flatten(double[] background = null, double? maxAlpha = null)
        {
            var options = new VOption();

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            if (maxAlpha.HasValue)
            {
                options.Add("max_alpha", maxAlpha);
            }

            return this.Call("flatten", options) as Image;
        }

        /// <summary>
        /// Flip an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Flip(direction);
        /// </code>
        /// </example>
        /// <param name="direction">Direction to flip image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Flip(string direction)
        {
            return this.Call("flip", direction) as Image;
        }

        /// <summary>
        /// Transform float RGB to Radiance coding
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Float2rad();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Float2rad()
        {
            return this.Call("float2rad") as Image;
        }

        /// <summary>
        /// Make a fractal surface
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Fractsurf(width, height, fractalDimension);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="fractalDimension">Fractal dimension</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Fractsurf(int width, int height, double fractalDimension)
        {
            return Operation.Call("fractsurf", width, height, fractalDimension) as Image;
        }

        /// <summary>
        /// Frequency-domain filtering
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Freqmult(mask);
        /// </code>
        /// </example>
        /// <param name="mask">Input mask image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Freqmult(Image mask)
        {
            return this.Call("freqmult", mask) as Image;
        }

        /// <summary>
        /// Forward FFT
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Fwfft();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Fwfft()
        {
            return this.Call("fwfft") as Image;
        }

        /// <summary>
        /// Gamma an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Gamma(exponent: double);
        /// </code>
        /// </example>
        /// <param name="exponent">Gamma factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Gamma(double? exponent = null)
        {
            var options = new VOption();

            if (exponent.HasValue)
            {
                options.Add("exponent", exponent);
            }

            return this.Call("gamma", options) as Image;
        }

        /// <summary>
        /// Gaussian blur
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Gaussblur(sigma, minAmpl: double, precision: string);
        /// </code>
        /// </example>
        /// <param name="sigma">Sigma of Gaussian</param>
        /// <param name="minAmpl">Minimum amplitude of Gaussian</param>
        /// <param name="precision">Convolve with this precision</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Gaussblur(double sigma, double? minAmpl = null, string precision = null)
        {
            var options = new VOption();

            if (minAmpl.HasValue)
            {
                options.Add("min_ampl", minAmpl);
            }

            if (precision != null)
            {
                options.Add("precision", precision);
            }

            return this.Call("gaussblur", options, sigma) as Image;
        }

        /// <summary>
        /// Make a gaussian image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Gaussmat(sigma, minAmpl, separable: bool, precision: string);
        /// </code>
        /// </example>
        /// <param name="sigma">Sigma of Gaussian</param>
        /// <param name="minAmpl">Minimum amplitude of Gaussian</param>
        /// <param name="separable">Generate separable Gaussian</param>
        /// <param name="precision">Generate with this precision</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Gaussmat(double sigma, double minAmpl, bool? separable = null, string precision = null)
        {
            var options = new VOption();

            if (separable.HasValue)
            {
                options.Add("separable", separable);
            }

            if (precision != null)
            {
                options.Add("precision", precision);
            }

            return Operation.Call("gaussmat", options, sigma, minAmpl) as Image;
        }

        /// <summary>
        /// Make a gaussnoise image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Gaussnoise(width, height, sigma: double, mean: double);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="sigma">Standard deviation of pixels in generated image</param>
        /// <param name="mean">Mean of pixels in generated image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Gaussnoise(int width, int height, double? sigma = null, double? mean = null)
        {
            var options = new VOption();

            if (sigma.HasValue)
            {
                options.Add("sigma", sigma);
            }

            if (mean.HasValue)
            {
                options.Add("mean", mean);
            }

            return Operation.Call("gaussnoise", options, width, height) as Image;
        }

        /// <summary>
        /// Read a point from an image
        /// </summary>
        /// <example>
        /// <code>
        /// double[] outArray = in.Getpoint(x, y);
        /// </code>
        /// </example>
        /// <param name="x">Point to read</param>
        /// <param name="y">Point to read</param>
        /// <returns>An array of doubles</returns>
        public double[] Getpoint(int x, int y)
        {
            return this.Call("getpoint", x, y) as double[];
        }

        /// <summary>
        /// Load GIF with giflib
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Gifload(filename, n: int, memory: bool, access: string, page: int, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Gifload(string filename, int? n = null, bool? memory = null, string access = null,
            int? page = null, bool? fail = null)
        {
            var options = new VOption();

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            return Operation.Call("gifload", options, filename) as Image;
        }

        /// <summary>
        /// Load GIF with giflib
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Gifload(filename, out var flags, n: int, memory: bool, access: string, page: int, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Gifload(string filename, out int flags, int? n = null, bool? memory = null,
            string access = null, int? page = null, bool? fail = null)
        {
            var options = new VOption();

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            options.Add("flags", true);

            var results = Operation.Call("gifload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load GIF with giflib
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.GifloadBuffer(buffer, n: int, memory: bool, access: string, page: int, fail: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image GifloadBuffer(byte[] buffer, int? n = null, bool? memory = null, string access = null,
            int? page = null, bool? fail = null)
        {
            var options = new VOption();

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            return Operation.Call("gifload_buffer", options, buffer) as Image;
        }

        /// <summary>
        /// Load GIF with giflib
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.GifloadBuffer(buffer, out var flags, n: int, memory: bool, access: string, page: int, fail: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image GifloadBuffer(byte[] buffer, out int flags, int? n = null, bool? memory = null,
            string access = null, int? page = null, bool? fail = null)
        {
            var options = new VOption();

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            options.Add("flags", true);

            var results = Operation.Call("gifload_buffer", options, buffer) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Global balance an image mosaic
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Globalbalance(gamma: double, intOutput: bool);
        /// </code>
        /// </example>
        /// <param name="gamma">Image gamma</param>
        /// <param name="intOutput">Integer output</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Globalbalance(double? gamma = null, bool? intOutput = null)
        {
            var options = new VOption();

            if (gamma.HasValue)
            {
                options.Add("gamma", gamma);
            }

            if (intOutput.HasValue)
            {
                options.Add("int_output", intOutput);
            }

            return this.Call("globalbalance", options) as Image;
        }

        /// <summary>
        /// Place an image within a larger image with a certain gravity
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Gravity(direction, width, height, extend: string, background: double[]);
        /// </code>
        /// </example>
        /// <param name="direction">direction to place image within width/height</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="extend">How to generate the extra pixels</param>
        /// <param name="background">Color for background pixels</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Gravity(string direction, int width, int height, string extend = null, double[] background = null)
        {
            var options = new VOption();

            if (extend != null)
            {
                options.Add("extend", extend);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("gravity", options, direction, width, height) as Image;
        }

        /// <summary>
        /// Make a grey ramp image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Grey(width, height, uchar: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Grey(int width, int height, bool? uchar = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            return Operation.Call("grey", options, width, height) as Image;
        }

        /// <summary>
        /// Grid an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Grid(tileHeight, across, down);
        /// </code>
        /// </example>
        /// <param name="tileHeight">chop into tiles this high</param>
        /// <param name="across">number of tiles across</param>
        /// <param name="down">number of tiles down</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Grid(int tileHeight, int across, int down)
        {
            return this.Call("grid", tileHeight, across, down) as Image;
        }

        /// <summary>
        /// Form cumulative histogram
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HistCum();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistCum()
        {
            return this.Call("hist_cum") as Image;
        }

        /// <summary>
        /// Estimate image entropy
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.HistEntropy();
        /// </code>
        /// </example>
        /// <returns>A double</returns>
        public double HistEntropy()
        {
            return this.Call("hist_entropy") is double result ? result : 0;
        }

        /// <summary>
        /// Histogram equalisation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HistEqual(band: int);
        /// </code>
        /// </example>
        /// <param name="band">Equalise with this band</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistEqual(int? band = null)
        {
            var options = new VOption();

            if (band.HasValue)
            {
                options.Add("band", band);
            }

            return this.Call("hist_equal", options) as Image;
        }

        /// <summary>
        /// Find image histogram
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HistFind(band: int);
        /// </code>
        /// </example>
        /// <param name="band">Find histogram of band</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistFind(int? band = null)
        {
            var options = new VOption();

            if (band.HasValue)
            {
                options.Add("band", band);
            }

            return this.Call("hist_find", options) as Image;
        }

        /// <summary>
        /// Find indexed image histogram
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HistFindIndexed(index, combine: string);
        /// </code>
        /// </example>
        /// <param name="index">Index image</param>
        /// <param name="combine">Combine bins like this</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistFindIndexed(Image index, string combine = null)
        {
            var options = new VOption();

            if (combine != null)
            {
                options.Add("combine", combine);
            }

            return this.Call("hist_find_indexed", options, index) as Image;
        }

        /// <summary>
        /// Find n-dimensional image histogram
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HistFindNdim(bins: int);
        /// </code>
        /// </example>
        /// <param name="bins">Number of bins in each dimension</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistFindNdim(int? bins = null)
        {
            var options = new VOption();

            if (bins.HasValue)
            {
                options.Add("bins", bins);
            }

            return this.Call("hist_find_ndim", options) as Image;
        }

        /// <summary>
        /// Test for monotonicity
        /// </summary>
        /// <example>
        /// <code>
        /// bool monotonic = in.HistIsmonotonic();
        /// </code>
        /// </example>
        /// <returns>A bool</returns>
        public bool HistIsmonotonic()
        {
            return this.Call("hist_ismonotonic") is bool result && result;
        }

        /// <summary>
        /// Local histogram equalisation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HistLocal(width, height, maxSlope: int);
        /// </code>
        /// </example>
        /// <param name="width">Window width in pixels</param>
        /// <param name="height">Window height in pixels</param>
        /// <param name="maxSlope">Maximum slope (CLAHE)</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistLocal(int width, int height, int? maxSlope = null)
        {
            var options = new VOption();

            if (maxSlope.HasValue)
            {
                options.Add("max_slope", maxSlope);
            }

            return this.Call("hist_local", options, width, height) as Image;
        }

        /// <summary>
        /// Match two histograms
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HistMatch(@ref);
        /// </code>
        /// </example>
        /// <param name="ref">Reference histogram</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistMatch(Image @ref)
        {
            return this.Call("hist_match", @ref) as Image;
        }

        /// <summary>
        /// Normalise histogram
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HistNorm();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistNorm()
        {
            return this.Call("hist_norm") as Image;
        }

        /// <summary>
        /// Plot histogram
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HistPlot();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistPlot()
        {
            return this.Call("hist_plot") as Image;
        }

        /// <summary>
        /// Find hough circle transform
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HoughCircle(scale: int, minRadius: int, maxRadius: int);
        /// </code>
        /// </example>
        /// <param name="scale">Scale down dimensions by this factor</param>
        /// <param name="minRadius">Smallest radius to search for</param>
        /// <param name="maxRadius">Largest radius to search for</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HoughCircle(int? scale = null, int? minRadius = null, int? maxRadius = null)
        {
            var options = new VOption();

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            if (minRadius.HasValue)
            {
                options.Add("min_radius", minRadius);
            }

            if (maxRadius.HasValue)
            {
                options.Add("max_radius", maxRadius);
            }

            return this.Call("hough_circle", options) as Image;
        }

        /// <summary>
        /// Find hough line transform
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HoughLine(width: int, height: int);
        /// </code>
        /// </example>
        /// <param name="width">horizontal size of parameter space</param>
        /// <param name="height">Vertical size of parameter space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HoughLine(int? width = null, int? height = null)
        {
            var options = new VOption();

            if (width.HasValue)
            {
                options.Add("width", width);
            }

            if (height.HasValue)
            {
                options.Add("height", height);
            }

            return this.Call("hough_line", options) as Image;
        }

        /// <summary>
        /// Transform HSV to sRGB
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.HSV2sRGB();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HSV2sRGB()
        {
            return this.Call("HSV2sRGB") as Image;
        }

        /// <summary>
        /// Output to device with ICC profile
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.IccExport(pcs: string, intent: string, outputProfile: string, depth: int);
        /// </code>
        /// </example>
        /// <param name="pcs">Set Profile Connection Space</param>
        /// <param name="intent">Rendering intent</param>
        /// <param name="outputProfile">Filename to load output profile from</param>
        /// <param name="depth">Output device space depth in bits</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image IccExport(string pcs = null, string intent = null, string outputProfile = null, int? depth = null)
        {
            var options = new VOption();

            if (pcs != null)
            {
                options.Add("pcs", pcs);
            }

            if (intent != null)
            {
                options.Add("intent", intent);
            }

            if (outputProfile != null)
            {
                options.Add("output_profile", outputProfile);
            }

            if (depth.HasValue)
            {
                options.Add("depth", depth);
            }

            return this.Call("icc_export", options) as Image;
        }

        /// <summary>
        /// Import from device with ICC profile
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.IccImport(pcs: string, intent: string, embedded: bool, inputProfile: string);
        /// </code>
        /// </example>
        /// <param name="pcs">Set Profile Connection Space</param>
        /// <param name="intent">Rendering intent</param>
        /// <param name="embedded">Use embedded input profile, if available</param>
        /// <param name="inputProfile">Filename to load input profile from</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image IccImport(string pcs = null, string intent = null, bool? embedded = null,
            string inputProfile = null)
        {
            var options = new VOption();

            if (pcs != null)
            {
                options.Add("pcs", pcs);
            }

            if (intent != null)
            {
                options.Add("intent", intent);
            }

            if (embedded.HasValue)
            {
                options.Add("embedded", embedded);
            }

            if (inputProfile != null)
            {
                options.Add("input_profile", inputProfile);
            }

            return this.Call("icc_import", options) as Image;
        }

        /// <summary>
        /// Transform between devices with ICC profiles
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.IccTransform(outputProfile, pcs: string, intent: string, embedded: bool, inputProfile: string, depth: int);
        /// </code>
        /// </example>
        /// <param name="outputProfile">Filename to load output profile from</param>
        /// <param name="pcs">Set Profile Connection Space</param>
        /// <param name="intent">Rendering intent</param>
        /// <param name="embedded">Use embedded input profile, if available</param>
        /// <param name="inputProfile">Filename to load input profile from</param>
        /// <param name="depth">Output device space depth in bits</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image IccTransform(string outputProfile, string pcs = null, string intent = null, bool? embedded = null,
            string inputProfile = null, int? depth = null)
        {
            var options = new VOption();

            if (pcs != null)
            {
                options.Add("pcs", pcs);
            }

            if (intent != null)
            {
                options.Add("intent", intent);
            }

            if (embedded.HasValue)
            {
                options.Add("embedded", embedded);
            }

            if (inputProfile != null)
            {
                options.Add("input_profile", inputProfile);
            }

            if (depth.HasValue)
            {
                options.Add("depth", depth);
            }

            return this.Call("icc_transform", options, outputProfile) as Image;
        }

        /// <summary>
        /// Make a 1D image where pixel values are indexes
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Identity(bands: int, @ushort: bool, size: int);
        /// </code>
        /// </example>
        /// <param name="bands">Number of bands in LUT</param>
        /// <param name="ushort">Create a 16-bit LUT</param>
        /// <param name="size">Size of 16-bit LUT</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Identity(int? bands = null, bool? @ushort = null, int? size = null)
        {
            var options = new VOption();

            if (bands.HasValue)
            {
                options.Add("bands", bands);
            }

            if (@ushort.HasValue)
            {
                options.Add("ushort", @ushort);
            }

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            return Operation.Call("identity", options) as Image;
        }

        /// <summary>
        /// Insert image @sub into @main at @x, @y
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = main.Insert(sub, x, y, expand: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="sub">Sub-image to insert into main image</param>
        /// <param name="x">Left edge of sub in main</param>
        /// <param name="y">Top edge of sub in main</param>
        /// <param name="expand">Expand output to hold all of both inputs</param>
        /// <param name="background">Color for new pixels</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Insert(Image sub, int x, int y, bool? expand = null, double[] background = null)
        {
            var options = new VOption();

            if (expand.HasValue)
            {
                options.Add("expand", expand);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("insert", options, sub, x, y) as Image;
        }

        /// <summary>
        /// Invert an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Invert();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Invert()
        {
            return this.Call("invert") as Image;
        }

        /// <summary>
        /// Build an inverted look-up table
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Invertlut(size: int);
        /// </code>
        /// </example>
        /// <param name="size">LUT size to generate</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Invertlut(int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            return this.Call("invertlut", options) as Image;
        }

        /// <summary>
        /// Inverse FFT
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Invfft(real: bool);
        /// </code>
        /// </example>
        /// <param name="real">Output only the real part of the transform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Invfft(bool? real = null)
        {
            var options = new VOption();

            if (real.HasValue)
            {
                options.Add("real", real);
            }

            return this.Call("invfft", options) as Image;
        }

        /// <summary>
        /// Join a pair of images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in1.Join(in2, direction, expand: bool, shim: int, background: double[], align: string);
        /// </code>
        /// </example>
        /// <param name="in2">Second input image</param>
        /// <param name="direction">Join left-right or up-down</param>
        /// <param name="expand">Expand output to hold all of both inputs</param>
        /// <param name="shim">Pixels between images</param>
        /// <param name="background">Colour for new pixels</param>
        /// <param name="align">Align on the low, centre or high coordinate edge</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Join(Image in2, string direction, bool? expand = null, int? shim = null,
            double[] background = null, string align = null)
        {
            var options = new VOption();

            if (expand.HasValue)
            {
                options.Add("expand", expand);
            }

            if (shim.HasValue)
            {
                options.Add("shim", shim);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            if (align != null)
            {
                options.Add("align", align);
            }

            return this.Call("join", options, in2, direction) as Image;
        }

        /// <summary>
        /// Load jpeg from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Jpegload(filename, memory: bool, access: string, shrink: int, fail: bool, autorotate: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="shrink">Shrink factor on load</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="autorotate">Rotate image using exif orientation</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Jpegload(string filename, bool? memory = null, string access = null, int? shrink = null,
            bool? fail = null, bool? autorotate = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (shrink.HasValue)
            {
                options.Add("shrink", shrink);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (autorotate.HasValue)
            {
                options.Add("autorotate", autorotate);
            }

            return Operation.Call("jpegload", options, filename) as Image;
        }

        /// <summary>
        /// Load jpeg from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Jpegload(filename, out var flags, memory: bool, access: string, shrink: int, fail: bool, autorotate: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="shrink">Shrink factor on load</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="autorotate">Rotate image using exif orientation</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Jpegload(string filename, out int flags, bool? memory = null, string access = null,
            int? shrink = null, bool? fail = null, bool? autorotate = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (shrink.HasValue)
            {
                options.Add("shrink", shrink);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (autorotate.HasValue)
            {
                options.Add("autorotate", autorotate);
            }

            options.Add("flags", true);

            var results = Operation.Call("jpegload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load jpeg from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.JpegloadBuffer(buffer, memory: bool, access: string, shrink: int, fail: bool, autorotate: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="shrink">Shrink factor on load</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="autorotate">Rotate image using exif orientation</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image JpegloadBuffer(byte[] buffer, bool? memory = null, string access = null, int? shrink = null,
            bool? fail = null, bool? autorotate = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (shrink.HasValue)
            {
                options.Add("shrink", shrink);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (autorotate.HasValue)
            {
                options.Add("autorotate", autorotate);
            }

            return Operation.Call("jpegload_buffer", options, buffer) as Image;
        }

        /// <summary>
        /// Load jpeg from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.JpegloadBuffer(buffer, out var flags, memory: bool, access: string, shrink: int, fail: bool, autorotate: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="shrink">Shrink factor on load</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="autorotate">Rotate image using exif orientation</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image JpegloadBuffer(byte[] buffer, out int flags, bool? memory = null, string access = null,
            int? shrink = null, bool? fail = null, bool? autorotate = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (shrink.HasValue)
            {
                options.Add("shrink", shrink);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (autorotate.HasValue)
            {
                options.Add("autorotate", autorotate);
            }

            options.Add("flags", true);

            var results = Operation.Call("jpegload_buffer", options, buffer) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Save image to jpeg file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Jpegsave(filename, pageHeight: int, q: int, profile: string, optimizeCoding: bool, interlace: bool, noSubsample: bool, trellisQuant: bool, overshootDeringing: bool, optimizeScans: bool, quantTable: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="q">Q factor</param>
        /// <param name="profile">ICC profile to embed</param>
        /// <param name="optimizeCoding">Compute optimal Huffman coding tables</param>
        /// <param name="interlace">Generate an interlaced (progressive) jpeg</param>
        /// <param name="noSubsample">Disable chroma subsample</param>
        /// <param name="trellisQuant">Apply trellis quantisation to each 8x8 block</param>
        /// <param name="overshootDeringing">Apply overshooting to samples with extreme values</param>
        /// <param name="optimizeScans">Split the spectrum of DCT coefficients into separate scans</param>
        /// <param name="quantTable">Use predefined quantization table with given index</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Jpegsave(string filename, int? pageHeight = null, int? q = null, string profile = null,
            bool? optimizeCoding = null, bool? interlace = null, bool? noSubsample = null, bool? trellisQuant = null,
            bool? overshootDeringing = null, bool? optimizeScans = null, int? quantTable = null, bool? strip = null,
            double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (q.HasValue)
            {
                options.Add("Q", q);
            }

            if (profile != null)
            {
                options.Add("profile", profile);
            }

            if (optimizeCoding.HasValue)
            {
                options.Add("optimize_coding", optimizeCoding);
            }

            if (interlace.HasValue)
            {
                options.Add("interlace", interlace);
            }

            if (noSubsample.HasValue)
            {
                options.Add("no_subsample", noSubsample);
            }

            if (trellisQuant.HasValue)
            {
                options.Add("trellis_quant", trellisQuant);
            }

            if (overshootDeringing.HasValue)
            {
                options.Add("overshoot_deringing", overshootDeringing);
            }

            if (optimizeScans.HasValue)
            {
                options.Add("optimize_scans", optimizeScans);
            }

            if (quantTable.HasValue)
            {
                options.Add("quant_table", quantTable);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("jpegsave", options, filename);
        }

        /// <summary>
        /// Save image to jpeg buffer
        /// </summary>
        /// <example>
        /// <code>
        /// byte[] buffer = in.JpegsaveBuffer(pageHeight: int, q: int, profile: string, optimizeCoding: bool, interlace: bool, noSubsample: bool, trellisQuant: bool, overshootDeringing: bool, optimizeScans: bool, quantTable: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="q">Q factor</param>
        /// <param name="profile">ICC profile to embed</param>
        /// <param name="optimizeCoding">Compute optimal Huffman coding tables</param>
        /// <param name="interlace">Generate an interlaced (progressive) jpeg</param>
        /// <param name="noSubsample">Disable chroma subsample</param>
        /// <param name="trellisQuant">Apply trellis quantisation to each 8x8 block</param>
        /// <param name="overshootDeringing">Apply overshooting to samples with extreme values</param>
        /// <param name="optimizeScans">Split the spectrum of DCT coefficients into separate scans</param>
        /// <param name="quantTable">Use predefined quantization table with given index</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>An array of bytes</returns>
        public byte[] JpegsaveBuffer(int? pageHeight = null, int? q = null, string profile = null,
            bool? optimizeCoding = null, bool? interlace = null, bool? noSubsample = null, bool? trellisQuant = null,
            bool? overshootDeringing = null, bool? optimizeScans = null, int? quantTable = null, bool? strip = null,
            double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (q.HasValue)
            {
                options.Add("Q", q);
            }

            if (profile != null)
            {
                options.Add("profile", profile);
            }

            if (optimizeCoding.HasValue)
            {
                options.Add("optimize_coding", optimizeCoding);
            }

            if (interlace.HasValue)
            {
                options.Add("interlace", interlace);
            }

            if (noSubsample.HasValue)
            {
                options.Add("no_subsample", noSubsample);
            }

            if (trellisQuant.HasValue)
            {
                options.Add("trellis_quant", trellisQuant);
            }

            if (overshootDeringing.HasValue)
            {
                options.Add("overshoot_deringing", overshootDeringing);
            }

            if (optimizeScans.HasValue)
            {
                options.Add("optimize_scans", optimizeScans);
            }

            if (quantTable.HasValue)
            {
                options.Add("quant_table", quantTable);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("jpegsave_buffer", options) as byte[];
        }

        /// <summary>
        /// Save image to jpeg mime
        /// </summary>
        /// <example>
        /// <code>
        /// in.JpegsaveMime(pageHeight: int, q: int, profile: string, optimizeCoding: bool, interlace: bool, noSubsample: bool, trellisQuant: bool, overshootDeringing: bool, optimizeScans: bool, quantTable: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="q">Q factor</param>
        /// <param name="profile">ICC profile to embed</param>
        /// <param name="optimizeCoding">Compute optimal Huffman coding tables</param>
        /// <param name="interlace">Generate an interlaced (progressive) jpeg</param>
        /// <param name="noSubsample">Disable chroma subsample</param>
        /// <param name="trellisQuant">Apply trellis quantisation to each 8x8 block</param>
        /// <param name="overshootDeringing">Apply overshooting to samples with extreme values</param>
        /// <param name="optimizeScans">Split the spectrum of DCT coefficients into separate scans</param>
        /// <param name="quantTable">Use predefined quantization table with given index</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void JpegsaveMime(int? pageHeight = null, int? q = null, string profile = null,
            bool? optimizeCoding = null, bool? interlace = null, bool? noSubsample = null, bool? trellisQuant = null,
            bool? overshootDeringing = null, bool? optimizeScans = null, int? quantTable = null, bool? strip = null,
            double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (q.HasValue)
            {
                options.Add("Q", q);
            }

            if (profile != null)
            {
                options.Add("profile", profile);
            }

            if (optimizeCoding.HasValue)
            {
                options.Add("optimize_coding", optimizeCoding);
            }

            if (interlace.HasValue)
            {
                options.Add("interlace", interlace);
            }

            if (noSubsample.HasValue)
            {
                options.Add("no_subsample", noSubsample);
            }

            if (trellisQuant.HasValue)
            {
                options.Add("trellis_quant", trellisQuant);
            }

            if (overshootDeringing.HasValue)
            {
                options.Add("overshoot_deringing", overshootDeringing);
            }

            if (optimizeScans.HasValue)
            {
                options.Add("optimize_scans", optimizeScans);
            }

            if (quantTable.HasValue)
            {
                options.Add("quant_table", quantTable);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("jpegsave_mime", options);
        }

        /// <summary>
        /// Transform float Lab to LabQ coding
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Lab2LabQ();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Lab2LabQ()
        {
            return this.Call("Lab2LabQ") as Image;
        }

        /// <summary>
        /// Transform float Lab to signed short
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Lab2LabS();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Lab2LabS()
        {
            return this.Call("Lab2LabS") as Image;
        }

        /// <summary>
        /// Transform Lab to LCh
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Lab2LCh();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Lab2LCh()
        {
            return this.Call("Lab2LCh") as Image;
        }

        /// <summary>
        /// Transform CIELAB to XYZ
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Lab2XYZ(temp: double[]);
        /// </code>
        /// </example>
        /// <param name="temp">Color temperature</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Lab2XYZ(double[] temp = null)
        {
            var options = new VOption();

            if (temp != null && temp.Length > 0)
            {
                options.Add("temp", temp);
            }

            return this.Call("Lab2XYZ", options) as Image;
        }

        /// <summary>
        /// Label regions in an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image mask = in.Labelregions();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Labelregions()
        {
            return this.Call("labelregions") as Image;
        }

        /// <summary>
        /// Label regions in an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image mask = in.Labelregions(out var segments);
        /// </code>
        /// </example>
        /// <param name="segments">Number of discrete contigious regions</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Labelregions(out int segments)
        {
            var optionalOutput = new VOption
            {
                {"segments", true}
            };

            var results = this.Call("labelregions", optionalOutput) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            segments = opts?["segments"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Unpack a LabQ image to float Lab
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.LabQ2Lab();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image LabQ2Lab()
        {
            return this.Call("LabQ2Lab") as Image;
        }

        /// <summary>
        /// Unpack a LabQ image to short Lab
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.LabQ2LabS();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image LabQ2LabS()
        {
            return this.Call("LabQ2LabS") as Image;
        }

        /// <summary>
        /// Convert a LabQ image to sRGB
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.LabQ2sRGB();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image LabQ2sRGB()
        {
            return this.Call("LabQ2sRGB") as Image;
        }

        /// <summary>
        /// Transform signed short Lab to float
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.LabS2Lab();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image LabS2Lab()
        {
            return this.Call("LabS2Lab") as Image;
        }

        /// <summary>
        /// Transform short Lab to LabQ coding
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.LabS2LabQ();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image LabS2LabQ()
        {
            return this.Call("LabS2LabQ") as Image;
        }

        /// <summary>
        /// Transform LCh to CMC
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.LCh2CMC();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image LCh2CMC()
        {
            return this.Call("LCh2CMC") as Image;
        }

        /// <summary>
        /// Transform LCh to Lab
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.LCh2Lab();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image LCh2Lab()
        {
            return this.Call("LCh2Lab") as Image;
        }

        /// <summary>
        /// Calculate (a * in + b)
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Linear(a, b, uchar: bool);
        /// </code>
        /// </example>
        /// <param name="a">Multiply by this</param>
        /// <param name="b">Add this</param>
        /// <param name="uchar">Output should be uchar</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Linear(double[] a, double[] b, bool? uchar = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            return this.Call("linear", options, a, b) as Image;
        }

        /// <summary>
        /// Cache an image as a set of lines
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Linecache(tileHeight: int, access: string, threaded: bool, persistent: bool);
        /// </code>
        /// </example>
        /// <param name="tileHeight">Tile height in pixels</param>
        /// <param name="access">Expected access pattern</param>
        /// <param name="threaded">Allow threaded access</param>
        /// <param name="persistent">Keep cache between evaluations</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Linecache(int? tileHeight = null, string access = null, bool? threaded = null,
            bool? persistent = null)
        {
            var options = new VOption();

            if (tileHeight.HasValue)
            {
                options.Add("tile_height", tileHeight);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (threaded.HasValue)
            {
                options.Add("threaded", threaded);
            }

            if (persistent.HasValue)
            {
                options.Add("persistent", persistent);
            }

            return this.Call("linecache", options) as Image;
        }

        /// <summary>
        /// Make a laplacian of gaussian image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Logmat(sigma, minAmpl, separable: bool, precision: string);
        /// </code>
        /// </example>
        /// <param name="sigma">Radius of Logmatian</param>
        /// <param name="minAmpl">Minimum amplitude of Logmatian</param>
        /// <param name="separable">Generate separable Logmatian</param>
        /// <param name="precision">Generate with this precision</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Logmat(double sigma, double minAmpl, bool? separable = null, string precision = null)
        {
            var options = new VOption();

            if (separable.HasValue)
            {
                options.Add("separable", separable);
            }

            if (precision != null)
            {
                options.Add("precision", precision);
            }

            return Operation.Call("logmat", options, sigma, minAmpl) as Image;
        }

        /// <summary>
        /// Load file with ImageMagick
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Magickload(filename, density: string, page: int, n: int, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="density">Canvas resolution for rendering vector formats like SVG</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Magickload(string filename, string density = null, int? page = null, int? n = null,
            bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

            if (density != null)
            {
                options.Add("density", density);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
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

            return Operation.Call("magickload", options, filename) as Image;
        }

        /// <summary>
        /// Load file with ImageMagick
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Magickload(filename, out var flags, density: string, page: int, n: int, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="density">Canvas resolution for rendering vector formats like SVG</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Magickload(string filename, out int flags, string density = null, int? page = null,
            int? n = null, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

            if (density != null)
            {
                options.Add("density", density);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
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

            options.Add("flags", true);

            var results = Operation.Call("magickload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load buffer with ImageMagick
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MagickloadBuffer(buffer, density: string, page: int, n: int, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="density">Canvas resolution for rendering vector formats like SVG</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MagickloadBuffer(byte[] buffer, string density = null, int? page = null, int? n = null,
            bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

            if (density != null)
            {
                options.Add("density", density);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
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

            return Operation.Call("magickload_buffer", options, buffer) as Image;
        }

        /// <summary>
        /// Load buffer with ImageMagick
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MagickloadBuffer(buffer, out var flags, density: string, page: int, n: int, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="density">Canvas resolution for rendering vector formats like SVG</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MagickloadBuffer(byte[] buffer, out int flags, string density = null, int? page = null,
            int? n = null, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

            if (density != null)
            {
                options.Add("density", density);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
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

            options.Add("flags", true);

            var results = Operation.Call("magickload_buffer", options, buffer) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Save file with ImageMagick
        /// </summary>
        /// <example>
        /// <code>
        /// in.Magicksave(filename, format: string, quality: int, pageHeight: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="format">Format to save in</param>
        /// <param name="quality">Quality to use</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Magicksave(string filename, string format = null, int? quality = null, int? pageHeight = null,
            bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (format != null)
            {
                options.Add("format", format);
            }

            if (quality.HasValue)
            {
                options.Add("quality", quality);
            }

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("magicksave", options, filename);
        }

        /// <summary>
        /// Save image to magick buffer
        /// </summary>
        /// <example>
        /// <code>
        /// byte[] buffer = in.MagicksaveBuffer(format: string, quality: int, pageHeight: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="format">Format to save in</param>
        /// <param name="quality">Quality to use</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>An array of bytes</returns>
        public byte[] MagicksaveBuffer(string format = null, int? quality = null, int? pageHeight = null,
            bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (format != null)
            {
                options.Add("format", format);
            }

            if (quality.HasValue)
            {
                options.Add("quality", quality);
            }

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("magicksave_buffer", options) as byte[];
        }

        /// <summary>
        /// Resample with an mapim image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Mapim(index, interpolate: GObject);
        /// </code>
        /// </example>
        /// <param name="index">Index pixels with this</param>
        /// <param name="interpolate">Interpolate pixels with this</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mapim(Image index, GObject interpolate = null)
        {
            var options = new VOption();

            if (interpolate != null)
            {
                options.Add("interpolate", interpolate);
            }

            return this.Call("mapim", options, index) as Image;
        }

        /// <summary>
        /// Map an image though a lut
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Maplut(lut, band: int);
        /// </code>
        /// </example>
        /// <param name="lut">Look-up table image</param>
        /// <param name="band">apply one-band lut to this band of in</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Maplut(Image lut, int? band = null)
        {
            var options = new VOption();

            if (band.HasValue)
            {
                options.Add("band", band);
            }

            return this.Call("maplut", options, lut) as Image;
        }

        /// <summary>
        /// Make a butterworth filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskButterworth(width, height, order, frequencyCutoff, amplitudeCutoff, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="order">Filter order</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskButterworth(int width, int height, double order, double frequencyCutoff,
            double amplitudeCutoff, bool? uchar = null, bool? nodc = null, bool? reject = null, bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_butterworth", options, width, height, order, frequencyCutoff,
                amplitudeCutoff) as Image;
        }

        /// <summary>
        /// Make a butterworth_band filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskButterworthBand(width, height, order, frequencyCutoffX, frequencyCutoffY, radius, amplitudeCutoff, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="order">Filter order</param>
        /// <param name="frequencyCutoffX">Frequency cutoff x</param>
        /// <param name="frequencyCutoffY">Frequency cutoff y</param>
        /// <param name="radius">radius of circle</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskButterworthBand(int width, int height, double order, double frequencyCutoffX,
            double frequencyCutoffY, double radius, double amplitudeCutoff, bool? uchar = null, bool? nodc = null,
            bool? reject = null, bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_butterworth_band", options, width, height, order, frequencyCutoffX,
                frequencyCutoffY, radius, amplitudeCutoff) as Image;
        }

        /// <summary>
        /// Make a butterworth ring filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskButterworthRing(width, height, order, frequencyCutoff, amplitudeCutoff, ringwidth, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="order">Filter order</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="ringwidth">Ringwidth</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskButterworthRing(int width, int height, double order, double frequencyCutoff,
            double amplitudeCutoff, double ringwidth, bool? uchar = null, bool? nodc = null, bool? reject = null,
            bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_butterworth_ring", options, width, height, order, frequencyCutoff,
                amplitudeCutoff, ringwidth) as Image;
        }

        /// <summary>
        /// Make fractal filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskFractal(width, height, fractalDimension, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="fractalDimension">Fractal dimension</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskFractal(int width, int height, double fractalDimension, bool? uchar = null,
            bool? nodc = null, bool? reject = null, bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_fractal", options, width, height, fractalDimension) as Image;
        }

        /// <summary>
        /// Make a gaussian filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskGaussian(width, height, frequencyCutoff, amplitudeCutoff, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskGaussian(int width, int height, double frequencyCutoff, double amplitudeCutoff,
            bool? uchar = null, bool? nodc = null, bool? reject = null, bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_gaussian", options, width, height, frequencyCutoff, amplitudeCutoff) as Image;
        }

        /// <summary>
        /// Make a gaussian filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskGaussianBand(width, height, frequencyCutoffX, frequencyCutoffY, radius, amplitudeCutoff, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoffX">Frequency cutoff x</param>
        /// <param name="frequencyCutoffY">Frequency cutoff y</param>
        /// <param name="radius">radius of circle</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskGaussianBand(int width, int height, double frequencyCutoffX, double frequencyCutoffY,
            double radius, double amplitudeCutoff, bool? uchar = null, bool? nodc = null, bool? reject = null,
            bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_gaussian_band", options, width, height, frequencyCutoffX, frequencyCutoffY,
                radius, amplitudeCutoff) as Image;
        }

        /// <summary>
        /// Make a gaussian ring filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskGaussianRing(width, height, frequencyCutoff, amplitudeCutoff, ringwidth, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="ringwidth">Ringwidth</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskGaussianRing(int width, int height, double frequencyCutoff, double amplitudeCutoff,
            double ringwidth, bool? uchar = null, bool? nodc = null, bool? reject = null, bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_gaussian_ring", options, width, height, frequencyCutoff, amplitudeCutoff,
                ringwidth) as Image;
        }

        /// <summary>
        /// Make an ideal filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskIdeal(width, height, frequencyCutoff, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskIdeal(int width, int height, double frequencyCutoff, bool? uchar = null,
            bool? nodc = null, bool? reject = null, bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_ideal", options, width, height, frequencyCutoff) as Image;
        }

        /// <summary>
        /// Make an ideal band filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskIdealBand(width, height, frequencyCutoffX, frequencyCutoffY, radius, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoffX">Frequency cutoff x</param>
        /// <param name="frequencyCutoffY">Frequency cutoff y</param>
        /// <param name="radius">radius of circle</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskIdealBand(int width, int height, double frequencyCutoffX, double frequencyCutoffY,
            double radius, bool? uchar = null, bool? nodc = null, bool? reject = null, bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_ideal_band", options, width, height, frequencyCutoffX, frequencyCutoffY,
                radius) as Image;
        }

        /// <summary>
        /// Make an ideal ring filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.MaskIdealRing(width, height, frequencyCutoff, ringwidth, uchar: bool, nodc: bool, reject: bool, optical: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="ringwidth">Ringwidth</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="nodc">Remove DC component</param>
        /// <param name="reject">Invert the sense of the filter</param>
        /// <param name="optical">Rotate quadrants to optical space</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskIdealRing(int width, int height, double frequencyCutoff, double ringwidth,
            bool? uchar = null, bool? nodc = null, bool? reject = null, bool? optical = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (nodc.HasValue)
            {
                options.Add("nodc", nodc);
            }

            if (reject.HasValue)
            {
                options.Add("reject", reject);
            }

            if (optical.HasValue)
            {
                options.Add("optical", optical);
            }

            return Operation.Call("mask_ideal_ring", options, width, height, frequencyCutoff, ringwidth) as Image;
        }

        /// <summary>
        /// First-order match of two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Match(sec, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, hwindow: int, harea: int, search: bool, interpolate: GObject);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="xr1">Position of first reference tie-point</param>
        /// <param name="yr1">Position of first reference tie-point</param>
        /// <param name="xs1">Position of first secondary tie-point</param>
        /// <param name="ys1">Position of first secondary tie-point</param>
        /// <param name="xr2">Position of second reference tie-point</param>
        /// <param name="yr2">Position of second reference tie-point</param>
        /// <param name="xs2">Position of second secondary tie-point</param>
        /// <param name="ys2">Position of second secondary tie-point</param>
        /// <param name="hwindow">Half window size</param>
        /// <param name="harea">Half area size</param>
        /// <param name="search">Search to improve tie-points</param>
        /// <param name="interpolate">Interpolate pixels with this</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Match(Image sec, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2, int ys2,
            int? hwindow = null, int? harea = null, bool? search = null, GObject interpolate = null)
        {
            var options = new VOption();

            if (hwindow.HasValue)
            {
                options.Add("hwindow", hwindow);
            }

            if (harea.HasValue)
            {
                options.Add("harea", harea);
            }

            if (search.HasValue)
            {
                options.Add("search", search);
            }

            if (interpolate != null)
            {
                options.Add("interpolate", interpolate);
            }

            return this.Call("match", options, sec, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2) as Image;
        }

        /// <summary>
        /// Apply a math operation to an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Math(math);
        /// </code>
        /// </example>
        /// <param name="math">math to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Math(string math)
        {
            return this.Call("math", math) as Image;
        }

        /// <summary>
        /// Binary math operations
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Math2(right, math2);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <param name="math2">math to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Math2(Image right, string math2)
        {
            return this.Call("math2", right, math2) as Image;
        }

        /// <summary>
        /// Binary math operations with a constant
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Math2Const(math2, c);
        /// </code>
        /// </example>
        /// <param name="math2">math to perform</param>
        /// <param name="c">Array of constants</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Math2Const(string math2, double[] c)
        {
            return this.Call("math2_const", math2, c) as Image;
        }

        /// <summary>
        /// Load mat from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Matload(filename, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Matload(string filename, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

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

            return Operation.Call("matload", options, filename) as Image;
        }

        /// <summary>
        /// Load mat from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Matload(filename, out var flags, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Matload(string filename, out int flags, bool? memory = null, string access = null,
            bool? fail = null)
        {
            var options = new VOption();

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

            options.Add("flags", true);

            var results = Operation.Call("matload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load matrix from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Matrixload(filename, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Matrixload(string filename, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

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

            return Operation.Call("matrixload", options, filename) as Image;
        }

        /// <summary>
        /// Load matrix from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Matrixload(filename, out var flags, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Matrixload(string filename, out int flags, bool? memory = null, string access = null,
            bool? fail = null)
        {
            var options = new VOption();

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

            options.Add("flags", true);

            var results = Operation.Call("matrixload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Print matrix
        /// </summary>
        /// <example>
        /// <code>
        /// in.Matrixprint(pageHeight: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Matrixprint(int? pageHeight = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("matrixprint", options);
        }

        /// <summary>
        /// Save image to matrix file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Matrixsave(filename, pageHeight: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Matrixsave(string filename, int? pageHeight = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("matrixsave", options, filename);
        }

        /// <summary>
        /// Find image maximum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Max(size: int);
        /// </code>
        /// </example>
        /// <param name="size">Number of maximum values to find</param>
        /// <returns>A double</returns>
        public double Max(int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            return this.Call("max", options) is double result ? result : 0;
        }

        /// <summary>
        /// Find image maximum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Max(out var x, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of maximum</param>
        /// <param name="size">Number of maximum values to find</param>
        /// <returns>A double</returns>
        public double Max(out int x, int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);

            var results = this.Call("max", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Find image maximum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Max(out var x, out var y, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of maximum</param>
        /// <param name="y">Vertical position of maximum</param>
        /// <param name="size">Number of maximum values to find</param>
        /// <returns>A double</returns>
        public double Max(out int x, out int y, int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);
            options.Add("y", true);

            var results = this.Call("max", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;
            y = opts?["y"] is int out2 ? out2 : 0;

            return finalResult;
        }

        /// <summary>
        /// Find image maximum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Max(out var x, out var y, out var outArray, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of maximum</param>
        /// <param name="y">Vertical position of maximum</param>
        /// <param name="outArray">Array of output values</param>
        /// <param name="size">Number of maximum values to find</param>
        /// <returns>A double</returns>
        public double Max(out int x, out int y, out double[] outArray, int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);
            options.Add("y", true);
            options.Add("out_array", true);

            var results = this.Call("max", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;
            y = opts?["y"] is int out2 ? out2 : 0;
            outArray = opts?["out_array"] as double[];

            return finalResult;
        }

        /// <summary>
        /// Find image maximum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Max(out var x, out var y, out var outArray, out var xArray, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of maximum</param>
        /// <param name="y">Vertical position of maximum</param>
        /// <param name="outArray">Array of output values</param>
        /// <param name="xArray">Array of horizontal positions</param>
        /// <param name="size">Number of maximum values to find</param>
        /// <returns>A double</returns>
        public double Max(out int x, out int y, out double[] outArray, out int[] xArray, int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);
            options.Add("y", true);
            options.Add("out_array", true);
            options.Add("x_array", true);

            var results = this.Call("max", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;
            y = opts?["y"] is int out2 ? out2 : 0;
            outArray = opts?["out_array"] as double[];
            xArray = opts?["x_array"] as int[];

            return finalResult;
        }

        /// <summary>
        /// Find image maximum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Max(out var x, out var y, out var outArray, out var xArray, out var yArray, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of maximum</param>
        /// <param name="y">Vertical position of maximum</param>
        /// <param name="outArray">Array of output values</param>
        /// <param name="xArray">Array of horizontal positions</param>
        /// <param name="yArray">Array of vertical positions</param>
        /// <param name="size">Number of maximum values to find</param>
        /// <returns>A double</returns>
        public double Max(out int x, out int y, out double[] outArray, out int[] xArray, out int[] yArray,
            int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);
            options.Add("y", true);
            options.Add("out_array", true);
            options.Add("x_array", true);
            options.Add("y_array", true);

            var results = this.Call("max", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;
            y = opts?["y"] is int out2 ? out2 : 0;
            outArray = opts?["out_array"] as double[];
            xArray = opts?["x_array"] as int[];
            yArray = opts?["y_array"] as int[];

            return finalResult;
        }

        /// <summary>
        /// Measure a set of patches on a color chart
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Measure(h, v, left: int, top: int, width: int, height: int);
        /// </code>
        /// </example>
        /// <param name="h">Number of patches across chart</param>
        /// <param name="v">Number of patches down chart</param>
        /// <param name="left">Left edge of extract area</param>
        /// <param name="top">Top edge of extract area</param>
        /// <param name="width">Width of extract area</param>
        /// <param name="height">Height of extract area</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Measure(int h, int v, int? left = null, int? top = null, int? width = null, int? height = null)
        {
            var options = new VOption();

            if (left.HasValue)
            {
                options.Add("left", left);
            }

            if (top.HasValue)
            {
                options.Add("top", top);
            }

            if (width.HasValue)
            {
                options.Add("width", width);
            }

            if (height.HasValue)
            {
                options.Add("height", height);
            }

            return this.Call("measure", options, h, v) as Image;
        }

        /// <summary>
        /// Merge two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Merge(sec, direction, dx, dy, mblend: int);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial merge</param>
        /// <param name="dx">Horizontal displacement from sec to ref</param>
        /// <param name="dy">Vertical displacement from sec to ref</param>
        /// <param name="mblend">Maximum blend size</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Merge(Image sec, string direction, int dx, int dy, int? mblend = null)
        {
            var options = new VOption();

            if (mblend.HasValue)
            {
                options.Add("mblend", mblend);
            }

            return this.Call("merge", options, sec, direction, dx, dy) as Image;
        }

        /// <summary>
        /// Find image minimum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Min(size: int);
        /// </code>
        /// </example>
        /// <param name="size">Number of minimum values to find</param>
        /// <returns>A double</returns>
        public double Min(int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            return this.Call("min", options) is double result ? result : 0;
        }

        /// <summary>
        /// Find image minimum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Min(out var x, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of minimum</param>
        /// <param name="size">Number of minimum values to find</param>
        /// <returns>A double</returns>
        public double Min(out int x, int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);

            var results = this.Call("min", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Find image minimum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Min(out var x, out var y, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of minimum</param>
        /// <param name="y">Vertical position of minimum</param>
        /// <param name="size">Number of minimum values to find</param>
        /// <returns>A double</returns>
        public double Min(out int x, out int y, int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);
            options.Add("y", true);

            var results = this.Call("min", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;
            y = opts?["y"] is int out2 ? out2 : 0;

            return finalResult;
        }

        /// <summary>
        /// Find image minimum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Min(out var x, out var y, out var outArray, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of minimum</param>
        /// <param name="y">Vertical position of minimum</param>
        /// <param name="outArray">Array of output values</param>
        /// <param name="size">Number of minimum values to find</param>
        /// <returns>A double</returns>
        public double Min(out int x, out int y, out double[] outArray, int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);
            options.Add("y", true);
            options.Add("out_array", true);

            var results = this.Call("min", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;
            y = opts?["y"] is int out2 ? out2 : 0;
            outArray = opts?["out_array"] as double[];

            return finalResult;
        }

        /// <summary>
        /// Find image minimum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Min(out var x, out var y, out var outArray, out var xArray, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of minimum</param>
        /// <param name="y">Vertical position of minimum</param>
        /// <param name="outArray">Array of output values</param>
        /// <param name="xArray">Array of horizontal positions</param>
        /// <param name="size">Number of minimum values to find</param>
        /// <returns>A double</returns>
        public double Min(out int x, out int y, out double[] outArray, out int[] xArray, int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);
            options.Add("y", true);
            options.Add("out_array", true);
            options.Add("x_array", true);

            var results = this.Call("min", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;
            y = opts?["y"] is int out2 ? out2 : 0;
            outArray = opts?["out_array"] as double[];
            xArray = opts?["x_array"] as int[];

            return finalResult;
        }

        /// <summary>
        /// Find image minimum
        /// </summary>
        /// <example>
        /// <code>
        /// double @out = in.Min(out var x, out var y, out var outArray, out var xArray, out var yArray, size: int);
        /// </code>
        /// </example>
        /// <param name="x">Horizontal position of minimum</param>
        /// <param name="y">Vertical position of minimum</param>
        /// <param name="outArray">Array of output values</param>
        /// <param name="xArray">Array of horizontal positions</param>
        /// <param name="yArray">Array of vertical positions</param>
        /// <param name="size">Number of minimum values to find</param>
        /// <returns>A double</returns>
        public double Min(out int x, out int y, out double[] outArray, out int[] xArray, out int[] yArray,
            int? size = null)
        {
            var options = new VOption();

            if (size.HasValue)
            {
                options.Add("size", size);
            }

            options.Add("x", true);
            options.Add("y", true);
            options.Add("out_array", true);
            options.Add("x_array", true);
            options.Add("y_array", true);

            var results = this.Call("min", options) as object[];
            var finalResult = results?[0] is double result ? result : 0;
            var opts = results?[1] as VOption;
            x = opts?["x"] is int out1 ? out1 : 0;
            y = opts?["y"] is int out2 ? out2 : 0;
            outArray = opts?["out_array"] as double[];
            xArray = opts?["x_array"] as int[];
            yArray = opts?["y_array"] as int[];

            return finalResult;
        }

        /// <summary>
        /// Morphology operation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Morph(mask, morph);
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="morph">Morphological operation to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Morph(Image mask, string morph)
        {
            return this.Call("morph", mask, morph) as Image;
        }

        /// <summary>
        /// Mosaic two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Mosaic(sec, direction, xref, yref, xsec, ysec, hwindow: int, harea: int, mblend: int, bandno: int);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial mosaic</param>
        /// <param name="xref">Position of reference tie-point</param>
        /// <param name="yref">Position of reference tie-point</param>
        /// <param name="xsec">Position of secondary tie-point</param>
        /// <param name="ysec">Position of secondary tie-point</param>
        /// <param name="hwindow">Half window size</param>
        /// <param name="harea">Half area size</param>
        /// <param name="mblend">Maximum blend size</param>
        /// <param name="bandno">Band to search for features on</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mosaic(Image sec, string direction, int xref, int yref, int xsec, int ysec, int? hwindow = null,
            int? harea = null, int? mblend = null, int? bandno = null)
        {
            var options = new VOption();

            if (hwindow.HasValue)
            {
                options.Add("hwindow", hwindow);
            }

            if (harea.HasValue)
            {
                options.Add("harea", harea);
            }

            if (mblend.HasValue)
            {
                options.Add("mblend", mblend);
            }

            if (bandno.HasValue)
            {
                options.Add("bandno", bandno);
            }

            return this.Call("mosaic", options, sec, direction, xref, yref, xsec, ysec) as Image;
        }

        /// <summary>
        /// Mosaic two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Mosaic(sec, direction, xref, yref, xsec, ysec, out var dx0, hwindow: int, harea: int, mblend: int, bandno: int);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial mosaic</param>
        /// <param name="xref">Position of reference tie-point</param>
        /// <param name="yref">Position of reference tie-point</param>
        /// <param name="xsec">Position of secondary tie-point</param>
        /// <param name="ysec">Position of secondary tie-point</param>
        /// <param name="dx0">Detected integer offset</param>
        /// <param name="hwindow">Half window size</param>
        /// <param name="harea">Half area size</param>
        /// <param name="mblend">Maximum blend size</param>
        /// <param name="bandno">Band to search for features on</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mosaic(Image sec, string direction, int xref, int yref, int xsec, int ysec, out int dx0,
            int? hwindow = null, int? harea = null, int? mblend = null, int? bandno = null)
        {
            var options = new VOption();

            if (hwindow.HasValue)
            {
                options.Add("hwindow", hwindow);
            }

            if (harea.HasValue)
            {
                options.Add("harea", harea);
            }

            if (mblend.HasValue)
            {
                options.Add("mblend", mblend);
            }

            if (bandno.HasValue)
            {
                options.Add("bandno", bandno);
            }

            options.Add("dx0", true);

            var results = this.Call("mosaic", options, sec, direction, xref, yref, xsec, ysec) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            dx0 = opts?["dx0"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Mosaic two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Mosaic(sec, direction, xref, yref, xsec, ysec, out var dx0, out var dy0, hwindow: int, harea: int, mblend: int, bandno: int);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial mosaic</param>
        /// <param name="xref">Position of reference tie-point</param>
        /// <param name="yref">Position of reference tie-point</param>
        /// <param name="xsec">Position of secondary tie-point</param>
        /// <param name="ysec">Position of secondary tie-point</param>
        /// <param name="dx0">Detected integer offset</param>
        /// <param name="dy0">Detected integer offset</param>
        /// <param name="hwindow">Half window size</param>
        /// <param name="harea">Half area size</param>
        /// <param name="mblend">Maximum blend size</param>
        /// <param name="bandno">Band to search for features on</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mosaic(Image sec, string direction, int xref, int yref, int xsec, int ysec, out int dx0,
            out int dy0, int? hwindow = null, int? harea = null, int? mblend = null, int? bandno = null)
        {
            var options = new VOption();

            if (hwindow.HasValue)
            {
                options.Add("hwindow", hwindow);
            }

            if (harea.HasValue)
            {
                options.Add("harea", harea);
            }

            if (mblend.HasValue)
            {
                options.Add("mblend", mblend);
            }

            if (bandno.HasValue)
            {
                options.Add("bandno", bandno);
            }

            options.Add("dx0", true);
            options.Add("dy0", true);

            var results = this.Call("mosaic", options, sec, direction, xref, yref, xsec, ysec) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            dx0 = opts?["dx0"] is int out1 ? out1 : 0;
            dy0 = opts?["dy0"] is int out2 ? out2 : 0;

            return finalResult;
        }

        /// <summary>
        /// Mosaic two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Mosaic(sec, direction, xref, yref, xsec, ysec, out var dx0, out var dy0, out var scale1, hwindow: int, harea: int, mblend: int, bandno: int);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial mosaic</param>
        /// <param name="xref">Position of reference tie-point</param>
        /// <param name="yref">Position of reference tie-point</param>
        /// <param name="xsec">Position of secondary tie-point</param>
        /// <param name="ysec">Position of secondary tie-point</param>
        /// <param name="dx0">Detected integer offset</param>
        /// <param name="dy0">Detected integer offset</param>
        /// <param name="scale1">Detected scale</param>
        /// <param name="hwindow">Half window size</param>
        /// <param name="harea">Half area size</param>
        /// <param name="mblend">Maximum blend size</param>
        /// <param name="bandno">Band to search for features on</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mosaic(Image sec, string direction, int xref, int yref, int xsec, int ysec, out int dx0,
            out int dy0, out double scale1, int? hwindow = null, int? harea = null, int? mblend = null,
            int? bandno = null)
        {
            var options = new VOption();

            if (hwindow.HasValue)
            {
                options.Add("hwindow", hwindow);
            }

            if (harea.HasValue)
            {
                options.Add("harea", harea);
            }

            if (mblend.HasValue)
            {
                options.Add("mblend", mblend);
            }

            if (bandno.HasValue)
            {
                options.Add("bandno", bandno);
            }

            options.Add("dx0", true);
            options.Add("dy0", true);
            options.Add("scale1", true);

            var results = this.Call("mosaic", options, sec, direction, xref, yref, xsec, ysec) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            dx0 = opts?["dx0"] is int out1 ? out1 : 0;
            dy0 = opts?["dy0"] is int out2 ? out2 : 0;
            scale1 = opts?["scale1"] is double out3 ? out3 : 0;

            return finalResult;
        }

        /// <summary>
        /// Mosaic two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Mosaic(sec, direction, xref, yref, xsec, ysec, out var dx0, out var dy0, out var scale1, out var angle1, hwindow: int, harea: int, mblend: int, bandno: int);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial mosaic</param>
        /// <param name="xref">Position of reference tie-point</param>
        /// <param name="yref">Position of reference tie-point</param>
        /// <param name="xsec">Position of secondary tie-point</param>
        /// <param name="ysec">Position of secondary tie-point</param>
        /// <param name="dx0">Detected integer offset</param>
        /// <param name="dy0">Detected integer offset</param>
        /// <param name="scale1">Detected scale</param>
        /// <param name="angle1">Detected rotation</param>
        /// <param name="hwindow">Half window size</param>
        /// <param name="harea">Half area size</param>
        /// <param name="mblend">Maximum blend size</param>
        /// <param name="bandno">Band to search for features on</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mosaic(Image sec, string direction, int xref, int yref, int xsec, int ysec, out int dx0,
            out int dy0, out double scale1, out double angle1, int? hwindow = null, int? harea = null,
            int? mblend = null, int? bandno = null)
        {
            var options = new VOption();

            if (hwindow.HasValue)
            {
                options.Add("hwindow", hwindow);
            }

            if (harea.HasValue)
            {
                options.Add("harea", harea);
            }

            if (mblend.HasValue)
            {
                options.Add("mblend", mblend);
            }

            if (bandno.HasValue)
            {
                options.Add("bandno", bandno);
            }

            options.Add("dx0", true);
            options.Add("dy0", true);
            options.Add("scale1", true);
            options.Add("angle1", true);

            var results = this.Call("mosaic", options, sec, direction, xref, yref, xsec, ysec) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            dx0 = opts?["dx0"] is int out1 ? out1 : 0;
            dy0 = opts?["dy0"] is int out2 ? out2 : 0;
            scale1 = opts?["scale1"] is double out3 ? out3 : 0;
            angle1 = opts?["angle1"] is double out4 ? out4 : 0;

            return finalResult;
        }

        /// <summary>
        /// Mosaic two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Mosaic(sec, direction, xref, yref, xsec, ysec, out var dx0, out var dy0, out var scale1, out var angle1, out var dy1, hwindow: int, harea: int, mblend: int, bandno: int);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial mosaic</param>
        /// <param name="xref">Position of reference tie-point</param>
        /// <param name="yref">Position of reference tie-point</param>
        /// <param name="xsec">Position of secondary tie-point</param>
        /// <param name="ysec">Position of secondary tie-point</param>
        /// <param name="dx0">Detected integer offset</param>
        /// <param name="dy0">Detected integer offset</param>
        /// <param name="scale1">Detected scale</param>
        /// <param name="angle1">Detected rotation</param>
        /// <param name="dy1">Detected first-order displacement</param>
        /// <param name="hwindow">Half window size</param>
        /// <param name="harea">Half area size</param>
        /// <param name="mblend">Maximum blend size</param>
        /// <param name="bandno">Band to search for features on</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mosaic(Image sec, string direction, int xref, int yref, int xsec, int ysec, out int dx0,
            out int dy0, out double scale1, out double angle1, out double dy1, int? hwindow = null, int? harea = null,
            int? mblend = null, int? bandno = null)
        {
            var options = new VOption();

            if (hwindow.HasValue)
            {
                options.Add("hwindow", hwindow);
            }

            if (harea.HasValue)
            {
                options.Add("harea", harea);
            }

            if (mblend.HasValue)
            {
                options.Add("mblend", mblend);
            }

            if (bandno.HasValue)
            {
                options.Add("bandno", bandno);
            }

            options.Add("dx0", true);
            options.Add("dy0", true);
            options.Add("scale1", true);
            options.Add("angle1", true);
            options.Add("dy1", true);

            var results = this.Call("mosaic", options, sec, direction, xref, yref, xsec, ysec) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            dx0 = opts?["dx0"] is int out1 ? out1 : 0;
            dy0 = opts?["dy0"] is int out2 ? out2 : 0;
            scale1 = opts?["scale1"] is double out3 ? out3 : 0;
            angle1 = opts?["angle1"] is double out4 ? out4 : 0;
            dy1 = opts?["dy1"] is double out5 ? out5 : 0;

            return finalResult;
        }

        /// <summary>
        /// Mosaic two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Mosaic(sec, direction, xref, yref, xsec, ysec, out var dx0, out var dy0, out var scale1, out var angle1, out var dy1, out var dx1, hwindow: int, harea: int, mblend: int, bandno: int);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial mosaic</param>
        /// <param name="xref">Position of reference tie-point</param>
        /// <param name="yref">Position of reference tie-point</param>
        /// <param name="xsec">Position of secondary tie-point</param>
        /// <param name="ysec">Position of secondary tie-point</param>
        /// <param name="dx0">Detected integer offset</param>
        /// <param name="dy0">Detected integer offset</param>
        /// <param name="scale1">Detected scale</param>
        /// <param name="angle1">Detected rotation</param>
        /// <param name="dy1">Detected first-order displacement</param>
        /// <param name="dx1">Detected first-order displacement</param>
        /// <param name="hwindow">Half window size</param>
        /// <param name="harea">Half area size</param>
        /// <param name="mblend">Maximum blend size</param>
        /// <param name="bandno">Band to search for features on</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mosaic(Image sec, string direction, int xref, int yref, int xsec, int ysec, out int dx0,
            out int dy0, out double scale1, out double angle1, out double dy1, out double dx1, int? hwindow = null,
            int? harea = null, int? mblend = null, int? bandno = null)
        {
            var options = new VOption();

            if (hwindow.HasValue)
            {
                options.Add("hwindow", hwindow);
            }

            if (harea.HasValue)
            {
                options.Add("harea", harea);
            }

            if (mblend.HasValue)
            {
                options.Add("mblend", mblend);
            }

            if (bandno.HasValue)
            {
                options.Add("bandno", bandno);
            }

            options.Add("dx0", true);
            options.Add("dy0", true);
            options.Add("scale1", true);
            options.Add("angle1", true);
            options.Add("dy1", true);
            options.Add("dx1", true);

            var results = this.Call("mosaic", options, sec, direction, xref, yref, xsec, ysec) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            dx0 = opts?["dx0"] is int out1 ? out1 : 0;
            dy0 = opts?["dy0"] is int out2 ? out2 : 0;
            scale1 = opts?["scale1"] is double out3 ? out3 : 0;
            angle1 = opts?["angle1"] is double out4 ? out4 : 0;
            dy1 = opts?["dy1"] is double out5 ? out5 : 0;
            dx1 = opts?["dx1"] is double out6 ? out6 : 0;

            return finalResult;
        }

        /// <summary>
        /// First-order mosaic of two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = ref.Mosaic1(sec, direction, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, hwindow: int, harea: int, search: bool, interpolate: GObject, mblend: int, bandno: int);
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial mosaic</param>
        /// <param name="xr1">Position of first reference tie-point</param>
        /// <param name="yr1">Position of first reference tie-point</param>
        /// <param name="xs1">Position of first secondary tie-point</param>
        /// <param name="ys1">Position of first secondary tie-point</param>
        /// <param name="xr2">Position of second reference tie-point</param>
        /// <param name="yr2">Position of second reference tie-point</param>
        /// <param name="xs2">Position of second secondary tie-point</param>
        /// <param name="ys2">Position of second secondary tie-point</param>
        /// <param name="hwindow">Half window size</param>
        /// <param name="harea">Half area size</param>
        /// <param name="search">Search to improve tie-points</param>
        /// <param name="interpolate">Interpolate pixels with this</param>
        /// <param name="mblend">Maximum blend size</param>
        /// <param name="bandno">Band to search for features on</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mosaic1(Image sec, string direction, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2,
            int ys2, int? hwindow = null, int? harea = null, bool? search = null, GObject interpolate = null,
            int? mblend = null, int? bandno = null)
        {
            var options = new VOption();

            if (hwindow.HasValue)
            {
                options.Add("hwindow", hwindow);
            }

            if (harea.HasValue)
            {
                options.Add("harea", harea);
            }

            if (search.HasValue)
            {
                options.Add("search", search);
            }

            if (interpolate != null)
            {
                options.Add("interpolate", interpolate);
            }

            if (mblend.HasValue)
            {
                options.Add("mblend", mblend);
            }

            if (bandno.HasValue)
            {
                options.Add("bandno", bandno);
            }

            return this.Call("mosaic1", options, sec, direction, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2) as Image;
        }

        /// <summary>
        /// Pick most-significant byte from an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Msb(band: int);
        /// </code>
        /// </example>
        /// <param name="band">Band to msb</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Msb(int? band = null)
        {
            var options = new VOption();

            if (band.HasValue)
            {
                options.Add("band", band);
            }

            return this.Call("msb", options) as Image;
        }

        /// <summary>
        /// Multiply two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Multiply(right);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Multiply(Image right)
        {
            return this.Call("multiply", right) as Image;
        }

        /// <summary>
        /// Load file with OpenSlide
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Openslideload(filename, memory: bool, access: string, level: int, autocrop: bool, fail: bool, associated: string, attachAssociated: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="level">Load this level from the file</param>
        /// <param name="autocrop">Crop to image bounds</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="associated">Load this associated image</param>
        /// <param name="attachAssociated">Attach all asssociated images</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Openslideload(string filename, bool? memory = null, string access = null, int? level = null,
            bool? autocrop = null, bool? fail = null, string associated = null, bool? attachAssociated = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (level.HasValue)
            {
                options.Add("level", level);
            }

            if (autocrop.HasValue)
            {
                options.Add("autocrop", autocrop);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (associated != null)
            {
                options.Add("associated", associated);
            }

            if (attachAssociated.HasValue)
            {
                options.Add("attach_associated", attachAssociated);
            }

            return Operation.Call("openslideload", options, filename) as Image;
        }

        /// <summary>
        /// Load file with OpenSlide
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Openslideload(filename, out var flags, memory: bool, access: string, level: int, autocrop: bool, fail: bool, associated: string, attachAssociated: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="level">Load this level from the file</param>
        /// <param name="autocrop">Crop to image bounds</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="associated">Load this associated image</param>
        /// <param name="attachAssociated">Attach all asssociated images</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Openslideload(string filename, out int flags, bool? memory = null, string access = null,
            int? level = null, bool? autocrop = null, bool? fail = null, string associated = null,
            bool? attachAssociated = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (level.HasValue)
            {
                options.Add("level", level);
            }

            if (autocrop.HasValue)
            {
                options.Add("autocrop", autocrop);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (associated != null)
            {
                options.Add("associated", associated);
            }

            if (attachAssociated.HasValue)
            {
                options.Add("attach_associated", attachAssociated);
            }

            options.Add("flags", true);

            var results = Operation.Call("openslideload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load PDF with libpoppler
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Pdfload(filename, memory: bool, access: string, page: int, n: int, fail: bool, dpi: double, scale: double);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="dpi">Render at this DPI</param>
        /// <param name="scale">Scale output by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Pdfload(string filename, bool? memory = null, string access = null, int? page = null,
            int? n = null, bool? fail = null, double? dpi = null, double? scale = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            return Operation.Call("pdfload", options, filename) as Image;
        }

        /// <summary>
        /// Load PDF with libpoppler
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Pdfload(filename, out var flags, memory: bool, access: string, page: int, n: int, fail: bool, dpi: double, scale: double);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="dpi">Render at this DPI</param>
        /// <param name="scale">Scale output by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Pdfload(string filename, out int flags, bool? memory = null, string access = null,
            int? page = null, int? n = null, bool? fail = null, double? dpi = null, double? scale = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            options.Add("flags", true);

            var results = Operation.Call("pdfload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load PDF with libpoppler
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.PdfloadBuffer(buffer, memory: bool, access: string, page: int, n: int, fail: bool, dpi: double, scale: double);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="dpi">Render at this DPI</param>
        /// <param name="scale">Scale output by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image PdfloadBuffer(byte[] buffer, bool? memory = null, string access = null, int? page = null,
            int? n = null, bool? fail = null, double? dpi = null, double? scale = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            return Operation.Call("pdfload_buffer", options, buffer) as Image;
        }

        /// <summary>
        /// Load PDF with libpoppler
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.PdfloadBuffer(buffer, out var flags, memory: bool, access: string, page: int, n: int, fail: bool, dpi: double, scale: double);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the file</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="dpi">Render at this DPI</param>
        /// <param name="scale">Scale output by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image PdfloadBuffer(byte[] buffer, out int flags, bool? memory = null, string access = null,
            int? page = null, int? n = null, bool? fail = null, double? dpi = null, double? scale = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            options.Add("flags", true);

            var results = Operation.Call("pdfload_buffer", options, buffer) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Find threshold for percent of pixels
        /// </summary>
        /// <example>
        /// <code>
        /// int threshold = in.Percent(percent);
        /// </code>
        /// </example>
        /// <param name="percent">Percent of pixels</param>
        /// <returns>A int</returns>
        public int Percent(double percent)
        {
            return this.Call("percent", percent) is int result ? result : 0;
        }

        /// <summary>
        /// Make a perlin noise image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Perlin(width, height, cellSize: int, uchar: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="cellSize">Size of Perlin cells</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Perlin(int width, int height, int? cellSize = null, bool? uchar = null)
        {
            var options = new VOption();

            if (cellSize.HasValue)
            {
                options.Add("cell_size", cellSize);
            }

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            return Operation.Call("perlin", options, width, height) as Image;
        }

        /// <summary>
        /// Calculate phase correlation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Phasecor(in2);
        /// </code>
        /// </example>
        /// <param name="in2">Second input image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Phasecor(Image in2)
        {
            return this.Call("phasecor", in2) as Image;
        }

        /// <summary>
        /// Load png from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Pngload(filename, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Pngload(string filename, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

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

            return Operation.Call("pngload", options, filename) as Image;
        }

        /// <summary>
        /// Load png from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Pngload(filename, out var flags, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Pngload(string filename, out int flags, bool? memory = null, string access = null,
            bool? fail = null)
        {
            var options = new VOption();

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

            options.Add("flags", true);

            var results = Operation.Call("pngload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load png from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.PngloadBuffer(buffer, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image PngloadBuffer(byte[] buffer, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

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

            return Operation.Call("pngload_buffer", options, buffer) as Image;
        }

        /// <summary>
        /// Load png from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.PngloadBuffer(buffer, out var flags, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image PngloadBuffer(byte[] buffer, out int flags, bool? memory = null, string access = null,
            bool? fail = null)
        {
            var options = new VOption();

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

            options.Add("flags", true);

            var results = Operation.Call("pngload_buffer", options, buffer) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Save image to png file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Pngsave(filename, compression: int, interlace: bool, pageHeight: int, profile: string, filter: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="compression">Compression factor</param>
        /// <param name="interlace">Interlace image</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="profile">ICC profile to embed</param>
        /// <param name="filter">libpng row filter flag(s)</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Pngsave(string filename, int? compression = null, bool? interlace = null, int? pageHeight = null,
            string profile = null, int? filter = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (compression.HasValue)
            {
                options.Add("compression", compression);
            }

            if (interlace.HasValue)
            {
                options.Add("interlace", interlace);
            }

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (profile != null)
            {
                options.Add("profile", profile);
            }

            if (filter.HasValue)
            {
                options.Add("filter", filter);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("pngsave", options, filename);
        }

        /// <summary>
        /// Save image to png buffer
        /// </summary>
        /// <example>
        /// <code>
        /// byte[] buffer = in.PngsaveBuffer(compression: int, interlace: bool, pageHeight: int, profile: string, filter: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="compression">Compression factor</param>
        /// <param name="interlace">Interlace image</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="profile">ICC profile to embed</param>
        /// <param name="filter">libpng row filter flag(s)</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>An array of bytes</returns>
        public byte[] PngsaveBuffer(int? compression = null, bool? interlace = null, int? pageHeight = null,
            string profile = null, int? filter = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (compression.HasValue)
            {
                options.Add("compression", compression);
            }

            if (interlace.HasValue)
            {
                options.Add("interlace", interlace);
            }

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (profile != null)
            {
                options.Add("profile", profile);
            }

            if (filter.HasValue)
            {
                options.Add("filter", filter);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("pngsave_buffer", options) as byte[];
        }

        /// <summary>
        /// Load ppm from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Ppmload(filename, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Ppmload(string filename, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

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

            return Operation.Call("ppmload", options, filename) as Image;
        }

        /// <summary>
        /// Load ppm from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Ppmload(filename, out var flags, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Ppmload(string filename, out int flags, bool? memory = null, string access = null,
            bool? fail = null)
        {
            var options = new VOption();

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

            options.Add("flags", true);

            var results = Operation.Call("ppmload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Save image to ppm file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Ppmsave(filename, pageHeight: int, ascii: bool, squash: bool, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="ascii">save as ascii</param>
        /// <param name="squash">save as one bit</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Ppmsave(string filename, int? pageHeight = null, bool? ascii = null, bool? squash = null,
            bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (ascii.HasValue)
            {
                options.Add("ascii", ascii);
            }

            if (squash.HasValue)
            {
                options.Add("squash", squash);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("ppmsave", options, filename);
        }

        /// <summary>
        /// Premultiply image alpha
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Premultiply(maxAlpha: double);
        /// </code>
        /// </example>
        /// <param name="maxAlpha">Maximum value of alpha channel</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Premultiply(double? maxAlpha = null)
        {
            var options = new VOption();

            if (maxAlpha.HasValue)
            {
                options.Add("max_alpha", maxAlpha);
            }

            return this.Call("premultiply", options) as Image;
        }

        /// <summary>
        /// Find image profiles
        /// </summary>
        /// <example>
        /// <code>
        /// var output = in.Profile();
        /// </code>
        /// </example>
        /// <returns>An array of objects</returns>
        public object[] Profile()
        {
            return this.Call("profile") as object[];
        }

        /// <summary>
        /// Find image projections
        /// </summary>
        /// <example>
        /// <code>
        /// var output = in.Project();
        /// </code>
        /// </example>
        /// <returns>An array of objects</returns>
        public object[] Project()
        {
            return this.Call("project") as object[];
        }

        /// <summary>
        /// Resample an image with a quadratic transform
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Quadratic(coeff, interpolate: GObject);
        /// </code>
        /// </example>
        /// <param name="coeff">Coefficient matrix</param>
        /// <param name="interpolate">Interpolate values with this</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Quadratic(Image coeff, GObject interpolate = null)
        {
            var options = new VOption();

            if (interpolate != null)
            {
                options.Add("interpolate", interpolate);
            }

            return this.Call("quadratic", options, coeff) as Image;
        }

        /// <summary>
        /// Unpack Radiance coding to float RGB
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Rad2float();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rad2float()
        {
            return this.Call("rad2float") as Image;
        }

        /// <summary>
        /// Load a Radiance image from a file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Radload(filename, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Radload(string filename, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

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

            return Operation.Call("radload", options, filename) as Image;
        }

        /// <summary>
        /// Load a Radiance image from a file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Radload(filename, out var flags, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Radload(string filename, out int flags, bool? memory = null, string access = null,
            bool? fail = null)
        {
            var options = new VOption();

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

            options.Add("flags", true);

            var results = Operation.Call("radload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Save image to Radiance file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Radsave(filename, pageHeight: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Radsave(string filename, int? pageHeight = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("radsave", options, filename);
        }

        /// <summary>
        /// Save image to Radiance buffer
        /// </summary>
        /// <example>
        /// <code>
        /// byte[] buffer = in.RadsaveBuffer(pageHeight: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>An array of bytes</returns>
        public byte[] RadsaveBuffer(int? pageHeight = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("radsave_buffer", options) as byte[];
        }

        /// <summary>
        /// Rank filter
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Rank(width, height, index);
        /// </code>
        /// </example>
        /// <param name="width">Window width in pixels</param>
        /// <param name="height">Window height in pixels</param>
        /// <param name="index">Select pixel at index</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rank(int width, int height, int index)
        {
            return this.Call("rank", width, height, index) as Image;
        }

        /// <summary>
        /// Save image to raw file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Rawsave(filename, pageHeight: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Rawsave(string filename, int? pageHeight = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("rawsave", options, filename);
        }

        /// <summary>
        /// Write raw image to file descriptor
        /// </summary>
        /// <example>
        /// <code>
        /// in.RawsaveFd(fd, pageHeight: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="fd">File descriptor to write to</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void RawsaveFd(int fd, int? pageHeight = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("rawsave_fd", options, fd);
        }

        /// <summary>
        /// Linear recombination with matrix
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Recomb(m);
        /// </code>
        /// </example>
        /// <param name="m">matrix of coefficients</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Recomb(Image m)
        {
            return this.Call("recomb", m) as Image;
        }

        /// <summary>
        /// Reduce an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Reduce(hshrink, vshrink, kernel: string, centre: bool);
        /// </code>
        /// </example>
        /// <param name="hshrink">Horizontal shrink factor</param>
        /// <param name="vshrink">Vertical shrink factor</param>
        /// <param name="kernel">Resampling kernel</param>
        /// <param name="centre">Use centre sampling convention</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Reduce(double hshrink, double vshrink, string kernel = null, bool? centre = null)
        {
            var options = new VOption();

            if (kernel != null)
            {
                options.Add("kernel", kernel);
            }

            if (centre.HasValue)
            {
                options.Add("centre", centre);
            }

            return this.Call("reduce", options, hshrink, vshrink) as Image;
        }

        /// <summary>
        /// Shrink an image horizontally
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Reduceh(hshrink, kernel: string, centre: bool);
        /// </code>
        /// </example>
        /// <param name="hshrink">Horizontal shrink factor</param>
        /// <param name="kernel">Resampling kernel</param>
        /// <param name="centre">Use centre sampling convention</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Reduceh(double hshrink, string kernel = null, bool? centre = null)
        {
            var options = new VOption();

            if (kernel != null)
            {
                options.Add("kernel", kernel);
            }

            if (centre.HasValue)
            {
                options.Add("centre", centre);
            }

            return this.Call("reduceh", options, hshrink) as Image;
        }

        /// <summary>
        /// Shrink an image vertically
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Reducev(vshrink, kernel: string, centre: bool);
        /// </code>
        /// </example>
        /// <param name="vshrink">Vertical shrink factor</param>
        /// <param name="kernel">Resampling kernel</param>
        /// <param name="centre">Use centre sampling convention</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Reducev(double vshrink, string kernel = null, bool? centre = null)
        {
            var options = new VOption();

            if (kernel != null)
            {
                options.Add("kernel", kernel);
            }

            if (centre.HasValue)
            {
                options.Add("centre", centre);
            }

            return this.Call("reducev", options, vshrink) as Image;
        }

        /// <summary>
        /// Relational operation on two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Relational(right, relational);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <param name="relational">relational to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Relational(Image right, string relational)
        {
            return this.Call("relational", right, relational) as Image;
        }

        /// <summary>
        /// Relational operations against a constant
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.RelationalConst(relational, c);
        /// </code>
        /// </example>
        /// <param name="relational">relational to perform</param>
        /// <param name="c">Array of constants</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image RelationalConst(string relational, double[] c)
        {
            return this.Call("relational_const", relational, c) as Image;
        }

        /// <summary>
        /// Remainder after integer division of two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Remainder(right);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Remainder(Image right)
        {
            return this.Call("remainder", right) as Image;
        }

        /// <summary>
        /// Remainder after integer division of an image and a constant
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.RemainderConst(c);
        /// </code>
        /// </example>
        /// <param name="c">Array of constants</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image RemainderConst(double[] c)
        {
            return this.Call("remainder_const", c) as Image;
        }

        /// <summary>
        /// Replicate an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Replicate(across, down);
        /// </code>
        /// </example>
        /// <param name="across">Repeat this many times horizontally</param>
        /// <param name="down">Repeat this many times vertically</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Replicate(int across, int down)
        {
            return this.Call("replicate", across, down) as Image;
        }

        /// <summary>
        /// Resize an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Resize(scale, kernel: string, vscale: double);
        /// </code>
        /// </example>
        /// <param name="scale">Scale image by this factor</param>
        /// <param name="kernel">Resampling kernel</param>
        /// <param name="vscale">Vertical scale image by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Resize(double scale, string kernel = null, double? vscale = null)
        {
            var options = new VOption();

            if (kernel != null)
            {
                options.Add("kernel", kernel);
            }

            if (vscale.HasValue)
            {
                options.Add("vscale", vscale);
            }

            return this.Call("resize", options, scale) as Image;
        }

        /// <summary>
        /// Rotate an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Rot(angle);
        /// </code>
        /// </example>
        /// <param name="angle">Angle to rotate image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rot(string angle)
        {
            return this.Call("rot", angle) as Image;
        }

        /// <summary>
        /// Rotate an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Rot45(angle: string);
        /// </code>
        /// </example>
        /// <param name="angle">Angle to rotate image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rot45(string angle = null)
        {
            var options = new VOption();

            if (angle != null)
            {
                options.Add("angle", angle);
            }

            return this.Call("rot45", options) as Image;
        }

        /// <summary>
        /// Perform a round function on an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Round(round);
        /// </code>
        /// </example>
        /// <param name="round">rounding operation to perform</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Round(string round)
        {
            return this.Call("round", round) as Image;
        }

        /// <summary>
        /// Convert scRGB to BW
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.ScRGB2BW(depth: int);
        /// </code>
        /// </example>
        /// <param name="depth">Output device space depth in bits</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ScRGB2BW(int? depth = null)
        {
            var options = new VOption();

            if (depth.HasValue)
            {
                options.Add("depth", depth);
            }

            return this.Call("scRGB2BW", options) as Image;
        }

        /// <summary>
        /// Convert an scRGB image to sRGB
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.ScRGB2sRGB(depth: int);
        /// </code>
        /// </example>
        /// <param name="depth">Output device space depth in bits</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ScRGB2sRGB(int? depth = null)
        {
            var options = new VOption();

            if (depth.HasValue)
            {
                options.Add("depth", depth);
            }

            return this.Call("scRGB2sRGB", options) as Image;
        }

        /// <summary>
        /// Transform scRGB to XYZ
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.ScRGB2XYZ();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ScRGB2XYZ()
        {
            return this.Call("scRGB2XYZ") as Image;
        }

        /// <summary>
        /// Check sequential access
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Sequential(tileHeight: int);
        /// </code>
        /// </example>
        /// <param name="tileHeight">Tile height in pixels</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Sequential(int? tileHeight = null)
        {
            var options = new VOption();

            if (tileHeight.HasValue)
            {
                options.Add("tile_height", tileHeight);
            }

            return this.Call("sequential", options) as Image;
        }

        /// <summary>
        /// Unsharp masking for print
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Sharpen(sigma: double, x1: double, y2: double, y3: double, m1: double, m2: double);
        /// </code>
        /// </example>
        /// <param name="sigma">Sigma of Gaussian</param>
        /// <param name="x1">Flat/jaggy threshold</param>
        /// <param name="y2">Maximum brightening</param>
        /// <param name="y3">Maximum darkening</param>
        /// <param name="m1">Slope for flat areas</param>
        /// <param name="m2">Slope for jaggy areas</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Sharpen(double? sigma = null, double? x1 = null, double? y2 = null, double? y3 = null,
            double? m1 = null, double? m2 = null)
        {
            var options = new VOption();

            if (sigma.HasValue)
            {
                options.Add("sigma", sigma);
            }

            if (x1.HasValue)
            {
                options.Add("x1", x1);
            }

            if (y2.HasValue)
            {
                options.Add("y2", y2);
            }

            if (y3.HasValue)
            {
                options.Add("y3", y3);
            }

            if (m1.HasValue)
            {
                options.Add("m1", m1);
            }

            if (m2.HasValue)
            {
                options.Add("m2", m2);
            }

            return this.Call("sharpen", options) as Image;
        }

        /// <summary>
        /// Shrink an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Shrink(hshrink, vshrink);
        /// </code>
        /// </example>
        /// <param name="hshrink">Horizontal shrink factor</param>
        /// <param name="vshrink">Vertical shrink factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Shrink(double hshrink, double vshrink)
        {
            return this.Call("shrink", hshrink, vshrink) as Image;
        }

        /// <summary>
        /// Shrink an image horizontally
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Shrinkh(hshrink);
        /// </code>
        /// </example>
        /// <param name="hshrink">Horizontal shrink factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Shrinkh(int hshrink)
        {
            return this.Call("shrinkh", hshrink) as Image;
        }

        /// <summary>
        /// Shrink an image vertically
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Shrinkv(vshrink);
        /// </code>
        /// </example>
        /// <param name="vshrink">Vertical shrink factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Shrinkv(int vshrink)
        {
            return this.Call("shrinkv", vshrink) as Image;
        }

        /// <summary>
        /// Unit vector of pixel
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Sign();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Sign()
        {
            return this.Call("sign") as Image;
        }

        /// <summary>
        /// Similarity transform of an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Similarity(background: double[], interpolate: GObject, scale: double, angle: double, odx: double, ody: double, idx: double, idy: double);
        /// </code>
        /// </example>
        /// <param name="background">Background value</param>
        /// <param name="interpolate">Interpolate pixels with this</param>
        /// <param name="scale">Scale by this factor</param>
        /// <param name="angle">Rotate anticlockwise by this many degrees</param>
        /// <param name="odx">Horizontal output displacement</param>
        /// <param name="ody">Vertical output displacement</param>
        /// <param name="idx">Horizontal input displacement</param>
        /// <param name="idy">Vertical input displacement</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Similarity(double[] background = null, GObject interpolate = null, double? scale = null,
            double? angle = null, double? odx = null, double? ody = null, double? idx = null, double? idy = null)
        {
            var options = new VOption();

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            if (interpolate != null)
            {
                options.Add("interpolate", interpolate);
            }

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            if (angle.HasValue)
            {
                options.Add("angle", angle);
            }

            if (odx.HasValue)
            {
                options.Add("odx", odx);
            }

            if (ody.HasValue)
            {
                options.Add("ody", ody);
            }

            if (idx.HasValue)
            {
                options.Add("idx", idx);
            }

            if (idy.HasValue)
            {
                options.Add("idy", idy);
            }

            return this.Call("similarity", options) as Image;
        }

        /// <summary>
        /// Make a 2D sine wave
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Sines(width, height, uchar: bool, hfreq: double, vfreq: double);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <param name="hfreq">Horizontal spatial frequency</param>
        /// <param name="vfreq">Vertical spatial frequency</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Sines(int width, int height, bool? uchar = null, double? hfreq = null, double? vfreq = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            if (hfreq.HasValue)
            {
                options.Add("hfreq", hfreq);
            }

            if (vfreq.HasValue)
            {
                options.Add("vfreq", vfreq);
            }

            return Operation.Call("sines", options, width, height) as Image;
        }

        /// <summary>
        /// Extract an area from an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = input.Smartcrop(width, height, interesting: string);
        /// </code>
        /// </example>
        /// <param name="width">Width of extract area</param>
        /// <param name="height">Height of extract area</param>
        /// <param name="interesting">How to measure interestingness</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Smartcrop(int width, int height, string interesting = null)
        {
            var options = new VOption();

            if (interesting != null)
            {
                options.Add("interesting", interesting);
            }

            return this.Call("smartcrop", options, width, height) as Image;
        }

        /// <summary>
        /// Spatial correlation
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Spcor(@ref);
        /// </code>
        /// </example>
        /// <param name="ref">Input reference image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Spcor(Image @ref)
        {
            return this.Call("spcor", @ref) as Image;
        }

        /// <summary>
        /// Make displayable power spectrum
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Spectrum();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Spectrum()
        {
            return this.Call("spectrum") as Image;
        }

        /// <summary>
        /// Transform sRGB to HSV
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.SRGB2HSV();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image SRGB2HSV()
        {
            return this.Call("sRGB2HSV") as Image;
        }

        /// <summary>
        /// Convert an sRGB image to scRGB
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.SRGB2scRGB();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image SRGB2scRGB()
        {
            return this.Call("sRGB2scRGB") as Image;
        }

        /// <summary>
        /// Find image average
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Stats();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Stats()
        {
            return this.Call("stats") as Image;
        }

        /// <summary>
        /// Statistical difference
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Stdif(width, height, s0: double, b: double, m0: double, a: double);
        /// </code>
        /// </example>
        /// <param name="width">Window width in pixels</param>
        /// <param name="height">Window height in pixels</param>
        /// <param name="s0">New deviation</param>
        /// <param name="b">Weight of new deviation</param>
        /// <param name="m0">New mean</param>
        /// <param name="a">Weight of new mean</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Stdif(int width, int height, double? s0 = null, double? b = null, double? m0 = null,
            double? a = null)
        {
            var options = new VOption();

            if (s0.HasValue)
            {
                options.Add("s0", s0);
            }

            if (b.HasValue)
            {
                options.Add("b", b);
            }

            if (m0.HasValue)
            {
                options.Add("m0", m0);
            }

            if (a.HasValue)
            {
                options.Add("a", a);
            }

            return this.Call("stdif", options, width, height) as Image;
        }

        /// <summary>
        /// Subsample an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = input.Subsample(xfac, yfac, point: bool);
        /// </code>
        /// </example>
        /// <param name="xfac">Horizontal subsample factor</param>
        /// <param name="yfac">Vertical subsample factor</param>
        /// <param name="point">Point sample</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Subsample(int xfac, int yfac, bool? point = null)
        {
            var options = new VOption();

            if (point.HasValue)
            {
                options.Add("point", point);
            }

            return this.Call("subsample", options, xfac, yfac) as Image;
        }

        /// <summary>
        /// Subtract two images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = left.Subtract(right);
        /// </code>
        /// </example>
        /// <param name="right">Right-hand image argument</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Subtract(Image right)
        {
            return this.Call("subtract", right) as Image;
        }

        /// <summary>
        /// Sum an array of images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Sum(@in);
        /// </code>
        /// </example>
        /// <param name="in">Array of input images</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Sum(Image[] @in)
        {
            return Operation.Call("sum", new object[] {@in}) as Image;
        }

        /// <summary>
        /// Load SVG with rsvg
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Svgload(filename, memory: bool, access: string, dpi: double, fail: bool, scale: double);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="dpi">Render at this DPI</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="scale">Scale output by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Svgload(string filename, bool? memory = null, string access = null, double? dpi = null,
            bool? fail = null, double? scale = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            return Operation.Call("svgload", options, filename) as Image;
        }

        /// <summary>
        /// Load SVG with rsvg
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Svgload(filename, out var flags, memory: bool, access: string, dpi: double, fail: bool, scale: double);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="dpi">Render at this DPI</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="scale">Scale output by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Svgload(string filename, out int flags, bool? memory = null, string access = null,
            double? dpi = null, bool? fail = null, double? scale = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            options.Add("flags", true);

            var results = Operation.Call("svgload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load SVG with rsvg
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.SvgloadBuffer(buffer, memory: bool, access: string, dpi: double, fail: bool, scale: double);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="dpi">Render at this DPI</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="scale">Scale output by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image SvgloadBuffer(byte[] buffer, bool? memory = null, string access = null, double? dpi = null,
            bool? fail = null, double? scale = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            return Operation.Call("svgload_buffer", options, buffer) as Image;
        }

        /// <summary>
        /// Load SVG with rsvg
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.SvgloadBuffer(buffer, out var flags, memory: bool, access: string, dpi: double, fail: bool, scale: double);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="dpi">Render at this DPI</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="scale">Scale output by this factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image SvgloadBuffer(byte[] buffer, out int flags, bool? memory = null, string access = null,
            double? dpi = null, bool? fail = null, double? scale = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (scale.HasValue)
            {
                options.Add("scale", scale);
            }

            options.Add("flags", true);

            var results = Operation.Call("svgload_buffer", options, buffer) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Run an external command
        /// </summary>
        /// <example>
        /// <code>
        /// NetVips.Image.System(cmdFormat, @in: Image[], outFormat: string, inFormat: string);
        /// </code>
        /// </example>
        /// <param name="cmdFormat">Command to run</param>
        /// <param name="in">Array of input images</param>
        /// <param name="outFormat">Format for output filename</param>
        /// <param name="inFormat">Format for input filename</param>
        /// <returns>None</returns>
        public static void System(string cmdFormat, Image[] @in = null, string outFormat = null, string inFormat = null)
        {
            var options = new VOption();

            if (@in != null && @in.Length > 0)
            {
                options.Add("in", @in);
            }

            if (outFormat != null)
            {
                options.Add("out_format", outFormat);
            }

            if (inFormat != null)
            {
                options.Add("in_format", inFormat);
            }

            Operation.Call("system", options, cmdFormat);
        }

        /// <summary>
        /// Run an external command
        /// </summary>
        /// <example>
        /// <code>
        /// NetVips.Image.System(cmdFormat, out var @out, @in: Image[], outFormat: string, inFormat: string);
        /// </code>
        /// </example>
        /// <param name="cmdFormat">Command to run</param>
        /// <param name="out">Output image</param>
        /// <param name="in">Array of input images</param>
        /// <param name="outFormat">Format for output filename</param>
        /// <param name="inFormat">Format for input filename</param>
        /// <returns>None</returns>
        public static void System(string cmdFormat, out Image @out, Image[] @in = null, string outFormat = null,
            string inFormat = null)
        {
            var options = new VOption();

            if (@in != null && @in.Length > 0)
            {
                options.Add("in", @in);
            }

            if (outFormat != null)
            {
                options.Add("out_format", outFormat);
            }

            if (inFormat != null)
            {
                options.Add("in_format", inFormat);
            }

            options.Add("out", true);

            var results = Operation.Call("system", options, cmdFormat) as object[];

            var opts = results?[1] as VOption;
            @out = opts?["out"] as Image;
        }

        /// <summary>
        /// Run an external command
        /// </summary>
        /// <example>
        /// <code>
        /// NetVips.Image.System(cmdFormat, out var @out, out var log, @in: Image[], outFormat: string, inFormat: string);
        /// </code>
        /// </example>
        /// <param name="cmdFormat">Command to run</param>
        /// <param name="out">Output image</param>
        /// <param name="log">Command log</param>
        /// <param name="in">Array of input images</param>
        /// <param name="outFormat">Format for output filename</param>
        /// <param name="inFormat">Format for input filename</param>
        /// <returns>None</returns>
        public static void System(string cmdFormat, out Image @out, out string log, Image[] @in = null,
            string outFormat = null, string inFormat = null)
        {
            var options = new VOption();

            if (@in != null && @in.Length > 0)
            {
                options.Add("in", @in);
            }

            if (outFormat != null)
            {
                options.Add("out_format", outFormat);
            }

            if (inFormat != null)
            {
                options.Add("in_format", inFormat);
            }

            options.Add("out", true);
            options.Add("log", true);

            var results = Operation.Call("system", options, cmdFormat) as object[];

            var opts = results?[1] as VOption;
            @out = opts?["out"] as Image;
            log = opts?["log"] is string out2 ? out2 : null;
        }

        /// <summary>
        /// Make a text image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Text(text, font: string, width: int, height: int, align: string, dpi: int, spacing: int);
        /// </code>
        /// </example>
        /// <param name="text">Text to render</param>
        /// <param name="font">Font to render with</param>
        /// <param name="width">Maximum image width in pixels</param>
        /// <param name="height">Maximum image height in pixels</param>
        /// <param name="align">Align on the low, centre or high edge</param>
        /// <param name="dpi">DPI to render at</param>
        /// <param name="spacing">Line spacing</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Text(string text, string font = null, int? width = null, int? height = null,
            string align = null, int? dpi = null, int? spacing = null)
        {
            var options = new VOption();

            if (font != null)
            {
                options.Add("font", font);
            }

            if (width.HasValue)
            {
                options.Add("width", width);
            }

            if (height.HasValue)
            {
                options.Add("height", height);
            }

            if (align != null)
            {
                options.Add("align", align);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (spacing.HasValue)
            {
                options.Add("spacing", spacing);
            }

            return Operation.Call("text", options, text) as Image;
        }

        /// <summary>
        /// Make a text image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Text(text, out var autofitDpi, font: string, width: int, height: int, align: string, dpi: int, spacing: int);
        /// </code>
        /// </example>
        /// <param name="text">Text to render</param>
        /// <param name="autofitDpi">DPI selected by autofit</param>
        /// <param name="font">Font to render with</param>
        /// <param name="width">Maximum image width in pixels</param>
        /// <param name="height">Maximum image height in pixels</param>
        /// <param name="align">Align on the low, centre or high edge</param>
        /// <param name="dpi">DPI to render at</param>
        /// <param name="spacing">Line spacing</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Text(string text, out int autofitDpi, string font = null, int? width = null,
            int? height = null, string align = null, int? dpi = null, int? spacing = null)
        {
            var options = new VOption();

            if (font != null)
            {
                options.Add("font", font);
            }

            if (width.HasValue)
            {
                options.Add("width", width);
            }

            if (height.HasValue)
            {
                options.Add("height", height);
            }

            if (align != null)
            {
                options.Add("align", align);
            }

            if (dpi.HasValue)
            {
                options.Add("dpi", dpi);
            }

            if (spacing.HasValue)
            {
                options.Add("spacing", spacing);
            }

            options.Add("autofit_dpi", true);

            var results = Operation.Call("text", options, text) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            autofitDpi = opts?["autofit_dpi"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Generate thumbnail from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Thumbnail(filename, width, height: int, size: string, autoRotate: bool, crop: string, linear: bool, importProfile: string, exportProfile: string, intent: string);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to read from</param>
        /// <param name="width">Size to this width</param>
        /// <param name="height">Size to this height</param>
        /// <param name="size">Only upsize, only downsize, or both</param>
        /// <param name="autoRotate">Use orientation tags to rotate image upright</param>
        /// <param name="crop">Reduce to fill target rectangle, then crop</param>
        /// <param name="linear">Reduce in linear light</param>
        /// <param name="importProfile">Fallback import profile</param>
        /// <param name="exportProfile">Fallback export profile</param>
        /// <param name="intent">Rendering intent</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Thumbnail(string filename, int width, int? height = null, string size = null,
            bool? autoRotate = null, string crop = null, bool? linear = null, string importProfile = null,
            string exportProfile = null, string intent = null)
        {
            var options = new VOption();

            if (height.HasValue)
            {
                options.Add("height", height);
            }

            if (size != null)
            {
                options.Add("size", size);
            }

            if (autoRotate.HasValue)
            {
                options.Add("auto_rotate", autoRotate);
            }

            if (crop != null)
            {
                options.Add("crop", crop);
            }

            if (linear.HasValue)
            {
                options.Add("linear", linear);
            }

            if (importProfile != null)
            {
                options.Add("import_profile", importProfile);
            }

            if (exportProfile != null)
            {
                options.Add("export_profile", exportProfile);
            }

            if (intent != null)
            {
                options.Add("intent", intent);
            }

            return Operation.Call("thumbnail", options, filename, width) as Image;
        }

        /// <summary>
        /// Generate thumbnail from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.ThumbnailBuffer(buffer, width, height: int, size: string, autoRotate: bool, crop: string, linear: bool, importProfile: string, exportProfile: string, intent: string);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="width">Size to this width</param>
        /// <param name="height">Size to this height</param>
        /// <param name="size">Only upsize, only downsize, or both</param>
        /// <param name="autoRotate">Use orientation tags to rotate image upright</param>
        /// <param name="crop">Reduce to fill target rectangle, then crop</param>
        /// <param name="linear">Reduce in linear light</param>
        /// <param name="importProfile">Fallback import profile</param>
        /// <param name="exportProfile">Fallback export profile</param>
        /// <param name="intent">Rendering intent</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image ThumbnailBuffer(byte[] buffer, int width, int? height = null, string size = null,
            bool? autoRotate = null, string crop = null, bool? linear = null, string importProfile = null,
            string exportProfile = null, string intent = null)
        {
            var options = new VOption();

            if (height.HasValue)
            {
                options.Add("height", height);
            }

            if (size != null)
            {
                options.Add("size", size);
            }

            if (autoRotate.HasValue)
            {
                options.Add("auto_rotate", autoRotate);
            }

            if (crop != null)
            {
                options.Add("crop", crop);
            }

            if (linear.HasValue)
            {
                options.Add("linear", linear);
            }

            if (importProfile != null)
            {
                options.Add("import_profile", importProfile);
            }

            if (exportProfile != null)
            {
                options.Add("export_profile", exportProfile);
            }

            if (intent != null)
            {
                options.Add("intent", intent);
            }

            return Operation.Call("thumbnail_buffer", options, buffer, width) as Image;
        }

        /// <summary>
        /// Generate thumbnail from image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.ThumbnailImage(width, height: int, size: string, autoRotate: bool, crop: string, linear: bool, importProfile: string, exportProfile: string, intent: string);
        /// </code>
        /// </example>
        /// <param name="width">Size to this width</param>
        /// <param name="height">Size to this height</param>
        /// <param name="size">Only upsize, only downsize, or both</param>
        /// <param name="autoRotate">Use orientation tags to rotate image upright</param>
        /// <param name="crop">Reduce to fill target rectangle, then crop</param>
        /// <param name="linear">Reduce in linear light</param>
        /// <param name="importProfile">Fallback import profile</param>
        /// <param name="exportProfile">Fallback export profile</param>
        /// <param name="intent">Rendering intent</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ThumbnailImage(int width, int? height = null, string size = null, bool? autoRotate = null,
            string crop = null, bool? linear = null, string importProfile = null, string exportProfile = null,
            string intent = null)
        {
            var options = new VOption();

            if (height.HasValue)
            {
                options.Add("height", height);
            }

            if (size != null)
            {
                options.Add("size", size);
            }

            if (autoRotate.HasValue)
            {
                options.Add("auto_rotate", autoRotate);
            }

            if (crop != null)
            {
                options.Add("crop", crop);
            }

            if (linear.HasValue)
            {
                options.Add("linear", linear);
            }

            if (importProfile != null)
            {
                options.Add("import_profile", importProfile);
            }

            if (exportProfile != null)
            {
                options.Add("export_profile", exportProfile);
            }

            if (intent != null)
            {
                options.Add("intent", intent);
            }

            return this.Call("thumbnail_image", options, width) as Image;
        }

        /// <summary>
        /// Load tiff from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Tiffload(filename, memory: bool, access: string, page: int, n: int, fail: bool, autorotate: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the image</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="autorotate">Rotate image using orientation tag</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Tiffload(string filename, bool? memory = null, string access = null, int? page = null,
            int? n = null, bool? fail = null, bool? autorotate = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (autorotate.HasValue)
            {
                options.Add("autorotate", autorotate);
            }

            return Operation.Call("tiffload", options, filename) as Image;
        }

        /// <summary>
        /// Load tiff from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Tiffload(filename, out var flags, memory: bool, access: string, page: int, n: int, fail: bool, autorotate: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the image</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="autorotate">Rotate image using orientation tag</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Tiffload(string filename, out int flags, bool? memory = null, string access = null,
            int? page = null, int? n = null, bool? fail = null, bool? autorotate = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (autorotate.HasValue)
            {
                options.Add("autorotate", autorotate);
            }

            options.Add("flags", true);

            var results = Operation.Call("tiffload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load tiff from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.TiffloadBuffer(buffer, memory: bool, access: string, page: int, n: int, fail: bool, autorotate: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the image</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="autorotate">Rotate image using orientation tag</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image TiffloadBuffer(byte[] buffer, bool? memory = null, string access = null, int? page = null,
            int? n = null, bool? fail = null, bool? autorotate = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (autorotate.HasValue)
            {
                options.Add("autorotate", autorotate);
            }

            return Operation.Call("tiffload_buffer", options, buffer) as Image;
        }

        /// <summary>
        /// Load tiff from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.TiffloadBuffer(buffer, out var flags, memory: bool, access: string, page: int, n: int, fail: bool, autorotate: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="page">Load this page from the image</param>
        /// <param name="n">Load this many pages</param>
        /// <param name="fail">Fail on first error</param>
        /// <param name="autorotate">Rotate image using orientation tag</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image TiffloadBuffer(byte[] buffer, out int flags, bool? memory = null, string access = null,
            int? page = null, int? n = null, bool? fail = null, bool? autorotate = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (page.HasValue)
            {
                options.Add("page", page);
            }

            if (n.HasValue)
            {
                options.Add("n", n);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            if (autorotate.HasValue)
            {
                options.Add("autorotate", autorotate);
            }

            options.Add("flags", true);

            var results = Operation.Call("tiffload_buffer", options, buffer) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Save image to tiff file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Tiffsave(filename, compression: string, q: int, predictor: string, pageHeight: int, profile: string, tile: bool, tileWidth: int, tileHeight: int, pyramid: bool, miniswhite: bool, squash: bool, resunit: string, xres: double, yres: double, bigtiff: bool, properties: bool, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="compression">Compression for this file</param>
        /// <param name="q">Q factor</param>
        /// <param name="predictor">Compression prediction</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="profile">ICC profile to embed</param>
        /// <param name="tile">Write a tiled tiff</param>
        /// <param name="tileWidth">Tile width in pixels</param>
        /// <param name="tileHeight">Tile height in pixels</param>
        /// <param name="pyramid">Write a pyramidal tiff</param>
        /// <param name="miniswhite">Use 0 for white in 1-bit images</param>
        /// <param name="squash">Squash images down to 1 bit</param>
        /// <param name="resunit">Resolution unit</param>
        /// <param name="xres">Horizontal resolution in pixels/mm</param>
        /// <param name="yres">Vertical resolution in pixels/mm</param>
        /// <param name="bigtiff">Write a bigtiff image</param>
        /// <param name="properties">Write a properties document to IMAGEDESCRIPTION</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Tiffsave(string filename, string compression = null, int? q = null, string predictor = null,
            int? pageHeight = null, string profile = null, bool? tile = null, int? tileWidth = null,
            int? tileHeight = null, bool? pyramid = null, bool? miniswhite = null, bool? squash = null,
            string resunit = null, double? xres = null, double? yres = null, bool? bigtiff = null,
            bool? properties = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (compression != null)
            {
                options.Add("compression", compression);
            }

            if (q.HasValue)
            {
                options.Add("Q", q);
            }

            if (predictor != null)
            {
                options.Add("predictor", predictor);
            }

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (profile != null)
            {
                options.Add("profile", profile);
            }

            if (tile.HasValue)
            {
                options.Add("tile", tile);
            }

            if (tileWidth.HasValue)
            {
                options.Add("tile_width", tileWidth);
            }

            if (tileHeight.HasValue)
            {
                options.Add("tile_height", tileHeight);
            }

            if (pyramid.HasValue)
            {
                options.Add("pyramid", pyramid);
            }

            if (miniswhite.HasValue)
            {
                options.Add("miniswhite", miniswhite);
            }

            if (squash.HasValue)
            {
                options.Add("squash", squash);
            }

            if (resunit != null)
            {
                options.Add("resunit", resunit);
            }

            if (xres.HasValue)
            {
                options.Add("xres", xres);
            }

            if (yres.HasValue)
            {
                options.Add("yres", yres);
            }

            if (bigtiff.HasValue)
            {
                options.Add("bigtiff", bigtiff);
            }

            if (properties.HasValue)
            {
                options.Add("properties", properties);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("tiffsave", options, filename);
        }

        /// <summary>
        /// Save image to tiff buffer
        /// </summary>
        /// <example>
        /// <code>
        /// byte[] buffer = in.TiffsaveBuffer(compression: string, q: int, predictor: string, pageHeight: int, profile: string, tile: bool, tileWidth: int, tileHeight: int, pyramid: bool, miniswhite: bool, squash: bool, resunit: string, xres: double, yres: double, bigtiff: bool, properties: bool, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="compression">Compression for this file</param>
        /// <param name="q">Q factor</param>
        /// <param name="predictor">Compression prediction</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="profile">ICC profile to embed</param>
        /// <param name="tile">Write a tiled tiff</param>
        /// <param name="tileWidth">Tile width in pixels</param>
        /// <param name="tileHeight">Tile height in pixels</param>
        /// <param name="pyramid">Write a pyramidal tiff</param>
        /// <param name="miniswhite">Use 0 for white in 1-bit images</param>
        /// <param name="squash">Squash images down to 1 bit</param>
        /// <param name="resunit">Resolution unit</param>
        /// <param name="xres">Horizontal resolution in pixels/mm</param>
        /// <param name="yres">Vertical resolution in pixels/mm</param>
        /// <param name="bigtiff">Write a bigtiff image</param>
        /// <param name="properties">Write a properties document to IMAGEDESCRIPTION</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>An array of bytes</returns>
        public byte[] TiffsaveBuffer(string compression = null, int? q = null, string predictor = null,
            int? pageHeight = null, string profile = null, bool? tile = null, int? tileWidth = null,
            int? tileHeight = null, bool? pyramid = null, bool? miniswhite = null, bool? squash = null,
            string resunit = null, double? xres = null, double? yres = null, bool? bigtiff = null,
            bool? properties = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (compression != null)
            {
                options.Add("compression", compression);
            }

            if (q.HasValue)
            {
                options.Add("Q", q);
            }

            if (predictor != null)
            {
                options.Add("predictor", predictor);
            }

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (profile != null)
            {
                options.Add("profile", profile);
            }

            if (tile.HasValue)
            {
                options.Add("tile", tile);
            }

            if (tileWidth.HasValue)
            {
                options.Add("tile_width", tileWidth);
            }

            if (tileHeight.HasValue)
            {
                options.Add("tile_height", tileHeight);
            }

            if (pyramid.HasValue)
            {
                options.Add("pyramid", pyramid);
            }

            if (miniswhite.HasValue)
            {
                options.Add("miniswhite", miniswhite);
            }

            if (squash.HasValue)
            {
                options.Add("squash", squash);
            }

            if (resunit != null)
            {
                options.Add("resunit", resunit);
            }

            if (xres.HasValue)
            {
                options.Add("xres", xres);
            }

            if (yres.HasValue)
            {
                options.Add("yres", yres);
            }

            if (bigtiff.HasValue)
            {
                options.Add("bigtiff", bigtiff);
            }

            if (properties.HasValue)
            {
                options.Add("properties", properties);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("tiffsave_buffer", options) as byte[];
        }

        /// <summary>
        /// Cache an image as a set of tiles
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Tilecache(tileWidth: int, tileHeight: int, maxTiles: int, access: string, threaded: bool, persistent: bool);
        /// </code>
        /// </example>
        /// <param name="tileWidth">Tile width in pixels</param>
        /// <param name="tileHeight">Tile height in pixels</param>
        /// <param name="maxTiles">Maximum number of tiles to cache</param>
        /// <param name="access">Expected access pattern</param>
        /// <param name="threaded">Allow threaded access</param>
        /// <param name="persistent">Keep cache between evaluations</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Tilecache(int? tileWidth = null, int? tileHeight = null, int? maxTiles = null,
            string access = null, bool? threaded = null, bool? persistent = null)
        {
            var options = new VOption();

            if (tileWidth.HasValue)
            {
                options.Add("tile_width", tileWidth);
            }

            if (tileHeight.HasValue)
            {
                options.Add("tile_height", tileHeight);
            }

            if (maxTiles.HasValue)
            {
                options.Add("max_tiles", maxTiles);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (threaded.HasValue)
            {
                options.Add("threaded", threaded);
            }

            if (persistent.HasValue)
            {
                options.Add("persistent", persistent);
            }

            return this.Call("tilecache", options) as Image;
        }

        /// <summary>
        /// Build a look-up table
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Tonelut(inMax: int, outMax: int, lb: double, lw: double, ps: double, pm: double, ph: double, s: double, m: double, h: double);
        /// </code>
        /// </example>
        /// <param name="inMax">Size of LUT to build</param>
        /// <param name="outMax">Maximum value in output LUT</param>
        /// <param name="lb">Lowest value in output</param>
        /// <param name="lw">Highest value in output</param>
        /// <param name="ps">Position of shadow</param>
        /// <param name="pm">Position of mid-tones</param>
        /// <param name="ph">Position of highlights</param>
        /// <param name="s">Adjust shadows by this much</param>
        /// <param name="m">Adjust mid-tones by this much</param>
        /// <param name="h">Adjust highlights by this much</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Tonelut(int? inMax = null, int? outMax = null, double? lb = null, double? lw = null,
            double? ps = null, double? pm = null, double? ph = null, double? s = null, double? m = null,
            double? h = null)
        {
            var options = new VOption();

            if (inMax.HasValue)
            {
                options.Add("in_max", inMax);
            }

            if (outMax.HasValue)
            {
                options.Add("out_max", outMax);
            }

            if (lb.HasValue)
            {
                options.Add("Lb", lb);
            }

            if (lw.HasValue)
            {
                options.Add("Lw", lw);
            }

            if (ps.HasValue)
            {
                options.Add("Ps", ps);
            }

            if (pm.HasValue)
            {
                options.Add("Pm", pm);
            }

            if (ph.HasValue)
            {
                options.Add("Ph", ph);
            }

            if (s.HasValue)
            {
                options.Add("S", s);
            }

            if (m.HasValue)
            {
                options.Add("M", m);
            }

            if (h.HasValue)
            {
                options.Add("H", h);
            }

            return Operation.Call("tonelut", options) as Image;
        }

        /// <summary>
        /// Unpremultiply image alpha
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Unpremultiply(maxAlpha: double);
        /// </code>
        /// </example>
        /// <param name="maxAlpha">Maximum value of alpha channel</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Unpremultiply(double? maxAlpha = null)
        {
            var options = new VOption();

            if (maxAlpha.HasValue)
            {
                options.Add("max_alpha", maxAlpha);
            }

            return this.Call("unpremultiply", options) as Image;
        }

        /// <summary>
        /// Load vips from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Vipsload(filename, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Vipsload(string filename, bool? memory = null, string access = null, bool? fail = null)
        {
            var options = new VOption();

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

            return Operation.Call("vipsload", options, filename) as Image;
        }

        /// <summary>
        /// Load vips from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Vipsload(filename, out var flags, memory: bool, access: string, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Vipsload(string filename, out int flags, bool? memory = null, string access = null,
            bool? fail = null)
        {
            var options = new VOption();

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

            options.Add("flags", true);

            var results = Operation.Call("vipsload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Save image to vips file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Vipssave(filename, pageHeight: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Vipssave(string filename, int? pageHeight = null, bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("vipssave", options, filename);
        }

        /// <summary>
        /// Load webp from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Webpload(filename, memory: bool, access: string, shrink: int, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="shrink">Shrink factor on load</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Webpload(string filename, bool? memory = null, string access = null, int? shrink = null,
            bool? fail = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (shrink.HasValue)
            {
                options.Add("shrink", shrink);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            return Operation.Call("webpload", options, filename) as Image;
        }

        /// <summary>
        /// Load webp from file
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Webpload(filename, out var flags, memory: bool, access: string, shrink: int, fail: bool);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="shrink">Shrink factor on load</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Webpload(string filename, out int flags, bool? memory = null, string access = null,
            int? shrink = null, bool? fail = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (shrink.HasValue)
            {
                options.Add("shrink", shrink);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            options.Add("flags", true);

            var results = Operation.Call("webpload", options, filename) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Load webp from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.WebploadBuffer(buffer, memory: bool, access: string, shrink: int, fail: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="shrink">Shrink factor on load</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image WebploadBuffer(byte[] buffer, bool? memory = null, string access = null, int? shrink = null,
            bool? fail = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (shrink.HasValue)
            {
                options.Add("shrink", shrink);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            return Operation.Call("webpload_buffer", options, buffer) as Image;
        }

        /// <summary>
        /// Load webp from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.WebploadBuffer(buffer, out var flags, memory: bool, access: string, shrink: int, fail: bool);
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="flags">Flags for this file</param>
        /// <param name="memory">Force open via memory</param>
        /// <param name="access">Required access pattern for this file</param>
        /// <param name="shrink">Shrink factor on load</param>
        /// <param name="fail">Fail on first error</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image WebploadBuffer(byte[] buffer, out int flags, bool? memory = null, string access = null,
            int? shrink = null, bool? fail = null)
        {
            var options = new VOption();

            if (memory.HasValue)
            {
                options.Add("memory", memory);
            }

            if (access != null)
            {
                options.Add("access", access);
            }

            if (shrink.HasValue)
            {
                options.Add("shrink", shrink);
            }

            if (fail.HasValue)
            {
                options.Add("fail", fail);
            }

            options.Add("flags", true);

            var results = Operation.Call("webpload_buffer", options, buffer) as object[];
            var finalResult = results?[0] as Image;
            var opts = results?[1] as VOption;
            flags = opts?["flags"] is int out1 ? out1 : 0;

            return finalResult;
        }

        /// <summary>
        /// Save image to webp file
        /// </summary>
        /// <example>
        /// <code>
        /// in.Webpsave(filename, pageHeight: int, q: int, lossless: bool, preset: string, smartSubsample: bool, nearLossless: bool, alphaQ: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="q">Q factor</param>
        /// <param name="lossless">enable lossless compression</param>
        /// <param name="preset">Preset for lossy compression</param>
        /// <param name="smartSubsample">Enable high quality chroma subsampling</param>
        /// <param name="nearLossless">Enable preprocessing in lossless mode (uses Q)</param>
        /// <param name="alphaQ">Change alpha plane fidelity for lossy compression</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>None</returns>
        public void Webpsave(string filename, int? pageHeight = null, int? q = null, bool? lossless = null,
            string preset = null, bool? smartSubsample = null, bool? nearLossless = null, int? alphaQ = null,
            bool? strip = null, double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (q.HasValue)
            {
                options.Add("Q", q);
            }

            if (lossless.HasValue)
            {
                options.Add("lossless", lossless);
            }

            if (preset != null)
            {
                options.Add("preset", preset);
            }

            if (smartSubsample.HasValue)
            {
                options.Add("smart_subsample", smartSubsample);
            }

            if (nearLossless.HasValue)
            {
                options.Add("near_lossless", nearLossless);
            }

            if (alphaQ.HasValue)
            {
                options.Add("alpha_q", alphaQ);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            this.Call("webpsave", options, filename);
        }

        /// <summary>
        /// Save image to webp buffer
        /// </summary>
        /// <example>
        /// <code>
        /// byte[] buffer = in.WebpsaveBuffer(pageHeight: int, q: int, lossless: bool, preset: string, smartSubsample: bool, nearLossless: bool, alphaQ: int, strip: bool, background: double[]);
        /// </code>
        /// </example>
        /// <param name="pageHeight">Set page height for multipage save</param>
        /// <param name="q">Q factor</param>
        /// <param name="lossless">enable lossless compression</param>
        /// <param name="preset">Preset for lossy compression</param>
        /// <param name="smartSubsample">Enable high quality chroma subsampling</param>
        /// <param name="nearLossless">Enable preprocessing in lossless mode (uses Q)</param>
        /// <param name="alphaQ">Change alpha plane fidelity for lossy compression</param>
        /// <param name="strip">Strip all metadata from image</param>
        /// <param name="background">Background value</param>
        /// <returns>An array of bytes</returns>
        public byte[] WebpsaveBuffer(int? pageHeight = null, int? q = null, bool? lossless = null, string preset = null,
            bool? smartSubsample = null, bool? nearLossless = null, int? alphaQ = null, bool? strip = null,
            double[] background = null)
        {
            var options = new VOption();

            if (pageHeight.HasValue)
            {
                options.Add("page_height", pageHeight);
            }

            if (q.HasValue)
            {
                options.Add("Q", q);
            }

            if (lossless.HasValue)
            {
                options.Add("lossless", lossless);
            }

            if (preset != null)
            {
                options.Add("preset", preset);
            }

            if (smartSubsample.HasValue)
            {
                options.Add("smart_subsample", smartSubsample);
            }

            if (nearLossless.HasValue)
            {
                options.Add("near_lossless", nearLossless);
            }

            if (alphaQ.HasValue)
            {
                options.Add("alpha_q", alphaQ);
            }

            if (strip.HasValue)
            {
                options.Add("strip", strip);
            }

            if (background != null && background.Length > 0)
            {
                options.Add("background", background);
            }

            return this.Call("webpsave_buffer", options) as byte[];
        }

        /// <summary>
        /// Make a worley noise image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Worley(width, height, cellSize: int);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="cellSize">Size of Worley cells</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Worley(int width, int height, int? cellSize = null)
        {
            var options = new VOption();

            if (cellSize.HasValue)
            {
                options.Add("cell_size", cellSize);
            }

            return Operation.Call("worley", options, width, height) as Image;
        }

        /// <summary>
        /// Wrap image origin
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Wrap(x: int, y: int);
        /// </code>
        /// </example>
        /// <param name="x">Left edge of input in output</param>
        /// <param name="y">Top edge of input in output</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Wrap(int? x = null, int? y = null)
        {
            var options = new VOption();

            if (x.HasValue)
            {
                options.Add("x", x);
            }

            if (y.HasValue)
            {
                options.Add("y", y);
            }

            return this.Call("wrap", options) as Image;
        }

        /// <summary>
        /// Make an image where pixel values are coordinates
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Xyz(width, height, csize: int, dsize: int, esize: int);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="csize">Size of third dimension</param>
        /// <param name="dsize">Size of fourth dimension</param>
        /// <param name="esize">Size of fifth dimension</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Xyz(int width, int height, int? csize = null, int? dsize = null, int? esize = null)
        {
            var options = new VOption();

            if (csize.HasValue)
            {
                options.Add("csize", csize);
            }

            if (dsize.HasValue)
            {
                options.Add("dsize", dsize);
            }

            if (esize.HasValue)
            {
                options.Add("esize", esize);
            }

            return Operation.Call("xyz", options, width, height) as Image;
        }

        /// <summary>
        /// Transform XYZ to Lab
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.XYZ2Lab(temp: double[]);
        /// </code>
        /// </example>
        /// <param name="temp">Colour temperature</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image XYZ2Lab(double[] temp = null)
        {
            var options = new VOption();

            if (temp != null && temp.Length > 0)
            {
                options.Add("temp", temp);
            }

            return this.Call("XYZ2Lab", options) as Image;
        }

        /// <summary>
        /// Transform XYZ to scRGB
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.XYZ2scRGB();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image XYZ2scRGB()
        {
            return this.Call("XYZ2scRGB") as Image;
        }

        /// <summary>
        /// Transform XYZ to Yxy
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.XYZ2Yxy();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image XYZ2Yxy()
        {
            return this.Call("XYZ2Yxy") as Image;
        }

        /// <summary>
        /// Transform Yxy to XYZ
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = in.Yxy2XYZ();
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Yxy2XYZ()
        {
            return this.Call("Yxy2XYZ") as Image;
        }

        /// <summary>
        /// Make a zone plate
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Zone(width, height, uchar: bool);
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="uchar">Output an unsigned char image</param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Zone(int width, int height, bool? uchar = null)
        {
            var options = new VOption();

            if (uchar.HasValue)
            {
                options.Add("uchar", uchar);
            }

            return Operation.Call("zone", options, width, height) as Image;
        }

        /// <summary>
        /// Zoom an image
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = input.Zoom(xfac, yfac);
        /// </code>
        /// </example>
        /// <param name="xfac">Horizontal zoom factor</param>
        /// <param name="yfac">Vertical zoom factor</param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Zoom(int xfac, int yfac)
        {
            return this.Call("zoom", xfac, yfac) as Image;
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
                in1 = Imageize(matchImage, in2);
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
        /// Image @out = NetVips.Image.Bandjoin(@in);
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
                    throw new Exception(
                        $"unsupported value type {other.GetType()} for Bandjoin"
                    );
            }
        }

        /// <summary>
        /// Band-wise rank of a set of images
        /// </summary>
        /// <example>
        /// <code>
        /// Image @out = NetVips.Image.Bandrank(@in, index: int);
        /// </code>
        /// </example>
        /// <param name="in">Array of input images</param>
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
        /// Image @out = NetVips.Image.Composite(@in, mode, compositingSpace: string, premultiplied: bool);
        /// </code>
        /// </example>
        /// <param name="in">Array of input images</param>
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

        #endregion

        #region autogenerated properties

        /// <summary>
        /// Class nickname
        /// </summary>
        public string Nickname => (string) Get("nickname");

        /// <summary>
        /// Class description
        /// </summary>
        public string Description => (string) Get("description");

        /// <summary>
        /// Image width in pixels
        /// </summary>
        public int Width => (int) Get("width");

        /// <summary>
        /// Image height in pixels
        /// </summary>
        public int Height => (int) Get("height");

        /// <summary>
        /// Number of bands in image
        /// </summary>
        public int Bands => (int) Get("bands");

        /// <summary>
        /// Pixel format in image
        /// </summary>
        public string Format => (string) Get("format");

        /// <summary>
        /// Pixel coding
        /// </summary>
        public string Coding => (string) Get("coding");

        /// <summary>
        /// Pixel interpretation
        /// </summary>
        public string Interpretation => (string) Get("interpretation");

        /// <summary>
        /// Horizontal resolution in pixels/mm
        /// </summary>
        public double Xres => (double) Get("xres");

        /// <summary>
        /// Vertical resolution in pixels/mm
        /// </summary>
        public double Yres => (double) Get("yres");

        /// <summary>
        /// Horizontal offset of origin
        /// </summary>
        public int Xoffset => (int) Get("xoffset");

        /// <summary>
        /// Vertical offset of origin
        /// </summary>
        public int Yoffset => (int) Get("yoffset");

        /// <summary>
        /// Image filename
        /// </summary>
        public string Filename => (string) Get("filename");

        /// <summary>
        /// Open mode
        /// </summary>
        public string Mode => (string) Get("filename");

        /// <summary>
        /// Block evaluation on this image
        /// </summary>
        public bool Kill => (bool) Get("kill");

        /// <summary>
        /// Preferred demand style for this image
        /// </summary>
        public string Demand => (string) Get("demand");

        /// <summary>
        /// Offset in bytes from start of file
        /// </summary>
        public int SizeOfHeader => (int) Get("sizeof_header");

        /// <summary>
        /// Pointer to foreign pixels
        /// </summary>
        public string ForeignBuffer => (string) Get("foreign_buffer");

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

        protected bool Equals(Image other)
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