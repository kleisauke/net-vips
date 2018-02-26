using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NetVips.AutoGen;
using NLog;

namespace NetVips
{
    /// <summary>
    /// Wrap GValue in a C# class.
    /// </summary>
    /// <remarks>
    /// This class wraps :class:`.GValue` in a convenient interface. You can use
    /// instances of this class to get and set :class:`.GObject` properties.
    /// 
    /// On construction, :class:`.GValue` is all zero (empty). You can pass it to
    /// a get function to have it filled by :class:`.GObject`, or use init to
    /// set a type, set to set a value, then use it to set an object property.
    /// 
    /// GValue lifetime is managed automatically.
    /// </remarks>
    public class GValue : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public AutoGen.GValue Pointer;

        // look up some common gtypes at init for speed
        public static ulong GBoolType = Base.TypeFromName("gboolean");
        public static ulong GIntType = Base.TypeFromName("gint");
        public static ulong GDoubleType = Base.TypeFromName("gdouble");
        public static ulong GStrType = Base.TypeFromName("gchararray");
        public static ulong GEnumType = Base.TypeFromName("GEnum");
        public static ulong GFlagsType = Base.TypeFromName("GFlags");
        public static ulong GObjectType = Base.TypeFromName("GObject");
        public static ulong ImageType = Base.TypeFromName("VipsImage");
        public static ulong ArrayIntType = Base.TypeFromName("VipsArrayInt");
        public static ulong ArrayDoubleType = Base.TypeFromName("VipsArrayDouble");
        public static ulong ArrayImageType = Base.TypeFromName("VipsArrayImage");
        public static ulong RefStrType = Base.TypeFromName("VipsRefString");
        public static ulong BlobType = Base.TypeFromName("VipsBlob");
        public static ulong BandFormatType;
        public static ulong BlendModeType;

