namespace NetVips
{
    using Internal;

    /// <summary>
    /// A class that provides the statistics of memory usage and opened files.
    /// </summary>
    /// <remarks>
    /// libvips watches the total amount of live tracked memory and
    /// uses this information to decide when to trim caches.
    /// </remarks>
    public static class Stats
    {
        /// <summary>
        /// Get the number of active allocations.
        /// </summary>
        public static int Allocations => Vips.TrackedGetAllocs();

        /// <summary>
        /// Get the number of bytes currently allocated `vips_malloc()` and friends.
        /// </summary>
        /// <remarks>
        /// libvips uses this figure to decide when to start dropping cache.
        /// </remarks>
        public static int Mem => Vips.TrackedGetMem();

        /// <summary>
        /// Returns the largest number of bytes simultaneously allocated via vips_tracked_malloc().
        /// </summary>
        /// <remarks>
        /// Handy for estimating max memory requirements for a program.
        /// </remarks>
        public static ulong MemHighwater => Vips.TrackedGetMemHighwater();

        /// <summary>
        /// Get the number of open files.
        /// </summary>
        public static int Files => Vips.TrackedGetFiles();
    }
}