using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetVips.AutoGen;
using NLog;

namespace NetVips
{
    public class Operation : VipsObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private VipsOperation VOperation;

        public Operation(VipsOperation vOperation) : base(vOperation.ParentInstance)
        {
            // logger.Debug($"Image: pointer = {Pointer}");
            VOperation = vOperation;
        }

        /// <summary>
        /// search an array with a predicate, recursing into subarrays as we see them
        /// used to find the match_image for an operation
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static T FindInside<T>(IEnumerable<object> thing, Func<object, bool> predicate) where T : Image
        {
            foreach (var item in thing)
            {
                var result = FindInside<T>(item, predicate);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static T FindInside<T>(object thing, Func<object, bool> predicate) where T : Image
        {
            if (!(thing is IEnumerable enumerable))
            {
                return predicate(thing) ? thing as T : null;
            }

            foreach (var item in enumerable)
            {
                var result = FindInside<T>(item, predicate);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static Operation NewFromName(string operationName)
        {
            var vop = operation.VipsOperationNew(operationName);
            if (vop == null)
            {
                throw new Exception($"no such operation {operationName}");
            }

            return new Operation(vop);
        }

        public void Set(string name, int flags, Image matchImage, object value)
        {
            // logger.Debug($"Operation.Set: name = {name}, flags = {flags}, " +
            //            $"matchImage = {matchImage} value = {value}");

            // if the object wants an image and we have a constant, _imageize it
            //
            // if the object wants an image array, _imageize any constants in the
            // array
            if (matchImage != null)
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
            if ((flags & (int) VipsArgumentFlags.VIPS_ARGUMENT_MODIFY) != 0)
            {
                //logger.Debug($"copying MODIFY arg {name}")
                // make sure we have a unique copy
                value = (value as Image)?.Copy().CopyMemory();
            }

            Set(name, value);
        }

        public VipsOperationFlags GetFlags()
        {
            return operation.VipsOperationGetFlags(VOperation);
        }

        // this is slow ... call as little as possible
        public IDictionary<string, VipsArgumentFlags> GetArgs()
        {
            var args = new Dictionary<string, VipsArgumentFlags>();
            VipsArgumentMapFn addConstruct = (self, pspec, argumentClass, argumentInstance, a, b) =>
            {
                var flags = VipsArgumentClass.__CreateInstance(argumentClass).Flags;
                if ((flags & VipsArgumentFlags.VIPS_ARGUMENT_CONSTRUCT) != 0)
                {
                    var name = GParamSpec.__CreateInstance(pspec).Name;
                    // libvips uses '-' to separate parts of arg names, but we
                    // need '_' for C#
                    name = name.Replace("-", "_");

                    args.Add(name, flags);
                }

                return IntPtr.Zero;
            };

            @object.VipsArgumentMap(VObject, addConstruct, IntPtr.Zero, IntPtr.Zero);
            return args;
        }

        /// <summary>
        /// Call a libvips operation.
        /// </summary>
        /// <remarks>
        /// Use this method to call any libvips operation. For example:
        /// 
        /// black_image = netvips.Operation.call('black', 10, 10)
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
        /// black_image = netvips.Operation.call('black', 10, 10)
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
        /// black_image = netvips.Operation.call('black', 10, 10)
        /// 
        /// See the Introduction for notes on how this works.
        /// </remarks>
        /// <param name="operationName"></param>
        /// <param name="kwargs"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Call(string operationName, IDictionary<string, object> kwargs, params object[] args)
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
            var flagsFromName = new Dictionary<string, VipsArgumentFlags>();

            var nRequired = 0;
            foreach (var entry in arguments)
            {
                var name = entry.Key;
                var flag = entry.Value;

                flagsFromName[name] = flag;

                // count required input args
                if ((flag & VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (flag & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    (flag & VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
                {
                    nRequired++;
                }
            }

            // the final supplied argument can be a hash of options, or undefined
            if (nRequired != args.Length &&
                nRequired + 1 != args.Length)
            {
                throw new Exception(
                    $"unable to call {operationName}: {args.Length} arguments given, but {nRequired} required");
            }

            // the first image argument is the thing we expand constants to
            // match ... look inside tables for images, since we may be passing
            // an array of image as a single param
            var matchImage = FindInside<Image>(args, x => x is Image);

            // logger.Debug($"VipsOperation.call: match_image = {matchImage}");

            // set any string options before any args so they can't be
            // overridden
            if (stringOptions != null && !op.SetString(stringOptions as string))
            {
                throw new Exception($"unable to call {operationName}");
            }

            // set required and optional args
            int n = 0;
            foreach (var entry in arguments)
            {
                var name = entry.Key;
                var flag = entry.Value;

                if ((flag & VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (flag & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    (flag & VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
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
            var vop = operation.VipsCacheOperationBuild(op.VOperation);
            if (vop == null)
            {
                throw new Exception($"unable to call {operationName}");
            }

            op = new Operation(vop);

            // fetch required output args, plus modified input images
            var result = new List<object>();

            foreach (var entry in arguments)
            {
                var name = entry.Key;
                var flag = entry.Value;

                if ((flag & VipsArgumentFlags.VIPS_ARGUMENT_OUTPUT) != 0 &&
                    (flag & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    (flag & VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
                {
                    result.Add(op.Get(name));
                }

                if ((flag & VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (flag & VipsArgumentFlags.VIPS_ARGUMENT_MODIFY) != 0)
                {
                    result.Add(op.Get(name));
                }
            }

            // fetch optional output args
            var opts = new Dictionary<object, object>();

            if (kwargs != null)
            {
                foreach (var entry in kwargs)
                {
                    var name = entry.Key;

                    var flags = flagsFromName[name];
                    if ((flags & VipsArgumentFlags.VIPS_ARGUMENT_OUTPUT) != 0 &&
                        (flags & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) == 0 &&
                        (flags & VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
                    {
                        opts[name] = op.Get(name);
                    }
                }
            }

            @object.VipsObjectUnrefOutputs(op.VObject);
            if (opts.Count > 0)
            {
                result.Add(opts);
            }

            // logger.Debug($"VipsOperation.call: result = {result}");

            return result.Count == 0 ? null : (result.Count == 1 ? result[0] : result);
        }

        /// <summary>
        /// Make a C#-style docstring + function.
        /// </summary>
        /// <remarks>
        /// This is used to generate the functions in NetVips.Image (<see cref="Image"/>).
        /// </remarks>
        /// <param name="operationName"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static string GenerateFunction(string operationName, string indent = "        ")
        {
            var op = NewFromName(operationName);
            if ((op.GetFlags() & VipsOperationFlags.VIPS_OPERATION_DEPRECATED) != 0)
            {
                throw new Exception($"No such operator. Operator \"{operationName}\" is deprecated");
            }

            // we are only interested in non-deprecated args
            var arguments = op.GetArgs().Where(x => (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_DEPRECATED) == 0)
                .Select(x => new KeyValuePair<string, VipsArgumentFlags>(x.Key, x.Value))
                .ToDictionary(x => x.Key, x => x.Value);


            // find the first required input image arg, if any ... that will be self
            string memberX = null;
            foreach (var entry in arguments)
            {
                var name = entry.Key;
                var flag = entry.Value;

                if ((flag & VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                    (flag & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                    op.GetTypeOf(name) == GValue.ImageType)
                {
                    memberX = name;
                    break;
                }
            }

            string[] reservedKeywords =
            {
                "in", "ref", "out"
            };

            var requiredInput = arguments.Where(x => (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                                                     (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                                                     x.Key != memberX)
                .Select(x => x.Key)
                .ToArray();
            var optionalInput = arguments.Where(x => (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                                                     (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) == 0)
                .Select(x => x.Key)
                .ToArray();
            var requiredOutput = arguments.Where(x => (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_OUTPUT) != 0 &&
                                                      (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 ||
                                                      (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_INPUT) != 0 &&
                                                      (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) != 0 &&
                                                      (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_MODIFY) != 0)
                .Select(x => x.Key)
                .ToArray();
            var optionalOutput = arguments.Where(x => (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_OUTPUT) != 0 &&
                                                      (x.Value & VipsArgumentFlags.VIPS_ARGUMENT_REQUIRED) == 0)
                .Select(x => x.Key)
                .ToArray();

            Func<string, string> safeIdentifier = name =>
                reservedKeywords.Contains(name)
                    ? "@" + name
                    : name;

            var result = new StringBuilder($"{indent}/// <summary>\n");

            var newOperationName = operationName.ToCamelCase();

            var description = op.GetDescription();
            result.AppendLine($"{indent}/// {description.FirstLetterToUpper()}")
                .AppendLine($"{indent}/// </summary>")
                .AppendLine($"{indent}/// <example>")
                .AppendLine($"{indent}/// <code>")
                .AppendLine($"{indent}/// <![CDATA[")
                .Append($"{indent}/// ");

            if (requiredOutput.Length == 1)
            {
                var name = requiredOutput[0];
                result.Append(
                    $"{GValue.GTypeToCSharp(op.GetTypeOf(name))} {safeIdentifier(name).ToCamelCase().FirstLetterToLower()} = ");
            }

            result.Append(memberX ?? "NetVips.Image")
                .Append(
                    $".{newOperationName}({string.Join(", ", requiredInput.Select(x => safeIdentifier(x).ToCamelCase().FirstLetterToLower()).ToArray())}");

            if (optionalInput.Length > 0)
            {
                if (requiredInput.Length > 0)
                {
                    result.Append(", ");
                }

                result.Append("new Dictionary<string, object>\n")
                    .AppendLine($"{indent}/// {{");
                foreach (var optionalName in optionalInput)
                {
                    result.AppendLine(
                        $"{indent}///     {{\"{optionalName}\", {GValue.GTypeToCSharp(op.GetTypeOf(optionalName))}}}");
                }

                result.Append($"{indent}/// }}");
            }

            result.AppendLine(");");

            result.AppendLine($"{indent}/// ]]>")
                .AppendLine($"{indent}/// </code>")
                .AppendLine($"{indent}/// </example>");

            foreach (var requiredName in requiredInput)
            {
                result.AppendLine(
                    $"{indent}/// <param name=\"{requiredName.ToCamelCase().FirstLetterToLower()}\">{op.GetBlurb(requiredName)}</param>");
            }

            if (optionalInput.Length > 0)
            {
                result.AppendLine($"{indent}/// <param name=\"kwargs\">");
                foreach (var optionalName in optionalInput)
                {
                    result.AppendLine(
                        $"{indent}/// {optionalName} ({GValue.GTypeToCSharp(op.GetTypeOf(optionalName))}): {op.GetBlurb(optionalName)}");
                }

                result.AppendLine($"{indent}/// </param>");
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

            bool shouldOutputAsObject = optionalOutput.Length > 0 && outputType != "object[]";

            if (shouldOutputAsObject)
            {
                outputType += " or object[]";
            }

            result.AppendLine($"{indent}/// <returns>{outputType}</returns>")
                .Append($"{indent}public ")
                .Append(memberX == null ? "static " : "")
                .Append(shouldOutputAsObject ? "object" : outputType)
                .Append(
                    $" {newOperationName}({string.Join(", ", requiredInput.Select(name => $"{GValue.GTypeToCSharp(op.GetTypeOf(name))} {safeIdentifier(name).ToCamelCase().FirstLetterToLower()}").ToArray())}");
            if (optionalInput.Length > 0)
            {
                if (requiredInput.Length > 0)
                {
                    result.Append(", ");
                }

                result.Append("IDictionary<string, object> kwargs = null");
            }

            result.AppendLine(")")
                .AppendLine($"{indent}{{")
                .Append($"{indent}    ");

            if (outputType != "void")
            {
                result.Append("return ");
            }

            result.Append(memberX == null ? "Operation" : "this");
            result.Append($".Call(\"{operationName}\"");
            if (optionalInput.Length > 0)
            {
                result.Append(", kwargs");
            }

            if (requiredInput.Length > 0)
            {
                result.Append(
                    $", {string.Join(", ", requiredInput.Select(x => safeIdentifier(x).ToCamelCase().FirstLetterToLower()).ToArray())}");
            }

            result.Append(")");

            if (shouldOutputAsObject)
            {
                result.Append(";");
            }
            else
            {
                switch (outputType)
                {
                    case "GObject":
                    case "Image":
                    case "int[]":
                    case "double[]":
                    case "Image[]":
                    case "object[]":
                        result.Append($" as {outputType};");
                        break;
                    case "int":
                    case "double":
                        result.Append($" is {outputType} result ? result : 0;");
                        break;
                    case "bool":
                        result.Append($" is {outputType} result && result;");
                        break;
                    case "string":
                        result.Append($" is {outputType} result ? result : null;");
                        break;
                    default:
                        result.Append(";");
                        break;
                }
            }

            result.AppendLine()
                .Append($"{indent}}}");

            return result.ToString();
        }

        private static void AddNickname(ulong gtype, List<string> allNickNames)
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
        /// Generate all functions.
        /// </summary>
        /// <remarks>
        /// This is used to generate the functions in NetVips.Image (<see cref="Image"/>).  Use it
        /// with something like:
        /// 
        ///     File.WriteAllText("functions.txt", Operation.GenerateAllFunctions());
        /// 
        /// And copy-paste the file contents into Image.cs in the appropriate
        /// place.
        /// </remarks>
        /// <returns></returns>
        public static string GenerateAllFunctions()
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

            var functions = new StringBuilder();
            foreach (var nickname in allNickNames)
            {
                try
                {
                    functions.AppendLine(GenerateFunction(nickname)).AppendLine();
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            return functions.ToString();
        }

        /// <summary>
        /// Set the maximum number of operations libvips will cache.
        /// </summary>
        /// <param name="max"></param>
        public static void VipsCacheSetMax(int max)
        {
            operation.VipsCacheSetMax(max);
        }

        /// <summary>
        /// Limit the operation cache by memory use.
        /// </summary>
        /// <param name="maxMem"></param>
        public static void VipsCacheSetMaxMem(ulong maxMem)
        {
            operation.VipsCacheSetMaxMem(maxMem);
        }

        /// <summary>
        /// Limit the operation cache by number of open files.
        /// </summary>
        /// <param name="maxFiles"></param>
        public static void VipsCacheSetMaxFiles(int maxFiles)
        {
            operation.VipsCacheSetMaxFiles(maxFiles);
        }

        /// <summary>
        /// Turn on libvips cache tracing.
        /// </summary>
        /// <param name="trace"></param>
        public static void VipsCacheSetTrace(int trace)
        {
            operation.VipsCacheSetTrace(trace);
        }
    }
}