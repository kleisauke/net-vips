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
        private Array _data;

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
            string newFormat;
            var originalFormat = image.Format;
            if (image.Format != "complex" && image.Format != "dpcomplex")
            {
                if (image.Bands % 2 != 0)
                {
                    throw new Exception("not an even number of bands");
                }

                if (image.Format != "float" && image.Format != "double")
                {
                    image = image.Cast("float");
                }

                newFormat = image.Format == "double" ? "dpcomplex" : "complex";

                image = image.Copy(new Dictionary<string, object>
                {
                    {"format", newFormat},
                    {"bands", image.Bands / 2}
                });
            }

            image = func(image);
            if (originalFormat != "complex" && originalFormat != "dpcomplex")
            {
                newFormat = image.Format == "dpcomplex" ? "double" : "float";

                image = image.Copy(new Dictionary<string, object>
                {
                    {"format", newFormat},
                    {"bands", image.Bands * 2}
                });
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
        ///     image = netvips.Image.NewFromFile('fred.jpg[shrink=2]')
        ///
        /// You can also supply options as keyword arguments, for example: 
        /// <![CDATA[
        ///     var image = netvips.Image.NewFromFile('fred.jpg', new Dictionary<string, object>
        ///     {
        ///         {"shrink", 2}
        ///     });
        /// ]]>
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
        /// <param name="kwargs">
        /// memory (bool): If set True, load the image via memory rather than
        ///     via a temporary disc file. See <see cref="NewTempFile"/> for
        ///     notes on where temporary files are created. Small images are
        ///     loaded via memory by default, use ``VIPS_DISC_THRESHOLD`` to
        ///     set the definition of small.
        /// access (Access): Hint the expected access pattern for the image.
        /// fail (bool): If set True, the loader will fail with an error on
        ///     the first serious error in the file. By default, libvips
        ///     will attempt to read everything it can from a damanged image.
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        /// <exception cref="T:System.Exception">If unable to load from <paramref name="vipsFilename" />.</exception>
        public static Image NewFromFile(string vipsFilename, IDictionary<string, object> kwargs = null)
        {
            var fileNamePtr = vipsFilename.ToUtf8Ptr();
            var filename = VipsImage.VipsFilenameGetFilename(fileNamePtr);
            var options = VipsImage.VipsFilenameGetOptions(fileNamePtr);

            var name = VipsForeign.VipsForeignFindLoad(filename.ToUtf8Ptr());
            if (name == null)
            {
                throw new Exception($"unable to load from file {vipsFilename}");
            }

            var stringOptions = new Dictionary<string, object>
            {
                {"string_options", options}
            };

            if (kwargs != null)
            {
                stringOptions.Merge(kwargs);
            }

            return Operation.Call(name, kwargs, filename) as Image;
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
        /// <param name="options">Load options as a string. Use ``""`` for no options.</param>
        /// <param name="kwargs">
        /// access (Access): Hint the expected access pattern for the image.
        /// fail (bool): If set True, the loader will fail with an error on the
        ///     first serious error in the image. By default, libvips will
        ///     attempt to read everything it can from a damanged image.
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        /// <exception cref="T:System.Exception">If unable to load from <paramref name="data" />.</exception>
        public static Image NewFromBuffer(object data, string options = "", IDictionary<string, object> kwargs = null)
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

            var stringOptions = new Dictionary<string, object>
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

            return Operation.Call(name, kwargs, data) as Image;
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

            var height = arr.Length;
            var width = arr.GetValue(0) is Array arrWidth ? arrWidth.Length : 1;
            var n = width * height;

            var a = new double[n];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var yValue = arr.GetValue(y);
                    var value = yValue is Array yArray ? (yArray.Length <= x ? 0 : yArray.GetValue(x)) : yValue;
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
            var image = pixel.Embed(0, 0, Width, Height, new Dictionary<string, object>
            {
                {"extend", "copy"}
            });
            image = image.Copy(new Dictionary<string, object>
            {
                {"interpretation", Interpretation},
                {"xres", Xres},
                {"yres", Yres},
                {"xoffset", Xoffset},
                {"yoffset", Yoffset}
            });
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
        /// 
        /// This method can save images in any format supported by vips. The format
        /// is selected from the filename suffix. The filename can include embedded
        /// save options, see <see cref="NewFromFile"/>.
        /// 
        /// For example:
        /// 
        ///     image.WriteToFile('fred.jpg[Q=95]')
        /// 
        /// You can also supply options as keyword arguments, for example: 
        /// <![CDATA[
        ///     image.WriteToFile('fred.jpg', new Dictionary<string, object>
        ///     {
        ///         {"Q", 95}
        ///     });
        /// ]]>
        /// The full set of options available depend upon the load operation that
        /// will be executed. Try something like: 
        /// 
        ///     $ vips jpegsave
        /// 
        /// at the command-line to see a summary of the available options for the
        /// JPEG saver.
        /// </remarks>
        /// <param name="vipsFilename">The disc file to save the image to, with
        /// optional appended arguments.</param>
        /// <param name="kwargs"></param>
        /// <returns>None</returns>
        /// <exception cref="T:System.Exception">If unable to write to <paramref name="vipsFilename" />.</exception>
        public void WriteToFile(string vipsFilename, IDictionary<string, object> kwargs = null)
        {
            var fileNamePtr = vipsFilename.ToUtf8Ptr();
            var filename = VipsImage.VipsFilenameGetFilename(fileNamePtr);
            var options = VipsImage.VipsFilenameGetOptions(fileNamePtr);

            var name = VipsForeign.VipsForeignFindSave(filename.ToUtf8Ptr());
            if (name == null)
            {
                throw new Exception($"unable to write to file {vipsFilename}");
            }

            var stringOptions = new Dictionary<string, object>
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
        /// <![CDATA[
        ///     var data = image.WriteToBuffer('.jpg', new Dictionary<string, object>
        ///     {
        ///         {"Q", 95}
        ///     });
        /// ]]>
        /// The full set of options available depend upon the load operation that
        /// will be executed. Try something like: 
        /// 
        ///     $ vips jpegsave_buffer
        /// 
        /// at the command-line to see a summary of the available options for the
        /// JPEG saver.
        /// </remarks>
        /// <param name="formatString">The suffix, plus any string-form arguments.</param>
        /// <param name="kwargs"></param>
        /// <returns>A byte string</returns>
        /// <exception cref="T:System.Exception">If unable to write to buffer.</exception>
        public byte[] WriteToBuffer(string formatString, IDictionary<string, object> kwargs = null)
        {
            var formatStrPtr = formatString.ToUtf8Ptr();
            var options = VipsImage.VipsFilenameGetOptions(formatStrPtr);

            var name = VipsForeign.VipsForeignFindSaveBuffer(formatString);
            if (name == null)
            {
                throw new Exception("unable to write to buffer");
            }

            var stringOptions = new Dictionary<string, object>
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
        /// <returns>buffer</returns>
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
            return $"<netvips.Image {Width}x{Height} {Format}, {Bands} bands, {Interpretation}>";
        }

        #endregion

        // TODO Should we define these in a separate file?

        #region auto-generated functions

        /// <summary>
        /// Absolute value of an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Abs();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = left.Add(right);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Affine(matrix, new Dictionary<string, object>
        /// {
        ///     {"interpolate", GObject}
        ///     {"oarea", int[]}
        ///     {"odx", double}
        ///     {"ody", double}
        ///     {"idx", double}
        ///     {"idy", double}
        ///     {"background", double[]}
        ///     {"extend", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="matrix">Transformation matrix</param>
        /// <param name="kwargs">
        /// interpolate (GObject): Interpolate pixels with this
        /// oarea (int[]): Area of output to generate
        /// odx (double): Horizontal output displacement
        /// ody (double): Vertical output displacement
        /// idx (double): Horizontal input displacement
        /// idy (double): Vertical input displacement
        /// background (double[]): Background value
        /// extend (string): How to generate the extra pixels
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Affine(double[] matrix, IDictionary<string, object> kwargs = null)
        {
            return this.Call("affine", kwargs, matrix) as Image;
        }

        /// <summary>
        /// Load an Analyze6 image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Analyzeload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Analyzeload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("analyzeload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Join an array of images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Arrayjoin(@in, new Dictionary<string, object>
        /// {
        ///     {"across", int}
        ///     {"shim", int}
        ///     {"background", double[]}
        ///     {"halign", string}
        ///     {"valign", string}
        ///     {"hspacing", int}
        ///     {"vspacing", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="in">Array of input images</param>
        /// <param name="kwargs">
        /// across (int): Number of images across grid
        /// shim (int): Pixels between images
        /// background (double[]): Colour for new pixels
        /// halign (string): Align on the left, centre or right
        /// valign (string): Align on the top, centre or bottom
        /// hspacing (int): Horizontal spacing between images
        /// vspacing (int): Vertical spacing between images
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Arrayjoin(Image[] @in, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("arrayjoin", kwargs, new object[] {@in}) as Image;
        }

        /// <summary>
        /// Autorotate image by exif tag
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Autorot();
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Autorot()
        {
            return this.Call("autorot") as Image;
        }

        /// <summary>
        /// Find image average
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// double @out = in.Avg();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Bandbool(boolean);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Bandfold(new Dictionary<string, object>
        /// {
        ///     {"factor", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// factor (int): Fold by this factor
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandfold(IDictionary<string, object> kwargs = null)
        {
            return this.Call("bandfold", kwargs) as Image;
        }

        /// <summary>
        /// Append a constant band to an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.BandjoinConst(c);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Bandmean();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Bandunfold(new Dictionary<string, object>
        /// {
        ///     {"factor", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// factor (int): Unfold by this factor
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandunfold(IDictionary<string, object> kwargs = null)
        {
            return this.Call("bandunfold", kwargs) as Image;
        }

        /// <summary>
        /// Make a black image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Black(width, height, new Dictionary<string, object>
        /// {
        ///     {"bands", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// bands (int): Number of bands in image
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Black(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("black", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Boolean operation on two images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = left.Boolean(right, boolean);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.BooleanConst(boolean, c);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Buildlut();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Byteswap();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Cache(new Dictionary<string, object>
        /// {
        ///     {"max_tiles", int}
        ///     {"tile_height", int}
        ///     {"tile_width", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// max_tiles (int): Maximum number of tiles to cache
        /// tile_height (int): Tile height in pixels
        /// tile_width (int): Tile width in pixels
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Cache(IDictionary<string, object> kwargs = null)
        {
            return this.Call("cache", kwargs) as Image;
        }

        /// <summary>
        /// Cast an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Cast(format, new Dictionary<string, object>
        /// {
        ///     {"shift", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="format">Format to cast to</param>
        /// <param name="kwargs">
        /// shift (bool): Shift integer values up and down
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Cast(string format, IDictionary<string, object> kwargs = null)
        {
            return this.Call("cast", kwargs, format) as Image;
        }

        /// <summary>
        /// Transform LCh to CMC
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.CMC2LCh();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Colourspace(space, new Dictionary<string, object>
        /// {
        ///     {"source_space", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="space">Destination color space</param>
        /// <param name="kwargs">
        /// source_space (string): Source color space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Colourspace(string space, IDictionary<string, object> kwargs = null)
        {
            return this.Call("colourspace", kwargs, space) as Image;
        }

        /// <summary>
        /// Convolve with rotating mask
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Compass(mask, new Dictionary<string, object>
        /// {
        ///     {"times", int}
        ///     {"angle", string}
        ///     {"combine", string}
        ///     {"precision", string}
        ///     {"layers", int}
        ///     {"cluster", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="kwargs">
        /// times (int): Rotate and convolve this many times
        /// angle (string): Rotate mask by this much between convolutions
        /// combine (string): Combine convolution results like this
        /// precision (string): Convolve with this precision
        /// layers (int): Use this many layers in approximation
        /// cluster (int): Cluster lines closer than this in approximation
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Compass(Image mask, IDictionary<string, object> kwargs = null)
        {
            return this.Call("compass", kwargs, mask) as Image;
        }

        /// <summary>
        /// Perform a complex operation on an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Complex(cmplx);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = left.Complex2(right, cmplx);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = left.Complexform(right);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Complexget(get);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = base.Composite2(overlay, mode, new Dictionary<string, object>
        /// {
        ///     {"compositing_space", string}
        ///     {"premultiplied", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="overlay">Overlay image</param>
        /// <param name="mode">VipsBlendMode to join with</param>
        /// <param name="kwargs">
        /// compositing_space (string): Composite images in this colour space
        /// premultiplied (bool): Images have premultiplied alpha
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Composite2(Image overlay, string mode, IDictionary<string, object> kwargs = null)
        {
            return this.Call("composite2", kwargs, overlay, mode) as Image;
        }

        /// <summary>
        /// Convolution operation
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Conv(mask, new Dictionary<string, object>
        /// {
        ///     {"precision", string}
        ///     {"layers", int}
        ///     {"cluster", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="kwargs">
        /// precision (string): Convolve with this precision
        /// layers (int): Use this many layers in approximation
        /// cluster (int): Cluster lines closer than this in approximation
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Conv(Image mask, IDictionary<string, object> kwargs = null)
        {
            return this.Call("conv", kwargs, mask) as Image;
        }

        /// <summary>
        /// Approximate integer convolution
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Conva(mask, new Dictionary<string, object>
        /// {
        ///     {"layers", int}
        ///     {"cluster", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="kwargs">
        /// layers (int): Use this many layers in approximation
        /// cluster (int): Cluster lines closer than this in approximation
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Conva(Image mask, IDictionary<string, object> kwargs = null)
        {
            return this.Call("conva", kwargs, mask) as Image;
        }

        /// <summary>
        /// Approximate separable integer convolution
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Convasep(mask, new Dictionary<string, object>
        /// {
        ///     {"layers", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="kwargs">
        /// layers (int): Use this many layers in approximation
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Convasep(Image mask, IDictionary<string, object> kwargs = null)
        {
            return this.Call("convasep", kwargs, mask) as Image;
        }

        /// <summary>
        /// Float convolution operation
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Convf(mask);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Convi(mask);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Convsep(mask, new Dictionary<string, object>
        /// {
        ///     {"precision", string}
        ///     {"layers", int}
        ///     {"cluster", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="mask">Input matrix image</param>
        /// <param name="kwargs">
        /// precision (string): Convolve with this precision
        /// layers (int): Use this many layers in approximation
        /// cluster (int): Cluster lines closer than this in approximation
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Convsep(Image mask, IDictionary<string, object> kwargs = null)
        {
            return this.Call("convsep", kwargs, mask) as Image;
        }

        /// <summary>
        /// Copy an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Copy(new Dictionary<string, object>
        /// {
        ///     {"width", int}
        ///     {"height", int}
        ///     {"bands", int}
        ///     {"format", string}
        ///     {"coding", string}
        ///     {"interpretation", string}
        ///     {"xres", double}
        ///     {"yres", double}
        ///     {"xoffset", int}
        ///     {"yoffset", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// width (int): Image width in pixels
        /// height (int): Image height in pixels
        /// bands (int): Number of bands in image
        /// format (string): Pixel format in image
        /// coding (string): Pixel coding
        /// interpretation (string): Pixel interpretation
        /// xres (double): Horizontal resolution in pixels/mm
        /// yres (double): Vertical resolution in pixels/mm
        /// xoffset (int): Horizontal offset of origin
        /// yoffset (int): Vertical offset of origin
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Copy(IDictionary<string, object> kwargs = null)
        {
            return this.Call("copy", kwargs) as Image;
        }

        /// <summary>
        /// Count lines in an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// double nolines = in.Countlines(direction);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Csvload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"skip", int}
        ///     {"lines", int}
        ///     {"fail", bool}
        ///     {"whitespace", string}
        ///     {"separator", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// skip (int): Skip this many lines at the start of the file
        /// lines (int): Read this many lines from the file
        /// fail (bool): Fail on first error
        /// whitespace (string): Set of whitespace characters
        /// separator (string): Set of separator characters
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Csvload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("csvload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Save image to csv file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Csvsave(filename, new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"separator", string}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// separator (string): Separator characters
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Csvsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("csvsave", kwargs, filename);
        }

        /// <summary>
        /// Calculate dE00
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = left.DE00(right);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = left.DE76(right);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = left.DECMC(right);
        /// ]]>
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
        /// <![CDATA[
        /// double @out = in.Deviate();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = left.Divide(right);
        /// ]]>
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
        /// <![CDATA[
        /// Image image = image.DrawCircle(ink, cx, cy, radius, new Dictionary<string, object>
        /// {
        ///     {"fill", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="cx">Centre of draw_circle</param>
        /// <param name="cy">Centre of draw_circle</param>
        /// <param name="radius">Radius in pixels</param>
        /// <param name="kwargs">
        /// fill (bool): Draw a solid object
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawCircle(double[] ink, int cx, int cy, int radius, IDictionary<string, object> kwargs = null)
        {
            return this.Call("draw_circle", kwargs, ink, cx, cy, radius) as Image;
        }

        /// <summary>
        /// Flood-fill an area
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var output = image.DrawFlood(ink, x, y, new Dictionary<string, object>
        /// {
        ///     {"test", Image}
        ///     {"equal", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="x">DrawFlood start point</param>
        /// <param name="y">DrawFlood start point</param>
        /// <param name="kwargs">
        /// test (Image): Test pixels in this image
        /// equal (bool): DrawFlood while equal to edge
        /// </param>
        /// <returns>A new <see cref="Image"/> or an array of new <see cref="Image"/>s</returns>
        public object DrawFlood(double[] ink, int x, int y, IDictionary<string, object> kwargs = null)
        {
            return this.Call("draw_flood", kwargs, ink, x, y);
        }

        /// <summary>
        /// Paint an image into another image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image image = image.DrawImage(sub, x, y, new Dictionary<string, object>
        /// {
        ///     {"mode", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sub">Sub-image to insert into main image</param>
        /// <param name="x">Draw image here</param>
        /// <param name="y">Draw image here</param>
        /// <param name="kwargs">
        /// mode (string): Combining mode
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawImage(Image sub, int x, int y, IDictionary<string, object> kwargs = null)
        {
            return this.Call("draw_image", kwargs, sub, x, y) as Image;
        }

        /// <summary>
        /// Draw a line on an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image image = image.DrawLine(ink, x1, y1, x2, y2);
        /// ]]>
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
        /// <![CDATA[
        /// Image image = image.DrawMask(ink, mask, x, y);
        /// ]]>
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
        /// <![CDATA[
        /// Image image = image.DrawRect(ink, left, top, width, height, new Dictionary<string, object>
        /// {
        ///     {"fill", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="ink">Color for pixels</param>
        /// <param name="left">Rect to fill</param>
        /// <param name="top">Rect to fill</param>
        /// <param name="width">Rect to fill</param>
        /// <param name="height">Rect to fill</param>
        /// <param name="kwargs">
        /// fill (bool): Draw a solid object
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image DrawRect(double[] ink, int left, int top, int width, int height,
            IDictionary<string, object> kwargs = null)
        {
            return this.Call("draw_rect", kwargs, ink, left, top, width, height) as Image;
        }

        /// <summary>
        /// Blur a rectangle on an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image image = image.DrawSmudge(left, top, width, height);
        /// ]]>
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
        /// <![CDATA[
        /// in.Dzsave(filename, new Dictionary<string, object>
        /// {
        ///     {"basename", string}
        ///     {"layout", string}
        ///     {"page_height", int}
        ///     {"suffix", string}
        ///     {"overlap", int}
        ///     {"tile_size", int}
        ///     {"centre", bool}
        ///     {"depth", string}
        ///     {"angle", string}
        ///     {"container", string}
        ///     {"properties", bool}
        ///     {"compression", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// basename (string): Base name to save to
        /// layout (string): Directory layout
        /// page_height (int): Set page height for multipage save
        /// suffix (string): Filename suffix for tiles
        /// overlap (int): Tile overlap in pixels
        /// tile_size (int): Tile size in pixels
        /// centre (bool): Center image in tile
        /// depth (string): Pyramid depth
        /// angle (string): Rotate image during save
        /// container (string): Pyramid container type
        /// properties (bool): Write a properties file to the output directory
        /// compression (int): ZIP deflate compression level
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Dzsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("dzsave", kwargs, filename);
        }

        /// <summary>
        /// Save image to dz buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// byte[] buffer = in.DzsaveBuffer(new Dictionary<string, object>
        /// {
        ///     {"basename", string}
        ///     {"layout", string}
        ///     {"page_height", int}
        ///     {"suffix", string}
        ///     {"overlap", int}
        ///     {"tile_size", int}
        ///     {"centre", bool}
        ///     {"depth", string}
        ///     {"angle", string}
        ///     {"container", string}
        ///     {"properties", bool}
        ///     {"compression", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// basename (string): Base name to save to
        /// layout (string): Directory layout
        /// page_height (int): Set page height for multipage save
        /// suffix (string): Filename suffix for tiles
        /// overlap (int): Tile overlap in pixels
        /// tile_size (int): Tile size in pixels
        /// centre (bool): Center image in tile
        /// depth (string): Pyramid depth
        /// angle (string): Rotate image during save
        /// container (string): Pyramid container type
        /// properties (bool): Write a properties file to the output directory
        /// compression (int): ZIP deflate compression level
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>An array of bytes</returns>
        public byte[] DzsaveBuffer(IDictionary<string, object> kwargs = null)
        {
            return this.Call("dzsave_buffer", kwargs) as byte[];
        }

        /// <summary>
        /// Embed an image in a larger image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Embed(x, y, width, height, new Dictionary<string, object>
        /// {
        ///     {"extend", string}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="x">Left edge of input in output</param>
        /// <param name="y">Top edge of input in output</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// extend (string): How to generate the extra pixels
        /// background (double[]): Color for background pixels
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Embed(int x, int y, int width, int height, IDictionary<string, object> kwargs = null)
        {
            return this.Call("embed", kwargs, x, y, width, height) as Image;
        }

        /// <summary>
        /// Extract an area from an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = input.ExtractArea(left, top, width, height);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.ExtractBand(band, new Dictionary<string, object>
        /// {
        ///     {"n", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="band">Band to extract</param>
        /// <param name="kwargs">
        /// n (int): Number of bands to extract
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ExtractBand(int band, IDictionary<string, object> kwargs = null)
        {
            return this.Call("extract_band", kwargs, band) as Image;
        }

        /// <summary>
        /// Make an image showing the eye's spatial response
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Eye(width, height, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"factor", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// factor (double): Maximum spatial frequency
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Eye(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("eye", kwargs, width, height) as Image;
        }

        /// <summary>
        /// False-color an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Falsecolour();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Fastcor(@ref);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.FillNearest();
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image FillNearest()
        {
            return this.Call("fill_nearest") as Image;
        }

        /// <summary>
        /// Search an image for non-edge areas
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var output = in.FindTrim(new Dictionary<string, object>
        /// {
        ///     {"threshold", double}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// threshold (double): Object threshold
        /// background (double[]): Color for background pixels
        /// </param>
        /// <returns>An array of objects</returns>
        public object[] FindTrim(IDictionary<string, object> kwargs = null)
        {
            return this.Call("find_trim", kwargs) as object[];
        }

        /// <summary>
        /// Flatten alpha out of an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Flatten(new Dictionary<string, object>
        /// {
        ///     {"background", double[]}
        ///     {"max_alpha", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// background (double[]): Background value
        /// max_alpha (double): Maximum value of alpha channel
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Flatten(IDictionary<string, object> kwargs = null)
        {
            return this.Call("flatten", kwargs) as Image;
        }

        /// <summary>
        /// Flip an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Flip(direction);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Float2rad();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Fractsurf(width, height, fractalDimension);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Freqmult(mask);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Fwfft();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Gamma(new Dictionary<string, object>
        /// {
        ///     {"exponent", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// exponent (double): Gamma factor
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Gamma(IDictionary<string, object> kwargs = null)
        {
            return this.Call("gamma", kwargs) as Image;
        }

        /// <summary>
        /// Gaussian blur
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Gaussblur(sigma, new Dictionary<string, object>
        /// {
        ///     {"min_ampl", double}
        ///     {"precision", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sigma">Sigma of Gaussian</param>
        /// <param name="kwargs">
        /// min_ampl (double): Minimum amplitude of Gaussian
        /// precision (string): Convolve with this precision
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Gaussblur(double sigma, IDictionary<string, object> kwargs = null)
        {
            return this.Call("gaussblur", kwargs, sigma) as Image;
        }

        /// <summary>
        /// Make a gaussian image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Gaussmat(sigma, minAmpl, new Dictionary<string, object>
        /// {
        ///     {"separable", bool}
        ///     {"precision", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sigma">Sigma of Gaussian</param>
        /// <param name="minAmpl">Minimum amplitude of Gaussian</param>
        /// <param name="kwargs">
        /// separable (bool): Generate separable Gaussian
        /// precision (string): Generate with this precision
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Gaussmat(double sigma, double minAmpl, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("gaussmat", kwargs, sigma, minAmpl) as Image;
        }

        /// <summary>
        /// Make a gaussnoise image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Gaussnoise(width, height, new Dictionary<string, object>
        /// {
        ///     {"sigma", double}
        ///     {"mean", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// sigma (double): Standard deviation of pixels in generated image
        /// mean (double): Mean of pixels in generated image
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Gaussnoise(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("gaussnoise", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Read a point from an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// double[] outArray = in.Getpoint(x, y);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Gifload(filename, new Dictionary<string, object>
        /// {
        ///     {"n", int}
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"page", int}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// n (int): Load this many pages
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// page (int): Load this page from the file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Gifload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("gifload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load GIF with giflib
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.GifloadBuffer(buffer, new Dictionary<string, object>
        /// {
        ///     {"n", int}
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"page", int}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="kwargs">
        /// n (int): Load this many pages
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// page (int): Load this page from the file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image GifloadBuffer(byte[] buffer, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("gifload_buffer", kwargs, buffer) as Image;
        }

        /// <summary>
        /// Global balance an image mosaic
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Globalbalance(new Dictionary<string, object>
        /// {
        ///     {"gamma", double}
        ///     {"int_output", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// gamma (double): Image gamma
        /// int_output (bool): Integer output
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Globalbalance(IDictionary<string, object> kwargs = null)
        {
            return this.Call("globalbalance", kwargs) as Image;
        }

        /// <summary>
        /// Place an image within a larger image with a certain gravity
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Gravity(direction, width, height, new Dictionary<string, object>
        /// {
        ///     {"extend", string}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="direction">direction to place image within width/height</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// extend (string): How to generate the extra pixels
        /// background (double[]): Color for background pixels
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Gravity(string direction, int width, int height, IDictionary<string, object> kwargs = null)
        {
            return this.Call("gravity", kwargs, direction, width, height) as Image;
        }

        /// <summary>
        /// Make a grey ramp image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Grey(width, height, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Grey(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("grey", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Grid an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Grid(tileHeight, across, down);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.HistCum();
        /// ]]>
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
        /// <![CDATA[
        /// double @out = in.HistEntropy();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.HistEqual(new Dictionary<string, object>
        /// {
        ///     {"band", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// band (int): Equalise with this band
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistEqual(IDictionary<string, object> kwargs = null)
        {
            return this.Call("hist_equal", kwargs) as Image;
        }

        /// <summary>
        /// Find image histogram
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.HistFind(new Dictionary<string, object>
        /// {
        ///     {"band", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// band (int): Find histogram of band
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistFind(IDictionary<string, object> kwargs = null)
        {
            return this.Call("hist_find", kwargs) as Image;
        }

        /// <summary>
        /// Find indexed image histogram
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.HistFindIndexed(index, new Dictionary<string, object>
        /// {
        ///     {"combine", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="index">Index image</param>
        /// <param name="kwargs">
        /// combine (string): Combine bins like this
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistFindIndexed(Image index, IDictionary<string, object> kwargs = null)
        {
            return this.Call("hist_find_indexed", kwargs, index) as Image;
        }

        /// <summary>
        /// Find n-dimensional image histogram
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.HistFindNdim(new Dictionary<string, object>
        /// {
        ///     {"bins", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// bins (int): Number of bins in each dimension
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistFindNdim(IDictionary<string, object> kwargs = null)
        {
            return this.Call("hist_find_ndim", kwargs) as Image;
        }

        /// <summary>
        /// Test for monotonicity
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// bool monotonic = in.HistIsmonotonic();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.HistLocal(width, height, new Dictionary<string, object>
        /// {
        ///     {"max_slope", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Window width in pixels</param>
        /// <param name="height">Window height in pixels</param>
        /// <param name="kwargs">
        /// max_slope (int): Maximum slope (CLAHE)
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HistLocal(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return this.Call("hist_local", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Match two histograms
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.HistMatch(@ref);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.HistNorm();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.HistPlot();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.HoughCircle(new Dictionary<string, object>
        /// {
        ///     {"scale", int}
        ///     {"min_radius", int}
        ///     {"max_radius", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// scale (int): Scale down dimensions by this factor
        /// min_radius (int): Smallest radius to search for
        /// max_radius (int): Largest radius to search for
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HoughCircle(IDictionary<string, object> kwargs = null)
        {
            return this.Call("hough_circle", kwargs) as Image;
        }

        /// <summary>
        /// Find hough line transform
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.HoughLine(new Dictionary<string, object>
        /// {
        ///     {"width", int}
        ///     {"height", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// width (int): horizontal size of parameter space
        /// height (int): Vertical size of parameter space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image HoughLine(IDictionary<string, object> kwargs = null)
        {
            return this.Call("hough_line", kwargs) as Image;
        }

        /// <summary>
        /// Transform HSV to sRGB
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.HSV2sRGB();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.IccExport(new Dictionary<string, object>
        /// {
        ///     {"pcs", string}
        ///     {"intent", string}
        ///     {"output_profile", string}
        ///     {"depth", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// pcs (string): Set Profile Connection Space
        /// intent (string): Rendering intent
        /// output_profile (string): Filename to load output profile from
        /// depth (int): Output device space depth in bits
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image IccExport(IDictionary<string, object> kwargs = null)
        {
            return this.Call("icc_export", kwargs) as Image;
        }

        /// <summary>
        /// Import from device with ICC profile
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.IccImport(new Dictionary<string, object>
        /// {
        ///     {"pcs", string}
        ///     {"intent", string}
        ///     {"embedded", bool}
        ///     {"input_profile", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// pcs (string): Set Profile Connection Space
        /// intent (string): Rendering intent
        /// embedded (bool): Use embedded input profile, if available
        /// input_profile (string): Filename to load input profile from
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image IccImport(IDictionary<string, object> kwargs = null)
        {
            return this.Call("icc_import", kwargs) as Image;
        }

        /// <summary>
        /// Transform between devices with ICC profiles
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.IccTransform(outputProfile, new Dictionary<string, object>
        /// {
        ///     {"pcs", string}
        ///     {"intent", string}
        ///     {"embedded", bool}
        ///     {"input_profile", string}
        ///     {"depth", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="outputProfile">Filename to load output profile from</param>
        /// <param name="kwargs">
        /// pcs (string): Set Profile Connection Space
        /// intent (string): Rendering intent
        /// embedded (bool): Use embedded input profile, if available
        /// input_profile (string): Filename to load input profile from
        /// depth (int): Output device space depth in bits
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image IccTransform(string outputProfile, IDictionary<string, object> kwargs = null)
        {
            return this.Call("icc_transform", kwargs, outputProfile) as Image;
        }

        /// <summary>
        /// Make a 1D image where pixel values are indexes
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Identity(new Dictionary<string, object>
        /// {
        ///     {"bands", int}
        ///     {"ushort", bool}
        ///     {"size", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// bands (int): Number of bands in LUT
        /// ushort (bool): Create a 16-bit LUT
        /// size (int): Size of 16-bit LUT
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Identity(IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("identity", kwargs) as Image;
        }

        /// <summary>
        /// Insert image @sub into @main at @x, @y
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = main.Insert(sub, x, y, new Dictionary<string, object>
        /// {
        ///     {"expand", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sub">Sub-image to insert into main image</param>
        /// <param name="x">Left edge of sub in main</param>
        /// <param name="y">Top edge of sub in main</param>
        /// <param name="kwargs">
        /// expand (bool): Expand output to hold all of both inputs
        /// background (double[]): Color for new pixels
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Insert(Image sub, int x, int y, IDictionary<string, object> kwargs = null)
        {
            return this.Call("insert", kwargs, sub, x, y) as Image;
        }

        /// <summary>
        /// Invert an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Invert();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Invertlut(new Dictionary<string, object>
        /// {
        ///     {"size", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// size (int): LUT size to generate
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Invertlut(IDictionary<string, object> kwargs = null)
        {
            return this.Call("invertlut", kwargs) as Image;
        }

        /// <summary>
        /// Inverse FFT
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Invfft(new Dictionary<string, object>
        /// {
        ///     {"real", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// real (bool): Output only the real part of the transform
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Invfft(IDictionary<string, object> kwargs = null)
        {
            return this.Call("invfft", kwargs) as Image;
        }

        /// <summary>
        /// Join a pair of images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in1.Join(in2, direction, new Dictionary<string, object>
        /// {
        ///     {"expand", bool}
        ///     {"shim", int}
        ///     {"background", double[]}
        ///     {"align", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="in2">Second input image</param>
        /// <param name="direction">Join left-right or up-down</param>
        /// <param name="kwargs">
        /// expand (bool): Expand output to hold all of both inputs
        /// shim (int): Pixels between images
        /// background (double[]): Colour for new pixels
        /// align (string): Align on the low, centre or high coordinate edge
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Join(Image in2, string direction, IDictionary<string, object> kwargs = null)
        {
            return this.Call("join", kwargs, in2, direction) as Image;
        }

        /// <summary>
        /// Load jpeg from file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Jpegload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"shrink", int}
        ///     {"fail", bool}
        ///     {"autorotate", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// shrink (int): Shrink factor on load
        /// fail (bool): Fail on first error
        /// autorotate (bool): Rotate image using exif orientation
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Jpegload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("jpegload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load jpeg from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.JpegloadBuffer(buffer, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"shrink", int}
        ///     {"fail", bool}
        ///     {"autorotate", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// shrink (int): Shrink factor on load
        /// fail (bool): Fail on first error
        /// autorotate (bool): Rotate image using exif orientation
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image JpegloadBuffer(byte[] buffer, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("jpegload_buffer", kwargs, buffer) as Image;
        }

        /// <summary>
        /// Save image to jpeg file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Jpegsave(filename, new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"Q", int}
        ///     {"profile", string}
        ///     {"optimize_coding", bool}
        ///     {"interlace", bool}
        ///     {"no_subsample", bool}
        ///     {"trellis_quant", bool}
        ///     {"overshoot_deringing", bool}
        ///     {"optimize_scans", bool}
        ///     {"quant_table", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// Q (int): Q factor
        /// profile (string): ICC profile to embed
        /// optimize_coding (bool): Compute optimal Huffman coding tables
        /// interlace (bool): Generate an interlaced (progressive) jpeg
        /// no_subsample (bool): Disable chroma subsample
        /// trellis_quant (bool): Apply trellis quantisation to each 8x8 block
        /// overshoot_deringing (bool): Apply overshooting to samples with extreme values
        /// optimize_scans (bool): Split the spectrum of DCT coefficients into separate scans
        /// quant_table (int): Use predefined quantization table with given index
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Jpegsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("jpegsave", kwargs, filename);
        }

        /// <summary>
        /// Save image to jpeg buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// byte[] buffer = in.JpegsaveBuffer(new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"Q", int}
        ///     {"profile", string}
        ///     {"optimize_coding", bool}
        ///     {"interlace", bool}
        ///     {"no_subsample", bool}
        ///     {"trellis_quant", bool}
        ///     {"overshoot_deringing", bool}
        ///     {"optimize_scans", bool}
        ///     {"quant_table", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// Q (int): Q factor
        /// profile (string): ICC profile to embed
        /// optimize_coding (bool): Compute optimal Huffman coding tables
        /// interlace (bool): Generate an interlaced (progressive) jpeg
        /// no_subsample (bool): Disable chroma subsample
        /// trellis_quant (bool): Apply trellis quantisation to each 8x8 block
        /// overshoot_deringing (bool): Apply overshooting to samples with extreme values
        /// optimize_scans (bool): Split the spectrum of DCT coefficients into separate scans
        /// quant_table (int): Use predefined quantization table with given index
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>An array of bytes</returns>
        public byte[] JpegsaveBuffer(IDictionary<string, object> kwargs = null)
        {
            return this.Call("jpegsave_buffer", kwargs) as byte[];
        }

        /// <summary>
        /// Save image to jpeg mime
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.JpegsaveMime(new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"Q", int}
        ///     {"profile", string}
        ///     {"optimize_coding", bool}
        ///     {"interlace", bool}
        ///     {"no_subsample", bool}
        ///     {"trellis_quant", bool}
        ///     {"overshoot_deringing", bool}
        ///     {"optimize_scans", bool}
        ///     {"quant_table", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// Q (int): Q factor
        /// profile (string): ICC profile to embed
        /// optimize_coding (bool): Compute optimal Huffman coding tables
        /// interlace (bool): Generate an interlaced (progressive) jpeg
        /// no_subsample (bool): Disable chroma subsample
        /// trellis_quant (bool): Apply trellis quantisation to each 8x8 block
        /// overshoot_deringing (bool): Apply overshooting to samples with extreme values
        /// optimize_scans (bool): Split the spectrum of DCT coefficients into separate scans
        /// quant_table (int): Use predefined quantization table with given index
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void JpegsaveMime(IDictionary<string, object> kwargs = null)
        {
            this.Call("jpegsave_mime", kwargs);
        }

        /// <summary>
        /// Transform float Lab to LabQ coding
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Lab2LabQ();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Lab2LabS();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Lab2LCh();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Lab2XYZ(new Dictionary<string, object>
        /// {
        ///     {"temp", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// temp (double[]): Color temperature
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Lab2XYZ(IDictionary<string, object> kwargs = null)
        {
            return this.Call("Lab2XYZ", kwargs) as Image;
        }

        /// <summary>
        /// Label regions in an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image mask = in.Labelregions();
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Labelregions()
        {
            return this.Call("labelregions") as Image;
        }

        /// <summary>
        /// Unpack a LabQ image to float Lab
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.LabQ2Lab();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.LabQ2LabS();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.LabQ2sRGB();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.LabS2Lab();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.LabS2LabQ();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.LCh2CMC();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.LCh2Lab();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Linear(a, b, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="a">Multiply by this</param>
        /// <param name="b">Add this</param>
        /// <param name="kwargs">
        /// uchar (bool): Output should be uchar
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Linear(double[] a, double[] b, IDictionary<string, object> kwargs = null)
        {
            return this.Call("linear", kwargs, a, b) as Image;
        }

        /// <summary>
        /// Cache an image as a set of lines
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Linecache(new Dictionary<string, object>
        /// {
        ///     {"tile_height", int}
        ///     {"access", string}
        ///     {"threaded", bool}
        ///     {"persistent", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// tile_height (int): Tile height in pixels
        /// access (string): Expected access pattern
        /// threaded (bool): Allow threaded access
        /// persistent (bool): Keep cache between evaluations
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Linecache(IDictionary<string, object> kwargs = null)
        {
            return this.Call("linecache", kwargs) as Image;
        }

        /// <summary>
        /// Make a laplacian of gaussian image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Logmat(sigma, minAmpl, new Dictionary<string, object>
        /// {
        ///     {"separable", bool}
        ///     {"precision", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sigma">Radius of Logmatian</param>
        /// <param name="minAmpl">Minimum amplitude of Logmatian</param>
        /// <param name="kwargs">
        /// separable (bool): Generate separable Logmatian
        /// precision (string): Generate with this precision
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Logmat(double sigma, double minAmpl, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("logmat", kwargs, sigma, minAmpl) as Image;
        }

        /// <summary>
        /// Load file with ImageMagick
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Magickload(filename, new Dictionary<string, object>
        /// {
        ///     {"density", string}
        ///     {"page", int}
        ///     {"n", int}
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// density (string): Canvas resolution for rendering vector formats like SVG
        /// page (int): Load this page from the file
        /// n (int): Load this many pages
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Magickload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("magickload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load buffer with ImageMagick
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MagickloadBuffer(buffer, new Dictionary<string, object>
        /// {
        ///     {"density", string}
        ///     {"page", int}
        ///     {"n", int}
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="kwargs">
        /// density (string): Canvas resolution for rendering vector formats like SVG
        /// page (int): Load this page from the file
        /// n (int): Load this many pages
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MagickloadBuffer(byte[] buffer, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("magickload_buffer", kwargs, buffer) as Image;
        }

        /// <summary>
        /// Save file with ImageMagick
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Magicksave(filename, new Dictionary<string, object>
        /// {
        ///     {"format", string}
        ///     {"quality", int}
        ///     {"page_height", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// format (string): Format to save in
        /// quality (int): Quality to use
        /// page_height (int): Set page height for multipage save
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Magicksave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("magicksave", kwargs, filename);
        }

        /// <summary>
        /// Save image to magick buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// byte[] buffer = in.MagicksaveBuffer(new Dictionary<string, object>
        /// {
        ///     {"format", string}
        ///     {"quality", int}
        ///     {"page_height", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// format (string): Format to save in
        /// quality (int): Quality to use
        /// page_height (int): Set page height for multipage save
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>An array of bytes</returns>
        public byte[] MagicksaveBuffer(IDictionary<string, object> kwargs = null)
        {
            return this.Call("magicksave_buffer", kwargs) as byte[];
        }

        /// <summary>
        /// Resample with an mapim image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Mapim(index, new Dictionary<string, object>
        /// {
        ///     {"interpolate", GObject}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="index">Index pixels with this</param>
        /// <param name="kwargs">
        /// interpolate (GObject): Interpolate pixels with this
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mapim(Image index, IDictionary<string, object> kwargs = null)
        {
            return this.Call("mapim", kwargs, index) as Image;
        }

        /// <summary>
        /// Map an image though a lut
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Maplut(lut, new Dictionary<string, object>
        /// {
        ///     {"band", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="lut">Look-up table image</param>
        /// <param name="kwargs">
        /// band (int): apply one-band lut to this band of in
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Maplut(Image lut, IDictionary<string, object> kwargs = null)
        {
            return this.Call("maplut", kwargs, lut) as Image;
        }

        /// <summary>
        /// Make a butterworth filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskButterworth(width, height, order, frequencyCutoff, amplitudeCutoff, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="order">Filter order</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskButterworth(int width, int height, double order, double frequencyCutoff,
            double amplitudeCutoff, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_butterworth", kwargs, width, height, order, frequencyCutoff,
                amplitudeCutoff) as Image;
        }

        /// <summary>
        /// Make a butterworth_band filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskButterworthBand(width, height, order, frequencyCutoffX, frequencyCutoffY, radius, amplitudeCutoff, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="order">Filter order</param>
        /// <param name="frequencyCutoffX">Frequency cutoff x</param>
        /// <param name="frequencyCutoffY">Frequency cutoff y</param>
        /// <param name="radius">radius of circle</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskButterworthBand(int width, int height, double order, double frequencyCutoffX,
            double frequencyCutoffY, double radius, double amplitudeCutoff, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_butterworth_band", kwargs, width, height, order, frequencyCutoffX,
                frequencyCutoffY, radius, amplitudeCutoff) as Image;
        }

        /// <summary>
        /// Make a butterworth ring filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskButterworthRing(width, height, order, frequencyCutoff, amplitudeCutoff, ringwidth, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="order">Filter order</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="ringwidth">Ringwidth</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskButterworthRing(int width, int height, double order, double frequencyCutoff,
            double amplitudeCutoff, double ringwidth, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_butterworth_ring", kwargs, width, height, order, frequencyCutoff,
                amplitudeCutoff, ringwidth) as Image;
        }

        /// <summary>
        /// Make fractal filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskFractal(width, height, fractalDimension, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="fractalDimension">Fractal dimension</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskFractal(int width, int height, double fractalDimension,
            IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_fractal", kwargs, width, height, fractalDimension) as Image;
        }

        /// <summary>
        /// Make a gaussian filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskGaussian(width, height, frequencyCutoff, amplitudeCutoff, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskGaussian(int width, int height, double frequencyCutoff, double amplitudeCutoff,
            IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_gaussian", kwargs, width, height, frequencyCutoff, amplitudeCutoff) as Image;
        }

        /// <summary>
        /// Make a gaussian filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskGaussianBand(width, height, frequencyCutoffX, frequencyCutoffY, radius, amplitudeCutoff, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoffX">Frequency cutoff x</param>
        /// <param name="frequencyCutoffY">Frequency cutoff y</param>
        /// <param name="radius">radius of circle</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskGaussianBand(int width, int height, double frequencyCutoffX, double frequencyCutoffY,
            double radius, double amplitudeCutoff, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_gaussian_band", kwargs, width, height, frequencyCutoffX, frequencyCutoffY,
                radius, amplitudeCutoff) as Image;
        }

        /// <summary>
        /// Make a gaussian ring filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskGaussianRing(width, height, frequencyCutoff, amplitudeCutoff, ringwidth, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="amplitudeCutoff">Amplitude cutoff</param>
        /// <param name="ringwidth">Ringwidth</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskGaussianRing(int width, int height, double frequencyCutoff, double amplitudeCutoff,
            double ringwidth, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_gaussian_ring", kwargs, width, height, frequencyCutoff, amplitudeCutoff,
                ringwidth) as Image;
        }

        /// <summary>
        /// Make an ideal filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskIdeal(width, height, frequencyCutoff, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskIdeal(int width, int height, double frequencyCutoff,
            IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_ideal", kwargs, width, height, frequencyCutoff) as Image;
        }

        /// <summary>
        /// Make an ideal band filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskIdealBand(width, height, frequencyCutoffX, frequencyCutoffY, radius, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoffX">Frequency cutoff x</param>
        /// <param name="frequencyCutoffY">Frequency cutoff y</param>
        /// <param name="radius">radius of circle</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskIdealBand(int width, int height, double frequencyCutoffX, double frequencyCutoffY,
            double radius, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_ideal_band", kwargs, width, height, frequencyCutoffX, frequencyCutoffY,
                radius) as Image;
        }

        /// <summary>
        /// Make an ideal ring filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.MaskIdealRing(width, height, frequencyCutoff, ringwidth, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"nodc", bool}
        ///     {"reject", bool}
        ///     {"optical", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="frequencyCutoff">Frequency cutoff</param>
        /// <param name="ringwidth">Ringwidth</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// nodc (bool): Remove DC component
        /// reject (bool): Invert the sense of the filter
        /// optical (bool): Rotate quadrants to optical space
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image MaskIdealRing(int width, int height, double frequencyCutoff, double ringwidth,
            IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("mask_ideal_ring", kwargs, width, height, frequencyCutoff, ringwidth) as Image;
        }

        /// <summary>
        /// First-order match of two images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = ref.Match(sec, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, new Dictionary<string, object>
        /// {
        ///     {"hwindow", int}
        ///     {"harea", int}
        ///     {"search", bool}
        ///     {"interpolate", GObject}
        /// });
        /// ]]>
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
        /// <param name="kwargs">
        /// hwindow (int): Half window size
        /// harea (int): Half area size
        /// search (bool): Search to improve tie-points
        /// interpolate (GObject): Interpolate pixels with this
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Match(Image sec, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2, int ys2,
            IDictionary<string, object> kwargs = null)
        {
            return this.Call("match", kwargs, sec, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2) as Image;
        }

        /// <summary>
        /// Apply a math operation to an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Math(math);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = left.Math2(right, math2);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Math2Const(math2, c);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Matload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Matload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("matload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load matrix from file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Matrixload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Matrixload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("matrixload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Print matrix
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Matrixprint(new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Matrixprint(IDictionary<string, object> kwargs = null)
        {
            this.Call("matrixprint", kwargs);
        }

        /// <summary>
        /// Save image to matrix file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Matrixsave(filename, new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Matrixsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("matrixsave", kwargs, filename);
        }

        /// <summary>
        /// Find image maximum
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var output = in.Max(new Dictionary<string, object>
        /// {
        ///     {"size", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// size (int): Number of maximum values to find
        /// </param>
        /// <returns>A double or an array of doubles</returns>
        public object Max(IDictionary<string, object> kwargs = null)
        {
            return this.Call("max", kwargs);
        }

        /// <summary>
        /// Measure a set of patches on a color chart
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Measure(h, v, new Dictionary<string, object>
        /// {
        ///     {"left", int}
        ///     {"top", int}
        ///     {"width", int}
        ///     {"height", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="h">Number of patches across chart</param>
        /// <param name="v">Number of patches down chart</param>
        /// <param name="kwargs">
        /// left (int): Left edge of extract area
        /// top (int): Top edge of extract area
        /// width (int): Width of extract area
        /// height (int): Height of extract area
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Measure(int h, int v, IDictionary<string, object> kwargs = null)
        {
            return this.Call("measure", kwargs, h, v) as Image;
        }

        /// <summary>
        /// Merge two images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = ref.Merge(sec, direction, dx, dy, new Dictionary<string, object>
        /// {
        ///     {"mblend", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial merge</param>
        /// <param name="dx">Horizontal displacement from sec to ref</param>
        /// <param name="dy">Vertical displacement from sec to ref</param>
        /// <param name="kwargs">
        /// mblend (int): Maximum blend size
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Merge(Image sec, string direction, int dx, int dy, IDictionary<string, object> kwargs = null)
        {
            return this.Call("merge", kwargs, sec, direction, dx, dy) as Image;
        }

        /// <summary>
        /// Find image minimum
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var output = in.Min(new Dictionary<string, object>
        /// {
        ///     {"size", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// size (int): Number of minimum values to find
        /// </param>
        /// <returns>A double or an array of doubles</returns>
        public object Min(IDictionary<string, object> kwargs = null)
        {
            return this.Call("min", kwargs);
        }

        /// <summary>
        /// Morphology operation
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Morph(mask, morph);
        /// ]]>
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
        /// <![CDATA[
        /// var output = ref.Mosaic(sec, direction, xref, yref, xsec, ysec, new Dictionary<string, object>
        /// {
        ///     {"hwindow", int}
        ///     {"harea", int}
        ///     {"mblend", int}
        ///     {"bandno", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="sec">Secondary image</param>
        /// <param name="direction">Horizontal or vertcial mosaic</param>
        /// <param name="xref">Position of reference tie-point</param>
        /// <param name="yref">Position of reference tie-point</param>
        /// <param name="xsec">Position of secondary tie-point</param>
        /// <param name="ysec">Position of secondary tie-point</param>
        /// <param name="kwargs">
        /// hwindow (int): Half window size
        /// harea (int): Half area size
        /// mblend (int): Maximum blend size
        /// bandno (int): Band to search for features on
        /// </param>
        /// <returns>A new <see cref="Image"/> or an array of new <see cref="Image"/>s</returns>
        public object Mosaic(Image sec, string direction, int xref, int yref, int xsec, int ysec,
            IDictionary<string, object> kwargs = null)
        {
            return this.Call("mosaic", kwargs, sec, direction, xref, yref, xsec, ysec);
        }

        /// <summary>
        /// First-order mosaic of two images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = ref.Mosaic1(sec, direction, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, new Dictionary<string, object>
        /// {
        ///     {"hwindow", int}
        ///     {"harea", int}
        ///     {"search", bool}
        ///     {"interpolate", GObject}
        ///     {"mblend", int}
        ///     {"bandno", int}
        /// });
        /// ]]>
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
        /// <param name="kwargs">
        /// hwindow (int): Half window size
        /// harea (int): Half area size
        /// search (bool): Search to improve tie-points
        /// interpolate (GObject): Interpolate pixels with this
        /// mblend (int): Maximum blend size
        /// bandno (int): Band to search for features on
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Mosaic1(Image sec, string direction, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2,
            int ys2, IDictionary<string, object> kwargs = null)
        {
            return this.Call("mosaic1", kwargs, sec, direction, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2) as Image;
        }

        /// <summary>
        /// Pick most-significant byte from an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Msb(new Dictionary<string, object>
        /// {
        ///     {"band", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// band (int): Band to msb
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Msb(IDictionary<string, object> kwargs = null)
        {
            return this.Call("msb", kwargs) as Image;
        }

        /// <summary>
        /// Multiply two images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = left.Multiply(right);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Openslideload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"level", int}
        ///     {"autocrop", bool}
        ///     {"fail", bool}
        ///     {"associated", string}
        ///     {"attach_associated", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// level (int): Load this level from the file
        /// autocrop (bool): Crop to image bounds
        /// fail (bool): Fail on first error
        /// associated (string): Load this associated image
        /// attach_associated (bool): Attach all asssociated images
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Openslideload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("openslideload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load PDF with libpoppler
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Pdfload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"page", int}
        ///     {"n", int}
        ///     {"fail", bool}
        ///     {"dpi", double}
        ///     {"scale", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// page (int): Load this page from the file
        /// n (int): Load this many pages
        /// fail (bool): Fail on first error
        /// dpi (double): Render at this DPI
        /// scale (double): Scale output by this factor
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Pdfload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("pdfload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load PDF with libpoppler
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.PdfloadBuffer(buffer, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"page", int}
        ///     {"n", int}
        ///     {"fail", bool}
        ///     {"dpi", double}
        ///     {"scale", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// page (int): Load this page from the file
        /// n (int): Load this many pages
        /// fail (bool): Fail on first error
        /// dpi (double): Render at this DPI
        /// scale (double): Scale output by this factor
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image PdfloadBuffer(byte[] buffer, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("pdfload_buffer", kwargs, buffer) as Image;
        }

        /// <summary>
        /// Find threshold for percent of pixels
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// int threshold = in.Percent(percent);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Perlin(width, height, new Dictionary<string, object>
        /// {
        ///     {"cell_size", int}
        ///     {"uchar", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// cell_size (int): Size of Perlin cells
        /// uchar (bool): Output an unsigned char image
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Perlin(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("perlin", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Calculate phase correlation
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Phasecor(in2);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Pngload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Pngload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("pngload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load png from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.PngloadBuffer(buffer, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image PngloadBuffer(byte[] buffer, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("pngload_buffer", kwargs, buffer) as Image;
        }

        /// <summary>
        /// Save image to png file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Pngsave(filename, new Dictionary<string, object>
        /// {
        ///     {"compression", int}
        ///     {"interlace", bool}
        ///     {"page_height", int}
        ///     {"profile", string}
        ///     {"filter", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// compression (int): Compression factor
        /// interlace (bool): Interlace image
        /// page_height (int): Set page height for multipage save
        /// profile (string): ICC profile to embed
        /// filter (int): libpng row filter flag(s)
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Pngsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("pngsave", kwargs, filename);
        }

        /// <summary>
        /// Save image to png buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// byte[] buffer = in.PngsaveBuffer(new Dictionary<string, object>
        /// {
        ///     {"compression", int}
        ///     {"interlace", bool}
        ///     {"page_height", int}
        ///     {"profile", string}
        ///     {"filter", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// compression (int): Compression factor
        /// interlace (bool): Interlace image
        /// page_height (int): Set page height for multipage save
        /// profile (string): ICC profile to embed
        /// filter (int): libpng row filter flag(s)
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>An array of bytes</returns>
        public byte[] PngsaveBuffer(IDictionary<string, object> kwargs = null)
        {
            return this.Call("pngsave_buffer", kwargs) as byte[];
        }

        /// <summary>
        /// Load ppm from file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Ppmload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Ppmload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("ppmload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Save image to ppm file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Ppmsave(filename, new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"ascii", bool}
        ///     {"squash", bool}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// ascii (bool): save as ascii
        /// squash (bool): save as one bit
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Ppmsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("ppmsave", kwargs, filename);
        }

        /// <summary>
        /// Premultiply image alpha
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Premultiply(new Dictionary<string, object>
        /// {
        ///     {"max_alpha", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// max_alpha (double): Maximum value of alpha channel
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Premultiply(IDictionary<string, object> kwargs = null)
        {
            return this.Call("premultiply", kwargs) as Image;
        }

        /// <summary>
        /// Find image profiles
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var output = in.Profile();
        /// ]]>
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
        /// <![CDATA[
        /// var output = in.Project();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Quadratic(coeff, new Dictionary<string, object>
        /// {
        ///     {"interpolate", GObject}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="coeff">Coefficient matrix</param>
        /// <param name="kwargs">
        /// interpolate (GObject): Interpolate values with this
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Quadratic(Image coeff, IDictionary<string, object> kwargs = null)
        {
            return this.Call("quadratic", kwargs, coeff) as Image;
        }

        /// <summary>
        /// Unpack Radiance coding to float RGB
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Rad2float();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Radload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Radload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("radload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Save image to Radiance file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Radsave(filename, new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Radsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("radsave", kwargs, filename);
        }

        /// <summary>
        /// Save image to Radiance buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// byte[] buffer = in.RadsaveBuffer(new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>An array of bytes</returns>
        public byte[] RadsaveBuffer(IDictionary<string, object> kwargs = null)
        {
            return this.Call("radsave_buffer", kwargs) as byte[];
        }

        /// <summary>
        /// Rank filter
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Rank(width, height, index);
        /// ]]>
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
        /// Load raw data from a file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Rawload(filename, width, height, bands, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        ///     {"offset", object}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="bands">Number of bands in image</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// offset (object): Offset in bytes from start of file
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Rawload(string filename, int width, int height, int bands,
            IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("rawload", kwargs, filename, width, height, bands) as Image;
        }

        /// <summary>
        /// Save image to raw file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Rawsave(filename, new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Rawsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("rawsave", kwargs, filename);
        }

        /// <summary>
        /// Write raw image to file descriptor
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.RawsaveFd(fd, new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="fd">File descriptor to write to</param>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void RawsaveFd(int fd, IDictionary<string, object> kwargs = null)
        {
            this.Call("rawsave_fd", kwargs, fd);
        }

        /// <summary>
        /// Linear recombination with matrix
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Recomb(m);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Reduce(hshrink, vshrink, new Dictionary<string, object>
        /// {
        ///     {"kernel", string}
        ///     {"centre", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="hshrink">Horizontal shrink factor</param>
        /// <param name="vshrink">Vertical shrink factor</param>
        /// <param name="kwargs">
        /// kernel (string): Resampling kernel
        /// centre (bool): Use centre sampling convention
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Reduce(double hshrink, double vshrink, IDictionary<string, object> kwargs = null)
        {
            return this.Call("reduce", kwargs, hshrink, vshrink) as Image;
        }

        /// <summary>
        /// Shrink an image horizontally
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Reduceh(hshrink, new Dictionary<string, object>
        /// {
        ///     {"kernel", string}
        ///     {"centre", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="hshrink">Horizontal shrink factor</param>
        /// <param name="kwargs">
        /// kernel (string): Resampling kernel
        /// centre (bool): Use centre sampling convention
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Reduceh(double hshrink, IDictionary<string, object> kwargs = null)
        {
            return this.Call("reduceh", kwargs, hshrink) as Image;
        }

        /// <summary>
        /// Shrink an image vertically
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Reducev(vshrink, new Dictionary<string, object>
        /// {
        ///     {"kernel", string}
        ///     {"centre", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="vshrink">Vertical shrink factor</param>
        /// <param name="kwargs">
        /// kernel (string): Resampling kernel
        /// centre (bool): Use centre sampling convention
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Reducev(double vshrink, IDictionary<string, object> kwargs = null)
        {
            return this.Call("reducev", kwargs, vshrink) as Image;
        }

        /// <summary>
        /// Relational operation on two images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = left.Relational(right, relational);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.RelationalConst(relational, c);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = left.Remainder(right);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.RemainderConst(c);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Replicate(across, down);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Resize(scale, new Dictionary<string, object>
        /// {
        ///     {"kernel", string}
        ///     {"vscale", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="scale">Scale image by this factor</param>
        /// <param name="kwargs">
        /// kernel (string): Resampling kernel
        /// vscale (double): Vertical scale image by this factor
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Resize(double scale, IDictionary<string, object> kwargs = null)
        {
            return this.Call("resize", kwargs, scale) as Image;
        }

        /// <summary>
        /// Rotate an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Rot(angle);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Rot45(new Dictionary<string, object>
        /// {
        ///     {"angle", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// angle (string): Angle to rotate image
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Rot45(IDictionary<string, object> kwargs = null)
        {
            return this.Call("rot45", kwargs) as Image;
        }

        /// <summary>
        /// Perform a round function on an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Round(round);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.ScRGB2BW(new Dictionary<string, object>
        /// {
        ///     {"depth", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// depth (int): Output device space depth in bits
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ScRGB2BW(IDictionary<string, object> kwargs = null)
        {
            return this.Call("scRGB2BW", kwargs) as Image;
        }

        /// <summary>
        /// Convert an scRGB image to sRGB
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.ScRGB2sRGB(new Dictionary<string, object>
        /// {
        ///     {"depth", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// depth (int): Output device space depth in bits
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ScRGB2sRGB(IDictionary<string, object> kwargs = null)
        {
            return this.Call("scRGB2sRGB", kwargs) as Image;
        }

        /// <summary>
        /// Transform scRGB to XYZ
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.ScRGB2XYZ();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Sequential(new Dictionary<string, object>
        /// {
        ///     {"tile_height", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// tile_height (int): Tile height in pixels
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Sequential(IDictionary<string, object> kwargs = null)
        {
            return this.Call("sequential", kwargs) as Image;
        }

        /// <summary>
        /// Unsharp masking for print
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Sharpen(new Dictionary<string, object>
        /// {
        ///     {"sigma", double}
        ///     {"x1", double}
        ///     {"y2", double}
        ///     {"y3", double}
        ///     {"m1", double}
        ///     {"m2", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// sigma (double): Sigma of Gaussian
        /// x1 (double): Flat/jaggy threshold
        /// y2 (double): Maximum brightening
        /// y3 (double): Maximum darkening
        /// m1 (double): Slope for flat areas
        /// m2 (double): Slope for jaggy areas
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Sharpen(IDictionary<string, object> kwargs = null)
        {
            return this.Call("sharpen", kwargs) as Image;
        }

        /// <summary>
        /// Shrink an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Shrink(hshrink, vshrink);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Shrinkh(hshrink);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Shrinkv(vshrink);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Sign();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Similarity(new Dictionary<string, object>
        /// {
        ///     {"background", double[]}
        ///     {"interpolate", GObject}
        ///     {"scale", double}
        ///     {"angle", double}
        ///     {"odx", double}
        ///     {"ody", double}
        ///     {"idx", double}
        ///     {"idy", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// background (double[]): Background value
        /// interpolate (GObject): Interpolate pixels with this
        /// scale (double): Scale by this factor
        /// angle (double): Rotate anticlockwise by this many degrees
        /// odx (double): Horizontal output displacement
        /// ody (double): Vertical output displacement
        /// idx (double): Horizontal input displacement
        /// idy (double): Vertical input displacement
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Similarity(IDictionary<string, object> kwargs = null)
        {
            return this.Call("similarity", kwargs) as Image;
        }

        /// <summary>
        /// Make a 2D sine wave
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Sines(width, height, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        ///     {"hfreq", double}
        ///     {"vfreq", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// hfreq (double): Horizontal spatial frequency
        /// vfreq (double): Vertical spatial frequency
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Sines(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("sines", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Extract an area from an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = input.Smartcrop(width, height, new Dictionary<string, object>
        /// {
        ///     {"interesting", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Width of extract area</param>
        /// <param name="height">Height of extract area</param>
        /// <param name="kwargs">
        /// interesting (string): How to measure interestingness
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Smartcrop(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return this.Call("smartcrop", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Spatial correlation
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Spcor(@ref);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Spectrum();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.SRGB2HSV();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.SRGB2scRGB();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Stats();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Stdif(width, height, new Dictionary<string, object>
        /// {
        ///     {"s0", double}
        ///     {"b", double}
        ///     {"m0", double}
        ///     {"a", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Window width in pixels</param>
        /// <param name="height">Window height in pixels</param>
        /// <param name="kwargs">
        /// s0 (double): New deviation
        /// b (double): Weight of new deviation
        /// m0 (double): New mean
        /// a (double): Weight of new mean
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Stdif(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return this.Call("stdif", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Subsample an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = input.Subsample(xfac, yfac, new Dictionary<string, object>
        /// {
        ///     {"point", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="xfac">Horizontal subsample factor</param>
        /// <param name="yfac">Vertical subsample factor</param>
        /// <param name="kwargs">
        /// point (bool): Point sample
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Subsample(int xfac, int yfac, IDictionary<string, object> kwargs = null)
        {
            return this.Call("subsample", kwargs, xfac, yfac) as Image;
        }

        /// <summary>
        /// Subtract two images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = left.Subtract(right);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Sum(@in);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Svgload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"dpi", double}
        ///     {"fail", bool}
        ///     {"scale", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// dpi (double): Render at this DPI
        /// fail (bool): Fail on first error
        /// scale (double): Scale output by this factor
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Svgload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("svgload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load SVG with rsvg
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.SvgloadBuffer(buffer, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"dpi", double}
        ///     {"fail", bool}
        ///     {"scale", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// dpi (double): Render at this DPI
        /// fail (bool): Fail on first error
        /// scale (double): Scale output by this factor
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image SvgloadBuffer(byte[] buffer, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("svgload_buffer", kwargs, buffer) as Image;
        }

        /// <summary>
        /// Run an external command
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var output = NetVips.Image.System(cmdFormat, new Dictionary<string, object>
        /// {
        ///     {"in", Image[]}
        ///     {"out_format", string}
        ///     {"in_format", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="cmdFormat">Command to run</param>
        /// <param name="kwargs">
        /// in (Image[]): Array of input images
        /// out_format (string): Format for output filename
        /// in_format (string): Format for input filename
        /// </param>
        /// <returns>None</returns>
        public static void System(string cmdFormat, IDictionary<string, object> kwargs = null)
        {
            Operation.Call("system", kwargs, cmdFormat);
        }

        /// <summary>
        /// Make a text image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var output = NetVips.Image.Text(text, new Dictionary<string, object>
        /// {
        ///     {"font", string}
        ///     {"width", int}
        ///     {"height", int}
        ///     {"align", string}
        ///     {"dpi", int}
        ///     {"spacing", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="text">Text to render</param>
        /// <param name="kwargs">
        /// font (string): Font to render with
        /// width (int): Maximum image width in pixels
        /// height (int): Maximum image height in pixels
        /// align (string): Align on the low, centre or high edge
        /// dpi (int): DPI to render at
        /// spacing (int): Line spacing
        /// </param>
        /// <returns>A new <see cref="Image"/> or a autofit_dpi</returns>
        public static object Text(string text, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("text", kwargs, text);
        }

        /// <summary>
        /// Generate thumbnail from file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Thumbnail(filename, width, new Dictionary<string, object>
        /// {
        ///     {"height", int}
        ///     {"size", string}
        ///     {"auto_rotate", bool}
        ///     {"crop", string}
        ///     {"linear", bool}
        ///     {"import_profile", string}
        ///     {"export_profile", string}
        ///     {"intent", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to read from</param>
        /// <param name="width">Size to this width</param>
        /// <param name="kwargs">
        /// height (int): Size to this height
        /// size (string): Only upsize, only downsize, or both
        /// auto_rotate (bool): Use orientation tags to rotate image upright
        /// crop (string): Reduce to fill target rectangle, then crop
        /// linear (bool): Reduce in linear light
        /// import_profile (string): Fallback import profile
        /// export_profile (string): Fallback export profile
        /// intent (string): Rendering intent
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Thumbnail(string filename, int width, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("thumbnail", kwargs, filename, width) as Image;
        }

        /// <summary>
        /// Generate thumbnail from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.ThumbnailBuffer(buffer, width, new Dictionary<string, object>
        /// {
        ///     {"height", int}
        ///     {"size", string}
        ///     {"auto_rotate", bool}
        ///     {"crop", string}
        ///     {"linear", bool}
        ///     {"import_profile", string}
        ///     {"export_profile", string}
        ///     {"intent", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="width">Size to this width</param>
        /// <param name="kwargs">
        /// height (int): Size to this height
        /// size (string): Only upsize, only downsize, or both
        /// auto_rotate (bool): Use orientation tags to rotate image upright
        /// crop (string): Reduce to fill target rectangle, then crop
        /// linear (bool): Reduce in linear light
        /// import_profile (string): Fallback import profile
        /// export_profile (string): Fallback export profile
        /// intent (string): Rendering intent
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image ThumbnailBuffer(byte[] buffer, int width, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("thumbnail_buffer", kwargs, buffer, width) as Image;
        }

        /// <summary>
        /// Generate thumbnail from image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.ThumbnailImage(width, new Dictionary<string, object>
        /// {
        ///     {"height", int}
        ///     {"size", string}
        ///     {"auto_rotate", bool}
        ///     {"crop", string}
        ///     {"linear", bool}
        ///     {"import_profile", string}
        ///     {"export_profile", string}
        ///     {"intent", string}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Size to this width</param>
        /// <param name="kwargs">
        /// height (int): Size to this height
        /// size (string): Only upsize, only downsize, or both
        /// auto_rotate (bool): Use orientation tags to rotate image upright
        /// crop (string): Reduce to fill target rectangle, then crop
        /// linear (bool): Reduce in linear light
        /// import_profile (string): Fallback import profile
        /// export_profile (string): Fallback export profile
        /// intent (string): Rendering intent
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ThumbnailImage(int width, IDictionary<string, object> kwargs = null)
        {
            return this.Call("thumbnail_image", kwargs, width) as Image;
        }

        /// <summary>
        /// Load tiff from file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Tiffload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"page", int}
        ///     {"n", int}
        ///     {"fail", bool}
        ///     {"autorotate", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// page (int): Load this page from the image
        /// n (int): Load this many pages
        /// fail (bool): Fail on first error
        /// autorotate (bool): Rotate image using orientation tag
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Tiffload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("tiffload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load tiff from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.TiffloadBuffer(buffer, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"page", int}
        ///     {"n", int}
        ///     {"fail", bool}
        ///     {"autorotate", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// page (int): Load this page from the image
        /// n (int): Load this many pages
        /// fail (bool): Fail on first error
        /// autorotate (bool): Rotate image using orientation tag
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image TiffloadBuffer(byte[] buffer, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("tiffload_buffer", kwargs, buffer) as Image;
        }

        /// <summary>
        /// Save image to tiff file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Tiffsave(filename, new Dictionary<string, object>
        /// {
        ///     {"compression", string}
        ///     {"Q", int}
        ///     {"predictor", string}
        ///     {"page_height", int}
        ///     {"profile", string}
        ///     {"tile", bool}
        ///     {"tile_width", int}
        ///     {"tile_height", int}
        ///     {"pyramid", bool}
        ///     {"miniswhite", bool}
        ///     {"squash", bool}
        ///     {"resunit", string}
        ///     {"xres", double}
        ///     {"yres", double}
        ///     {"bigtiff", bool}
        ///     {"properties", bool}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// compression (string): Compression for this file
        /// Q (int): Q factor
        /// predictor (string): Compression prediction
        /// page_height (int): Set page height for multipage save
        /// profile (string): ICC profile to embed
        /// tile (bool): Write a tiled tiff
        /// tile_width (int): Tile width in pixels
        /// tile_height (int): Tile height in pixels
        /// pyramid (bool): Write a pyramidal tiff
        /// miniswhite (bool): Use 0 for white in 1-bit images
        /// squash (bool): Squash images down to 1 bit
        /// resunit (string): Resolution unit
        /// xres (double): Horizontal resolution in pixels/mm
        /// yres (double): Vertical resolution in pixels/mm
        /// bigtiff (bool): Write a bigtiff image
        /// properties (bool): Write a properties document to IMAGEDESCRIPTION
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Tiffsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("tiffsave", kwargs, filename);
        }

        /// <summary>
        /// Save image to tiff buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// byte[] buffer = in.TiffsaveBuffer(new Dictionary<string, object>
        /// {
        ///     {"compression", string}
        ///     {"Q", int}
        ///     {"predictor", string}
        ///     {"page_height", int}
        ///     {"profile", string}
        ///     {"tile", bool}
        ///     {"tile_width", int}
        ///     {"tile_height", int}
        ///     {"pyramid", bool}
        ///     {"miniswhite", bool}
        ///     {"squash", bool}
        ///     {"resunit", string}
        ///     {"xres", double}
        ///     {"yres", double}
        ///     {"bigtiff", bool}
        ///     {"properties", bool}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// compression (string): Compression for this file
        /// Q (int): Q factor
        /// predictor (string): Compression prediction
        /// page_height (int): Set page height for multipage save
        /// profile (string): ICC profile to embed
        /// tile (bool): Write a tiled tiff
        /// tile_width (int): Tile width in pixels
        /// tile_height (int): Tile height in pixels
        /// pyramid (bool): Write a pyramidal tiff
        /// miniswhite (bool): Use 0 for white in 1-bit images
        /// squash (bool): Squash images down to 1 bit
        /// resunit (string): Resolution unit
        /// xres (double): Horizontal resolution in pixels/mm
        /// yres (double): Vertical resolution in pixels/mm
        /// bigtiff (bool): Write a bigtiff image
        /// properties (bool): Write a properties document to IMAGEDESCRIPTION
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>An array of bytes</returns>
        public byte[] TiffsaveBuffer(IDictionary<string, object> kwargs = null)
        {
            return this.Call("tiffsave_buffer", kwargs) as byte[];
        }

        /// <summary>
        /// Cache an image as a set of tiles
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Tilecache(new Dictionary<string, object>
        /// {
        ///     {"tile_width", int}
        ///     {"tile_height", int}
        ///     {"max_tiles", int}
        ///     {"access", string}
        ///     {"threaded", bool}
        ///     {"persistent", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// tile_width (int): Tile width in pixels
        /// tile_height (int): Tile height in pixels
        /// max_tiles (int): Maximum number of tiles to cache
        /// access (string): Expected access pattern
        /// threaded (bool): Allow threaded access
        /// persistent (bool): Keep cache between evaluations
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Tilecache(IDictionary<string, object> kwargs = null)
        {
            return this.Call("tilecache", kwargs) as Image;
        }

        /// <summary>
        /// Build a look-up table
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Tonelut(new Dictionary<string, object>
        /// {
        ///     {"in_max", int}
        ///     {"out_max", int}
        ///     {"Lb", double}
        ///     {"Lw", double}
        ///     {"Ps", double}
        ///     {"Pm", double}
        ///     {"Ph", double}
        ///     {"S", double}
        ///     {"M", double}
        ///     {"H", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// in_max (int): Size of LUT to build
        /// out_max (int): Maximum value in output LUT
        /// Lb (double): Lowest value in output
        /// Lw (double): Highest value in output
        /// Ps (double): Position of shadow
        /// Pm (double): Position of mid-tones
        /// Ph (double): Position of highlights
        /// S (double): Adjust shadows by this much
        /// M (double): Adjust mid-tones by this much
        /// H (double): Adjust highlights by this much
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Tonelut(IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("tonelut", kwargs) as Image;
        }

        /// <summary>
        /// Unpremultiply image alpha
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Unpremultiply(new Dictionary<string, object>
        /// {
        ///     {"max_alpha", double}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// max_alpha (double): Maximum value of alpha channel
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Unpremultiply(IDictionary<string, object> kwargs = null)
        {
            return this.Call("unpremultiply", kwargs) as Image;
        }

        /// <summary>
        /// Load vips from file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Vipsload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Vipsload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("vipsload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Save image to vips file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Vipssave(filename, new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Vipssave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("vipssave", kwargs, filename);
        }

        /// <summary>
        /// Load webp from file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Webpload(filename, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"shrink", int}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// shrink (int): Shrink factor on load
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Webpload(string filename, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("webpload", kwargs, filename) as Image;
        }

        /// <summary>
        /// Load webp from buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.WebploadBuffer(buffer, new Dictionary<string, object>
        /// {
        ///     {"memory", bool}
        ///     {"access", string}
        ///     {"shrink", int}
        ///     {"fail", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="buffer">Buffer to load from</param>
        /// <param name="kwargs">
        /// memory (bool): Force open via memory
        /// access (string): Required access pattern for this file
        /// shrink (int): Shrink factor on load
        /// fail (bool): Fail on first error
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image WebploadBuffer(byte[] buffer, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("webpload_buffer", kwargs, buffer) as Image;
        }

        /// <summary>
        /// Save image to webp file
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// in.Webpsave(filename, new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"Q", int}
        ///     {"lossless", bool}
        ///     {"preset", string}
        ///     {"smart_subsample", bool}
        ///     {"near_lossless", bool}
        ///     {"alpha_q", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="filename">Filename to save to</param>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// Q (int): Q factor
        /// lossless (bool): enable lossless compression
        /// preset (string): Preset for lossy compression
        /// smart_subsample (bool): Enable high quality chroma subsampling
        /// near_lossless (bool): Enable preprocessing in lossless mode (uses Q)
        /// alpha_q (int): Change alpha plane fidelity for lossy compression
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>None</returns>
        public void Webpsave(string filename, IDictionary<string, object> kwargs = null)
        {
            this.Call("webpsave", kwargs, filename);
        }

        /// <summary>
        /// Save image to webp buffer
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// byte[] buffer = in.WebpsaveBuffer(new Dictionary<string, object>
        /// {
        ///     {"page_height", int}
        ///     {"Q", int}
        ///     {"lossless", bool}
        ///     {"preset", string}
        ///     {"smart_subsample", bool}
        ///     {"near_lossless", bool}
        ///     {"alpha_q", int}
        ///     {"strip", bool}
        ///     {"background", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// page_height (int): Set page height for multipage save
        /// Q (int): Q factor
        /// lossless (bool): enable lossless compression
        /// preset (string): Preset for lossy compression
        /// smart_subsample (bool): Enable high quality chroma subsampling
        /// near_lossless (bool): Enable preprocessing in lossless mode (uses Q)
        /// alpha_q (int): Change alpha plane fidelity for lossy compression
        /// strip (bool): Strip all metadata from image
        /// background (double[]): Background value
        /// </param>
        /// <returns>An array of bytes</returns>
        public byte[] WebpsaveBuffer(IDictionary<string, object> kwargs = null)
        {
            return this.Call("webpsave_buffer", kwargs) as byte[];
        }

        /// <summary>
        /// Make a worley noise image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Worley(width, height, new Dictionary<string, object>
        /// {
        ///     {"cell_size", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// cell_size (int): Size of Worley cells
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Worley(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("worley", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Wrap image origin
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.Wrap(new Dictionary<string, object>
        /// {
        ///     {"x", int}
        ///     {"y", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// x (int): Left edge of input in output
        /// y (int): Top edge of input in output
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Wrap(IDictionary<string, object> kwargs = null)
        {
            return this.Call("wrap", kwargs) as Image;
        }

        /// <summary>
        /// Make an image where pixel values are coordinates
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Xyz(width, height, new Dictionary<string, object>
        /// {
        ///     {"csize", int}
        ///     {"dsize", int}
        ///     {"esize", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// csize (int): Size of third dimension
        /// dsize (int): Size of fourth dimension
        /// esize (int): Size of fifth dimension
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Xyz(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("xyz", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Transform XYZ to Lab
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.XYZ2Lab(new Dictionary<string, object>
        /// {
        ///     {"temp", double[]}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// temp (double[]): Colour temperature
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image XYZ2Lab(IDictionary<string, object> kwargs = null)
        {
            return this.Call("XYZ2Lab", kwargs) as Image;
        }

        /// <summary>
        /// Transform XYZ to scRGB
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = in.XYZ2scRGB();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.XYZ2Yxy();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.Yxy2XYZ();
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = NetVips.Image.Zone(width, height, new Dictionary<string, object>
        /// {
        ///     {"uchar", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="kwargs">
        /// uchar (bool): Output an unsigned char image
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Zone(int width, int height, IDictionary<string, object> kwargs = null)
        {
            return Operation.Call("zone", kwargs, width, height) as Image;
        }

        /// <summary>
        /// Zoom an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = input.Zoom(xfac, yfac);
        /// ]]>
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
        /// <![CDATA[
        /// Image @out = in.ScaleImage(new Dictionary<string, object>
        /// {
        ///     {"exp", double}
        ///     {"log", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="kwargs">
        /// exp (double): Exponent for log scale
        /// log (bool): Log scale
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image ScaleImage(IDictionary<string, object> kwargs = null)
        {
            return this.Call("scale", kwargs) as Image;
        }

        /// <summary>
        /// Ifthenelse an image
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = cond.Ifthenelse(in1, in2, new Dictionary<string, object>
        /// {
        ///     {"blend", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="in1">Source for TRUE pixels</param>
        /// <param name="in2">Source for FALSE pixels</param>
        /// <param name="kwargs">
        /// blend (bool): Blend smoothly between then and else parts
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Ifthenelse(object in1, object in2, IDictionary<string, object> kwargs = null)
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

            return this.Call("ifthenelse", kwargs, in1, in2) as Image;
        }

        /// <summary>
        /// Append a set of images or constants bandwise.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Bandjoin(@in);
        /// ]]>
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

            if (!(other is object[] values))
            {
                return null;
            }

            var allNumbers = values.Any(x => x.IsNumeric());

            // if [other] is all numbers, we can use BandjoinConst
            if (allNumbers)
            {
                return BandjoinConst(values.Select(x => (double) x).ToArray());
            }

            return Operation.Call("bandjoin", null, new object[] {values.PrependImage(this)}) as Image;
        }

        /// <summary>
        /// Band-wise rank of a set of images
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Bandrank(@in, new Dictionary<string, object>
        /// {
        ///     {"index", int}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="other">Array of input images</param>
        /// <param name="kwargs">
        /// index (int): Select this band element from sorted list
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Bandrank(object other, IDictionary<string, object> kwargs = null)
        {
            if (!(other is IEnumerable))
            {
                other = new[] {other};
            }

            if (!(other is object[] values))
            {
                return null;
            }

            return Operation.Call("bandrank", kwargs, new object[] {values.PrependImage(this)}) as Image;
        }

        /// <summary>
        /// Blend an array of images with an array of blend modes
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Image @out = NetVips.Image.Composite(@in, mode, new Dictionary<string, object>
        /// {
        ///     {"compositing_space", string}
        ///     {"premultiplied", bool}
        /// });
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="in">Array of input images</param>
        /// <param name="mode">Array of VipsBlendMode to join with</param>
        /// <param name="kwargs">
        /// compositing_space (string): Composite images in this colour space
        /// premultiplied (bool): Images have premultiplied alpha
        /// </param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image Composite(object @in, object mode, IDictionary<string, object> kwargs = null)
        {
            if (!(@in is IEnumerable))
            {
                @in = new[] {@in};
            }

            if (!(@in is object[] images))
            {
                return null;
            }

            if (!(mode is IEnumerable))
            {
                mode = new[] {mode};
            }

            if (!(mode is object[] modes))
            {
                return null;
            }

            // modes are VipsBlendMode enums, but we have to pass as array of int --
            // we need to map str->int by hand
            var blendModes = modes.Select(x => GValue.ToEnum(GValue.BlendModeType, x)).ToArray();
            return Operation.Call("composite", kwargs, images, blendModes) as Image;
        }

        /// <summary>
        /// Return the coordinates of the image maximum.
        /// </summary>
        /// <returns>An array of objects</returns>
        public object[] MaxPos()
        {
            var result = Max(new Dictionary<string, object>
            {
                {"x", 1},
                {"y", 1}
            }) as object[];
            var v = result?[0];
            var opts = result?[1] as Dictionary<object, object>;
            return new[] {v, opts?["x"], opts?["y"]};
        }

        /// <summary>
        /// Return the coordinates of the image maximum.
        /// </summary>
        /// <returns>An array of objects</returns>
        public object[] MinPos()
        {
            var result = Min(new Dictionary<string, object>
            {
                {"x", 1},
                {"y", 1}
            }) as object[];
            var v = result?[0];
            var opts = result?[1] as Dictionary<object, object>;
            return new[] {v, opts?["x"], opts?["y"]};
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
                    var image = this.Call("extract_band", new Dictionary<string, object>
                    {
                        {"n", nLeft}
                    }, 0) as Image;
                    componentsList.Add(image);
                }

                componentsList.Add(value);

                if (nRight > 0)
                {
                    var image = this.Call("extract_band", new Dictionary<string, object>
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
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Image) obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion
    }
}