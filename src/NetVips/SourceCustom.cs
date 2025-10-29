using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace NetVips;

/// <summary>
/// An source you can connect delegates to implement behaviour.
/// </summary>
public class SourceCustom : Source
{
    /// <summary>
    /// A read delegate.
    /// </summary>
    /// <remarks>
    /// The interface is exactly as <see cref="Stream.Read(byte[], int, int)"/>.
    /// The handler is given a number of bytes to fetch, and should return a
    /// bytes-like object containing up to that number of bytes. If there is
    /// no more data available, it should return <see langword="0"/>.
    /// </remarks>
    /// <param name="buffer">An array of bytes.</param>
    /// <param name="length">The maximum number of bytes to be read.</param>
    /// <returns>The total number of bytes read into the buffer.</returns>
    public delegate int ReadDelegate(byte[] buffer, int length);

    /// <summary>
    /// A seek delegate.
    /// </summary>
    /// <remarks>
    /// The interface is exactly as <see cref="Stream.Seek"/>. The handler is given
    /// parameters for offset and whence with the same meanings. It also returns the
    /// new position within the current source.
    ///
    /// Seek handlers are optional. If you do not set one, your source will be
    /// treated as unseekable and libvips will do extra caching.
    /// </remarks>
    /// <param name="offset">A byte offset relative to the <paramref name="origin"/>
    /// parameter.</param>
    /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the
    /// reference point used to obtain the new position.</param>
    /// <returns>The new position within the current source.</returns>
    public delegate long SeekDelegate(long offset, SeekOrigin origin);

    /// <summary>
    /// Attach a read delegate.
    /// </summary>
    public event ReadDelegate OnRead;

    /// <summary>
    /// Attach a seek delegate.
    /// </summary>
    public event SeekDelegate OnSeek;

    /// <inheritdoc cref="Source"/>
    public SourceCustom() : base(Internal.VipsSourceCustom.New())
    {
        SignalConnect("read", (Internal.VipsSourceCustom.ReadSignal)ReadHandler);
        SignalConnect("seek", (Internal.VipsSourceCustom.SeekSignal)SeekHandler);
    }

    /// <summary>
    /// The internal read handler.
    /// </summary>
    /// <param name="sourcePtr">The underlying pointer to the source.</param>
    /// <param name="buffer">A pointer to an array of bytes.</param>
    /// <param name="length">The maximum number of bytes to be read.</param>
    /// <param name="userDataPtr">User data associated with the source.</param>
    /// <returns>The total number of bytes read into the buffer.</returns>
    internal long ReadHandler(nint sourcePtr, nint buffer, long length, nint userDataPtr)
    {
        if (length <= 0)
        {
            return 0;
        }
        if (OnRead == null)
        {
            return -1;
        }

        var tempArray = ArrayPool<byte>.Shared.Rent((int)length);
        try
        {
            var readLength = OnRead.Invoke(tempArray, (int)length);
            if (readLength > 0)
            {
                Marshal.Copy(tempArray, 0, buffer, readLength);
            }

            return readLength;
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
    /// <param name="sourcePtr">The underlying pointer to the source.</param>
    /// <param name="offset">A byte offset relative to the <paramref name="whence"/>
    /// parameter.</param>
    /// <param name="whence">A value of type <see cref="SeekOrigin"/> indicating the
    /// reference point used to obtain the new position.</param>
    /// <param name="userDataPtr">User data associated with the source.</param>
    /// <returns>The new position within the current source.</returns>
    internal long SeekHandler(nint sourcePtr, long offset, int whence, nint userDataPtr)
    {
        var newPosition = OnSeek?.Invoke(offset, (SeekOrigin)whence);
        return newPosition ?? -1;
    }
}