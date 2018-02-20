using CppSharp.AST;
using CppSharp.Passes;

namespace NetVips
{
    public class IgnoreNonVipsDeclsPass : TranslationUnitPass
    {
        public override bool VisitFunctionDecl(Function function)
        {
            if (!function.Name.StartsWith("vips_"))
            {
                function.ExplicitlyIgnore();
                return false;
            }
            else
            {
                if (function.IsVariadic)
                {
                    // TODO: Investigate how to include VOption for variadic arguments. We currently use a string array just for example.
                    var vOption = new ArrayType
                    {
                        QualifiedType = new QualifiedType(new BuiltinType(PrimitiveType.String)),
                        ElementSize = 0,
                        Size = 0,
                        SizeType = ArrayType.ArraySize.Incomplete,
                    };

                    function.Parameters.Add(new Parameter
                    {
                        Kind = ParameterKind.Regular,
                        QualifiedType = new QualifiedType(vOption),
                        Name = "options",
                        Namespace = function.Namespace,
                        Usage = ParameterUsage.Unknown,
                        DefaultArgument = new BuiltinTypeExpression
                        {
                            Value = 00,
                            Type = new BuiltinType(PrimitiveType.Null),
                            String = "null"
                        }
                    });
                }

                return true;
            }
        }

        public override bool VisitClassDecl(Class @class)
        {
            if (!@class.Name.StartsWith("_Vips") && !@class.Name.StartsWith("Vips"))
            {
                @class.ExplicitlyIgnore();
                return false;
            }
            else
            {
                return true;
            }
        }

        public override bool VisitEnumDecl(Enumeration @enum)
        {
            if (!@enum.Name.StartsWith("Vips"))
            {
                @enum.ExplicitlyIgnore();
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}