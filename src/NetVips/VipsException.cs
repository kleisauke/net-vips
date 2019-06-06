namespace NetVips
{
    using System;
    using System.Runtime.InteropServices;
    using global::NetVips.Internal;

    /// <summary>
    /// Our own exception class which handles the libvips error buffer.
    /// </summary>
    public class VipsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VipsException"/> class.
        /// </summary>
        public VipsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VipsException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public VipsException(string message)
            : base($"{message}{Environment.NewLine}{VipsErrorBuffer()}")
        {
            Vips.ErrorClear();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VipsException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public VipsException(string message, Exception inner)
            : base($"{message}{Environment.NewLine}{VipsErrorBuffer()}", inner)
        {
            Vips.ErrorClear();
        }

        private static string VipsErrorBuffer()
        {
            return Marshal.PtrToStringAnsi(Vips.ErrorBuffer());
        }
    }
}