namespace NetVips.Samples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class GenerateImageOperators : ISample
    {
        public string Name => "Generate operator overloads";
        public string Category => "Internal";

        private const string Indent = "        ";

        private readonly string _docstring = $@"{Indent}/// <summary>
{Indent}/// This operation {{0}}.
{Indent}/// </summary>
{Indent}/// <param name=""left"">{{1}}.</param>
{Indent}/// <param name=""right"">{{2}}.</param>
{Indent}/// <returns>A new <see cref=""Image""/>.</returns>";

        /// <summary>
        /// Make a C#-style docstring + operator overload.
        /// </summary>
        /// <remarks>
        /// This is used to generate the functions in <see cref="Image"/>.
        /// </remarks>
        /// <param name="operatorStr">Overload operator.</param>
        /// <param name="type">The type to overload for.</param>
        /// <param name="invert">Is the image located on the left side of the operand?</param>
        /// <returns>A C#-style docstring + operator overload.</returns>
        private string GenerateOverload(string operatorStr, string type, bool invert = false)
        {
            const string calculatesSummary = "calculates <paramref name=\"left\"/> {0} <paramref name=\"right\"/>";
            const string imageCref = "<see cref=\"Image\"/>";

            string operation;
            string leftArg;
            string rightArg;

            var leftParam = invert ? "right" : "left";
            var rightParam = invert ? "left" : "right";

            string summary;

            var isImage = type == "Image";

            switch (operatorStr)
            {
                case "+":
                    summary = string.Format(calculatesSummary, operatorStr);
                    operation = isImage ? "add" : "linear";
                    leftArg = isImage ? rightParam : "1.0";
                    rightArg = isImage ? string.Empty : rightParam;
                    break;
                case "-" when invert:
                    summary = string.Format(calculatesSummary, operatorStr);
                    operation = "linear";
                    leftArg = "-1.0";
                    rightArg = rightParam;
                    break;
                case "-":
                    summary = string.Format(calculatesSummary, operatorStr);
                    operation = isImage ? "subtract" : "linear";
                    leftArg = isImage ? rightParam : "1.0";
                    rightArg = isImage ? string.Empty :
                        type.EndsWith("[]") ? $"{rightParam}.Negate()" : $"-{rightParam}";
                    break;
                case "*":
                    summary = string.Format(calculatesSummary, operatorStr);
                    operation = isImage ? "multiply" : "linear";
                    leftArg = rightParam;
                    rightArg = isImage ? string.Empty : "0.0";
                    break;
                case "/" when invert:
                    summary = string.Format(calculatesSummary, operatorStr);
                    operation = "linear";
                    leftParam += ".Pow(-1.0)";
                    leftArg = rightParam;
                    rightArg = "0.0";
                    break;
                case "/":
                    summary = string.Format(calculatesSummary, operatorStr);
                    operation = isImage ? "divide" : "linear";
                    leftArg = isImage ? rightParam :
                        type.EndsWith("[]") ? $"{rightParam}.Invert()" : $"1.0 / {rightParam}";
                    rightArg = isImage ? string.Empty : "0.0";
                    break;
                case "%":
                    summary = string.Format(calculatesSummary, operatorStr) +
                              $"\n{Indent}/// (remainder after integer division)";
                    operation = isImage ? "remainder" : "remainder_const";
                    leftArg = rightParam;
                    rightArg = string.Empty;
                    break;
                case "&":
                    summary = "computes the logical bitwise AND of its operands";
                    operation = isImage ? "boolean" : "boolean_const";
                    leftArg = isImage ? rightParam : "Enums.OperationBoolean.And";
                    rightArg = isImage ? "Enums.OperationBoolean.And" : rightParam;
                    break;
                case "|":
                    summary = "computes the bitwise OR of its operands";
                    operation = isImage ? "boolean" : "boolean_const";
                    leftArg = isImage ? rightParam : "Enums.OperationBoolean.Or";
                    rightArg = isImage ? "Enums.OperationBoolean.Or" : rightParam;
                    break;
                case "^":
                    summary = "computes the bitwise exclusive-OR of its operands";
                    operation = isImage ? "boolean" : "boolean_const";
                    leftArg = isImage ? rightParam : "Enums.OperationBoolean.Eor";
                    rightArg = isImage ? "Enums.OperationBoolean.Eor" : rightParam;
                    break;
                case "==":
                    summary = "compares two images on equality";
                    operation = "relational_const";
                    leftArg = "Enums.OperationRelational.Equal";
                    rightArg = rightParam;
                    break;
                case "!=":
                    summary = "compares two images on inequality";
                    operation = "relational_const";
                    leftArg = "Enums.OperationRelational.Noteq";
                    rightArg = rightParam;
                    break;
                case "<<":
                    summary = "shifts its first operand left by the number of bits specified by its second operand";
                    operation = "boolean_const";
                    leftArg = "Enums.OperationBoolean.Lshift";
                    rightArg = rightParam;
                    break;
                case ">>":
                    summary = "shifts its first operand right by the number of bits specified by its second operand";
                    operation = "boolean_const";
                    leftArg = "Enums.OperationBoolean.Rshift";
                    rightArg = rightParam;
                    break;
                case "<" when invert:
                    summary = "compares if the left operand is less than the right operand";
                    operation = "relational_const";
                    leftArg = "Enums.OperationRelational.More";
                    rightArg = rightParam;
                    break;
                case "<":
                    summary = "compares if the left operand is less than the right operand";
                    operation = isImage ? "relational" : "relational_const";
                    leftArg = isImage ? rightParam : "Enums.OperationRelational.Less";
                    rightArg = isImage ? "Enums.OperationRelational.Less" : rightParam;
                    break;
                case ">" when invert:
                    summary = "compares if the left operand is greater than the right operand";
                    operation = "relational_const";
                    leftArg = "Enums.OperationRelational.Less";
                    rightArg = rightParam;
                    break;
                case ">":
                    summary = "compares if the left operand is greater than the right operand";
                    operation = isImage ? "relational" : "relational_const";
                    leftArg = isImage ? rightParam : "Enums.OperationRelational.More";
                    rightArg = isImage ? "Enums.OperationRelational.More" : rightParam;
                    break;
                case "<=" when invert:
                    summary = "compares if the left operand is less than or equal to the right operand";
                    operation = "relational_const";
                    leftArg = "Enums.OperationRelational.Moreeq";
                    rightArg = rightParam;
                    break;
                case "<=":
                    summary = "compares if the left operand is less than or equal to the right operand";
                    operation = isImage ? "relational" : "relational_const";
                    leftArg = isImage ? rightParam : "Enums.OperationRelational.Lesseq";
                    rightArg = isImage ? "Enums.OperationRelational.Lesseq" : rightParam;
                    break;
                case ">=" when invert:
                    summary = "compares if the left operand is greater than or equal to the right operand";
                    operation = "relational_const";
                    leftArg = "Enums.OperationRelational.Lesseq";
                    rightArg = rightParam;
                    break;
                case ">=":
                    summary = "compares if the left operand is greater than or equal to the right operand";
                    operation = isImage ? "relational" : "relational_const";
                    leftArg = isImage ? rightParam : "Enums.OperationRelational.Moreeq";
                    rightArg = isImage ? "Enums.OperationRelational.Moreeq" : rightParam;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operatorStr), operatorStr, "Operator out of range");
            }

            string leftDesc;
            string rightDesc;

            switch (type)
            {
                case "Image":
                    leftDesc = $"Left {imageCref}";
                    rightDesc = $"Right {imageCref}";
                    break;
                case "double[]" when invert:
                    leftDesc = "Left double array";
                    rightDesc = $"Right {imageCref}";
                    break;
                case "double[]":
                    leftDesc = $"Left {imageCref}";
                    rightDesc = "Right double array";
                    break;
                case "int[]" when invert:
                    leftDesc = "Left integer array";
                    rightDesc = $"Right {imageCref}";
                    break;
                case "int[]":
                    leftDesc = $"Left {imageCref}";
                    rightDesc = "Right integer array";
                    break;
                case "double" when invert:
                    leftDesc = "Left double constant";
                    rightDesc = $"Right {imageCref}";
                    break;
                case "double":
                    leftDesc = $"Left {imageCref}";
                    rightDesc = "Right double constant";
                    break;
                case "int":
                    leftDesc = $"Left {imageCref}";
                    rightDesc = "The number of bits";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Type out of range");
            }

            if (operatorStr == "==" || operatorStr == "!=")
            {
                leftDesc += " to compare";
                rightDesc += " to compare";
            }

            var result = new StringBuilder(string.Format(_docstring, summary, leftDesc, rightDesc));

            result
                .AppendLine()
                .AppendLine(invert
                    ? $"{Indent}public static Image operator {operatorStr}({type} left, Image right) =>"
                    : $"{Indent}public static Image operator {operatorStr}(Image left, {type} right) =>")
                .AppendLine(
                    string.IsNullOrEmpty(rightArg)
                        ? $"{Indent}    {leftParam}.Call(\"{operation}\", {leftArg}) as Image;"
                        : $"{Indent}    {leftParam}.Call(\"{operation}\", {leftArg}, {rightArg}) as Image;");

            return result.ToString();
        }

        /// <summary>
        /// Generate the `Image.Operators.cs` file.
        /// </summary>
        /// <remarks>
        /// This is used to generate the `Image.Operators.cs` file (<see cref="Image"/>).
        /// Use it with something like:
        /// <code language="lang-csharp">
        /// File.WriteAllText("Image.Operators.cs", GenerateOperators());
        /// </code>
        /// </remarks>
        /// <returns>The `Image.Operators.cs` as string.</returns>
        public string GenerateOperators()
        {
            // generate list of all operator overloads and supported types
            var allOverloads = new Dictionary<string, string[]>
            {
                {"+", new[] {"Image", "double", "double[]", "int[]"}},
                {"-", new[] {"Image", "double", "double[]", "int[]"}},
                {"*", new[] {"Image", "double", "double[]", "int[]"}},
                {"/", new[] {"Image", "double", "double[]", "int[]"}},
                {"%", new[] {"Image", "double", "double[]", "int[]"}},
                {"&", new[] {"Image", "double", "double[]", "int[]"}},
                {"|", new[] {"Image", "double", "double[]", "int[]"}},
                {"^", new[] {"Image", "double", "double[]", "int[]"}},
                {"<<", new[] {"int"}},
                {">>", new[] {"int"}},
                {"==", new[] {"double", "double[]", "int[]"}},
                {"!=", new[] {"double", "double[]", "int[]"}},
                {"<", new[] {"Image", "double", "double[]", "int[]"}},
                {">", new[] {"Image", "double", "double[]", "int[]"}},
                {"<=", new[] {"Image", "double", "double[]", "int[]"}},
                {">=", new[] {"Image", "double", "double[]", "int[]"}}
            };

            const string preamble = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------";

            var stringBuilder = new StringBuilder(preamble);
            stringBuilder.AppendLine()
                .AppendLine()
                .AppendLine("namespace NetVips")
                .AppendLine("{")
                .AppendLine("    public sealed partial class Image")
                .AppendLine("    {")
                .AppendLine($"{Indent}#region auto-generated operator overloads")
                .AppendLine();

            foreach (var (operatorStr, types) in allOverloads)
            {
                foreach (var type in types)
                {
                    if (type != "Image" && types.Length > 1)
                    {
                        stringBuilder.AppendLine(GenerateOverload(operatorStr, type, true));
                    }

                    // We only generate the inverted types of the `==` and `!=` operators
                    // to avoid conflicts with `null` checks (for e.g. `image == null`).
                    // See: `Equal()` and `NotEqual()` for comparison with the other types.
                    if (operatorStr == "==" || operatorStr == "!=")
                    {
                        continue;
                    }

                    stringBuilder.AppendLine(GenerateOverload(operatorStr, type));
                }
            }

            stringBuilder.AppendLine($"{Indent}/// <summary>")
                .AppendLine(
                    $"{Indent}/// Returns a value indicating whether a given <see cref=\"Image\"/> is definitely <see langword=\"true\"/>.")
                .AppendLine($"{Indent}/// </summary>")
                .AppendLine($"{Indent}/// <param name=\"image\">The image to check.</param>")
                .AppendLine(
                    $"{Indent}/// <returns><see langword=\"true\"/> if <paramref name=\"image\"/> is definitely <see langword=\"true\"/>; otherwise, <see langword=\"false\"/>.</returns>")
                .AppendLine($"{Indent}public static bool operator true(Image image) =>")
                .AppendLine(
                    $"{Indent}    // Always evaluate to false so that each side of the && equation is evaluated")
                .AppendLine($"{Indent}    false;")
                .AppendLine();

            stringBuilder.AppendLine($"{Indent}/// <summary>")
                .AppendLine(
                    $"{Indent}/// Returns a value indicating whether a given <see cref=\"Image\"/> is definitely <see langword=\"false\"/>.")
                .AppendLine($"{Indent}/// </summary>")
                .AppendLine($"{Indent}/// <param name=\"image\">The image to check.</param>")
                .AppendLine(
                    $"{Indent}/// <returns><see langword=\"true\"/> if <paramref name=\"image\"/> is definitely <see langword=\"false\"/>; otherwise, <see langword=\"false\"/>.</returns>")
                .AppendLine($"{Indent}public static bool operator false(Image image) =>")
                .AppendLine(
                    $"{Indent}    // Always evaluate to false so that each side of the && equation is evaluated")
                .AppendLine($"{Indent}    false;")
                .AppendLine();

            stringBuilder.AppendLine($"{Indent}#endregion")
                .AppendLine("    }")
                .AppendLine("}");
            return stringBuilder.ToString();
        }

        public void Execute(string[] args)
        {
            File.WriteAllText("Image.Operators.cs", GenerateOperators());
            Console.WriteLine("See Image.Operators.cs");
        }
    }
}