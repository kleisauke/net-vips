using NetVips.AutoGen;
using System;
using System.Runtime.InteropServices;
using NLog;

namespace NetVips
{
    /// <summary>
    /// Manage a VipsObject.
    /// </summary>
    public class VipsObject : GObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public AutoGen.VipsObject VObject;

        public VipsObject(AutoGen.VipsObject vipsObject) : base(vipsObject.ParentInstance)
        {
            // logger.Debug($"VipsObject: VipsObject = {vipsObject}");
            VObject = vipsObject;
        }

        /// <summary>
        /// Print a table of all active libvips objects. Handy for debugging.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void PrintAll(string msg)
        {
            GC.Collect();
            // logger.Debug(msg);
            @object.VipsObjectPrintAll();
        }

        /// <summary>
        /// slow! eeeeew
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GParamSpec GetPspec(string name)
        {
            // logger.Debug($"VipsObject.GetPspec: this = {this}, name = {name}");
            var pspec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            var argumentClass = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            var argumentInstance = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));

            var result =
                @object.__Internal.VipsObjectGetArgument(VObject.__Instance, name, pspec, argumentClass,
                    argumentInstance);
            return result != 0
                ? null
                : GParamSpec.__CreateInstance(pspec.Dereference<IntPtr>().Dereference<GParamSpec.__Internal>());
        }

        /// <summary>
        /// Get the GType of a GObject property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>This function returns 0 if the property does not exist.</returns>
        public virtual ulong GetTypeOf(string name)
        {
            // logger.Debug($"VipsObject.GetPspec: this = {this}, name = {name}");
            var pspec = GetPspec(name);

            return pspec?.ValueType ?? 0;
        }

        /// <summary>
        /// Get the blurb for a GObject property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetBlurb(string name)
        {
            return gparam.GParamSpecGetBlurb(GetPspec(name));
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
            // logger.Debug($"VipsObject.Get: name = {name}");
            var pspec = GetPspec(name);
            if (pspec == null)
            {
                throw new Exception("Property not found.");
            }

            var gtype = pspec.ValueType;
            var gv = new GValue();
            gv.SetType(gtype);
            gobject.GObjectGetProperty(Pointer, name, gv.Pointer);
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
            // logger.Debug($"VipsObject.Set: name = {name}, value = {value}");
            var gtype = GetTypeOf(name);
            var gv = new GValue();
            gv.SetType(gtype);
            gv.Set(value);
            gobject.GObjectSetProperty(Pointer, name, gv.Pointer);
        }

        /// <summary>
        /// Set a series of properties using a string.
        /// </summary>
        /// <remarks>
        /// For example:
        ///  'fred=12, tile'
        ///   '[fred=12]'
        /// </remarks>
        /// <param name="stringOptions"></param>
        /// <returns></returns>
        public bool SetString(string stringOptions)
        {
            var result = @object.VipsObjectSetFromString(VObject, stringOptions);
            return result == 0;
        }

        /// <summary>
        /// Get the description of a GObject.
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return @object.VipsObjectGetDescription(VObject);
        }
    }
}