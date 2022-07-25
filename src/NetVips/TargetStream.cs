namespace NetVips
{
    using System;
    using System.IO;

    /// <summary>
    /// An target connected to a writable <see cref="Stream"/>.
    /// </summary>
    internal class TargetStream : TargetCustom
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Write to this stream.
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// The start position within the stream.
        /// </summary>
        private readonly long _startPosition;

        /// <inheritdoc cref="GObject"/>
        internal TargetStream(Stream stream)
        {
            // logger.Debug($"TargetStream: stream = {stream}");
            var readable = stream.CanRead;
            var seekable = stream.CanSeek;

            _stream = stream;
            _startPosition = seekable ? _stream.Position : 0;

            OnWrite += Write;
            if (readable)
            {
                OnRead += Read;
            }
            if (seekable)
            {
                OnSeek += Seek;
            }
            OnEnd += End;
        }

        /// <summary>
        /// Create a <see cref="TargetStream"/> which will output to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">Write to this stream.</param>
        /// <returns>A new <see cref="TargetStream"/>.</returns>
        /// <exception cref="T:System.ArgumentException">If <paramref name="stream"/> is not writable.</exception>
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
        /// Attach a read handler.
        /// </summary>
        /// <param name="buffer">An array of bytes.</param>
        /// <param name="length">The maximum number of bytes to be read.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        public int Read(byte[] buffer, int length)
        {
            return _stream.Read(buffer, 0, length);
        }

        /// <summary>
        /// Attach a seek handler.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/>
        /// parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the
        /// reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public long Seek(long offset, SeekOrigin origin)
        {
            try
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        return _stream.Seek(_startPosition + offset, SeekOrigin.Begin) - _startPosition;
                    case SeekOrigin.Current:
                        return _stream.Seek(offset, SeekOrigin.Current) - _startPosition;
                    case SeekOrigin.End:
                        return _stream.Seek(offset, SeekOrigin.End) - _startPosition;
                    default:
                        return -1;
                }
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Attach a end handler.
        /// </summary>
        /// <returns>0 on success, -1 on error.</returns>
        public int End()
        {
            try
            {
                _stream.Flush();
            }
            catch
            {
                return -1;
            }

            return 0;
        }
    }
}