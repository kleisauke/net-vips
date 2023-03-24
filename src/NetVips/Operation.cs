namespace NetVips
{
    using System;
    using Internal;

    /// <summary>
    /// Wrap a <see cref="VipsOperation"/> object.
    /// </summary>
    public class Operation : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc cref="VipsObject"/>
        private Operation(IntPtr pointer)
            : base(pointer)
        {
            // logger.Debug($"Operation = {pointer}");
        }

        /// <summary>
        /// Create a new <see cref="VipsOperation"/> with the specified nickname.
        /// </summary>
        /// <remarks>
        /// You'll need to set any arguments and build the operation before you can use it. See
        /// <see cref="O:Call"/> for a higher-level way to make new operations.
        /// </remarks>
        /// <param name="operationName">Nickname of operation to create.</param>
        /// <returns>The new operation.</returns>
        /// <exception cref="VipsException">If the operation doesn't exist.</exception>
        public static Operation NewFromName(string operationName)
        {
            var vop = VipsOperation.New(operationName);
            if (vop == IntPtr.Zero)
            {
                throw new VipsException($"no such operation {operationName}");
            }

            return new Operation(vop);
        }

        /// <summary>
        /// Set a GObject property. The value is converted to the property type, if possible.
        /// </summary>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="matchImage">A <see cref="Image"/> used as guide.</param>
        /// <param name="value">The value.</param>
        /// <param name="gtype">The GType of the property.</param>
        private void Set(IntPtr gtype, Image matchImage, string name, object value)
        {
            // logger.Debug($"Operation.Set: name = {name}, matchImage = {matchImage}, value = {value}");

            // if the object wants an image and we have a constant, Imageize it
            //
            // if the object wants an image array, Imageize any constants in the
            // array
            if (matchImage != null)
            {
                if (gtype == GValue.ImageType)
                {
                    value = Image.Imageize(matchImage, value);
                }
                else if (gtype == GValue.ArrayImageType)
                {
                    if (!(value is Array values) || values.Rank != 1)
                    {
                        throw new ArgumentException(
                            $"unsupported value type {value.GetType()} for VipsArrayImage");
                    }

                    var images = new Image[values.Length];
                    for (var i = 0; i < values.Length; i++)
                    {
                        ref var image = ref images[i];
                        image = Image.Imageize(matchImage, values.GetValue(i));
                    }

                    value = images;
                }
            }

            Set(gtype, name, value);
        }

        /// <summary>
        /// Lookup the set of flags for this operation.
        /// </summary>
        /// <returns>Flags for this operation.</returns>
        public Enums.OperationFlags GetFlags() => VipsOperation.GetFlags(this);

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <remarks>
        /// Use this method to call any libvips operation. For example:
        /// <code language="lang-csharp">
        /// using Image blackImage = Operation.Call("black", 10, 10) as Image;
        /// </code>
        /// See the Introduction for notes on how this works.
        /// </remarks>
        /// <param name="operationName">Operation name.</param>
        /// <param name="args">An arbitrary number and variety of arguments.</param>
        /// <returns>A new object.</returns>
        public static object Call(string operationName, params object[] args) =>
            Call(operationName, null, null, args);

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <remarks>
        /// Use this method to call any libvips operation. For example:
        /// <code language="lang-csharp">
        /// using Image blackImage = Operation.Call("black", 10, 10) as Image;
        /// </code>
        /// See the Introduction for notes on how this works.
        /// </remarks>
        /// <param name="operationName">Operation name.</param>
        /// <param name="kwargs">Optional arguments.</param>
        /// <param name="args">An arbitrary number and variety of arguments.</param>
        /// <returns>A new object.</returns>
        public static object Call(string operationName, VOption kwargs = null, params object[] args) =>
            Call(operationName, kwargs, null, args);

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <remarks>
        /// Use this method to call any libvips operation. For example:
        /// <code language="lang-csharp">
        /// using Image blackImage = Operation.Call("black", 10, 10) as Image;
        /// </code>
        /// See the Introduction for notes on how this works.
        /// </remarks>
        /// <param name="operationName">Operation name.</param>
        /// <param name="kwargs">Optional arguments.</param>
        /// <param name="matchImage">A <see cref="Image"/> used as guide.</param>
        /// <param name="args">An arbitrary number and variety of arguments.</param>
        /// <returns>A new object.</returns>
        public static object Call(string operationName, VOption kwargs = null, Image matchImage = null,
            params object[] args)
        {
            // logger.Debug($"Operation.call: operationName = {operationName}");
            // logger.Debug($"Operation.call: matchImage = {matchImage}");
            // logger.Debug($"Operation.call: args = {args}, kwargs = {kwargs}");

            // pull out the special string_options kwarg
            object stringOptions = null;
            kwargs?.Remove("string_options", out stringOptions);
            // logger.Debug($"Operation.call: stringOptions = {stringOptions}");

            var intro = Introspect.Get(operationName);
            if (intro.RequiredInput.Count != args.Length)
            {
                throw new ArgumentException(
                    $"unable to call {operationName}: {args.Length} arguments given, but {intro.RequiredInput.Count} required");
            }

            if (!intro.Mutable && matchImage is MutableImage)
            {
                throw new VipsException($"unable to call {operationName}: operation must be mutable");
            }

            IntPtr vop;
            using (var op = NewFromName(operationName))
            {
                // set any string options before any args so they can't be
                // overridden
                if (stringOptions != null && !op.SetString(stringOptions as string))
                {
                    throw new VipsException($"unable to call {operationName}");
                }

                // set all required inputs
                if (matchImage != null && intro.MemberX.HasValue)
                {
                    var memberX = intro.MemberX.Value;
                    op.Set(memberX.Type, memberX.Name, matchImage);
                }

                for (var i = 0; i < intro.RequiredInput.Count; i++)
                {
                    var arg = intro.RequiredInput[i];
                    op.Set(arg.Type, matchImage, arg.Name, args[i]);
                }

                // set all optional inputs, if any
                if (kwargs != null)
                {
                    foreach (var item in kwargs)
                    {
                        var name = item.Key;
                        var value = item.Value;

                        if (intro.OptionalInput.TryGetValue(name, out var arg))
                        {
                            op.Set(arg.Type, matchImage, name, value);
                        }
                        else if (!intro.OptionalOutput.ContainsKey(name))
                        {
                            throw new ArgumentException($"{operationName} does not support optional argument: {name}");
                        }
                    }
                }

                // build operation
                vop = VipsOperation.Build(op);
                if (vop == IntPtr.Zero)
                {
                    Internal.VipsObject.UnrefOutputs(op);
                    throw new VipsException($"unable to call {operationName}");
                }
            }

            var results = new object[intro.RequiredOutput.Count];
            using (var op = new Operation(vop))
            {
                // get all required results
                for (var i = 0; i < intro.RequiredOutput.Count; i++)
                {
                    var arg = intro.RequiredOutput[i];

                    ref var result = ref results[i];
                    result = op.Get(arg.Name);
                }

                // fetch optional output args, if any
                if (kwargs != null)
                {
                    var optionalArgs = new VOption();

                    foreach (var item in kwargs)
                    {
                        var name = item.Key;

                        if (intro.OptionalOutput.ContainsKey(name))
                        {
                            optionalArgs[name] = op.Get(name);
                        }
                    }

                    if (optionalArgs.Count > 0)
                    {
                        var resultsLength = results.Length;
                        Array.Resize(ref results, resultsLength + 1);
                        results[resultsLength] = optionalArgs;
                    }
                }

                Internal.VipsObject.UnrefOutputs(op);
            }

            // logger.Debug($"Operation.call: result = {result}");

            return results.Length == 1 ? results[0] : results;
        }

        /// <summary>
        /// Set the block state on all operations in the libvips class hierarchy at
        /// <paramref name="name"/> and below.
        /// </summary>
        /// <remarks>
        /// For example:
        /// <code language="lang-csharp">
        /// Operation.Block("VipsForeignLoad", true);
        /// Operation.Block("VipsForeignLoadJpeg", false);
        /// </code>
        /// Will block all load operations, except JPEG. Use:
        /// <code language="lang-shell">
        /// $ vips -l
        /// </code>
        /// at the command-line to see the class hierarchy.
        /// Use <see cref="NetVips.BlockUntrusted"/> to set the
        /// block state on all untrusted operations.
        ///
        /// This call does nothing if the named operation is not found.
        /// At least libvips 8.13 is needed.
        /// </remarks>
        /// <param name="name">Set block state at this point and below.</param>
        /// <param name="state">The block state to set.</param>
        public static void Block(string name, bool state)
        {
            VipsOperation.BlockSet(name, state);
        }
    }
}