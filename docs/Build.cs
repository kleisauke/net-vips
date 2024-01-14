using System;
using System.IO;
using System.Threading.Tasks;
using Docfx;
using Docfx.Dotnet;

namespace NetVips.Docs;

internal class Build
{
    private static async Task Main(string[] args)
    {
        var projectDir =
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        var currentDirectory = Directory.GetCurrentDirectory();

        Directory.SetCurrentDirectory(projectDir);
        await DotnetApiCatalog.GenerateManagedReferenceYamlFiles("docfx.json");
        await Docset.Build("docfx.json");
        Directory.SetCurrentDirectory(currentDirectory);
    }
}