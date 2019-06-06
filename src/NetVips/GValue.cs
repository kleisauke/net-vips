namespace NetVips
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using global::NetVips.Internal;

    /// <summary>
    /// Wrap <see cref="Internal.GValue"/> in a C# class.
    /// </summary>
    /// <remarks>
    /// This class wraps <see cref="Internal.GValue"/> in a convenient interface. You can use
    /// instances of this class to get and set <see cref="GObject"/> properties.
    ///
    /// On construction, <see cref="Internal.GValue"/> is all zero (empty). You can pass it to
    /// a get function to have it filled by <see cref="GObject"/>, or use init to
    /// set a type, set to set a value, then use it to set an object property.
    ///
    /// GValue lifetime is managed automatically.
    /// </remarks>
    public class GValue : IDisposable
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The specified struct to wrap around.
        /// </summary>
        internal Internal.GValue.Struct Struct;

        /// <summary>
        /// Track whether Dispose has been called.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Shift value used in converting numbers to type IDs.
        /// </summary>
        private const int FundamentalShift = 2;

        // look up some common gtypes at init for speed

        /// <summary>
        /// The fundamental type corresponding to gboolean.
        /// </summary>
        public static readonly IntPtr GBoolType = new IntPtr(5 << FundamentalShift);

        /// <summary>
        /// The fundamental type corresponding to gint.
        /// </summary>
        public static readonly IntPtr GIntType = new IntPtr(6 << FundamentalShift);

        /// <summary>
        /// The fundamental type corresponding to guint64.
        /// </summary>
        public static readonly IntPtr GUint64Type = new IntPtr(11 << FundamentalShift);

        /// <summary>
        /// The fundamental type from which all enumeration types are derived.
        /// </summary>
        public static readonly IntPtr GEnumType = new IntPtr(12 << FundamentalShift);

        /// <summary>
        /// The fundamental type from which all flags types are derived.
        /// </summary>
        public static readonly IntPtr GFlagsType = new IntPtr(13 << FundamentalShift);

        /// <summary>
        /// The fundamental type corresponding to gdouble.
        /// </summary>
        public static readonly IntPtr GDoubleType = new IntPtr(15 << FundamentalShift);

        /// <summary>
        /// The fundamental type corresponding to null-terminated C strings.
        /// </summary>
        public static readonly IntPtr GStrType = new IntPtr(16 << FundamentalShift);

        /// <summary>
        /// The fundamental type for GObject.
        /// </summary>
        public static readonly IntPtr GObjectType = new IntPtr(20 << FundamentalShift);

        /// <summary>
        /// The fundamental type for VipsImage.
        /// </summary>
        public static readonly IntPtr ImageType = NetVips.TypeFromName("VipsImage");

        /// <summary>
        /// The fundamental type for VipsArrayInt.
        /// </summary>
        public static readonly IntPtr ArrayIntType = NetVips.TypeFromName("VipsArrayInt");

        /// <summary>
        /// The fundamental type for VipsArrayDouble.
        /// </summary>
        public static readonly IntPtr ArrayDoubleType = NetVips.TypeFromName("VipsArrayDouble");

        /// <summary>
        /// The fundamental type for VipsArrayImage.
        /// </summary>
        public static readonly IntPtr ArrayImageType = NetVips.TypeFromName("VipsArrayImage");

        /// <summary>
        /// The fundamental type for VipsRefString.
        /// </summary>
        public static readonly IntPtr RefStrType = NetVips.TypeFromName("VipsRefString");

        /// <summary>
        /// The fundamental type for VipsBlob.
        /// </summary>
        public static readonly IntPtr BlobType = NetVips.TypeFromName("VipsBlob");

        /// <summary>
        /// The fundamental type for VipsBandFormat. See <see cref="Enums.BandFormat"/>.
        /// </summary>
        public static readonly IntPtr BandFormatType;

        /// <summary>
        /// The fundamental type for VipsBlendMode. See <see cref="Enums.BlendMode"/>.
        /// </summary>
        public static readonly IntPtr BlendModeType;

        /// <summary>
        /// Hint of how much native memory is actually occupied by the object.
        /// </summary>
        private long? _memoryPressure;

        static GValue()
        {
            Vips.BandFormatGetType();
            BandFormatType = NetVips.TypeFromName("VipsBandFormat");

            if (NetVips.AtLeastLibvips(8, 6))
            {
                Vips.BlendModeGetType();
                BlendModeType = NetVips.TypeFromName("VipsBlendMode");
            }
        }

        private static readonly Dictionary<IntPtr, string> GTypeToCSharpDict = new Dictionary<IntPtr, string>
        {
            {GBoolType, "bool"},
            {GIntType, "int"},
            {GUint64Type, "ulong"},
            {GEnumType, "string"},
            {GFlagsType, "int"},
            {GDoubleType, "double"},
            {GStrType, "string"},
            {GObjectType, "GObject"},
            {ImageType, "Image"},
            {ArrayIntType, "int[]"},
            {ArrayDoubleType, "double[]"},
            {ArrayImageType, "Image[]"},
            {RefStrType, "string"},
            {BlobType, "byte[]"}
        };

        /// <summary>
        /// Map a GType to the name of the C# type we use to represent it.
        /// </summary>
        /// <param name="gtype">The GType to map.</param>
        /// <returns>The C# type we use to represent it.</returns>
        public static string GTypeToCSharp(IntPtr gtype)
        {
            var fundamental = GType.Fundamental(gtype);

            if (GTypeToCSharpDict.ContainsKey(gtype))
            {
                return GTypeToCSharpDict[gtype];
            }

            if (GTypeToCSharpDict.ContainsKey(fundamental))
            {
                return GTypeToCSharpDict[fundamental];
            }

            return "object";
        }

        /// <summary>
        /// Turn a string or integer into an enum value ready to be passed into libvips.
        /// </summary>
        /// <param name="gtype">The GType.</param>
        /// <param name="value">The string or integer to convert.</param>
        /// <returns>An enum value ready to be passed into libvips.</returns>
        public static int ToEnum(IntPtr gtype, object value)
        {
            return value is string strValue
                ? Vips.EnumFromNick("NetVips", gtype, strValue)
                : Convert.ToInt32(value);
        }

        /// <summary>
        /// Turn an int back into an enum string.
        /// </summary>
        /// <param name="gtype">The GType.</param>
        /// <param name="enumValue">The integer to convert.</param>
        /// <returns>An enum value as string.</returns>
        public static string FromEnum(IntPtr gtype, int enumValue)
        {
            var cstr = Marshal.PtrToStringAnsi(Vips.EnumNick(gtype, enumValue));
            if (cstr == null)
            {
                throw new VipsException("value not in enum");
            }

            return cstr;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GValue"/> class.
        /// </summary>
        public GValue()
        {
            Struct = new Internal.GValue.Struct();
            // logger.Debug($"GValue = {Struct}");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GValue"/> class
        /// with the specified struct to wrap around.
        /// </summary>
        /// <param name="value">The specified struct to wrap around.</param>
        internal GValue(Internal.GValue.Struct value)
        {
            Struct = value;
            // logger.Debug($"GValue = {Struct}");
        }

        /// <summary>
        /// Set the type of a GValue.
        /// </summary>
        /// <remarks>
        /// GValues have a set type, fixed at creation time. Use SetType to set
        /// the type of a GValue before assigning to it.
        ///
        /// GTypes are 32 or 64-bit integers (depending on the platform). See
        /// TypeFind.
        /// </remarks>
        /// <param name="gtype">Type the GValue should hold values of.</param>
        public void SetType(IntPtr gtype)
        {
            Internal.GValue.Init(ref Struct, gtype);
        }

        /// <summary>
        /// Set a GValue.
        /// </summary>
        /// <remarks>
        /// The value is converted to the type of the GValue, if possible, and
        /// assigned.
        /// </remarks>
        /// <param name="value">Value to be set.</param>
        public void Set(object value)
        {
            // logger.Debug($"Set: value = {value}");
            var gtype = GetTypeOf();
            var fundamental = GType.Fundamental(gtype);
            if (gtype == GBoolType)
            {
                Internal.GValue.SetBoolean(ref Struct, Convert.ToBoolean(value) ? 1 : 0);
            }
            else if (gtype == GIntType)
            {
                Internal.GValue.SetInt(ref Struct, Convert.ToInt32(value));
            }
            else if (gtype == GUint64Type)
            {
                Internal.GValue.SetUint64(ref Struct, Convert.ToUInt64(value));
            }
            else if (gtype == GDoubleType)
            {
                Internal.GValue.SetDouble(ref Struct, Convert.ToDouble(value));
            }
            else if (fundamental == GEnumType)
            {
                Internal.GValue.SetEnum(ref Struct, ToEnum(gtype, value));
            }
            else if (fundamental == GFlagsType)
            {
                Internal.GValue.SetFlags(ref Struct, Convert.ToUInt32(value));
            }
            else if (gtype == GStrType)
            {
                ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(Convert.ToString(value));
                Internal.GValue.SetString(ref Struct, MemoryMarshal.GetReference(span));
            }
            else if (gtype == RefStrType)
            {
                ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(Convert.ToString(value));
                VipsValue.SetRefString(ref Struct, MemoryMarshal.GetReference(span));
            }
            else if (fundamental == GObjectType)
            {
                if (!(value is GObject gObject))
                {
                    throw new Exception(
                        $"unsupported value type {value.GetType()} for gtype {NetVips.TypeName(gtype)}");
                }

                Internal.GValue.SetObject(ref Struct, gObject);
            }
            else if (gtype == ArrayIntType)
            {
                if (!(value is IEnumerable))
                {
                    value = new[] { value };
                }

                int[] integers;
                switch (value)
                {
                    case int[] ints:
                        integers = ints;
                        break;
                    case double[] doubles:
                        integers = Array.ConvertAll(doubles, Convert.ToInt32);
                        break;
                    case object[] objects:
                        integers = Array.ConvertAll(objects, Convert.ToInt32);
                        break;
                    default:
                        throw new Exception(
                            $"unsupported value type {value.GetType()} for gtype {NetVips.TypeName(gtype)}");
                }

                VipsValue.SetArrayInt(ref Struct, integers, integers.Length);
            }
            else if (gtype == ArrayDoubleType)
            {
                if (!(value is IEnumerable))
                {
                    value = new[] { value };
                }

                double[] doubles;
                switch (value)
                {
                    case double[] dbls:
                        doubles = dbls;
                        break;
                    case int[] ints:
                        doubles = Array.ConvertAll(ints, Convert.ToDouble);
                        break;
                    case object[] objects:
                        doubles = Array.ConvertAll(objects, Convert.ToDouble);
                        break;
                    default:
                        throw new Exception(
                            $"unsupported value type {value.GetType()} for gtype {NetVips.TypeName(gtype)}");
                }

                VipsValue.SetArrayDouble(ref Struct, doubles, doubles.Length);
            }
            else if (gtype == ArrayImageType)
            {
                if (!(value is Image[] images))
                {
                    throw new Exception(
                        $"unsupported value type {value.GetType()} for gtype {NetVips.TypeName(gtype)}");
                }

                var size = images.Length;
                VipsValue.SetArrayImage(ref Struct, size);

                var ptrArr = VipsValue.GetArrayImage(in Struct, out _);

                for (var i = 0; i < size; i++)
                {
                    Marshal.WriteIntPtr(ptrArr, i * IntPtr.Size, images[i].DangerousGetHandle());

                    // the gvalue needs a ref on each of the images
                    images[i].ObjectRef();
                }
            }
            else if (gtype == BlobType)
            {
                byte[] memory;
                switch (value)
                {
                    case string strValue:
                        memory = Encoding.UTF8.GetBytes(strValue);
                        break;
                    case char[] charArrValue:
                        memory = Encoding.UTF8.GetBytes(charArrValue);
                        break;
                    case byte[] byteArrValue:
                        memory = byteArrValue;
                        break;
                    default:
                        throw new Exception(
                            $"unsupported value type {value.GetType()} for gtype {NetVips.TypeName(gtype)}");
                }

                // We need to set the blob to a copy of the string that vips
                // can own
                var ptr = GLib.GMalloc((ulong)memory.Length);
                Marshal.Copy(memory, 0, ptr, memory.Length);

                // Make sure that the GC knows the true cost of the object during collection.
                // If the object is actually bigger than the managed size reflects, it may be a candidate for quick(er) collection.
                GC.AddMemoryPressure(memory.Length);
                _memoryPressure = memory.Length;

                if (NetVips.AtLeastLibvips(8, 6))
                {
                    VipsValue.SetBlobFree(ref Struct, ptr, (ulong)memory.Length);
                }
                else
                {
                    int FreeFn(IntPtr a, IntPtr b)
                    {
                        GLib.GFree(a);

                        return 0;
                    }

                    VipsValue.SetBlob(ref Struct, FreeFn, ptr, (ulong)memory.Length);
                }
            }
            else
            {
                throw new Exception(
                    $"unsupported gtype for set {NetVips.TypeName(gtype)}, fundamental {NetVips.TypeName(fundamental)}, value type {value.GetType()}");
            }
        }

        /// <summary>
        /// Get the contents of a GValue.
        /// </summary>
        /// <remarks>
        /// The contents of the GValue are read out as a C# type.
        /// </remarks>
        /// <returns>The contents of this GValue.</returns>
        public object Get()
        {
            // logger.Debug($"Get: this = {this}");
            var gtype = GetTypeOf();
            var fundamental = GType.Fundamental(gtype);

            object result;
            if (gtype == GBoolType)
            {
                result = Internal.GValue.GetBoolean(in Struct) != 0;
            }
            else if (gtype == GIntType)
            {
                result = Internal.GValue.GetInt(in Struct);
            }
            else if (gtype == GUint64Type)
            {
                result = Internal.GValue.GetUint64(in Struct);
            }
            else if (gtype == GDoubleType)
            {
                result = Internal.GValue.GetDouble(in Struct);
            }
            else if (fundamental == GEnumType)
            {
                result = FromEnum(gtype, Internal.GValue.GetEnum(in Struct));
            }
            else if (fundamental == GFlagsType)
            {
                result = Internal.GValue.GetFlags(in Struct);
            }
            else if (gtype == GStrType)
            {
                result = Internal.GValue.GetString(in Struct).ToUtf8String();
            }
            else if (gtype == RefStrType)
            {
                result = VipsValue.GetRefString(in Struct, out var psize).ToUtf8String(size: (int)psize);
            }
            else if (gtype == ImageType)
            {
                // GValueGetObject() will not add a ref ... that is
                // held by the gvalue
                var vi = Internal.GValue.GetObject(in Struct);

                // we want a ref that will last with the life of the vimage:
                // this ref is matched by the unref that's attached to finalize
                // by GObject
                var image = new Image(vi);
                image.ObjectRef();

                result = image;
            }
            else if (gtype == ArrayIntType)
            {
                var intPtr = VipsValue.GetArrayInt(in Struct, out var psize);

                var intArr = new int[psize];
                Marshal.Copy(intPtr, intArr, 0, psize);
                result = intArr;
            }
            else if (gtype == ArrayDoubleType)
            {
                var intPtr = VipsValue.GetArrayDouble(in Struct, out var psize);

                var doubleArr = new double[psize];
                Marshal.Copy(intPtr, doubleArr, 0, psize);
                result = doubleArr;
            }
            else if (gtype == ArrayImageType)
            {
                var ptrArr = VipsValue.GetArrayImage(in Struct, out var psize);

                var images = new Image[psize];
                for (var i = 0; i < psize; i++)
                {
                    var vi = Marshal.ReadIntPtr(ptrArr, i * IntPtr.Size);
                    images[i] = new Image(vi);
                    images[i].ObjectRef();
                }

                result = images;
            }
            else if (gtype == BlobType)
            {
                var array = VipsValue.GetBlob(in Struct, out var psize);

                // Blob types are returned as an array of bytes.
                var byteArr = new byte[psize];
                Marshal.Copy(array, byteArr, 0, (int)psize);
                result = byteArr;
            }
            else
            {
                throw new Exception($"unsupported gtype for get {NetVips.TypeName(gtype)}");
            }

            return result;
        }

        /// <summary>
        /// Get the GType of this GValue.
        /// </summary>
        /// <returns>The GType of this GValue.</returns>
        public IntPtr GetTypeOf()
        {
            return Struct.GType;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GValue"/> class.
        /// </summary>
        /// <remarks>
        /// Allows an object to try to free resources and perform other cleanup
        /// operations before it is reclaimed by garbage collection.
        /// </remarks>
        ~GValue()
        {
            // Do not re-create Dispose clean-up code here.
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            // logger.Debug($"GC: GValue = {Struct}");

            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // and tag it to be unset on GC as well
                Internal.GValue.Unset(ref Struct);

                if (_memoryPressure.HasValue)
                {
                    GC.RemoveMemoryPressure(_memoryPressure.Value);
                }

                // Note disposing has been done.
                _disposed = true;
            }

            // logger.Debug($"GC: GValue = {Struct}");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }
    }
}