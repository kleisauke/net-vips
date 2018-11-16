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
        /// This variable will store the exception when initialization of libvips fails.
        /// </summary>
        public static Exception Exception;

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                VipsInitialized = Base.VipsInit();
                if (!VipsInitialized)
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