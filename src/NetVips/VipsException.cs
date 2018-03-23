using System;
using System.Runtime.InteropServices;
using NetVips.Internal;

namespace NetVips
{
    /// <summary>
    /// Our own exception class which handles the libvips error buffer.
    /// </summary>
    public class VipsException : Exception
    {
        public VipsException()
        {
        }

        public VipsException(string message) : base($"{message}{Environment.NewLine}{VipsErrorBuffer()}")
        {
            Vips.VipsErrorClear();
        }

        public VipsException(string message, Exception inner) : base(
            $"{message}{Environment.NewLine}{VipsErrorBuffer()}", inner)
        {
            Vips.VipsErrorClear();
        }

        private static string VipsErrorBuffer()
        {
            return Marshal.PtrToStringAnsi(Vips.VipsErrorBuffer());
        }
    }
}