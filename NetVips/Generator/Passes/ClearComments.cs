using CppSharp.AST;
using CppSharp.Passes;

namespace NetVips.Generator.Passes
{
    public class ClearComments : TranslationUnitPass
    {
        public override bool VisitDeclaration(Declaration decl)
        {
            decl.Comment = null;
            return base.VisitDeclaration(decl);
        }
    }
}