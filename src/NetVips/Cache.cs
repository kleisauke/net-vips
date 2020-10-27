namespace NetVips
{
    using Internal;

    /// <summary>
    /// A class around libvips' operation cache.
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// Gets or sets the maximum number of operations libvips keeps in cache.
        /// </summary>
        public static int Max
        {
            get => Vips.CacheGetMax();
            set => Vips.CacheSetMax(value);
        }

        /// <summary>
        /// Gets or sets the maximum amount of tracked memory allowed.
        /// </summary>
        public static ulong MaxMem
        {
            get => (ulong) Vips.CacheGetMaxMem();
            set => Vips.CacheSetMaxMem(value);
        }

        /// <summary>
        /// Gets or sets the maximum amount of tracked files allowed.
        /// </summary>
        public static int MaxFiles
        {
            get => Vips.CacheGetMaxFiles();
            set => Vips.CacheSetMaxFiles(value);
        }

        /// <summary>
        /// Gets the current number of operations in cache.
        /// </summary>
        public static int Size => Vips.CacheGetSize();

        /// <summary>
        /// Enable or disable libvips cache tracing.
        /// </summary>
        public static bool Trace
        {
            set => Vips.CacheSetTrace(value);
        }
    }
}