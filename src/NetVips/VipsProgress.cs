namespace NetVips
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Records a start time, and counts microseconds elapsed since that time.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GTimer
    {
        /// <summary>
        /// Monotonic start time, in microseconds.
        /// </summary>
        public ulong Start;

        /// <summary>
        /// Monotonic end time, in microseconds.
        /// </summary>
        public ulong End;

        /// <summary>
        /// Is the timer currently active?
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool Active;
    }

    /// <summary>
    /// Struct we keep a record of execution time in. Passed to eval signal so
    /// it can assess progress.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VipsProgress
    {
        /// <summary>
        /// Image we are part of.
        /// </summary>
        private IntPtr Im;

        /// <summary>
        /// Time we have been running.
        /// </summary>
        public int Run;

        /// <summary>
        /// Estimated seconds of computation left.
        /// </summary>
        public int Eta;

        /// <summary>
        /// Number of pels we expect to calculate.
        /// </summary>
        public long TPels;

        /// <summary>
        ///  Number of pels calculated so far.
        /// </summary>
        public long NPels;

        /// <summary>
        /// Percent complete.
        /// </summary>
        public int Percent;

        /// <summary>
        /// Start time.
        /// </summary>
        private IntPtr StartPtr;

        /// <summary>
        /// Start time.
        /// </summary>
        public GTimer Start => Marshal.PtrToStructure<GTimer>(StartPtr);
    }
}