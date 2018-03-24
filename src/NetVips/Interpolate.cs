using System;
using NetVips.Internal;

namespace NetVips
{
    /// <summary>
    /// Make interpolators for operators like <see cref="Image.Affine"/>.
    /// </summary>
    public class Interpolate : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        internal VipsInterpolate IntlVipsInterpolate;

        internal Interpolate(VipsInterpolate interpolate) : base(interpolate.ParentObject)
        {
            // logger.Debug($"VipsInterpolate = {interpolate}");
            IntlVipsInterpolate = interpolate;
        }

        /// <summary>
        /// Make a new interpolator by name.
        /// </summary>
        /// <remarks>
        /// Make a new interpolator from the libvips class nickname. For example:
        /// <code language="lang-csharp">
        /// var inter = Interpolate.NewFromName("bicubic");
        /// </code>
        /// You can get a list of all supported interpolators from the command-line
        /// with:
        /// <code language="lang-shell">
        /// $ vips -l interpolate
        /// </code>
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
                throw new VipsException($"no such interpolator {name}");
            }

            return new Interpolate(new VipsInterpolate(vi));
        }
    }
}