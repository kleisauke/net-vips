#if NET6_0_OR_GREATER

using System;

namespace NetVips;

/// <summary>
/// An input connection.
/// </summary>
public partial class Source
{
    /// <summary>
    /// Make a new source from a memory object.
    /// </summary>
    /// <remarks>
    /// Make a new source that is attached to the memory object. For example:
    /// <code language="lang-csharp">
    /// using var source = Source.NewFromMemory(data);
    /// </code>
    /// You can pass this source to (for example) <see cref="Image.NewFromSource"/>.
    /// </remarks>
    /// <param name="data">The memory object.</param>
    /// <returns>A new <see cref="Source"/>.</returns>
    /// <exception cref="VipsException">If unable to create a new <see cref="Source"/> from <paramref name="data"/>.</exception>
    public static unsafe Source NewFromMemory(ReadOnlySpan<byte> data)
    {
        fixed (byte* dataFixed = data)
        {
            var ptr = Internal.VipsBlob.Copy(dataFixed, (nuint)data.Length);
            if (ptr == IntPtr.Zero)
            {
                throw new VipsException("can't create input source from memory");
            }

            using var blob = new VipsBlob(ptr);
            var pointer = Internal.VipsSource.NewFromBlob(blob);
            if (pointer == IntPtr.Zero)
            {
                throw new VipsException("can't create input source from memory");
            }

            return new Source(pointer);
        }
    }
}

#endif