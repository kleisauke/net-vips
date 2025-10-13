#if NET6_0_OR_GREATER

using System;
using System.Runtime.InteropServices;
using NetVips.Internal;

namespace NetVips;

/// <summary>
/// Wrap a <see cref="VipsImage"/> object.
/// </summary>
public partial class Image
{
    #region helpers

    /// <summary>
    /// Find the name of the load operation vips will use to load a buffer.
    /// </summary>
    /// <remarks>
    /// For example "VipsForeignLoadJpegBuffer". You can use this to work out what
    /// options to pass to <see cref="NewFromBuffer(ReadOnlySpan{byte}, string, Enums.Access?, Enums.FailOn?, VOption)"/>.
    /// </remarks>
    /// <param name="data">The buffer to test.</param>
    /// <param name="size">Length of the buffer.</param>
    /// <returns>The name of the load operation, or <see langword="null"/>.</returns>
    private static unsafe string FindLoadBuffer(void* data, ulong size) =>
        Marshal.PtrToStringAnsi(VipsForeign.FindLoadBuffer(data, size));

    /// <summary>
    /// Find the name of the load operation vips will use to load a buffer.
    /// </summary>
    /// <remarks>
    /// For example "VipsForeignLoadJpegBuffer". You can use this to work out what
    /// options to pass to <see cref="NewFromBuffer(ReadOnlySpan{byte}, string, Enums.Access?, Enums.FailOn?, VOption)"/>.
    /// </remarks>
    /// <param name="data">The buffer to test.</param>
    /// <returns>The name of the load operation, or <see langword="null"/>.</returns>
    public static unsafe string FindLoadBuffer(ReadOnlySpan<byte> data)
    {
        fixed (byte* dataFixed = data)
        {
            return FindLoadBuffer(dataFixed, (ulong)data.Length);
        }
    }

    #endregion

    #region constructors

    /// <summary>
    /// Load a formatted image from memory.
    /// </summary>
    /// <remarks>
    /// This behaves exactly as <see cref="NewFromFile"/>, but the image is
    /// loaded from the memory object rather than from a file. The memory
    /// object can be a string or buffer.
    /// </remarks>
    /// <param name="data">The memory object to load the image from.</param>
    /// <param name="strOptions">Load options as a string. Use <see cref="string.Empty"/> for no options.</param>
    /// <param name="access">Hint the expected access pattern for the image.</param>
    /// <param name="failOn">The type of error that will cause load to fail. By
    /// default, loaders are permissive, that is, <see cref="Enums.FailOn.None"/>.</param>
    /// <param name="kwargs">Optional options that depend on the load operation.</param>
    /// <returns>A new <see cref="Image"/>.</returns>
    /// <exception cref="VipsException">If unable to load from <paramref name="data"/>.</exception>
    public static unsafe Image NewFromBuffer(
        ReadOnlySpan<byte> data,
        string strOptions = "",
        Enums.Access? access = null,
        Enums.FailOn? failOn = null,
        VOption kwargs = null)
    {
        fixed (byte* dataFixed = data)
        {
            var operationName = FindLoadBuffer(dataFixed, (ulong)data.Length);
            if (operationName == null)
            {
                throw new VipsException("unable to load from buffer");
            }

            var options = new VOption();
            if (kwargs != null)
            {
                options.Merge(kwargs);
            }

            options.AddIfPresent(nameof(access), access);
            options.AddFailOn(failOn);

            options.Add("string_options", strOptions);

            var ptr = Internal.VipsBlob.Copy(dataFixed, (nuint)data.Length);
            if (ptr == IntPtr.Zero)
            {
                throw new VipsException("unable to load from buffer");
            }

            using var blob = new VipsBlob(ptr);
            return Operation.Call(operationName, options, blob) as Image;
        }
    }

    /// <summary>
    /// Wrap an image around a memory array.
    /// </summary>
    /// <param name="data">A <see cref="ReadOnlyMemory{T}"/>.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="bands">Number of bands.</param>
    /// <param name="format">Band format.</param>
    /// <returns>A new <see cref="Image"/>.</returns>
    /// <exception cref="VipsException">If unable to make image from <paramref name="data"/>.</exception>
    public static unsafe Image NewFromMemory<T>(
        ReadOnlyMemory<T> data,
        int width,
        int height,
        int bands,
        Enums.BandFormat format) where T : unmanaged
    {
        var handle = data.Pin();
        var size = (nuint)data.Length * (nuint)sizeof(T);
        var vi = VipsImage.NewFromMemory(handle.Pointer, size, width, height, bands, format);
        if (vi == IntPtr.Zero)
        {
            handle.Dispose();

            throw new VipsException("unable to make image from memory");
        }

        var image = new Image(vi) { MemoryPressure = (long)size };

        // Need to release the pinned MemoryHandle when the image is closed.
        image.OnPostClose += () => handle.Dispose();

        return image;
    }

    /// <summary>
    /// Like <see cref="NewFromMemory{T}(ReadOnlyMemory{T}, int, int, int, Enums.BandFormat)"/>, but
    /// for <see cref="ReadOnlySpan{T}"/>, so we must copy as it could be allocated on the stack.
    /// </summary>
    /// <param name="data">A <see cref="ReadOnlySpan{T}"/>.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="bands">Number of bands.</param>
    /// <param name="format">Band format.</param>
    /// <returns>A new <see cref="Image"/>.</returns>
    /// <exception cref="VipsException">If unable to make image from <paramref name="data"/>.</exception>
    public static unsafe Image NewFromMemoryCopy<T>(
        ReadOnlySpan<T> data,
        int width,
        int height,
        int bands,
        Enums.BandFormat format) where T : unmanaged
    {
        fixed (T* dataFixed = data)
        {
            var size = (nuint)data.Length * (nuint)sizeof(T);
            var vi = VipsImage.NewFromMemoryCopy(dataFixed, size, width, height, bands, format);
            if (vi == IntPtr.Zero)
            {
                throw new VipsException("unable to make image from memory");
            }

            return new Image(vi) { MemoryPressure = (long)size };
        }
    }

    #endregion
}

#endif