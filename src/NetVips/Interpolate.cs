namespace NetVips
{
    using System;
    using Internal;

    /// <summary>
    /// Make interpolators for operators like <see cref="Image.Affine"/>.
    /// </summary>
    public class Interpolate : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        private Interpolate(IntPtr pointer)
            : base(pointer)
        {
            // logger.Debug($"VipsInterpolate = {pointer}");
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
        /// <param name="name">libvips class nickname.</param>
        /// <returns>A new <see cref="Interpolate"/>.</returns>
        /// <exception cref="VipsException">If unable to make a new interpolator from <paramref name="name"/>.</exception>
        public static Interpolate NewFromName(string name)
        {
            // logger.Debug($"Interpolate.NewFromName: name = {name}");
            var vi = VipsInterpolate.New(name);
            if (vi == IntPtr.Zero)
            {
                throw new VipsException($"no such interpolator {name}");
            }

            return new Interpolate(vi);
        }
    }
}