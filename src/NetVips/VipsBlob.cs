using System;
using System.Runtime.InteropServices;

namespace NetVips;

/// <summary>
/// Manage a <see cref="Internal.VipsBlob"/>.
/// </summary>
internal class VipsBlob : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VipsBlob"/> class
    /// with the specified pointer to wrap around.
    /// </summary>
    /// <param name="pointer">The pointer to wrap around.</param>
    internal VipsBlob(nint pointer) : base(IntPtr.Zero, true)
    {
        // record the pointer we were given to manage
        SetHandle(pointer);
    }

    /// <summary>
    /// Get the data from a <see cref="Internal.VipsBlob"/>.
    /// </summary>
    /// <param name="length">Return number of bytes of data.</param>
    /// <returns>A <see langword="nint"/> containing the data.</returns>
    internal nint GetData(out nuint length)
    {
        return Internal.VipsBlob.Get(this, out length);
    }

    /// <summary>
    /// Decreases the reference count of the blob.
    /// When its reference count drops to 0, the blob is finalized (i.e. its memory is freed).
    /// </summary>
    /// <returns><see langword="true"/> if the handle is released successfully; otherwise,
    /// in the event of a catastrophic failure, <see langword="false"/>.</returns>
    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            // Free the VipsArea
            Internal.VipsArea.Unref(handle);
        }

        return true;
    }

    /// <summary>
    /// Gets a value indicating whether the handle is invalid.
    /// </summary>
    /// <returns><see langword="true"/> if the handle is not valid; otherwise, <see langword="false"/>.</returns>
    public override bool IsInvalid => handle == IntPtr.Zero;

    /// <summary>
    /// Get the number of bytes of data.
    /// </summary>
    internal ulong Length => Marshal.PtrToStructure<Internal.VipsArea.Struct>(handle).Length;

    /// <summary>
    /// Get the reference count of the blob. Handy for debugging.
    /// </summary>
    internal int RefCount => Marshal.PtrToStructure<Internal.VipsArea.Struct>(handle).Count;
}