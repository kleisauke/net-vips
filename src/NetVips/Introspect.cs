namespace NetVips
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Runtime.InteropServices;
    using Internal;

    /// <summary>
    /// Build introspection data for operations.
    /// </summary>
    /// <remarks>
    /// Make an operation, introspect it, and build a structure representing
    /// everything we know about it.
    /// </remarks>
    public class Introspect
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A cache for introspection data.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Introspect> IntrospectCache =
            new ConcurrentDictionary<string, Introspect>();

        /// <summary>
        /// An object structure that encapsulates the metadata
        /// required to specify arguments.
        /// </summary>
        public struct Argument
        {
            /// <summary>
            /// Name of this argument.
            /// </summary>
            public string Name;

            /// <summary>
            /// Flags for this argument.
            /// </summary>
            public Enums.ArgumentFlags Flags;

            /// <summary>
            /// The GType for this argument.
            /// </summary>
            public IntPtr Type;
        }

        /// <summary>
        /// The first required input image or <see langword="null"/>.
        /// </summary>
        public Argument? MemberX;

        /// <summary>
        /// A bool indicating if this operation is mutable.
        /// </summary>
        public bool Mutable;

        /// <summary>
        /// The required input for this operation.
        /// </summary>
        public List<Argument> RequiredInput = new List<Argument>();

        /// <summary>
        /// The optional input for this operation.
        /// </summary>
        public Dictionary<string, Argument> OptionalInput = new Dictionary<string, Argument>();

        /// <summary>
        /// The required output for this operation.
        /// </summary>
        public List<Argument> RequiredOutput = new List<Argument>();

        /// <summary>
        /// The optional output for this operation.
        /// </summary>
        public Dictionary<string, Argument> OptionalOutput = new Dictionary<string, Argument>();

        /// <summary>
        /// Build introspection data for a specified operation name.
        /// </summary>
        /// <param name="operationName">The operation name to introspect.</param>
        private Introspect(string operationName)
        {
            // logger.Debug($"Introspect = {operationName}");
            using var op = Operation.NewFromName(operationName);
            var arguments = GetArgs(op);

            foreach (var entry in arguments)
            {
                var name = entry.Key;
                var flag = entry.Value;
                var gtype = op.GetTypeOf(name);

                var details = new Argument
                {
                    Name = name,
                    Flags = flag,
                    Type = gtype
                };

                if ((flag & Enums.ArgumentFlags.INPUT) != 0)
                {
                    if ((flag & Enums.ArgumentFlags.REQUIRED) != 0 &&
                        (flag & Enums.ArgumentFlags.DEPRECATED) == 0)
                    {
                        // the first required input image arg will be self
                        if (!MemberX.HasValue && gtype == GValue.ImageType)
                        {
                            MemberX = details;
                        }
                        else
                        {
                            RequiredInput.Add(details);
                        }
                    }
                    else
                    {
                        // we allow deprecated optional args
                        OptionalInput[name] = details;
                    }

                    // modified input arguments count as mutable.
                    if ((flag & Enums.ArgumentFlags.MODIFY) != 0 &&
                        (flag & Enums.ArgumentFlags.REQUIRED) != 0 &&
                        (flag & Enums.ArgumentFlags.DEPRECATED) == 0)
                    {
                        Mutable = true;
                    }
                }
                else if ((flag & Enums.ArgumentFlags.OUTPUT) != 0)
                {
                    if ((flag & Enums.ArgumentFlags.REQUIRED) != 0 &&
                        (flag & Enums.ArgumentFlags.DEPRECATED) == 0)
                    {
                        RequiredOutput.Add(details);
                    }
                    else
                    {
                        // again, allow deprecated optional args
                        OptionalOutput[name] = details;
                    }
                }
            }
        }

        /// <summary>
        /// Get all arguments for an operation.
        /// </summary>
        /// <remarks>
        /// Not quick! Try to call this infrequently.
        /// </remarks>
        /// <param name="operation">Operation to lookup.</param>
        /// <returns>Arguments for the operation.</returns>
        private IEnumerable<KeyValuePair<string, Enums.ArgumentFlags>> GetArgs(Operation operation)
        {
            var args = new List<KeyValuePair<string, Enums.ArgumentFlags>>();

            void AddArg(string name, Enums.ArgumentFlags flags)
            {
                // libvips uses '-' to separate parts of arg names, but we
                // need '_' for C#
                name = name.Replace("-", "_");

                args.Add(new KeyValuePair<string, Enums.ArgumentFlags>(name, flags));
            }

            // vips_object_get_args was added in 8.7
            if (NetVips.AtLeastLibvips(8, 7))
            {
                var result = Internal.VipsObject.GetArgs(operation, out var names, out var flags, out var nArgs);

                if (result != 0)
                {
                    throw new VipsException("unable to get arguments from operation");
                }

                for (var i = 0; i < nArgs; i++)
                {
                    var flag = (Enums.ArgumentFlags)Marshal.PtrToStructure<int>(flags + i * sizeof(int));
                    if ((flag & Enums.ArgumentFlags.CONSTRUCT) == 0)
                    {
                        continue;
                    }

                    var name = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(names, i * IntPtr.Size));

                    AddArg(name, flag);
                }
            }
            else
            {
                IntPtr AddConstruct(IntPtr self, IntPtr pspec, IntPtr argumentClass, IntPtr argumentInstance,
                    IntPtr a, IntPtr b)
                {
                    var flags = Marshal.PtrToStructure<VipsArgumentClass>(argumentClass).Flags;
                    if ((flags & Enums.ArgumentFlags.CONSTRUCT) == 0)
                    {
                        return IntPtr.Zero;
                    }

                    var name = Marshal.PtrToStringAnsi(Marshal.PtrToStructure<GParamSpec.Struct>(pspec).Name);

                    AddArg(name, flags);

                    return IntPtr.Zero;
                }

                Vips.ArgumentMap(operation, AddConstruct, IntPtr.Zero, IntPtr.Zero);
            }

            return args;
        }

        /// <summary>
        /// Get introspection data for a specified operation name.
        /// </summary>
        /// <param name="operationName">Operation name.</param>
        /// <returns>Introspection data.</returns>
        public static Introspect Get(string operationName)
        {
            return IntrospectCache.GetOrAdd(operationName, name => new Introspect(name));
        }
    }
}