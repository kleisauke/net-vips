// This file enables ModuleInitializer, a C# 9 feature to work with .NET Standard/Framework.

#if NETSTANDARD || NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class ModuleInitializerAttribute : Attribute
    {
    }
}
#endif