namespace NetVips
{
    using System;
    using System.IO;

    /// <summary>
    /// An target you can connect delegates to implement behaviour.
    /// </summary>
    public class TargetCustom : Target
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A write delegate.
        /// </summary>
        /// <remarks>
        /// The interface is the same as <see cref="Stream.Write"/>, so the handler is
        /// given a bytes-like object to write. However, the handler MUST return the number
        /// of bytes written.
        /// </remarks>
        /// <param name="buffer">An array of bytes.</param>
        /// <param name="length">The number of bytes to be written to the current target.</param>
        /// <returns>The total number of bytes written to the target.</returns>
        public delegate long WriteDelegate(byte[] buffer, int length);

        /// <summary>
        /// A finish delegate.
        /// </summary>
        /// <remarks>
        /// This optional handler is called at the end of write. It should do any
        /// cleaning up, if necessary.
        /// </remarks>
        public delegate void FinishDelegate();

        /// <summary>
        /// Attach a write delegate.
        /// </summary>
        public event WriteDelegate OnWrite;

        /// <summary>
        /// Attach a finish delegate.
        /// </summary>
        public event FinishDelegate OnFinish;

        /// <inheritdoc cref="Target"/>
        public TargetCustom() : base(Internal.VipsTargetCustom.New())
        {
            SignalConnect("write", (Internal.VipsTargetCustom.WriteSignal)WriteHandler);
            SignalConnect("finish", (Internal.VipsTargetCustom.FinishSignal)FinishHandler);
        }

        /// <summary>
        /// The internal write handler.
        /// </summary>
        /// <param name="targetPtr">The underlying pointer to the target.</param>
        /// <param name="buffer">An array of bytes.</param>
        /// <param name="length">The number of bytes to be written to the current target.</param>
        /// <param name="userDataPtr">User data associated with the target.</param>
        /// <returns>The total number of bytes written to the target.</returns>
        internal long WriteHandler(IntPtr targetPtr, byte[] buffer, int length, IntPtr userDataPtr)
        {
            var bytesWritten = OnWrite?.Invoke(buffer, length);
            return bytesWritten ?? -1;
        }

        /// <summary>
        /// The internal finish handler.
        /// </summary>
        /// <param name="targetPtr">The underlying pointer to the target.</param>
        /// <param name="userDataPtr">User data associated with the target.</param>
        internal void FinishHandler(IntPtr targetPtr, IntPtr userDataPtr)
        {
            OnFinish?.Invoke();
        }
    }
}