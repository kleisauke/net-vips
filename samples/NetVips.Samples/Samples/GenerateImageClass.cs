namespace NetVips.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    // TODO(kleisauke): This class should probably be refactored, although it does its job.
    public class GenerateImageClass : ISample
    {
        public string Name => "Generate image class";
        public string Category => "Internal";

        private readonly Dictionary<IntPtr, string> _gTypeToCSharpDict = new Dictionary<IntPtr, string>
        {
            {GValue.GBoolType, "bool"},
            {GValue.GIntType, "int"},
            {GValue.GUint64Type, "ulong"},
            //{GValue.GEnumType, "string"}, // Checked below
            //{GValue.GFlagsType, "int"}, // Checked below
            {GValue.GDoubleType, "double"},
            {GValue.GStrType, "string"},
            {GValue.GObjectType, "GObject"},
            {GValue.ImageType, "Image"},
            {GValue.ArrayIntType, "int[]"},
            {GValue.ArrayDoubleType, "double[]"},
            {GValue.ArrayImageType, "Image[]"},
            {GValue.RefStrType, "string"},
            {GValue.BlobType, "byte[]"}
        };

        private readonly List<string> _allNickNames = NetVips.GetOperations();

        /// <summary>
        /// The fundamental type for VipsFailOn. See <see cref="Enums.FailOn"/>.
        /// </summary>
        public static readonly IntPtr FailOnType = NetVips.TypeFromName("VipsFailOn");

        /// <summary>
        /// The fundamental type for VipsForeignKeep. See <see cref="Enums.ForeignKeep"/>.
        /// </summary>
        public static readonly IntPtr ForeignKeepType = NetVips.TypeFromName("VipsForeignKeep");

        public GenerateImageClass()
        {
            // Classes
            _gTypeToCSharpDict.Add(GValue.SourceType, "Source");
            _gTypeToCSharpDict.Add(GValue.TargetType, "Target");

            // Enums
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsAccess"), "Enums.Access");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsAlign"), "Enums.Align");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsAngle"), "Enums.Angle");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsAngle45"), "Enums.Angle45");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsBandFormat"), "Enums.BandFormat");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsBlendMode"), "Enums.BlendMode");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsCoding"), "Enums.Coding");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsCombine"), "Enums.Combine");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsCombineMode"), "Enums.CombineMode");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsCompassDirection"), "Enums.CompassDirection");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsDemandStyle"), "Enums.DemandStyle");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsDirection"), "Enums.Direction");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsExtend"), "Enums.Extend");
            if (FailOnType != IntPtr.Zero)
            {
                _gTypeToCSharpDict.Add(FailOnType, "Enums.FailOn");
            }

            if (NetVips.TypeFind("VipsOperation", "dzsave") != IntPtr.Zero)
            {
                _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignDzContainer"), "Enums.ForeignDzContainer");
                _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignDzLayout"), "Enums.ForeignDzLayout");
            }

            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignDzDepth"), "Enums.ForeignDzDepth");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignHeifCompression"), "Enums.ForeignHeifCompression");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignPpmFormat"), "Enums.ForeignPpmFormat");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignSubsample"), "Enums.ForeignSubsample");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignTiffCompression"), "Enums.ForeignTiffCompression");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignTiffPredictor"), "Enums.ForeignTiffPredictor");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignTiffResunit"), "Enums.ForeignTiffResunit");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignWebpPreset"), "Enums.ForeignWebpPreset");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsIntent"), "Enums.Intent");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsInteresting"), "Enums.Interesting");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsInterpretation"), "Enums.Interpretation");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsKernel"), "Enums.Kernel");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsOperationBoolean"), "Enums.OperationBoolean");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsOperationComplex"), "Enums.OperationComplex");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsOperationComplex2"), "Enums.OperationComplex2");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsOperationComplexget"), "Enums.OperationComplexget");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsOperationMath"), "Enums.OperationMath");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsOperationMath2"), "Enums.OperationMath2");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsOperationMorphology"), "Enums.OperationMorphology");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsOperationRelational"), "Enums.OperationRelational");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsOperationRound"), "Enums.OperationRound");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsPCS"), "Enums.PCS");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsPrecision"), "Enums.Precision");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsRegionShrink"), "Enums.RegionShrink");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsSize"), "Enums.Size");
            if (NetVips.AtLeastLibvips(8, 14))
            {
                _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsTextWrap"), "Enums.TextWrap");
                _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignHeifEncoder"), "Enums.ForeignHeifEncoder");
            }

            // Flags
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignFlags"), "Enums.ForeignFlags");
            _gTypeToCSharpDict.Add(NetVips.TypeFromName("VipsForeignPngFilter"), "Enums.ForeignPngFilter");
            if (ForeignKeepType != IntPtr.Zero)
            {
                _gTypeToCSharpDict.Add(ForeignKeepType, "Enums.ForeignKeep");
            }
        }

        /// <summary>
        /// Map a GType to the name of the C# type we use to represent it.
        /// </summary>
        /// <param name="name">The GType identifier.</param>
        /// <param name="gtype">The GType to map.</param>
        /// <returns>The C# type we use to represent it.</returns>
        private string GTypeToCSharp(string name, IntPtr gtype)
        {
            if (_gTypeToCSharpDict.ContainsKey(gtype))
            {
                return _gTypeToCSharpDict[gtype];
            }

            var fundamental = NetVips.FundamentalType(gtype);

            if (_gTypeToCSharpDict.ContainsKey(fundamental))
            {
                return _gTypeToCSharpDict[fundamental];
            }

            throw new Exception($"Unsupported type: {gtype} name: {name}");
        }

        /// <summary>
        /// Make a C#-style docstring + function declaration.
        /// </summary>
        /// <remarks>
        /// This is used to generate the functions in <see cref="Image"/>.
        /// </remarks>
        /// <param name="operationName">Operation name.</param>
        /// <param name="indent">Indentation level.</param>
        /// <param name="mutable">Generate <see cref="MutableImage"/>.</param>
        /// <param name="outParameters">The out parameters of this function.</param>
        /// <returns>A C#-style docstring + function declaration.</returns>
        private string GenerateFunction(string operationName, string indent = "        ",
            bool mutable = false, IReadOnlyList<Introspect.Argument> outParameters = null)
        {
            using var op = Operation.NewFromName(operationName);
            if ((op.GetFlags() & Enums.OperationFlags.DEPRECATED) != 0)
            {
                throw new ArgumentException($"No such operator. Operator \"{operationName}\" is deprecated");
            }

            var intro = Introspect.Get(operationName);

            // we are only interested in non-deprecated args
            var optionalInput = intro.OptionalInput
                .Where(arg => (arg.Value.Flags & Enums.ArgumentFlags.DEPRECATED) == 0)
                .Select(x => x.Value)
                .ToArray();
            var optionalOutput = intro.OptionalOutput
                .Where(arg => (arg.Value.Flags & Enums.ArgumentFlags.DEPRECATED) == 0)
                .Select(x => x.Value)
                .ToArray();

            // these are always non-deprecated
            var requiredInput = intro.RequiredInput.ToArray();
            var requiredOutput = intro.RequiredOutput.ToArray();

            if (mutable ^ intro.Mutable)
            {
                throw new ArgumentException(
                    $"Cannot generate \"{operationName}\" as this is a {(intro.Mutable ? string.Empty : "non-")}mutable operation.");
            }

            string[] reservedKeywords =
            {
                "in", "ref", "out", "ushort"
            };

            string SafeIdentifier(string name) =>
                reservedKeywords.Contains(name)
                    ? $"@{name}"
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
                    case "bool":
                        return $" is {type} {name} && {name};";
                    case "int":
                        return $" is {type} {name} ? {name} : 0;";
                    case "ulong":
                        return $" is {type} {name} ? {name} : 0ul;";
                    case "double":
                        return $" is {type} {name} ? {name} : 0d;";
                    case "string":
                        return $" is {type} {name} ? {name} : null;";
                    default:
                        return ";";
                }
            }

            string ExplicitCast(string type)
            {
                return type switch
                {
                    { } enumString when enumString.StartsWith("Enums.") => $"({type})",
                    _ => string.Empty
                };
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
                    case "bool":
                    case "int":
                    case "ulong":
                    case "double":
                    case { } enumString when enumString.StartsWith("Enums."):
                        return $"{type}? {name} = null";
                    default:
                        throw new Exception($"Unsupported type: {type}");
                }
            }

            var result = new StringBuilder($"{indent}/// <summary>\n");

            var newOperationName = operationName.ToPascalCase();

            var description = op.GetDescription();
            result.AppendLine($"{indent}/// {description.FirstLetterToUpper()}.")
                .AppendLine($"{indent}/// </summary>")
                .AppendLine($"{indent}/// <example>")
                .AppendLine($"{indent}/// <code language=\"lang-csharp\">")
                .Append($"{indent}/// ");

            if (requiredOutput.Length == 1)
            {
                var arg = requiredOutput.First();
                result.Append(
                    $"{(arg.Type == GValue.ImageType ? "using " : string.Empty)}{GTypeToCSharp(arg.Name, arg.Type)} {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()} = ");
            }
            else if (requiredOutput.Length > 1)
            {
                result.Append("var output = ");
            }
            else if (intro.Mutable && intro.MemberX.HasValue)
            {
                result.Append("image.Mutate(x => ");
            }

            if (intro.MemberX.HasValue)
            {
                result.Append(intro.Mutable ? "x" : intro.MemberX.Value.Name);
            }
            else
            {
                result.Append("NetVips.Image");
            }

            result.Append(
                $".{newOperationName}({string.Join(", ", requiredInput.Select(arg => SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()).ToArray())}");

            if (outParameters != null)
            {
                if (requiredInput.Length > 0)
                {
                    result.Append(", ");
                }

                result.Append(
                    $"{string.Join(", ", outParameters.Select(arg => $"out var {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}").ToArray())}");
            }

            if (optionalInput.Length > 0)
            {
                if (requiredInput.Length > 0 || outParameters != null)
                {
                    result.Append(", ");
                }

                for (var i = 0; i < optionalInput.Length; i++)
                {
                    var arg = optionalInput[i];
                    result.Append(
                            $"{SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}: {GTypeToCSharp(arg.Name, arg.Type)}")
                        .Append(i != optionalInput.Length - 1 ? ", " : string.Empty);
                }
            }

            result.Append(intro.Mutable ? ")" : string.Empty)
                .AppendLine(");");

            result.AppendLine($"{indent}/// </code>")
                .AppendLine($"{indent}/// </example>");

            foreach (var arg in requiredInput)
            {
                result.AppendLine(
                    $"{indent}/// <param name=\"{arg.Name.ToPascalCase().FirstLetterToLower()}\">{op.GetBlurb(arg.Name)}.</param>");
            }

            if (outParameters != null)
            {
                foreach (var outParameter in outParameters)
                {
                    result.AppendLine(
                        $"{indent}/// <param name=\"{outParameter.Name.ToPascalCase().FirstLetterToLower()}\">{op.GetBlurb(outParameter.Name)}.</param>");
                }
            }

            foreach (var arg in optionalInput)
            {
                result.AppendLine(
                    $"{indent}/// <param name=\"{arg.Name.ToPascalCase().FirstLetterToLower()}\">{op.GetBlurb(arg.Name)}.</param>");
            }

            string outputType;

            var outputTypes = requiredOutput.Select(arg => GTypeToCSharp(arg.Name, arg.Type)).ToArray();
            outputType = outputTypes.Length switch
            {
                0 => "void",
                1 => outputTypes[0],
                _ => outputTypes.Any(o => o != outputTypes[0]) ? $"{outputTypes[0]}[]" : "object[]"
            };

            string ToCref(string name) =>
                name.Equals("Image") || name.Equals("GObject") ? $"new <see cref=\"{name}\"/>" : name;

            if (outputType.EndsWith("[]"))
            {
                result.AppendLine(
                    $"{indent}/// <returns>An array of {ToCref(outputType.Remove(outputType.Length - 2))}s.</returns>");
            }
            else if (!outputType.Equals("void"))
            {
                result.AppendLine($"{indent}/// <returns>A {ToCref(outputType)}.</returns>");
            }

            result.Append($"{indent}public ")
                .Append(intro.MemberX.HasValue ? string.Empty : "static ")
                .Append(outputType);

            if (requiredInput.Length == 1 && outParameters == null && optionalInput.Length == 0 &&
                requiredInput[0].Type == GValue.ArrayImageType)
            {
                // We could safely use the params keyword
                result.Append(
                    $" {newOperationName}(params Image[] {SafeIdentifier(requiredInput[0].Name).ToPascalCase().FirstLetterToLower()}");
            }
            else
            {
                result.Append(
                    $" {newOperationName}(" +
                    string.Join(", ",
                        requiredInput.Select(arg =>
                                $"{GTypeToCSharp(arg.Name, arg.Type)} {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}")
                            .ToArray()));
            }

            if (outParameters != null)
            {
                if (requiredInput.Length > 0)
                {
                    result.Append(", ");
                }

                result.Append(string.Join(", ",
                    outParameters.Select(arg =>
                            $"out {GTypeToCSharp(arg.Name, arg.Type)} {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}")
                        .ToArray()));
            }

            if (optionalInput.Length > 0)
            {
                if (requiredInput.Length > 0 || outParameters != null)
                {
                    result.Append(", ");
                }

                result.Append(string.Join(", ",
                    optionalInput.Select(arg =>
                            $"{ToNullable(GTypeToCSharp(arg.Name, arg.Type), SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower())}")
                        .ToArray()));
            }

            result.AppendLine(")")
                .AppendLine($"{indent}{{");

            if (optionalInput.Length > 0)
            {
                result.AppendLine($"{indent}    var options = new VOption();").AppendLine();

                foreach (var arg in optionalInput)
                {
                    var safeIdentifier = SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower();
                    var optionsName = safeIdentifier == arg.Name
                        ? $"nameof({arg.Name})"
                        : $"\"{arg.Name}\"";

                    if (operationName.StartsWith("dzsave") && arg.Name == "imagename")
                    {
                        result.AppendLine(
                            $"{indent}    options.AddIfPresent(NetVips.AtLeastLibvips(8, 15) ? {optionsName} : \"basename\", {safeIdentifier});");
                    }
                    else if (arg.Type == FailOnType)
                    {
                        result.AppendLine($"{indent}    options.AddFailOn({safeIdentifier});");
                    }
                    else if (arg.Type == ForeignKeepType)
                    {
                        result.Append($"{indent}    options.AddForeignKeep({safeIdentifier}");
                        result.AppendLine(operationName.StartsWith("dzsave") ? ", true);" : ");");
                    }
                    else
                    {
                        result.AppendLine($"{indent}    options.AddIfPresent({optionsName}, {safeIdentifier});");
                    }
                }

                result.AppendLine();
            }

            if (outParameters != null)
            {
                if (optionalInput.Length > 0)
                {
                    foreach (var arg in outParameters)
                    {
                        result.AppendLine($"{indent}    options.Add(\"{arg.Name}\", true);");
                    }
                }
                else
                {
                    result.AppendLine($"{indent}    var optionalOutput = new VOption")
                        .AppendLine($"{indent}    {{");
                    for (var i = 0; i < outParameters.Count; i++)
                    {
                        var arg = outParameters[i];
                        result.Append($"{indent}        {{\"{arg.Name}\", true}}")
                            .AppendLine(i != outParameters.Count - 1 ? "," : string.Empty);
                    }

                    result.AppendLine($"{indent}    }};");
                }

                result.AppendLine()
                    .Append($"{indent}    var results = ")
                    .Append(intro.MemberX.HasValue ? "this" : "Operation")
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

                result.Append(intro.MemberX.HasValue ? "this" : "Operation")
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
                                 GTypeToCSharp(requiredInput[0].Name, requiredInput[0].Type).Equals("Image[]");
                if (needToWrap)
                {
                    result.Append("new object[] { ");
                }

                result.Append(string.Join(", ",
                    requiredInput.Select(arg => SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower())
                        .ToArray()));

                if (needToWrap)
                {
                    result.Append(" }");
                }
            }

            result.Append(')');

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
                for (var i = 0; i < outParameters.Count; i++)
                {
                    var arg = outParameters[i];
                    result.Append(
                            $"{indent}    {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()} = {ExplicitCast(GTypeToCSharp(arg.Name, arg.Type))}opts?[\"{arg.Name}\"]")
                        .AppendLine(SafeCast(GTypeToCSharp(arg.Name, arg.Type), $"out{i + 1}"));
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

            var firstArgType = requiredInput.Length > 0 ? op.GetTypeOf(requiredInput[0].Name) : IntPtr.Zero;

            // Create stream overload if necessary
            if (firstArgType == GValue.SourceType || firstArgType == GValue.TargetType)
            {
                var replace = firstArgType == GValue.SourceType ? "source" : "target";
                requiredInput = requiredInput.Skip(1).ToArray();
                var oldOperationName = newOperationName;
                newOperationName = newOperationName.Replace(replace.FirstLetterToUpper(), "Stream");

                result.AppendLine()
                    .AppendLine($"{indent}/// <summary>")
                    .AppendLine($"{indent}/// {description.Replace(replace, "stream").FirstLetterToUpper()}.")
                    .AppendLine($"{indent}/// </summary>")
                    .AppendLine($"{indent}/// <example>")
                    .AppendLine($"{indent}/// <code language=\"lang-csharp\">")
                    .Append($"{indent}/// ");

                if (requiredOutput.Length == 1)
                {
                    var arg = requiredOutput.First();
                    result.Append(
                        $"{(arg.Type == GValue.ImageType ? "using " : string.Empty)}{GTypeToCSharp(arg.Name, arg.Type)} {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()} = ");
                }
                else if (requiredOutput.Length > 1)
                {
                    result.Append("var output = ");
                }

                result.Append(intro.MemberX.HasValue ? intro.MemberX.Value.Name : "NetVips.Image")
                    .Append(
                        $".{newOperationName}(stream, {string.Join(", ", requiredInput.Select(arg => SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()).ToArray())}");

                if (outParameters != null)
                {
                    if (requiredInput.Length > 0)
                    {
                        result.Append(", ");
                    }

                    result.Append(
                        $"{string.Join(", ", outParameters.Select(arg => $"out var {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}").ToArray())}");
                }

                if (optionalInput.Length > 0)
                {
                    if (requiredInput.Length > 0 || outParameters != null)
                    {
                        result.Append(", ");
                    }

                    for (var i = 0; i < optionalInput.Length; i++)
                    {
                        var arg = optionalInput[i];
                        result.Append(
                                $"{SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}: {GTypeToCSharp(arg.Name, arg.Type)}")
                            .Append(i != optionalInput.Length - 1 ? ", " : string.Empty);
                    }
                }

                result.AppendLine(");");

                result.AppendLine($"{indent}/// </code>")
                    .AppendLine($"{indent}/// </example>");

                result.AppendLine(
                    $"{indent}/// <param name=\"stream\">Stream to {(firstArgType == GValue.SourceType ? "load from" : "save to")}.</param>");

                if (outParameters != null)
                {
                    foreach (var outParameter in outParameters)
                    {
                        result.AppendLine(
                            $"{indent}/// <param name=\"{outParameter.Name.ToPascalCase().FirstLetterToLower()}\">{op.GetBlurb(outParameter.Name)}.</param>");
                    }
                }

                foreach (var arg in requiredInput)
                {
                    result.AppendLine(
                        $"{indent}/// <param name=\"{arg.Name.ToPascalCase().FirstLetterToLower()}\">{op.GetBlurb(arg.Name)}.</param>");
                }

                foreach (var arg in optionalInput)
                {
                    result.AppendLine(
                        $"{indent}/// <param name=\"{arg.Name.ToPascalCase().FirstLetterToLower()}\">{op.GetBlurb(arg.Name)}.</param>");
                }

                outputType = outputTypes.Length switch
                {
                    0 => "void",
                    1 => outputTypes[0],
                    _ => outputTypes.Any(o => o != outputTypes[0]) ? $"{outputTypes[0]}[]" : "object[]"
                };

                if (!outputType.Equals("void"))
                {
                    result.AppendLine($"{indent}/// <returns>A {ToCref(outputType)}.</returns>");
                }

                result.Append($"{indent}public ")
                    .Append(intro.MemberX.HasValue ? string.Empty : "static ")
                    .Append(outputType);

                result.Append(
                    $" {newOperationName}(Stream stream, " +
                    string.Join(", ",
                        requiredInput.Select(arg =>
                                $"{GTypeToCSharp(arg.Name, arg.Type)} {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}")
                            .ToArray()));

                if (outParameters != null)
                {
                    if (requiredInput.Length > 0)
                    {
                        result.Append(", ");
                    }

                    result.Append(string.Join(", ",
                        outParameters.Select(arg =>
                                $"out {GTypeToCSharp(arg.Name, arg.Type)} {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}")
                            .ToArray()));
                }

                if (optionalInput.Length > 0)
                {
                    if (requiredInput.Length > 0 || outParameters != null)
                    {
                        result.Append(", ");
                    }

                    result.Append(string.Join(", ",
                        optionalInput.Select(arg =>
                                $"{ToNullable(GTypeToCSharp(arg.Name, arg.Type), SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower())}")
                            .ToArray()));
                }

                result.AppendLine(")")
                    .AppendLine($"{indent}{{")
                    .AppendLine(firstArgType == GValue.SourceType
                        ? $"{indent}    var source = SourceStream.NewFromStream(stream);"
                        : $"{indent}    using var target = TargetStream.NewFromStream(stream);")
                    .Append($"{indent}    ")
                    .Append(firstArgType == GValue.SourceType ? "var image = " : string.Empty)
                    .Append($"{oldOperationName}(")
                    .Append(firstArgType == GValue.SourceType ? "source" : "target")
                    .Append(", ")
                    .Append(string.Join(", ",
                        requiredInput.Select(arg => $"{SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}")
                            .ToArray()));

                if (outParameters != null)
                {
                    if (requiredInput.Length > 0)
                    {
                        result.Append(", ");
                    }

                    result.Append(string.Join(", ",
                        outParameters.Select(arg =>
                                $"out {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}")
                            .ToArray()));
                }

                if (optionalInput.Length > 0)
                {
                    if (requiredInput.Length > 0 || outParameters != null)
                    {
                        result.Append(", ");
                    }

                    result.Append(string.Join(", ",
                        optionalInput.Select(arg =>
                                $"{SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}")
                            .ToArray()));
                }

                result.AppendLine(");");

                if (firstArgType == GValue.SourceType)
                {
                    result.AppendLine()
                        .AppendLine($"{indent}    image.OnPostClose += () => source.Dispose();")
                        .AppendLine()
                        .AppendLine($"{indent}    return image;");
                }

                result
                    .AppendLine($"{indent}}}");
            }

            // Create method overloading if necessary
            if (optionalOutput.Length > 0 && outParameters == null)
            {
                result.AppendLine()
                    .Append(GenerateFunction(operationName, indent, mutable, new[] { optionalOutput[0] }));
            }
            else if (outParameters != null && outParameters.Count != optionalOutput.Length)
            {
                result.AppendLine()
                    .Append(GenerateFunction(operationName, indent, mutable,
                        optionalOutput.Take(outParameters.Count + 1).ToArray()));
            }

            return result.ToString();
        }

        /// <summary>
        /// Generate the `Image.Generated.cs` file.
        /// </summary>
        /// <remarks>
        /// This is used to generate the `Image.Generated.cs` file (<see cref="Image"/>).
        /// </remarks>
        /// <param name="indent">Indentation level.</param>
        /// <returns>The `Image.Generated.cs` as string.</returns>
        private string Generate(string indent = "        ")
        {
            // remove operations we have to wrap by hand
            var exclude = new[]
            {
                "addalpha",
                "bandjoin",
                "bandrank",
                "case",
                "composite",
                "ifthenelse",
                "scale"
            };

            // get the list of all nicknames we can generate docstrings for.
            var allNickNames = _allNickNames.Where(x => !exclude.Contains(x)).ToList();

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
                new StringBuilder(string.Format(preamble,
                    $"{NetVips.Version(0)}.{NetVips.Version(1)}.{NetVips.Version(2)}"));
            stringBuilder.AppendLine()
                .AppendLine()
                .AppendLine("namespace NetVips")
                .AppendLine("{")
                .AppendLine("    using System.IO;")
                .AppendLine()
                .AppendLine("    public partial class Image")
                .AppendLine("    {")
                .AppendLine($"{indent}#region auto-generated functions")
                .AppendLine();
            foreach (var nickname in allNickNames)
            {
                try
                {
                    stringBuilder.AppendLine(GenerateFunction(nickname, indent));
                }
                catch (ArgumentException)
                {
                    // ignore
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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
                var type = GTypeToCSharp(property, tmpFile.GetTypeOf(property));
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
        /// Generate the `MutableImage.Generated.cs` file.
        /// </summary>
        /// <remarks>
        /// This is used to generate the `MutableImage.Generated.cs` file (<see cref="MutableImage"/>).
        /// </remarks>
        /// <param name="indent">Indentation level.</param>
        /// <returns>The `MutableImage.Generated.cs` as string.</returns>
        private string GenerateMutable(string indent = "        ")
        {
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
                new StringBuilder(string.Format(preamble,
                    $"{NetVips.Version(0)}.{NetVips.Version(1)}.{NetVips.Version(2)}"));
            stringBuilder.AppendLine()
                .AppendLine()
                .AppendLine("namespace NetVips")
                .AppendLine("{")
                .AppendLine("    public sealed partial class MutableImage")
                .AppendLine("    {")
                .AppendLine($"{indent}#region auto-generated functions")
                .AppendLine();
            foreach (var nickname in _allNickNames)
            {
                try
                {
                    stringBuilder.AppendLine(GenerateFunction(nickname, indent, true));
                }
                catch (ArgumentException)
                {
                    // ignore
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            stringBuilder.AppendLine($"{indent}#endregion")
                .AppendLine("    }")
                .AppendLine("}");

            return stringBuilder.ToString();
        }

        public void Execute(string[] args)
        {
            File.WriteAllText("Image.Generated.cs", Generate());
            File.WriteAllText("MutableImage.Generated.cs", GenerateMutable());

            Console.WriteLine("See *.Generated.cs");
        }
    }
}