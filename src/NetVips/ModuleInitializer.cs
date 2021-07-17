namespace NetVips
{
    using System;

    /// <summary>
    /// All code inside the <see cref="Initialize"/> method is ran as soon as the assembly is loaded.
    /// </summary>
    public static class ModuleInitializer
    {
        /// <summary>
        /// Is vips initialized?
        /// </summary>
        public static bool VipsInitialized;

        /// <summary>
        /// Contains the exception when initialization of libvips fails.
        /// </summary>
        public static Exception Exception;

        /// <summary>
        /// Could contain the version number of libvips in an 3-bytes integer.
        /// </summary>
        public static int? Version;

        /// <summary>
        /// Initializes the module.
        /// </summary>
        [System.Runtime.CompilerServices.ModuleInitializer]
        public static void Initialize()
        {
            try
            {
                VipsInitialized = NetVips.Init();
                if (VipsInitialized)
                {
                    Version = NetVips.Version(0, false);
                    Version = (Version << 8) + NetVips.Version(1, false);
                    Version = (Version << 8) + NetVips.Version(2, false);
                }
                else
                {
                    Exception = new VipsException("unable to initialize libvips");
                }
            }
            catch (Exception e)
            {
                VipsInitialized = false;
                Exception = e;
            }
        }
    }
}