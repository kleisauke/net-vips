namespace NetVips
{
    using System;

    /// <summary>
    /// The abstract base Connection class.
    /// </summary>
    public abstract class Connection : VipsObject
    {
        /// <inheritdoc cref="GObject"/>
        internal Connection(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Get the filename associated with a connection. Return <see langword="null"/> if there
        /// is no associated file.
        /// </summary>
        /// <returns>The filename associated with this connection or <see langword="null"/>.</returns>
        public string GetFileName()
        {
            return Internal.VipsConnection.FileName(this).ToUtf8String();
        }

        /// <summary>
        /// Make a human-readable name for a connection suitable for error
        /// messages.
        /// </summary>
        /// <returns>The human-readable name for this connection.</returns>
        public string GetNick()
        {
            return Internal.VipsConnection.Nick(this).ToUtf8String();
        }
    }
}