using CppSharp.AST;
using CppSharp.Passes;

namespace NetVips.Generator.Passes
{
    public class FixTypes : TranslationUnitPass
    {
        public override bool VisitFunctionDecl(Function function)
        {
            if (!base.VisitFunctionDecl(function))
            {
                return false;
            }

            // Some return types needs to be fixed by hand
            if (function.Name.Equals("vips_value_get_array_image"))
            {
                function.ReturnType =
                    new QualifiedType(new PointerType(new QualifiedType(new BuiltinType(PrimitiveType.Void))));
            }

            return true;
        }

        public override bool VisitParameterDecl(Parameter parameter)
        {
            if (!base.VisitParameterDecl(parameter))
            {
                return false;
            }

            // Some parameter types needs to be fixed by hand
            if (parameter.QualifiedName.Equals("vips_value_set_array_int::array"))
            {
                parameter.QualifiedType = new QualifiedType(new ArrayType
                {
                    QualifiedType = new QualifiedType(new BuiltinType(PrimitiveType.Int)),
                    ElementSize = 0,
                    Size = 0,
                    SizeType = ArrayType.ArraySize.Incomplete,
                });
            }

            if (parameter.QualifiedName.Equals("vips_value_set_array_double::array"))
            {
                parameter.QualifiedType = new QualifiedType(new ArrayType
                {
                    QualifiedType = new QualifiedType(new BuiltinType(PrimitiveType.Double)),
                    ElementSize = 0,
                    Size = 0,
                    SizeType = ArrayType.ArraySize.Incomplete,
                });
            }

            // TODO UTF-8 fix?
            /*if (parameter.QualifiedName.Equals("vips_filename_get_filename::vips_filename") ||
                parameter.QualifiedName.Equals("vips_filename_get_options::vips_filename") ||
                parameter.QualifiedName.Equals("vips_foreign_find_load::filename"))
            {
                parameter.QualifiedType = new QualifiedType(new ArrayType
                {
                    QualifiedType = new QualifiedType(new BuiltinType(PrimitiveType.UChar)),
                    ElementSize = 0,
                    Size = 0,
                    SizeType = ArrayType.ArraySize.Incomplete,
                });
            }*/

            return true;
        }
    }
}