        static GValue()
        {
            enumtypes.VipsBandFormatGetType();
            BandFormatType = Base.TypeFromName("VipsBandFormat");

            if (Base.AtLeastLibvips(8, 6))
            {
                enumtypes.VipsBlendModeGetType();
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
            {BlobType, "string"}
        };

        /// <summary>
        /// Map a gtype to the name of the C# type we use to represent it.
        /// </summary>
        /// <param name="gtype"></param>
        /// <returns></returns>
        public static string GTypeToCSharp(ulong gtype)
        {
            var fundamental = AutoGen.gtype.GTypeFundamental(gtype);

            if (GTypeToCSharpDict.ContainsKey(gtype))
            {
                return GTypeToCSharpDict[gtype];
            }

            if (GTypeToCSharpDict.ContainsKey(fundamental))
            {
                return GTypeToCSharpDict[fundamental];
            }

            return "<unknown type>";
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
                ? util.VipsEnumFromNick("netvips", gtype, strValue)
                : (int) value;
        }


        /// <summary>
        /// Turn an int back into an enum string.
        /// </summary>
        /// <param name="gtype"></param>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string FromEnum(ulong gtype, int enumValue)
        {
            var cstr = util.VipsEnumNick(gtype, enumValue);
            if (cstr == null)
            {
                throw new Exception("value not in enum");
            }

            return cstr;
        }

        public GValue()
        {
            // allocate memory for the gvalue which will be freed on GC
            Pointer = AutoGen.GValue.__CreateInstance(new AutoGen.GValue.__Internal());

            // logger.Debug($"GValue: GValue = {Pointer}");
        }

        ~GValue()
        {
            Dispose(false);
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
            gvalue.GValueInit(Pointer, gtype);
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
            // logger.Debug($"GValue.Set: value = {value}");

            var gtype = Pointer.GType;
            var fundamental = AutoGen.gtype.GTypeFundamental(gtype);
            if (gtype == GBoolType)
            {
                gvaluetypes.GValueSetBoolean(Pointer, (bool) value ? 1 : 0);
            }
            else if (gtype == GIntType)
            {
                gvaluetypes.GValueSetInt(Pointer, (int) value);
            }
            else if (gtype == GDoubleType)
            {
                gvaluetypes.GValueSetDouble(Pointer, (double) value);
            }
            else if (fundamental == GEnumType)
            {
                genums.GValueSetEnum(Pointer, ToEnum(gtype, value));
            }
            else if (fundamental == GFlagsType)
            {
                genums.GValueSetFlags(Pointer, Convert.ToUInt32(value));
            }
            else if (gtype == GStrType)
            {
                gvaluetypes.GValueSetString(Pointer, (string) value);
            }
            else if (gtype == RefStrType)
            {
                type.VipsValueSetRefString(Pointer, (string) value);
            }
            else if (fundamental == GObjectType)
            {
                gobject.GValueSetObject(Pointer, ((GObject) value).Pointer.__Instance);
            }
            else if (gtype == ArrayIntType)
            {
                if (value is int[] values)
                {
                    type.VipsValueSetArrayInt(Pointer, values, values.Length);
                }
            }
            else if (gtype == ArrayDoubleType)
            {
                if (value is double[] values)
                {
                    type.VipsValueSetArrayDouble(Pointer, values, values.Length);
                }
            }
            else if (gtype == ArrayImageType)
            {
                if (value is Image[] values)
                {
                    var size = values.Length;
                    image.VipsValueSetArrayImage(Pointer, size);

                    var psize = 0;

                    unsafe
                    {
                        var ptr = (IntPtr*) image.VipsValueGetArrayImage(Pointer, ref psize);

                        for (var i = 0; i < size; i++)
                        {
                            var gObject = values[i].Pointer.__Instance;
                            gobject.GObjectRef(gObject);

                            ptr[i] = gObject;
                        }
                    }
                }
            }
            else if (gtype == BlobType)
            {
                if (value is string blob)
                {
                    var length = blob.Length;

                    // We need to set the blob to a copy of the string that vips_lib
                    // can own
                    var memory = Marshal.StringToHGlobalUni(blob);

                    if (Base.AtLeastLibvips(8, 6))
                    {
                        type.VipsValueSetBlobFree(Pointer, memory, (ulong) length);
                    }
                    else
                    {
                        type.VipsValueSetBlob(Pointer, (a, b) =>
                        {
                            gmem.GFree(a);

                            return 0;
                        }, memory, (ulong) length);
                    }
                }
            }
            else
            {
                throw new Exception(
                    $"unsupported gtype for set {Base.TypeName(gtype)}, fundamental {Base.TypeName(fundamental)}"
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
            // logger.Debug($"GValue.Get: this = {this}");

            var gtype = Pointer.GType;
            var fundamental = AutoGen.gtype.GTypeFundamental(gtype);

            object result;
            if (gtype == GBoolType)
            {
                result = gvaluetypes.GValueGetBoolean(Pointer) != 0;
            }
            else if (gtype == GIntType)
            {
                result = gvaluetypes.GValueGetInt(Pointer);
            }
            else if (gtype == GDoubleType)
            {
                result = gvaluetypes.GValueGetDouble(Pointer);
            }
            else if (fundamental == GEnumType)
            {
                result = FromEnum(gtype, genums.GValueGetEnum(Pointer));
            }
            else if (fundamental == GFlagsType)
            {
                result = genums.GValueGetFlags(Pointer);
            }
            else if (gtype == GStrType)
            {
                result = gvaluetypes.GValueGetString(Pointer);
            }
            else if (gtype == RefStrType)
            {
                ulong psize = 0;
                result = type.VipsValueGetRefString(Pointer, ref psize);
            }
            else if (gtype == ImageType)
            {
                // g_value_get_object() will not add a ref ... that is
                // held by the gvalue
                var go = gobject.GValueGetObject(Pointer);

                // we want a ref that will last with the life of the vimage:
                // this ref is matched by the unref that's attached to finalize
                // by Image()
                gobject.GObjectRef(go);

                result = new Image(VipsImage.__CreateInstance(go, true));
            }
            else if (gtype == ArrayIntType)
            {
                var psize = 0;
                IntPtr intPtr;
                unsafe
                {
                    intPtr = new IntPtr(type.VipsValueGetArrayInt(Pointer, ref psize));
                }

                var intArr = new int[psize];
                Marshal.Copy(intPtr, intArr, 0, psize);
                result = intArr;
            }
            else if (gtype == ArrayDoubleType)
            {
                var psize = 0;
                IntPtr intPtr;
                unsafe
                {
                    intPtr = new IntPtr(type.VipsValueGetArrayDouble(Pointer, ref psize));
                }

                var doubleArr = new double[psize];
                Marshal.Copy(intPtr, doubleArr, 0, psize);
                result = doubleArr;
            }
            else if (gtype == ArrayImageType)
            {
                var psize = 0;
                var ptr = image.VipsValueGetArrayImage(Pointer, ref psize);

                var images = new Image[psize];
                for (var i = 0; i < psize; i++)
                {
                    var vi = VipsImage.__CreateInstance(Marshal.ReadIntPtr(ptr, i * IntPtr.Size));
                    gobject.GObjectRef(vi.__Instance);
                    images[i] = new Image(vi);
                }

                result = images;
            }
            else if (gtype == BlobType)
            {
                ulong psize = 0;
                var array = type.VipsValueGetBlob(Pointer, ref psize);

                result = Marshal.PtrToStringUni(array, (int) psize);
            }
            else
            {
                throw new Exception($"unsupported gtype for get {Base.TypeName(gtype)}");
            }

            return result;
        }

        private void ReleaseUnmanagedResources()
        {
            // and tag it to be unset on GC as well
            // logger.Debug($"GObject GC: GValue = {Pointer}");
            gvalue.GValueUnset(Pointer);
            // logger.Debug($"GObject GC: GValue = {Pointer}");
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                Pointer?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}