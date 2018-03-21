using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NetVips.Internal;

namespace NetVips
{
    /// <summary>
    /// Wrap GValue in a C# class.
    /// </summary>
    /// <remarks>
    /// This class wraps <see cref="NetVips.Internal.GValue"/> in a convenient interface. You can use
    /// instances of this class to get and set <see cref="GObject"/> properties.
    /// 
    /// On construction, <see cref="NetVips.Internal.GValue"/> is all zero (empty). You can pass it to
    /// a get function to have it filled by <see cref="GObject"/>, or use init to
    /// set a type, set to set a value, then use it to set an object property.
    /// 
    /// GValue lifetime is managed automatically.
    /// </remarks>
    public class GValue : IDisposable
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        internal readonly Internal.GValue IntlGValue;

        // Track whether Dispose has been called.
        private bool _disposed;

        // look up some common gtypes at init for speed
        public static readonly ulong GBoolType = Base.TypeFromName("gboolean");
        public static readonly ulong GIntType = Base.TypeFromName("gint");
        public static readonly ulong GDoubleType = Base.TypeFromName("gdouble");
        public static readonly ulong GStrType = Base.TypeFromName("gchararray");
        public static readonly ulong GEnumType = Base.TypeFromName("GEnum");
        public static readonly ulong GFlagsType = Base.TypeFromName("GFlags");
        public static readonly ulong GObjectType = Base.TypeFromName("GObject");
        public static readonly ulong ImageType = Base.TypeFromName("VipsImage");
        public static readonly ulong ArrayIntType = Base.TypeFromName("VipsArrayInt");
        public static readonly ulong ArrayDoubleType = Base.TypeFromName("VipsArrayDouble");
        public static readonly ulong ArrayImageType = Base.TypeFromName("VipsArrayImage");
        public static readonly ulong RefStrType = Base.TypeFromName("VipsRefString");
        public static readonly ulong BlobType = Base.TypeFromName("VipsBlob");
        public static readonly ulong BandFormatType;
        public static readonly ulong BlendModeType;

        static GValue()
        {
            Vips.VipsBandFormatGetType();
            BandFormatType = Base.TypeFromName("VipsBandFormat");

            if (Base.AtLeastLibvips(8, 6))
            {
                Vips.VipsBlendModeGetType();
                BlendModeType = Base.TypeFromName("VipsBlendMode");
            }
        }

        private static readonly Dictionary<ulong, string> GTypeToCSharpDict = new Dictionary<ulong, string>
        {
            {GBoolType, "bool"},
            {GIntType, "int"},
            {GDoubleType, "double"},
            {GStrType, "string"},
            {RefStrType, "string"},
            {GEnumType, "string"},
            {GFlagsType, "int"},
            {GObjectType, "GObject"},
            {ImageType, "Image"},
            {ArrayIntType, "int[]"},
            {ArrayDoubleType, "double[]"},
            {ArrayImageType, "Image[]"},
            {BlobType, "byte[]"}
        };

