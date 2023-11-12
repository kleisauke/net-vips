namespace NetVips.Docs
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Docfx.Dotnet;

    class Build
    {
        static async Task Main(string[] args)
        {
            var projectDir =
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            var currentDirectory = Directory.GetCurrentDirectory();

            Directory.SetCurrentDirectory(projectDir);
            await DotnetApiCatalog.GenerateManagedReferenceYamlFiles("docfx.json");
            await Docfx.Docset.Build("docfx.json");
            Directory.SetCurrentDirectory(currentDirectory);
        }
    }
}