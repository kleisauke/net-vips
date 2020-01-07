namespace NetVips
{
    using System;
    using System.IO;

    /// <summary>
    /// An source connected to a writable <see cref="Stream"/>.
    /// </summary>
    internal class TargetStream : TargetCustom
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Write to this stream.
        /// </summary>
        private readonly Stream _stream;

        /// <inheritdoc cref="GObject"/>
        internal TargetStream(Stream stream)
        {
            // logger.Debug($"TargetStream: stream = {stream}");
            _stream = stream;

            OnWrite += Write;
            OnFinish += Finish;
        }

        /// <summary>
        /// Create a <see cref="TargetStream"/> which will output to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">Write to this stream.</param>
        /// <returns>A new <see cref="TargetStream"/>.</returns>
        /// <exception cref="ArgumentException">If <paramref name="stream"/> is not writable.</exception>
        internal static TargetStream NewFromStream(Stream stream)
        {
            // logger.Debug($"TargetStream.NewFromStream: stream = {stream}");
            if (!stream.CanWrite)
            {
                throw new ArgumentException("The stream should be writable.", nameof(stream));
            }

            return new TargetStream(stream);
        }

        /// <summary>
        /// Attach a write handler.
        /// </summary>
        /// <param name="buffer">An array of bytes.</param>
        /// <param name="length">The number of bytes to be written to the current stream.</param>
        /// <returns>The total number of bytes written to the stream.</returns>
        private long Write(byte[] buffer, int length)
        {
            try
            {
                _stream.Write(buffer, 0, length);
            }
            catch
            {
                return -1;
            }

            return length;
        }

        /// <summary>
        /// Attach a finish handler.
        /// </summary>
        private void Finish()
        {
            // TODO: Should we Close() instead?
            _stream.Flush();
        }
    }
}