        /// <summary>
        /// Map a gtype to the name of the C# type we use to represent it.
        /// </summary>
        /// <param name="gtype"></param>
        /// <returns></returns>
        public static string GTypeToCSharp(ulong gtype)
        {
            var fundamental = GType.GTypeFundamental(gtype);

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
        /// Turn a string into an enum value ready to be passed into libvips.       
        /// </summary>
        /// <param name="gtype"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToEnum(ulong gtype, object value)
        {
            return value is string strValue
                ? Vips.VipsEnumFromNick("NetVips", gtype, strValue)
                : Convert.ToInt32(value);
        }

        /// <summary>
        /// Turn an int back into an enum string.
        /// </summary>
        /// <param name="gtype"></param>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string FromEnum(ulong gtype, int enumValue)
        {
            var cstr = Marshal.PtrToStringAnsi(Vips.VipsEnumNick(gtype, enumValue));
            if (cstr == null)
            {
                throw new Exception("value not in enum");
            }

            return cstr;
        }

        public GValue()
        {
            // allocate memory for the gvalue which will be freed on GC
            IntlGValue = new Internal.GValue();
            // logger.Debug($"GValue = {IntlGValue}");
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
        /// <param name="gtype"></param>
        /// <returns></returns>
        public void SetType(ulong gtype)
        {
            Internal.GValue.GValueInit(IntlGValue, gtype);
        }

        /// <summary>
        /// Set a GValue.
        /// </summary>
        /// <remarks>
        /// The value is converted to the type of the GValue, if possible, and
        /// assigned.
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Set(object value)
        {
            // logger.Debug($"Set: value = {value}");
            var gtype = IntlGValue.GType;
            var fundamental = GType.GTypeFundamental(gtype);
            if (gtype == GBoolType)
            {
                Internal.GValue.GValueSetBoolean(IntlGValue, Convert.ToBoolean(value) ? 1 : 0);
            }
            else if (gtype == GIntType)
            {
                Internal.GValue.GValueSetInt(IntlGValue, Convert.ToInt32(value));
            }
            else if (gtype == GDoubleType)
            {
                Internal.GValue.GValueSetDouble(IntlGValue, Convert.ToDouble(value));
            }
            else if (fundamental == GEnumType)
            {
                Internal.GValue.GValueSetEnum(IntlGValue, ToEnum(gtype, value));
            }
            else if (fundamental == GFlagsType)
            {
                Internal.GValue.GValueSetFlags(IntlGValue, Convert.ToUInt32(value));
            }
            else if (gtype == GStrType)
            {
                Internal.GValue.GValueSetString(IntlGValue, Convert.ToString(value));
            }
            else if (gtype == RefStrType)
            {
                VipsType.VipsValueSetRefString(IntlGValue, Convert.ToString(value));
            }
            else if (fundamental == GObjectType)
            {
                switch (value)
                {
                    case Image image:
                        Internal.GObject.GValueSetObject(IntlGValue, image.IntlImage.Pointer);
                        break;
                    case Interpolate interpolate:
                        Internal.GObject.GValueSetObject(IntlGValue, interpolate.IntlVipsInterpolate.Pointer);
                        break;
                    default:
                        throw new Exception(
                            $"unsupported value type {value.GetType()} for gtype {Base.TypeName(gtype)}"
                        );
                }
            }
            else if (gtype == ArrayIntType)
            {
                if (!(value is IEnumerable))
                {
                    value = new[] {value};
                }

                switch (value)
                {
                    case int[] ints:
                        VipsType.VipsValueSetArrayInt(IntlGValue, ints, ints.Length);
                        break;
                    case double[] doubles:
                        VipsType.VipsValueSetArrayInt(IntlGValue, Array.ConvertAll(doubles, Convert.ToInt32),
                            doubles.Length);
                        break;
                    case object[] objects:
                        VipsType.VipsValueSetArrayInt(IntlGValue, Array.ConvertAll(objects, Convert.ToInt32),
                            objects.Length);
                        break;
                    default:
                        throw new Exception(
                            $"unsupported value type {value.GetType()} for gtype {Base.TypeName(gtype)}"
                        );
                }
            }
            else if (gtype == ArrayDoubleType)
            {
                if (!(value is IEnumerable))
                {
                    value = new[] {value};
                }

                switch (value)
                {
                    case double[] doubles:
                        VipsType.VipsValueSetArrayDouble(IntlGValue, doubles, doubles.Length);
                        break;
                    case int[] ints:
                        VipsType.VipsValueSetArrayDouble(IntlGValue, Array.ConvertAll(ints, Convert.ToDouble),
                            ints.Length);
                        break;
                    case object[] objects:
                        VipsType.VipsValueSetArrayDouble(IntlGValue, Array.ConvertAll(objects, Convert.ToDouble),
                            objects.Length);
                        break;
                    default:
                        throw new Exception(
                            $"unsupported value type {value.GetType()} for gtype {Base.TypeName(gtype)}"
                        );
                }
            }
            else if (gtype == ArrayImageType)
            {
                if (!(value is Image[] images))
                {
                    throw new Exception(
                        $"unsupported value type {value.GetType()} for gtype {Base.TypeName(gtype)}"
                    );
                }

                var size = images.Length;
                VipsImage.VipsValueSetArrayImage(IntlGValue, size);

                var psize = 0;

                var ptrArr = VipsImage.VipsValueGetArrayImage(IntlGValue, ref psize);

                for (var i = 0; i < size; i++)
                {
                    var pointer = images[i].IntlGObject.Pointer;
                    Internal.GObject.GObjectRef(pointer);
                    Marshal.WriteIntPtr(ptrArr, i * IntPtr.Size, pointer);
                }
            }
            else if (gtype == BlobType)
            {
                int length;
                IntPtr memory;
                switch (value)
                {
                    case string strValue:
                        length = Encoding.UTF8.GetByteCount(strValue);

                        // We need to set the blob to a copy of the string that vips
                        // can own
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
                            $"unsupported value type {value.GetType()} for gtype {Base.TypeName(gtype)}"
                        );
                }

                if (Base.AtLeastLibvips(8, 6))
                {
                    VipsType.VipsValueSetBlobFree(IntlGValue, memory, (ulong) length);
                }
                else
                {
                    int FreeFn(IntPtr a, IntPtr b)
                    {
                        GLib.GFree(a);

                        return 0;
                    }

                    VipsType.VipsValueSetBlob(IntlGValue, FreeFn, memory, (ulong) length);
                }
            }
            else
            {
                throw new Exception(
                    $"unsupported gtype for set {Base.TypeName(gtype)}, fundamental {Base.TypeName(fundamental)}, value type {value.GetType()}"
                );
            }
        }

        /// <summary>
        /// Get the contents of a GValue.
        /// </summary>
        /// <remarks>
        /// The contents of the GValue are read out as a C# type.
        /// </remarks>
        /// <returns></returns>
        public object Get()
        {
            // logger.Debug($"Get: this = {this}");
            var gtype = IntlGValue.GType;
            var fundamental = GType.GTypeFundamental(gtype);

            object result;
            if (gtype == GBoolType)
            {
                result = Internal.GValue.GValueGetBoolean(IntlGValue) != 0;
            }
            else if (gtype == GIntType)
            {
                result = Internal.GValue.GValueGetInt(IntlGValue);
            }
            else if (gtype == GDoubleType)
            {
                result = Internal.GValue.GValueGetDouble(IntlGValue);
            }
            else if (fundamental == GEnumType)
            {
                result = FromEnum(gtype, Internal.GValue.GValueGetEnum(IntlGValue));
            }
            else if (fundamental == GFlagsType)
            {
                result = Internal.GValue.GValueGetFlags(IntlGValue);
            }
            else if (gtype == GStrType)
            {
                result = Internal.GValue.GValueGetString(IntlGValue);
            }
            else if (gtype == RefStrType)
            {
                ulong psize = 0;
                result = VipsType.VipsValueGetRefString(IntlGValue, ref psize);
            }
            else if (gtype == ImageType)
            {
                // GValueGetObject() will not add a ref ... that is
                // held by the gvalue
                var go = Internal.GObject.GValueGetObject(IntlGValue);

                // we want a ref that will last with the life of the vimage:
                // this ref is matched by the unref that's attached to finalize
                // by GObject
                Internal.GObject.GObjectRef(go);

                var vi = new VipsImage(go);

                result = new Image(vi);
            }
            else if (gtype == ArrayIntType)
            {
                var psize = 0;
                var intPtr = VipsType.VipsValueGetArrayInt(IntlGValue, ref psize);

                var intArr = new int[psize];
                Marshal.Copy(intPtr, intArr, 0, psize);
                result = intArr;
            }
            else if (gtype == ArrayDoubleType)
            {
                var psize = 0;
                var intPtr = VipsType.VipsValueGetArrayDouble(IntlGValue, ref psize);

                var doubleArr = new double[psize];
                Marshal.Copy(intPtr, doubleArr, 0, psize);
                result = doubleArr;
            }
            else if (gtype == ArrayImageType)
            {
                var psize = 0;
                var ptr = VipsImage.VipsValueGetArrayImage(IntlGValue, ref psize);

                var images = new Image[psize];
                for (var i = 0; i < psize; i++)
                {
                    var vi = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
                    Internal.GObject.GObjectRef(vi);
                    images[i] = new Image(new VipsImage(vi));
                }

                result = images;
            }
            else if (gtype == BlobType)
            {
                ulong psize = 0;
                var array = VipsType.VipsValueGetBlob(IntlGValue, ref psize);

                // Blob types are returned as an array of bytes.
                result = array.ToByteString((int) psize);
            }
            else
            {
                throw new Exception($"unsupported gtype for get {Base.TypeName(gtype)}");
            }

            return result;
        }

        ~GValue()
        {
            // Do not re-create Dispose clean-up code here.
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged resources
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            // logger.Debug($"GC: GValue = {IntlGValue}");
            IntlGValue.Dispose();
            // logger.Debug($"GC: GValue = {IntlGValue}");
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // Dispose unmanaged resources.
                ReleaseUnmanagedResources();

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }
    }
}