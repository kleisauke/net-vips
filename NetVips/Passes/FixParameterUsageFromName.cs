using CppSharp.AST;
using CppSharp.Passes;

namespace NetVips.Passes
{
    public class FixParameterUsageFromName : TranslationUnitPass
    {
        public override bool VisitParameterDecl(Parameter parameter)
        {
            if (parameter.Name.Equals("out") &&
                parameter.Type.ToString().EndsWith("VipsImage") &&
                !parameter.QualifiedName.Equals("vips_allocate_input_array::out"))
            {
                parameter.Usage = ParameterUsage.Out;
            }

            if (parameter.Name.Equals("in"))
            {
                parameter.Usage = ParameterUsage.In;
            }

            return true;
        }
    }
}