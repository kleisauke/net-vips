using System;
using System.Runtime.InteropServices;
using NetVips.Internal;

namespace NetVips
{
    /// <summary>
    /// Manage a <see cref="Internal.VipsObject"/>.
    /// </summary>
    public class VipsObject : GObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        internal VipsObject(IntPtr pointer) : base(pointer)
        {
            // logger.Debug($"VipsObject = {pointer}");
        }

        /// <summary>
        /// Print a table of all active libvips objects. Handy for debugging.
        /// </summary>
        /// <returns></returns>
        public static void PrintAll()
        {
            GC.Collect();
            Internal.VipsObject.VipsObjectPrintAll();
        }

        /// <summary>
        /// slow! eeeeew
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal IntPtr GetPspec(string name)
        {
            // logger.Debug($"GetPspec: this = {this}, name = {name}");
            var pspec = new GParamSpec.Fields().ToIntPtr<GParamSpec.Fields>();
            var argumentClass = new VipsArgumentClass.Fields().ToIntPtr<VipsArgumentClass.Fields>();
            var argumentInstance = new VipsArgumentInstance.Fields().ToIntPtr<VipsArgumentInstance.Fields>();
            var result =
                Internal.VipsObject.VipsObjectGetArgument(Pointer, name, pspec, argumentClass, argumentInstance);

            return result != 0 ? IntPtr.Zero : pspec.Dereference<IntPtr>();
        }

        /// <summary>
        /// Get the GType of a GObject property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>This function returns 0 if the property does not exist.</returns>
        public virtual ulong GetTypeOf(string name)
        {
            // logger.Debug($"GetTypeOf: this = {this}, name = {name}");
            var pspec = GetPspec(name);

            if (pspec == IntPtr.Zero)
            {
                // need to clear any error, this is horrible
                Vips.VipsErrorClear();
                return 0;
            }

            return pspec.Dereference<GParamSpec.Fields>().ValueType;
        }

        /// <summary>
        /// Get the blurb for a GObject property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetBlurb(string name)
        {
            var pspec = GetPspec(name);

            return pspec == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(GParamSpec.GParamSpecGetBlurb(pspec));
        }

        /// <summary>
        /// Get a GObject property.
        /// </summary>
        /// <remarks>
        /// The value of the property is converted to a C# value.
        /// </remarks>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual object Get(string name)
        {
            // logger.Debug($"Get: name = {name}");
            var pspec = GetPspec(name);
            if (pspec == IntPtr.Zero)
            {
                throw new VipsException("Property not found.");
            }

            var gtype = pspec.Dereference<GParamSpec.Fields>().ValueType;
            var gv = new GValue();
            gv.SetType(gtype);
            Internal.GObject.GObjectGetProperty(Pointer, name, gv.Pointer);
            return gv.Get();
        }

        /// <summary>
        /// Set a GObject property. The value is converted to the property type, if possible.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual void Set(string name, object value)
        {
            // logger.Debug($"Set: name = {name}, value = {value}");
            var gtype = GetTypeOf(name);
            var gv = new GValue();
            gv.SetType(gtype);
            gv.Set(value);
            Internal.GObject.GObjectSetProperty(Pointer, name, gv.Pointer);
        }

        /// <summary>
        /// Set a series of properties using a string.
        /// </summary>
        /// <remarks>
        /// For example:
        /// "fred=12, tile"
        /// "[fred=12]"
        /// </remarks>
        /// <param name="stringOptions"></param>
        /// <returns></returns>
        public bool SetString(string stringOptions)
        {
            var result = Internal.VipsObject.VipsObjectSetFromString(Pointer, stringOptions);
            return result == 0;
        }

        /// <summary>
        /// Get the description of a GObject.
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return Marshal.PtrToStringAnsi(Internal.VipsObject.VipsObjectGetDescription(Pointer));
        }
    }
}