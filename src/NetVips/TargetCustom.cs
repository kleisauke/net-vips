namespace NetVips
{
    using System;
    using System.IO;
    using System.Buffers;
    using System.Runtime.InteropServices;

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
        /// The interface is the same as <see cref="Stream.Write(byte[], int, int)"/>.
        /// The handler is given a bytes-like object to write. However, the handler MUST
        /// return the number of bytes written.
        /// </remarks>
        /// <param name="buffer">An array of bytes.</param>
        /// <param name="length">The number of bytes to be written to the current target.</param>
        /// <returns>The total number of bytes written to the target.</returns>
        public delegate long WriteDelegate(byte[] buffer, int length);

        /// <summary>
        /// A read delegate.
        /// </summary>
        /// <remarks>
        /// libtiff needs to be able to read on targets, unfortunately.
        /// </remarks>
        /// <param name="buffer">An array of bytes.</param>
        /// <param name="length">The maximum number of bytes to be read.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        public delegate int ReadDelegate(byte[] buffer, int length);

        /// <summary>
        /// A seek delegate.
        /// </summary>
        /// <remarks>
        /// libtiff needs to be able to seek on targets, unfortunately.
        /// </remarks>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/>
        /// parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the
        /// reference point used to obtain the new position.</param>
        /// <returns>The new position within the current target.</returns>
        public delegate long SeekDelegate(long offset, SeekOrigin origin);

        /// <summary>
        /// A end delegate.
        /// </summary>
        /// <remarks>
        /// This optional handler is called at the end of write. It should do any
        /// cleaning up, if necessary.
        /// </remarks>
        /// <returns>0 on success, -1 on error.</returns>
        public delegate int EndDelegate();

        /// <summary>
        /// Attach a write delegate.
        /// </summary>
        public event WriteDelegate OnWrite;

        /// <summary>
        /// Attach a read delegate.
        /// </summary>
        /// <remarks>
        /// This is not called prior libvips 8.13.
        /// </remarks>
        public event ReadDelegate OnRead;

        /// <summary>
        /// Attach a seek delegate.
        /// </summary>
        /// <remarks>
        /// This is not called prior libvips 8.13.
        /// </remarks>
        public event SeekDelegate OnSeek;

        /// <summary>
        /// Attach a end delegate.
        /// </summary>
        public event EndDelegate OnEnd;

        /// <inheritdoc cref="Target"/>
        public TargetCustom() : base(Internal.VipsTargetCustom.New())
        {
            var vips813 = NetVips.AtLeastLibvips(8, 13);

            SignalConnect("write", (Internal.VipsTargetCustom.WriteSignal)WriteHandler);
            if (vips813)
            {
                SignalConnect("read", (Internal.VipsTargetCustom.ReadSignal)ReadHandler);
                SignalConnect("seek", (Internal.VipsTargetCustom.SeekSignal)SeekHandler);
            }
            SignalConnect(vips813 ? "end" : "finish", (Internal.VipsTargetCustom.EndSignal)EndHandler);
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
        /// The internal read handler.
        /// </summary>
        /// <param name="targetPtr">The underlying pointer to the target.</param>
        /// <param name="buffer">A pointer to an array of bytes.</param>
        /// <param name="length">The maximum number of bytes to be read.</param>
        /// <param name="userDataPtr">User data associated with the target.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        internal long ReadHandler(IntPtr targetPtr, IntPtr buffer, long length, IntPtr userDataPtr)
        {
            if (length <= 0)
            {
                return 0;
            }

            var tempArray = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                var readLength = OnRead?.Invoke(tempArray, (int)length);
                if (!readLength.HasValue)
                {
                    return -1;
                }

                if (readLength.Value > 0)
                {
                    Marshal.Copy(tempArray, 0, buffer, readLength.Value);
                }

                return readLength.Value;
            }
            catch
            {
                return -1;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tempArray);
            }
        }

        /// <summary>
        /// The internal seek handler.
        /// </summary>
        /// <param name="targetPtr">The underlying pointer to the target.</param>
        /// <param name="offset">A byte offset relative to the <paramref name="whence"/>
        /// parameter.</param>
        /// <param name="whence">A value of type <see cref="SeekOrigin"/> indicating the
        /// reference point used to obtain the new position.</param>
        /// <param name="userDataPtr">User data associated with the target.</param>
        /// <returns>The new position within the current target.</returns>
        internal long SeekHandler(IntPtr targetPtr, long offset, int whence, IntPtr userDataPtr)
        {
            var newPosition = OnSeek?.Invoke(offset, (SeekOrigin)whence);
            return newPosition ?? -1;
        }

        /// <summary>
        /// The internal end handler.
        /// </summary>
        /// <param name="targetPtr">The underlying pointer to the target.</param>
        /// <param name="userDataPtr">User data associated with the target.</param>
        /// <returns>0 on success, -1 on error.</returns>
        internal int EndHandler(IntPtr targetPtr, IntPtr userDataPtr)
        {
            return OnEnd?.Invoke() ?? 0;
        }
    }
}