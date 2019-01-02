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

        internal Internal.GValue.Struct Struct;

        // Track whether Dispose has been called.		
        private bool _disposed;

        // look up some common gtypes at init for speed

        /// <summary>
        /// The GType for gboolean.
        /// </summary>
        public static readonly IntPtr GBoolType = Base.TypeFromName("gboolean");

        /// <summary>
        /// The GType for gint.
        /// </summary>
        public static readonly IntPtr GIntType = Base.TypeFromName("gint");

        /// <summary>
        /// The GType for gdouble.
        /// </summary>
        public static readonly IntPtr GDoubleType = Base.TypeFromName("gdouble");

        /// <summary>
        /// The GType for gchararray.
        /// </summary>
        public static readonly IntPtr GStrType = Base.TypeFromName("gchararray");

        /// <summary>
        /// The GType for GEnum.
        /// </summary>
        public static readonly IntPtr GEnumType = Base.TypeFromName("GEnum");

        /// <summary>
        /// The GType for GFlags.
        /// </summary>
        public static readonly IntPtr GFlagsType = Base.TypeFromName("GFlags");

        /// <summary>
        /// The GType for GObject.
        /// </summary>
        public static readonly IntPtr GObjectType = Base.TypeFromName("GObject");

        /// <summary>
        /// The GType for VipsImage.
        /// </summary>
        public static readonly IntPtr ImageType = Base.TypeFromName("VipsImage");

        /// <summary>
        /// The GType for VipsArrayInt.
        /// </summary>
        public static readonly IntPtr ArrayIntType = Base.TypeFromName("VipsArrayInt");

        /// <summary>
        /// The GType for VipsArrayDouble.
        /// </summary>
        public static readonly IntPtr ArrayDoubleType = Base.TypeFromName("VipsArrayDouble");

        /// <summary>
        /// The GType for VipsArrayImage.
        /// </summary>
        public static readonly IntPtr ArrayImageType = Base.TypeFromName("VipsArrayImage");

        /// <summary>
        /// The GType for VipsRefString.
        /// </summary>
        public static readonly IntPtr RefStrType = Base.TypeFromName("VipsRefString");

        /// <summary>
        /// The GType for VipsBlob.
        /// </summary>
        public static readonly IntPtr BlobType = Base.TypeFromName("VipsBlob");

        /// <summary>
        /// The GType for VipsBandFormat. See <see cref="Enums.BandFormat"/>.
        /// </summary>
        public static readonly IntPtr BandFormatType;

        /// <summary>
        /// The GType for VipsBlendMode. See <see cref="Enums.BlendMode"/>.
        /// </summary>
        public static readonly IntPtr BlendModeType;

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

        private static readonly Dictionary<IntPtr, string> GTypeToCSharpDict = new Dictionary<IntPtr, string>
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
        public static string GTypeToCSharp(IntPtr gtype)
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
        public static int ToEnum(IntPtr gtype, object value)
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
        public static string FromEnum(IntPtr gtype, int enumValue)
        {
            var cstr = Marshal.PtrToStringAnsi(Vips.VipsEnumNick(gtype, enumValue));
            if (cstr == null)
            {
                throw new VipsException("value not in enum");
            }

            return cstr;
        }

        /// <summary>
        /// Wrap GValue in a C# class.
        /// </summary>
        public GValue()
        {
            Struct = new Internal.GValue.Struct();
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
        /// <param name="gtype"></param>
        public void SetType(IntPtr gtype)
        {
            Internal.GValue.GValueInit(ref Struct, gtype);
        }

        /// <summary>
        /// Set a GValue.
        /// </summary>
        /// <remarks>
        /// The value is converted to the type of the GValue, if possible, and
        /// assigned.
        /// </remarks>
        /// <param name="value"></param>
        public void Set(object value)
        {
            // logger.Debug($"Set: value = {value}");
            var gtype = GetTypeOf();
            var fundamental = GType.GTypeFundamental(gtype);
            if (gtype == GBoolType)
            {
                Internal.GValue.GValueSetBoolean(ref Struct, Convert.ToBoolean(value) ? 1 : 0);
            }
            else if (gtype == GIntType)
            {
                Internal.GValue.GValueSetInt(ref Struct, Convert.ToInt32(value));
            }
            else if (gtype == GDoubleType)
            {
                Internal.GValue.GValueSetDouble(ref Struct, Convert.ToDouble(value));
            }
            else if (fundamental == GEnumType)
            {
                Internal.GValue.GValueSetEnum(ref Struct, ToEnum(gtype, value));
            }
            else if (fundamental == GFlagsType)
            {
                Internal.GValue.GValueSetFlags(ref Struct, Convert.ToUInt32(value));
            }
            else if (gtype == GStrType)
            {
                var pointer = Convert.ToString(value).ToUtf8Ptr();
                Internal.GValue.GValueSetString(ref Struct, pointer);
                GLib.GFree(pointer);
            }
            else if (gtype == RefStrType)
            {
                var pointer = Convert.ToString(value).ToUtf8Ptr();
                VipsType.VipsValueSetRefString(ref Struct, pointer);
                GLib.GFree(pointer);
            }
            else if (fundamental == GObjectType)
            {
                if (!(value is GObject gObject))
                {
                    throw new Exception(
                        $"unsupported value type {value.GetType()} for gtype {Base.TypeName(gtype)}"
                    );
                }

                Internal.GObject.GValueSetObject(ref Struct, gObject);
            }
            else if (gtype == ArrayIntType)
            {
                if (!(value is IEnumerable))
                {
                    value = new[] { value };
                }

                switch (value)
                {
                    case int[] ints:
                        VipsType.VipsValueSetArrayInt(ref Struct, ints, ints.Length);
                        break;
                    case double[] doubles:
                        VipsType.VipsValueSetArrayInt(ref Struct, Array.ConvertAll(doubles, Convert.ToInt32),
                            doubles.Length);
                        break;
                    case object[] objects:
                        VipsType.VipsValueSetArrayInt(ref Struct, Array.ConvertAll(objects, Convert.ToInt32),
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
                    value = new[] { value };
                }

                switch (value)
                {
                    case double[] doubles:
                        VipsType.VipsValueSetArrayDouble(ref Struct, doubles, doubles.Length);
                        break;
                    case int[] ints:
                        VipsType.VipsValueSetArrayDouble(ref Struct, Array.ConvertAll(ints, Convert.ToDouble),
                            ints.Length);
                        break;
                    case object[] objects:
                        VipsType.VipsValueSetArrayDouble(ref Struct, Array.ConvertAll(objects, Convert.ToDouble),
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
                VipsImage.VipsValueSetArrayImage(ref Struct, size);

                var ptrArr = VipsImage.VipsValueGetArrayImage(ref Struct, IntPtr.Zero);

                for (var i = 0; i < size; i++)
                {
                    Marshal.WriteIntPtr(ptrArr, i * IntPtr.Size, images[i].DangerousGetHandle());

                    // the gvalue needs a ref on each of the images
                    images[i].ObjectRef();
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
                    VipsType.VipsValueSetBlobFree(ref Struct, memory, new UIntPtr((ulong)length));
                }
                else
                {
                    int FreeFn(IntPtr a, IntPtr b)
                    {
                        GLib.GFree(a);

                        return 0;
                    }

                    VipsType.VipsValueSetBlob(ref Struct, FreeFn, memory, new UIntPtr((ulong)length));
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
            var gtype = GetTypeOf();
            var fundamental = GType.GTypeFundamental(gtype);

            object result;
            if (gtype == GBoolType)
            {
                result = Internal.GValue.GValueGetBoolean(ref Struct) != 0;
            }
            else if (gtype == GIntType)
            {
                result = Internal.GValue.GValueGetInt(ref Struct);
            }
            else if (gtype == GDoubleType)
            {
                result = Internal.GValue.GValueGetDouble(ref Struct);
            }
            else if (fundamental == GEnumType)
            {
                result = FromEnum(gtype, Internal.GValue.GValueGetEnum(ref Struct));
            }
            else if (fundamental == GFlagsType)
            {
                result = Internal.GValue.GValueGetFlags(ref Struct);
            }
            else if (gtype == GStrType)
            {
                result = Internal.GValue.GValueGetString(ref Struct).ToUtf8String();
            }
            else if (gtype == RefStrType)
            {
                // don't bother getting the size -- assume these are always
                // null-terminated C strings
                result = VipsType.VipsValueGetRefString(ref Struct, out var psize).ToUtf8String();
            }
            else if (gtype == ImageType)
            {
                // GValueGetObject() will not add a ref ... that is
                // held by the gvalue
                var vi = Internal.GObject.GValueGetObject(ref Struct);

                // we want a ref that will last with the life of the vimage:
                // this ref is matched by the unref that's attached to finalize
                // by GObject
                var image = new Image(vi);
                image.ObjectRef();

                result = image;
            }
            else if (gtype == ArrayIntType)
            {
                var intPtr = VipsType.VipsValueGetArrayInt(ref Struct, out var psize);

                var intArr = new int[psize];
                Marshal.Copy(intPtr, intArr, 0, psize);
                result = intArr;
            }
            else if (gtype == ArrayDoubleType)
            {
                var intPtr = VipsType.VipsValueGetArrayDouble(ref Struct, out var psize);

                var doubleArr = new double[psize];
                Marshal.Copy(intPtr, doubleArr, 0, psize);
                result = doubleArr;
            }
            else if (gtype == ArrayImageType)
            {
                var ptrArr = VipsImage.VipsValueGetArrayImage(ref Struct, out var psize);

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
                var array = VipsType.VipsValueGetBlob(ref Struct, out var psize);

                // Blob types are returned as an array of bytes.
                var byteArr = new byte[psize];
                Marshal.Copy(array, byteArr, 0, (int)psize);
                result = byteArr;
            }
            else
            {
                throw new Exception($"unsupported gtype for get {Base.TypeName(gtype)}");
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
        /// Allows an object to try to free resources and perform other cleanup 
        /// operations before it is reclaimed by garbage collection.
        /// </summary>
        ~GValue()
        {
            // Do not re-create Dispose clean-up code here.
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            // logger.Debug($"GC: GValue = {Struct}");

            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // and tag it to be unset on GC as well	
                Internal.GValue.GValueUnset(ref Struct);

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