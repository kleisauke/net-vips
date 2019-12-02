namespace NetVips.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    public class GenerateImageClass : ISample
    {
        public string Name => "Generate image class";
        public string Category => "Internal";

        private readonly Dictionary<IntPtr, string> _gTypeToCSharpDict = new Dictionary<IntPtr, string>
        {
            {GValue.GBoolType, "bool"},
            {GValue.GIntType, "int"},
            {GValue.GUint64Type, "ulong"},
            {GValue.GEnumType, "string"},
            {GValue.GFlagsType, "int"},
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

        /// <summary>
        /// Map a GType to the name of the C# type we use to represent it.
        /// </summary>
        /// <param name="gtype">The GType to map.</param>
        /// <returns>The C# type we use to represent it.</returns>
        private string GTypeToCSharp(IntPtr gtype)
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

            return "object";
        }

        /// <summary>
        /// Make a C#-style docstring + function declaration.
        /// </summary>
        /// <remarks>
        /// This is used to generate the functions in <see cref="Image"/>.
        /// </remarks>
        /// <param name="operationName">Operation name.</param>
        /// <param name="indent">Indentation level.</param>
        /// <param name="outParameters">The out parameters of this function.</param>
        /// <returns>A C#-style docstring + function declaration.</returns>
        private string GenerateFunction(string operationName, string indent = "        ",
            IReadOnlyList<Introspect.Argument> outParameters = null)
        {
            var op = Operation.NewFromName(operationName);
            if ((op.GetFlags() & Enums.OperationFlags.DEPRECATED) != 0)
            {
                throw new Exception($"No such operator. Operator \"{operationName}\" is deprecated");
            }

            var intro = Introspect.Get(operationName);

            // we are only interested in non-deprecated args
            var optionalInput = intro.OptionalInput
                .Where(arg => (arg.Value.Flags & Enums.ArgumentFlags.DEPRECATED) == 0)
                .Select(x => x.Value)
                .ToList();

            var optionalOutput = intro.OptionalOutput
                .Where(arg => (arg.Value.Flags & Enums.ArgumentFlags.DEPRECATED) == 0)
                .Select(x => x.Value)
                .ToList();

            string[] reservedKeywords =
            {
                "in", "ref", "out", "ushort"
            };

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
                    case "Image":
                    case "string":
                        return $"{name} != null";
                    case "bool":
                    case "int":
                    case "ulong":
                    case "double":
                        return $"{name}.HasValue";
                    default:
                        throw new Exception("Unsupported type: " + type);
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

            if (intro.RequiredOutput.Count == 1)
            {
                var arg = intro.RequiredOutput.First();
                result.Append(
                    $"{GTypeToCSharp(arg.Type)} {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()} = ");
            }
            else if (intro.RequiredOutput.Count > 1)
            {
                result.Append("var output = ");
            }

            result.Append(intro.MemberX.HasValue ? intro.MemberX.Value.Name : "NetVips.Image")
                .Append(
                    $".{newOperationName}({string.Join(", ", intro.RequiredInput.Select(arg => SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()).ToArray())}");

            if (outParameters != null)
            {
                if (intro.RequiredInput.Count > 0)
                {
                    result.Append(", ");
                }

                result.Append(
                    $"{string.Join(", ", outParameters.Select(arg => $"out var {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}").ToArray())}");
            }

            if (optionalInput.Count > 0)
            {
                if (intro.RequiredInput.Count > 0 || outParameters != null)
                {
                    result.Append(", ");
                }

                for (var i = 0; i < optionalInput.Count; i++)
                {
                    var arg = optionalInput[i];
                    result.Append(
                            $"{SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}: {GTypeToCSharp(arg.Type)}")
                        .Append(i != optionalInput.Count - 1 ? ", " : string.Empty);
                }
            }

            result.AppendLine(");");

            result.AppendLine($"{indent}/// </code>")
                .AppendLine($"{indent}/// </example>");

            foreach (var arg in intro.RequiredInput)
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

            var outputTypes = intro.RequiredOutput.Select(arg => GTypeToCSharp(arg.Type)).ToArray();
            switch (outputTypes.Length)
            {
                case 0:
                    outputType = "void";
                    break;
                case 1:
                    outputType = outputTypes[0];
                    break;
                default:
                    outputType = outputTypes.Any(o => o != outputTypes[0]) ? $"{outputTypes[0]}[]" : "object[]";
                    break;
            }

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

            if (intro.RequiredInput.Count == 1 && outParameters == null && optionalInput.Count == 0 &&
                intro.RequiredInput[0].Type == GValue.ArrayImageType)
            {
                // We could safely use the params keyword
                result.Append(
                    $" {newOperationName}(params Image[] {SafeIdentifier(intro.RequiredInput[0].Name).ToPascalCase().FirstLetterToLower()}");
            }
            else
            {
                result.Append(
                    $" {newOperationName}(" +
                    string.Join(", ",
                        intro.RequiredInput.Select(arg =>
                                $"{GTypeToCSharp(arg.Type)} {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}")
                            .ToArray()));
            }

            if (outParameters != null)
            {
                if (intro.RequiredInput.Count > 0)
                {
                    result.Append(", ");
                }

                result.Append(string.Join(", ",
                    outParameters.Select(arg =>
                            $"out {GTypeToCSharp(arg.Type)} {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()}")
                        .ToArray()));
            }

            if (optionalInput.Count > 0)
            {
                if (intro.RequiredInput.Count > 0 || outParameters != null)
                {
                    result.Append(", ");
                }

                result.Append(string.Join(", ",
                    optionalInput.Select(arg =>
                            $"{ToNullable(GTypeToCSharp(arg.Type), SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower())}")
                        .ToArray()));
            }

            result.AppendLine(")")
                .AppendLine($"{indent}{{");

            if (optionalInput.Count > 0)
            {
                result.AppendLine($"{indent}    var options = new VOption();").AppendLine();

                foreach (var arg in optionalInput)
                {
                    var safeIdentifier = SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower();
                    var optionsName = safeIdentifier == arg.Name
                        ? "nameof(" + arg.Name + ")"
                        : "\"" + arg.Name + "\"";

                    result.Append($"{indent}    if (")
                        .Append(CheckNullable(GTypeToCSharp(arg.Type), safeIdentifier))
                        .AppendLine(")")
                        .AppendLine($"{indent}    {{")
                        .AppendLine($"{indent}        options.Add({optionsName}, {safeIdentifier});")
                        .AppendLine($"{indent}    }}")
                        .AppendLine();
                }
            }

            if (outParameters != null)
            {
                if (optionalInput.Count > 0)
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
                    .Append(optionalInput.Count > 0 ? ", options" : ", optionalOutput");
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
                if (optionalInput.Count > 0)
                {
                    result.Append(", options");
                }
            }

            if (intro.RequiredInput.Count > 0)
            {
                result.Append(", ");

                // Co-variant array conversion from Image[] to object[] can cause run-time exception on write operation.
                // So we need to wrap the image array into a object array.
                var needToWrap = intro.RequiredInput.Count == 1 &&
                                 GTypeToCSharp(intro.RequiredInput[0].Type).Equals("Image[]");
                if (needToWrap)
                {
                    result.Append("new object[] { ");
                }

                result.Append(string.Join(", ",
                    intro.RequiredInput.Select(arg => SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower())
                        .ToArray()));

                if (needToWrap)
                {
                    result.Append(" }");
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
                for (var i = 0; i < outParameters.Count; i++)
                {
                    var arg = outParameters[i];
                    result.Append(
                            $"{indent}    {SafeIdentifier(arg.Name).ToPascalCase().FirstLetterToLower()} = opts?[\"{arg.Name}\"]")
                        .AppendLine(SafeCast(GTypeToCSharp(arg.Type), $"out{i + 1}"));
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

            // Create method overloading if necessary
            if (optionalOutput.Count > 0 && outParameters == null)
            {
                result.AppendLine()
                    .Append(GenerateFunction(operationName, indent, new[] { optionalOutput.ElementAt(0) }));
            }
            else if (outParameters != null && outParameters.Count != optionalOutput.Count)
            {
                result.AppendLine()
                    .Append(GenerateFunction(operationName, indent,
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
            // get the list of all nicknames we can generate docstrings for.
            var allNickNames = NetVips.GetOperations();

            // remove operations we have to wrap by hand
            var exclude = new[]
            {
                "scale",
                "ifthenelse",
                "bandjoin",
                "bandrank",
                "composite",
                "case"
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
                new StringBuilder(string.Format(preamble,
                    $"{NetVips.Version(0)}.{NetVips.Version(1)}.{NetVips.Version(2)}"));
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
                var type = GTypeToCSharp(tmpFile.GetTypeOf(property));
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

        public string Execute(string[] args)
        {
            File.WriteAllText("Image.Generated.cs", Generate());

            return "See Image.Generated.cs";
        }
    }
}