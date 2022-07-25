namespace NetVips
{
    using System;
    using System.IO;

    /// <summary>
    /// An source connected to a readable <see cref="Stream"/>.
    /// </summary>
    internal class SourceStream : SourceCustom
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Read from this stream.
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// The start position within the stream.
        /// </summary>
        private readonly long _startPosition;

        /// <inheritdoc cref="SourceCustom"/>
        internal SourceStream(Stream stream)
        {
            // logger.Debug($"SourceStream: stream = {stream}");
            var seekable = stream.CanSeek;

            _stream = stream;
            _startPosition = seekable ? _stream.Position : 0;

            OnRead += Read;
            if (seekable)
            {
                OnSeek += Seek;
            }
        }

        /// <summary>
        /// Create a <see cref="SourceStream"/> attached to an <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">Read from this stream.</param>
        /// <returns>A new <see cref="SourceStream"/>.</returns>
        /// <exception cref="T:System.ArgumentException">If <paramref name="stream"/> is not readable.</exception>
        internal static SourceStream NewFromStream(Stream stream)
        {
            // logger.Debug($"SourceStream.NewFromStream: stream = {stream}");

            if (!stream.CanRead)
            {
                throw new ArgumentException("The stream should be readable.", nameof(stream));
            }

            return new SourceStream(stream);
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
    }
}