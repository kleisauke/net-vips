using System;

namespace NetVips
{
    /// <summary>
    /// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
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
        public static void Initialize()
        {
            try
            {
                VipsInitialized = Base.VipsInit();
                if (VipsInitialized)
                {
                    Version = Base.Version(0, false);
                    Version = (Version << 8) + Base.Version(1, false);
                    Version = (Version << 8) + Base.Version(2, false);
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