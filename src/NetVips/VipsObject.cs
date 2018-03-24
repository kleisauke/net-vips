using System;
using NetVips.Internal;

namespace NetVips
{
    /// <summary>
    /// Manage a <see cref="Internal.VipsObject"/>.
    /// </summary>
    public class VipsObject : GObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        internal Internal.VipsObject IntlVipsObject;

        internal VipsObject(Internal.VipsObject vipsObject) : base(vipsObject.ParentInstance)
        {
            IntlVipsObject = vipsObject;
            // logger.Debug($"VipsObject = {vipsObject}");
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
        internal GParamSpec GetPspec(string name)
        {
            // logger.Debug($"GetPspec: this = {this}, name = {name}");
            var pspec = new GParamSpec();
            var argumentClass = new VipsArgumentClass();
            var argumentInstance = new VipsArgumentInstance();

            var result =
                Internal.VipsObject.VipsObjectGetArgument(IntlVipsObject, name, pspec, argumentClass, argumentInstance);
            if (result != 0)
            {
                return null;
            }

            return new GParamSpec(pspec.Pointer.Dereference<IntPtr>());
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

            if (pspec == null)
            {
                // need to clear any error, this is horrible
                Vips.VipsErrorClear();
                return 0;
            }

            return pspec.ValueType;
        }

        /// <summary>
        /// Get the blurb for a GObject property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetBlurb(string name)
        {
            var pspec = GetPspec(name);

            if (pspec == null)
            {
                return null;
            }

            return GParamSpec.GParamSpecGetBlurb(GetPspec(name));
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
            if (pspec == null)
            {
                throw new VipsException("Property not found.");
            }

            var gtype = pspec.ValueType;
            var gv = new GValue();
            gv.SetType(gtype);
            Internal.GObject.GObjectGetProperty(IntlGObject, name, gv.IntlGValue);
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
            Internal.GObject.GObjectSetProperty(IntlGObject, name, gv.IntlGValue);
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
            var result = Internal.VipsObject.VipsObjectSetFromString(IntlVipsObject, stringOptions);
            return result == 0;
        }

        /// <summary>
        /// Get the description of a GObject.
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return Internal.VipsObject.VipsObjectGetDescription(IntlVipsObject);
        }
    }
}