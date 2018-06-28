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
        internal static void PrintAll()
        {
            GC.Collect();
            Internal.VipsObject.VipsObjectPrintAll();
        }

        /// <summary>
        /// slow! eeeeew
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private GParamSpec.Struct? GetPspec(string name)
        {
            // logger.Debug($"GetPspec: this = {this}, name = {name}");
            var pspec = new GParamSpec.Struct();
            var argumentClass = new VipsArgumentClass.Struct();
            var argumentInstance = new VipsArgumentInstance.Struct();
            var argument = Internal.VipsObject.VipsObjectGetArgument(this, name, ref pspec, ref argumentClass,
                ref argumentInstance);

            return argument != 0
                ? default(GParamSpec.Struct?)
                : pspec.ToIntPtr<GParamSpec.Struct>().Dereference<IntPtr>().Dereference<GParamSpec.Struct>();
        }

        /// <summary>
        /// Get the GType of a GObject property.
        /// </summary>
        /// <param name="name">The name of the GType to get the type of.</param>
        /// <returns>A new instance of <see cref="IntPtr" /> initialized to the GType or
        /// <see cref="IntPtr.Zero" /> if the property does not exist.</returns>
        public virtual IntPtr GetTypeOf(string name)
        {
            // logger.Debug($"GetTypeOf: this = {this}, name = {name}");
            var pspec = GetPspec(name);

            if (!pspec.HasValue)
            {
                // need to clear any error, this is horrible
                Vips.VipsErrorClear();
                return IntPtr.Zero;
            }

            return pspec.Value.ValueType;
        }

        /// <summary>
        /// Get the blurb for a GObject property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal string GetBlurb(string name)
        {
            var pspec = GetPspec(name);

            if (!pspec.HasValue)
            {
                return null;
            }

            var pspecValue = pspec.Value;
            return Marshal.PtrToStringAnsi(GParamSpec.GParamSpecGetBlurb(ref pspecValue));
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
            if (!pspec.HasValue)
            {
                throw new VipsException("Property not found.");
            }

            var gtype = pspec.Value.ValueType;
            var gv = new GValue();
            gv.SetType(gtype);

            // this will add a ref for GObject properties, that ref will be
            // unreffed when the gvalue is finalized
            Internal.GObject.GObjectGetProperty(this, name, ref gv.Struct);

            return gv.Get();
        }

        /// <summary>
        /// Set a GObject property. The value is converted to the property type, if possible.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public virtual void Set(string name, object value)
        {
            // logger.Debug($"Set: name = {name}, value = {value}");
            var gtype = GetTypeOf(name);
            var gv = new GValue();
            gv.SetType(gtype);
            gv.Set(value);
            Internal.GObject.GObjectSetProperty(this, name, ref gv.Struct);
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
            var result = Internal.VipsObject.VipsObjectSetFromString(this, stringOptions);
            return result == 0;
        }

        /// <summary>
        /// Get the description of a GObject.
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return Marshal.PtrToStringAnsi(Internal.VipsObject.VipsObjectGetDescription(this));
        }
    }
}