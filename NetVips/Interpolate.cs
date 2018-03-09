using System;
using NetVips.Internal;
using NLog;

namespace NetVips
{
    /// <summary>
    /// Make interpolators for operators like <see cref="Image.Affine"/>.
    /// </summary>
    public class Interpolate : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        public VipsInterpolate IntlVipsInterpolate;

        public Interpolate(VipsInterpolate interpolate) : base(interpolate.ParentObject)
        {
            // logger.Debug($"VipsInterpolate = {interpolate}");
            IntlVipsInterpolate = interpolate;
        }

        /// <summary>
        /// Make a new interpolator by name.
        /// </summary>
        /// <remarks>
        /// Make a new interpolator from the libvips class nickname. For example:
        /// 
        ///     var inter = NetVips.Interpolate.NewFromName("bicubic")
        /// 
        /// You can get a list of all supported interpolators from the command-line
        /// with:
        /// 
        ///     $ vips -l interpolate
        /// 
        /// See for example <see cref="Image.Affine"/>.
        /// </remarks>
        /// <param name="name">libvips class nickname</param>
        /// <returns>A new <see cref="Interpolate"/></returns>
        public static Interpolate NewFromName(string name)
        {
            // logger.Debug($"Interpolate.NewFromName: name = {name}");
            var vi = VipsInterpolate.VipsInterpolateNew(name);
            if (vi == IntPtr.Zero)
            {
                throw new Exception($"no such interpolator {name}");
            }

            return new Interpolate(new VipsInterpolate(vi));
        }
    }
}