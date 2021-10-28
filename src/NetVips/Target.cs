namespace NetVips
{
    using System;
    using System.Text;

    /// <summary>
    /// An output connection.
    /// </summary>
    public class Target : Connection
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc cref="Connection"/>
        internal Target(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Get the memory object held by the target when using <see cref="NewToMemory"/>.
        /// </summary>
        public byte[] Blob => (byte[])Get("blob");

        /// <summary>
        /// Make a new target to write to a file descriptor (a small integer).
        /// </summary>
        /// <remarks>
        /// Make a new target that is attached to the descriptor. For example:
        /// <code language="lang-csharp">
        /// using var target = Target.NewToDescriptor(1);
        /// </code>
        /// Makes a descriptor attached to stdout.
        ///
        /// You can pass this target to (for example) <see cref="Image.WriteToTarget"/>.
        /// </remarks>
        /// <param name="descriptor">Write to this file descriptor.</param>
        /// <returns>A new <see cref="Target"/>.</returns>
        /// <exception cref="VipsException">If unable to create a new <see cref="Target"/> from <paramref name="descriptor"/>.</exception>
        public static Target NewToDescriptor(int descriptor)
        {
            // logger.Debug($"Target.NewToDescriptor: descriptor = {descriptor}");

            var pointer = Internal.VipsTarget.NewToDescriptor(descriptor);
            if (pointer == IntPtr.Zero)
            {
                throw new VipsException($"can't create output target to descriptor {descriptor}");
            }

            return new Target(pointer);
        }

        /// <summary>
        /// Make a new target to write to a file.
        /// </summary>
        /// <remarks>
        /// Make a new target that will write to the named file. For example:
        /// <code language="lang-csharp">
        /// using var target = Target.NewToFile("myfile.jpg");
        /// </code>
        /// You can pass this target to (for example) <see cref="Image.WriteToTarget"/>.
        /// </remarks>
        /// <param name="filename">Write to this this file.</param>
        /// <returns>A new <see cref="Target"/>.</returns>
        /// <exception cref="VipsException">If unable to create a new <see cref="Target"/> from <paramref name="filename"/>.</exception>
        public static Target NewToFile(string filename)
        {
            // logger.Debug($"Target.NewToFile: filename = {filename}");

            var bytes = Encoding.UTF8.GetBytes(filename + char.MinValue); // Ensure null-terminated string
            var pointer = Internal.VipsTarget.NewToFile(bytes);
            if (pointer == IntPtr.Zero)
            {
                throw new VipsException($"can't create output target to filename {filename}");
            }

            return new Target(pointer);
        }

        /// <summary>
        /// Make a new target to write to an area of memory.
        /// </summary>
        /// <remarks>
        /// Make a new target that will write to memory. For example:
        /// <code language="lang-csharp">
        /// using var target = Target.NewToMemory();
        /// </code>
        /// You can pass this target to (for example) <see cref="Image.WriteToTarget"/>.
        ///
        /// After writing to the target, fetch the bytes from the target object with:
        /// <code language="lang-csharp">
        /// var bytes = target.Blob;
        /// </code>
        /// </remarks>
        /// <returns>A new <see cref="Target"/>.</returns>
        /// <exception cref="VipsException">If unable to create a new <see cref="Target"/>.</exception>
        public static Target NewToMemory()
        {
            // logger.Debug($"Target.NewToMemory");

            var pointer = Internal.VipsTarget.NewToMemory();

            if (pointer == IntPtr.Zero)
            {
                throw new VipsException("can't create output target to memory");
            }

            return new Target(pointer);
        }
    }
}