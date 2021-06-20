namespace NetVips
{
    using System;
    using System.Text;
    using System.Runtime.InteropServices;

    /// <summary>
    /// An input connection.
    /// </summary>
    public class Source : Connection
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Secret ref for <see cref="NewFromMemory(byte[])"/>.
        /// </summary>
        private GCHandle _dataHandle;

        /// <inheritdoc cref="Connection"/>
        internal Source(IntPtr pointer)
            : base(pointer)
        {
        }

        /// <summary>
        /// Make a new source from a file descriptor (a small integer).
        /// </summary>
        /// <remarks>
        /// Make a new source that is attached to the descriptor. For example:
        /// <code language="lang-csharp">
        /// using var source = Source.NewFromDescriptor(0);
        /// </code>
        /// Makes a descriptor attached to stdin.
        ///
        /// You can pass this source to (for example) <see cref="Image.NewFromSource"/>.
        /// </remarks>
        /// <param name="descriptor">Read from this file descriptor.</param>
        /// <returns>A new <see cref="Source"/>.</returns>
        /// <exception cref="VipsException">If unable to create a new <see cref="Source"/> from <paramref name="descriptor"/>.</exception>
        public static Source NewFromDescriptor(int descriptor)
        {
            // logger.Debug($"Source.NewFromDescriptor: descriptor = {descriptor}");

            var pointer = Internal.VipsSource.NewFromDescriptor(descriptor);
            if (pointer == IntPtr.Zero)
            {
                throw new VipsException($"can't create source from descriptor {descriptor}");
            }

            return new Source(pointer);
        }

        /// <summary>
        /// Make a new source from a filename.
        /// </summary>
        /// <remarks>
        /// Make a new source that is attached to the named file. For example:
        /// <code language="lang-csharp">
        /// using var source = Source.NewFromFile("myfile.jpg");
        /// </code>
        /// You can pass this source to (for example) <see cref="Image.NewFromSource"/>.
        /// </remarks>
        /// <param name="filename">Read from this filename.</param>
        /// <returns>A new <see cref="Source"/>.</returns>
        /// <exception cref="VipsException">If unable to create a new <see cref="Source"/> from <paramref name="filename"/>.</exception>
        public static Source NewFromFile(string filename)
        {
            // logger.Debug($"Source.NewFromFile: filename = {filename}");

            var bytes = Encoding.UTF8.GetBytes(filename + char.MinValue); // Ensure null-terminated string
            var pointer = Internal.VipsSource.NewFromFile(bytes);
            if (pointer == IntPtr.Zero)
            {
                throw new VipsException($"can't create source from filename {filename}");
            }

            return new Source(pointer);
        }

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
        public static Source NewFromMemory(byte[] data)
        {
            // logger.Debug($"Source.NewFromMemory");

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var pointer = Internal.VipsSource.NewFromMemory(handle.AddrOfPinnedObject(), (UIntPtr)data.Length);

            if (pointer == IntPtr.Zero)
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }

                throw new VipsException("can't create input source from memory");
            }

            return new Source(pointer) { _dataHandle = handle };
        }

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
        public static Source NewFromMemory(string data) => NewFromMemory(Encoding.UTF8.GetBytes(data));

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
        public static Source NewFromMemory(char[] data) => NewFromMemory(Encoding.UTF8.GetBytes(data));

        /// <inheritdoc cref="GObject"/>
        protected override void Dispose(bool disposing)
        {
            // release reference to our secret ref
            if (_dataHandle.IsAllocated)
            {
                _dataHandle.Free();
            }

            // Call our base Dispose method
            base.Dispose(disposing);
        }
    }
}