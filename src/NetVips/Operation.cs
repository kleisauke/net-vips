using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NetVips.Internal;

namespace NetVips
{
    /// <summary>
    /// Wrap a <see cref="NetVips.Internal.VipsOperation"/> object.
    /// </summary>
    public class Operation : VipsObject
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly VipsOperation _intlOperation;

        internal Operation(VipsOperation vOperation) : base(vOperation.ParentInstance)
        {
            _intlOperation = vOperation;
            // logger.Debug($"VipsOperation = {vOperation}");
        }

        /// <summary>
        /// Recursive search for <see cref="T" /> into an array and the underlying
        /// subarrays. This is used to find the matchImage for an operation
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        private static T FindInside<T>(IEnumerable<object> thing) where T : class
        {
            return thing.Select(FindInside<T>).FirstOrDefault(result => result != null);
        }

        private static T FindInside<T>(object thing) where T : class
        {
            if (!(thing is object[] enumerable))
            {
                return thing.GetType() == typeof(T) ? thing as T : null;
            }

            return FindInside<T>(enumerable);
        }

        public static Operation NewFromName(string operationName)
        {
            var vop = VipsOperation.VipsOperationNew(operationName);
            if (vop == IntPtr.Zero)
            {
                throw new VipsException($"no such operation {operationName}");
            }

            return new Operation(new VipsOperation(vop));
        }

        public void Set(string name, int flags, Image matchImage, object value)
        {
            // logger.Debug($"Operation.Set: name = {name}, flags = {flags}, " +
            //           $"matchImage = {matchImage} value = {value}");

            // if the object wants an image and we have a constant, Imageize it
            //
            // if the object wants an image array, Imageize any constants in the
            // array
            if (!(matchImage is null))
            {
                var gtype = GetTypeOf(name);
                if (gtype == GValue.ImageType)
                {
                    value = Image.Imageize(matchImage, value);
                }
                else if (gtype == GValue.ArrayImageType)
                {
                    if (value is object[] values)
                    {
                        value = values.Smap(x => Image.Imageize(matchImage, x));
                    }
                }
            }

            // MODIFY args need to be copied before they are set
            if ((flags & (int) Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_MODIFY) != 0)
            {
                // logger.Debug($"copying MODIFY arg {name}");
                // make sure we have a unique copy
                value = (value as Image)?.Copy().CopyMemory();
            }

            Set(name, value);
        }

        internal Internal.Enums.VipsOperationFlags GetFlags()
        {
            return VipsOperation.VipsOperationGetFlags(_intlOperation);
        }

