using CppSharp.AST;
using CppSharp.Passes;

namespace NetVips.Passes
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