        // this is slow ... call as little as possible
        internal IDictionary<string, Internal.Enums.VipsArgumentFlags> GetArgs()
        {
            var args = new Dictionary<string, Internal.Enums.VipsArgumentFlags>();

            VipsArgumentMapFn addConstruct = (self, pspec, argumentClass, argumentInstance, a, b) =>
            {
                var flags = new VipsArgumentClass(argumentClass).Flags;
                if ((flags & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_CONSTRUCT) != 0)
                {
                    var name = new GParamSpec(pspec).Name;

                    // libvips uses '-' to separate parts of arg names, but we
                    // need '_' for C#
                    name = name?.Replace("-", "_");

                    args.Add(name, flags);
                }

                return IntPtr.Zero;
            };

            // prevent it from being re-located or disposed of by the garbage collector
            var gchCallbackDelegate = GCHandle.Alloc(addConstruct);

            Internal.VipsObject.VipsArgumentMap(IntlVipsObject, addConstruct, IntPtr.Zero, IntPtr.Zero);

            if (gchCallbackDelegate.IsAllocated)
            {
                // release reference to delegate
                gchCallbackDelegate.Free();
            }

            return args;
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <remarks>
        /// Use this method to call any libvips operation. For example:
        /// 
        /// var blackImage = NetVips.Operation.call('black', 10, 10)
        /// 
        /// See the Introduction for notes on how this works.
        /// </remarks>
        /// <param name="operationName"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static object Call(string operationName, object arg)
        {
            return Call(operationName, null, arg);
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <remarks>
        /// Use this method to call any libvips operation. For example:
        /// 
        /// var blackImage = NetVips.Operation.call('black', 10, 10)
        /// 
        /// See the Introduction for notes on how this works.
        /// </remarks>
        /// <param name="operationName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Call(string operationName, params object[] args)
        {
            return Call(operationName, null, args);
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <remarks>
        /// Use this method to call any libvips operation. For example:
        /// 
        /// var blackImage = NetVips.Operation.call('black', 10, 10)
        /// 
        /// See the Introduction for notes on how this works.
        /// </remarks>
        /// <param name="operationName"></param>
        /// <param name="kwargs"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Call(string operationName, VOption kwargs, params object[] args)
        {
            // logger.Debug($"VipsOperation.call: operation_name = {operationName}");
            // logger.Debug($"VipsOperation.call: args = {args}, kwargs = {kwargs}");

            // pull out the special string_options kwarg
            object stringOptions = null;
            kwargs?.Remove("string_options", out stringOptions);
            // logger.Debug($"VipsOperation.call: stringOptions = {stringOptions}");

            var op = NewFromName(operationName);
            var arguments = op.GetArgs();

            // logger.Debug($"VipsOperation.call: arguments = {arguments}");

            // make a thing to quickly get flags from an arg name
            var flagsFromName = new Dictionary<string, Internal.Enums.VipsArgumentFlags>();

            var nRequired = 0;
            foreach (var entry in arguments)
            {
                var name = entry.Key;
                var flag = entry.Value;

                flagsFromName[name] = flag;

                // count required input args
                if ((flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    (flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
                {
                    nRequired++;
                }
            }

            if (nRequired != args.Length)
            {
                throw new ArgumentException(
                    $"unable to call {operationName}: {args.Length} arguments given, but {nRequired} required");
            }

            // the first image argument is the thing we expand constants to
            // match ... look inside tables for images, since we may be passing
            // an array of image as a single param
            var matchImage = FindInside<Image>(args);

            // logger.Debug($"VipsOperation.call: matchImage = {matchImage}");

            // set any string options before any args so they can't be
            // overridden
            if (stringOptions != null && !op.SetString(stringOptions as string))
            {
                throw new VipsException($"unable to call {operationName}");
            }

            // set required and optional args
            var n = 0;
            foreach (var entry in arguments)
            {
                var name = entry.Key;
                var flag = entry.Value;

                if ((flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    (flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
                {
                    op.Set(name, (int) flag, matchImage, args[n]);
                    n++;
                }
            }

            if (kwargs != null)
            {
                foreach (var entry in kwargs)
                {
                    var name = entry.Key;
                    var value = entry.Value;

                    if (!flagsFromName.ContainsKey(name))
                    {
                        throw new Exception($"{operationName} does not support argument {name}");
                    }

                    op.Set(name, (int) flagsFromName[name], matchImage, value);
                }
            }

            // build operation
            var vop = VipsOperation.VipsCacheOperationBuild(op._intlOperation);
            if (vop == IntPtr.Zero)
            {
                throw new VipsException($"unable to call {operationName}");
            }

            op = new Operation(new VipsOperation(vop));

            // fetch required output args, plus modified input images
            var result = new List<object>();

            foreach (var entry in arguments)
            {
                var name = entry.Key;
                var flag = entry.Value;

                if ((flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_OUTPUT) != 0 &&
                    (flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    (flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
                {
                    result.Add(op.Get(name));
                }

                if ((flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_MODIFY) != 0)
                {
                    result.Add(op.Get(name));
                }
            }

            // fetch optional output args
            var opts = new VOption();

            if (kwargs != null)
            {
                foreach (var entry in kwargs)
                {
                    var name = entry.Key;

                    var flags = flagsFromName[name];
                    if ((flags & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_OUTPUT) != 0 &&
                        (flags & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) == 0 &&
                        (flags & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
                    {
                        opts[name] = op.Get(name);
                    }
                }
            }

            Internal.VipsObject.VipsObjectUnrefOutputs(op.IntlVipsObject);

            if (opts.Count > 0)
            {
                result.Add(opts);
            }

            // logger.Debug($"VipsOperation.call: result = {result}");

            return result.Count == 0 ? null : (result.Count == 1 ? result[0] : result.ToArray());
        }

        /// <summary>
        /// Make a C#-style docstring + function.
        /// </summary>
        /// <remarks>
        /// This is used to generate the functions in NetVips.Image (<see cref="Image"/>).
        /// </remarks>
        /// <param name="operationName"></param>
        /// <param name="indent"></param>
        /// <param name="outParameters"></param>
        /// <returns></returns>
        public static string GenerateFunction(string operationName, string indent = "        ",
            string[] outParameters = null)
        {
            var op = NewFromName(operationName);
            if ((op.GetFlags() & Internal.Enums.VipsOperationFlags.VIPS_OPERATION_DEPRECATED) != 0)
            {
                throw new Exception($"No such operator. Operator \"{operationName}\" is deprecated");
            }

            // we are only interested in non-deprecated args
            var arguments = op.GetArgs()
                .Where(x => (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
                .Select(x => new KeyValuePair<string, Internal.Enums.VipsArgumentFlags>(x.Key, x.Value))
                .ToDictionary(x => x.Key, x => x.Value);

            // find the first required input image arg, if any ... that will be self
            string memberX = null;
            foreach (var entry in arguments)
            {
                var name = entry.Key;
                var flag = entry.Value;

                if ((flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (flag & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    op.GetTypeOf(name) == GValue.ImageType)
                {
                    memberX = name;
                    break;
                }
            }

            string[] reservedKeywords =
            {
                "in", "ref", "out", "ushort"
            };

            var requiredInput = arguments.Where(x =>
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    x.Key != memberX)
                .Select(x => x.Key)
                .ToArray();
            var optionalInput = arguments.Where(x =>
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) == 0)
                .Select(x => x.Key)
                .ToArray();
            var requiredOutput = arguments.Where(x =>
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_OUTPUT) != 0 &&
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 ||
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_MODIFY) != 0)
                .Select(x => x.Key)
                .ToArray();

            string SafeIdentifier(string name) =>
                reservedKeywords.Contains(name)
                    ? "@" + name
                    : name;

            string SafeCast(string type, string name = "result")
            {
                switch (type)
                {
                    case "GObject":
                    case "Image":
                    case "int[]":
                    case "double[]":
                    case "byte[]":
                    case "Image[]":
                    case "object[]":
                        return $" as {type};";
                    case "int":
                    case "double":
                        return $" is {type} {name} ? {name} : 0;";
                    case "bool":
                        return $" is {type} {name} && {name};";
                    case "string":
                        return $" is {type} {name} ? {name} : null;";
                    default:
                        return ";";
                }
            }

            string ToNullable(string type, string name)
            {
                switch (type)
                {
                    case "Image[]":
                    case "object[]":
                    case "int[]":
                    case "double[]":
                    case "byte[]":
                    case "GObject":
                    case "Image":
                    case "string":
                        return $"{type} {name} = null";
                    case "int":
                    case "double":
                    case "bool":
                        return $"{type}? {name} = null";
                    default:
                        throw new Exception("Unsupported type: " + type);
                }
            }

            string CheckNullable(string type, string name)
            {
                switch (type)
                {
                    case "Image[]":
                    case "object[]":
                    case "int[]":
                    case "double[]":
                    case "byte[]":
                        return $"{name} != null && {name}.Length > 0";
                    case "GObject":
                    case "string":
                        return $"{name} != null";
                    case "int":
                    case "double":
                    case "bool":
                        return $"{name}.HasValue";
                    case "Image":
                        return $"!({name} is null)";
                    default:
                        throw new Exception("Unsupported type: " + type);
                }
            }

            var result = new StringBuilder($"{indent}/// <summary>\n");

            var newOperationName = operationName.ToPascalCase();

            var description = op.GetDescription();
            result.AppendLine($"{indent}/// {description.FirstLetterToUpper()}")
                .AppendLine($"{indent}/// </summary>")
                .AppendLine($"{indent}/// <example>")
                .AppendLine($"{indent}/// <code>")
                .Append($"{indent}/// ");

            if (requiredOutput.Length == 1)
            {
                var name = requiredOutput[0];
                result.Append(
                    $"{GValue.GTypeToCSharp(op.GetTypeOf(name))} {SafeIdentifier(name).ToPascalCase().FirstLetterToLower()} = ");
            }
            else if (requiredOutput.Length > 1)
            {
                result.Append("var output = ");
            }

            result.Append(memberX ?? "NetVips.Image")
                .Append(
                    $".{newOperationName}({string.Join(", ", requiredInput.Select(x => SafeIdentifier(x).ToPascalCase().FirstLetterToLower()).ToArray())}");

            if (outParameters != null)
            {
                if (requiredInput.Length > 0)
                {
                    result.Append(", ");
                }

                result.Append(
                    $"{string.Join(", ", outParameters.Select(name => $"out var {SafeIdentifier(name).ToPascalCase().FirstLetterToLower()}").ToArray())}");
            }

            if (optionalInput.Length > 0)
            {
                if (requiredInput.Length > 0 || outParameters != null)
                {
                    result.Append(", ");
                }

                for (var i = 0; i < optionalInput.Length; i++)
                {
                    var optionalName = optionalInput[i];
                    result.Append(
                            $"{SafeIdentifier(optionalName).ToPascalCase().FirstLetterToLower()}: {GValue.GTypeToCSharp(op.GetTypeOf(optionalName))}")
                        .Append(i != optionalInput.Length - 1 ? ", " : "");
                }
            }

            result.AppendLine(");");

            result.AppendLine($"{indent}/// </code>")
                .AppendLine($"{indent}/// </example>");

            foreach (var requiredName in requiredInput)
            {
                result.AppendLine(
                    $"{indent}/// <param name=\"{requiredName.ToPascalCase().FirstLetterToLower()}\">{op.GetBlurb(requiredName)}</param>");
            }

            if (outParameters != null)
            {
                foreach (var outParameter in outParameters)
                {
                    result.AppendLine(
                        $"{indent}/// <param name=\"{outParameter.ToPascalCase().FirstLetterToLower()}\">{op.GetBlurb(outParameter)}</param>");
                }
            }

            foreach (var optionalName in optionalInput)
            {
                result.AppendLine(
                    $"{indent}/// <param name=\"{optionalName.ToPascalCase().FirstLetterToLower()}\">{op.GetBlurb(optionalName)}</param>");
            }

            string outputType;

            var outputTypes = requiredOutput.Select(name => GValue.GTypeToCSharp(op.GetTypeOf(name))).ToArray();
            if (outputTypes.Length == 1)
            {
                outputType = outputTypes[0];
            }
            else if (outputTypes.Length == 0)
            {
                outputType = "void";
            }
            else if (outputTypes.Any(o => o != outputTypes[0]))
            {
                outputType = $"{outputTypes[0]}[]";
            }
            else
            {
                outputType = "object[]";
            }

            string ToCref(string name) =>
                name.Equals("Image") || name.Equals("GObject") ? $"new <see cref=\"{name}\"/>" : name;

            if (outputType.Equals("void"))
            {
                result.Append(
                    $"{indent}/// <returns>None");
            }
            else if (outputType.EndsWith("[]"))
            {
                result.Append(
                    $"{indent}/// <returns>An array of {ToCref(outputType.Remove(outputType.Length - 2))}s");
            }
            else
            {
                result.Append($"{indent}/// <returns>A {ToCref(outputType)}");
            }

            result.AppendLine("</returns>")
                .Append($"{indent}public ")
                .Append(memberX == null ? "static " : "")
                .Append(outputType)
                .Append(
                    $" {newOperationName}({string.Join(", ", requiredInput.Select(name => $"{GValue.GTypeToCSharp(op.GetTypeOf(name))} {SafeIdentifier(name).ToPascalCase().FirstLetterToLower()}").ToArray())}");

            if (outParameters != null)
            {
                if (requiredInput.Length > 0)
                {
                    result.Append(", ");
                }

                result.Append(string.Join(", ",
                    outParameters.Select(name =>
                            $"out {GValue.GTypeToCSharp(op.GetTypeOf(name))} {SafeIdentifier(name).ToPascalCase().FirstLetterToLower()}")
                        .ToArray()));
            }

            if (optionalInput.Length > 0)
            {
                if (requiredInput.Length > 0 || outParameters != null)
                {
                    result.Append(", ");
                }

                result.Append(string.Join(", ",
                    optionalInput.Select(name =>
                            $"{ToNullable(GValue.GTypeToCSharp(op.GetTypeOf(name)), SafeIdentifier(name).ToPascalCase().FirstLetterToLower())}")
                        .ToArray()));
            }

            result.AppendLine(")")
                .AppendLine($"{indent}{{");

            if (optionalInput.Length > 0)
            {
                result.AppendLine($"{indent}    var options = new VOption();").AppendLine();

                foreach (var optionalName in optionalInput)
                {
                    var safeIdentifier = SafeIdentifier(optionalName).ToPascalCase().FirstLetterToLower();

                    result.Append($"{indent}    if (")
                        .Append(CheckNullable(GValue.GTypeToCSharp(op.GetTypeOf(optionalName)), safeIdentifier))
                        .AppendLine(")")
                        .AppendLine($"{indent}    {{")
                        .AppendLine($"{indent}        options.Add(\"{optionalName}\", {safeIdentifier});")
                        .AppendLine($"{indent}    }}")
                        .AppendLine();
                }
            }

            if (outParameters != null)
            {
                if (optionalInput.Length > 0)
                {
                    foreach (var outParameterName in outParameters)
                    {
                        result.AppendLine($"{indent}    options.Add(\"{outParameterName}\", true);");
                    }
                }
                else
                {
                    result.AppendLine($"{indent}    var optionalOutput = new VOption")
                        .AppendLine($"{indent}    {{");
                    for (var i = 0; i < outParameters.Length; i++)
                    {
                        var outParameterName = outParameters[i];
                        result.Append($"{indent}        {{\"{outParameterName}\", true}}")
                            .AppendLine(i != outParameters.Length - 1 ? "," : "");
                    }

                    result.AppendLine($"{indent}    }};");
                }

                result.AppendLine()
                    .Append($"{indent}    var results = ")
                    .Append(memberX == null ? "Operation" : "this")
                    .Append($".Call(\"{operationName}\"")
                    .Append(optionalInput.Length > 0 ? ", options" : ", optionalOutput");
            }
            else
            {
                result.Append($"{indent}    ");
                if (outputType != "void")
                {
                    result.Append("return ");
                }

                result.Append(memberX == null ? "Operation" : "this")
                    .Append($".Call(\"{operationName}\"");
                if (optionalInput.Length > 0)
                {
                    result.Append(", options");
                }
            }

            if (requiredInput.Length > 0)
            {
                result.Append(", ");

                // Co-variant array conversion from Image[] to object[] can cause run-time exception on write operation.
                // So we need to wrap the image array into a object array.
                var needToWrap = requiredInput.Length == 1 &&
                                 GValue.GTypeToCSharp(op.GetTypeOf(requiredInput[0])).Equals("Image[]");
                if (needToWrap)
                {
                    result.Append("new object[] {");
                }

                result.Append(string.Join(", ",
                    requiredInput.Select(x => SafeIdentifier(x).ToPascalCase().FirstLetterToLower()).ToArray()));

                if (needToWrap)
                {
                    result.Append("}");
                }
            }

            result.Append(")");

            if (outParameters != null)
            {
                result.AppendLine(" as object[];");
                if (outputType != "void")
                {
                    result.Append($"{indent}    var finalResult = results?[0]")
                        .Append(SafeCast(outputType));
                }
            }
            else
            {
                result.Append(SafeCast(outputType))
                    .AppendLine();
            }

            if (outParameters != null)
            {
                result.AppendLine()
                    .AppendLine($"{indent}    var opts = results?[1] as VOption;");
                for (var i = 0; i < outParameters.Length; i++)
                {
                    var outParameter = outParameters[i];
                    result.Append(
                            $"{indent}    {SafeIdentifier(outParameter).ToPascalCase().FirstLetterToLower()} = opts?[\"{outParameter}\"]")
                        .AppendLine(SafeCast(GValue.GTypeToCSharp(op.GetTypeOf(outParameter)), $"out{i + 1}"));
                }

                if (outputType != "void")
                {
                    result.AppendLine()
                        .Append($"{indent}    return finalResult;")
                        .AppendLine();
                }
            }

            result.Append($"{indent}}}")
                .AppendLine();

            var optionalOutput = arguments.Where(x =>
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_OUTPUT) != 0 &&
                    (x.Value & Internal.Enums.VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) == 0)
                .Select(x => x.Key)
                .ToArray();

            // Create method overloading if necessary
            if (optionalOutput.Length > 0 && outParameters == null)
            {
                result.AppendLine()
                    .Append(GenerateFunction(operationName, indent, new[] {optionalOutput[0]}));
            }
            else if (outParameters != null && outParameters.Length != optionalOutput.Length)
            {
                result.AppendLine()
                    .Append(GenerateFunction(operationName, indent,
                        optionalOutput.Take(outParameters.Length + 1).ToArray()));
            }

            return result.ToString();
        }

        private static void AddNickname(ulong gtype, ICollection<string> allNickNames)
        {
            var nickname = Base.NicknameFind(gtype);
            allNickNames.Add(nickname);

            Base.TypeMap(gtype, (gtype2, a2, b2) =>
            {
                AddNickname(gtype2, allNickNames);

                return IntPtr.Zero;
            });
        }

        /// <summary>
        /// Generate the 'Image.Generated.cs' file.
        /// </summary>
        /// <remarks>
        /// This is used to generate the 'Image.Generated.cs' file (<see cref="Image"/>).
        /// Use it with something like:
        /// 
        ///     File.WriteAllText("Image.Generated.cs", Operation.GenerateImageClass());
        /// </remarks>
        /// <returns></returns>
        public static string GenerateImageClass(string indent = "        ")
        {
            // generate list of all nicknames we can generate docstrings for
            var allNickNames = new List<string>();

            Base.TypeMap(Base.TypeFromName("VipsOperation"), (gtype, a, b) =>
            {
                AddNickname(gtype, allNickNames);

                return IntPtr.Zero;
            });

            // Sort
            allNickNames.Sort();

            // Filter duplicates
            allNickNames = allNickNames.Distinct().ToList();

            // remove operations we have to wrap by hand
            var exclude = new[]
            {
                "scale",
                "ifthenelse",
                "bandjoin",
                "bandrank",
                "composite"
            };

            allNickNames = allNickNames.Where(x => !exclude.Contains(x)).ToList();

            const string preamble = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     libvips version: {0}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------";

            var stringBuilder =
                new StringBuilder(string.Format(preamble, $"{Base.Version(0)}.{Base.Version(1)}.{Base.Version(2)}"));
            stringBuilder.AppendLine()
                .AppendLine()
                .AppendLine("namespace NetVips")
                .AppendLine("{")
                .AppendLine("    public sealed partial class Image")
                .AppendLine("    {")
                .AppendLine($"{indent}#region auto-generated functions")
                .AppendLine();
            foreach (var nickname in allNickNames)
            {
                try
                {
                    stringBuilder.AppendLine(GenerateFunction(nickname, indent));
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            stringBuilder.AppendLine($"{indent}#endregion")
                .AppendLine()
                .AppendLine($"{indent}#region auto-generated properties")
                .AppendLine();

            var tmpFile = Image.NewTempFile("%s.v");
            var allProperties = tmpFile.GetFields();
            foreach (var property in allProperties)
            {
                var type = GValue.GTypeToCSharp(tmpFile.GetTypeOf(property));
                stringBuilder.AppendLine($"{indent}/// <summary>")
                    .AppendLine($"{indent}/// {tmpFile.GetBlurb(property)}")
                    .AppendLine($"{indent}/// </summary>")
                    .AppendLine($"{indent}public {type} {property.ToPascalCase()} => ({type})Get(\"{property}\");")
                    .AppendLine();
            }

            stringBuilder.AppendLine($"{indent}#endregion")
                .AppendLine("    }")
                .AppendLine("}");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Set the maximum number of operations libvips will cache.
        /// </summary>
        /// <param name="max"></param>
        public static void VipsCacheSetMax(int max)
        {
            VipsOperation.VipsCacheSetMax(max);
        }

        /// <summary>
        /// Limit the operation cache by memory use.
        /// </summary>
        /// <param name="maxMem"></param>
        public static void VipsCacheSetMaxMem(ulong maxMem)
        {
            VipsOperation.VipsCacheSetMaxMem(maxMem);
        }

        /// <summary>
        /// Limit the operation cache by number of open files.
        /// </summary>
        /// <param name="maxFiles"></param>
        public static void VipsCacheSetMaxFiles(int maxFiles)
        {
            VipsOperation.VipsCacheSetMaxFiles(maxFiles);
        }

        /// <summary>
        /// Turn on libvips cache tracing.
        /// </summary>
        /// <param name="trace"></param>
        public static void VipsCacheSetTrace(int trace)
        {
            VipsOperation.VipsCacheSetTrace(trace);
        }
    }
}