using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    BuildParameters Parameters { get; set; }

    protected override void OnBuildInitialized()
    {
        Parameters = new BuildParameters(this);
        Information("Building version {0} of NetVips ({1}).",
            Parameters.Version,
            Parameters.Configuration);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Information("OS: Windows");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Information("OS: Linux");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Information("OS: macOS");
        }

        Information("Bitness: " + (Environment.Is64BitProcess ? "64 bit" : "32 bit"));
        Information("Host type: " + Host);
        Information("Version of libvips: " + Parameters.VipsVersion);
        Information("Configuration: " + Parameters.Configuration);

        void ExecWait(string preamble, string command, string args)
        {
            Console.WriteLine(preamble);
            Process.Start(new ProcessStartInfo(command, args) { UseShellExecute = false })?.WaitForExit();
        }

        ExecWait("dotnet version:", "dotnet", "--version");
    }

    Target Clean => _ => _
        .Executes(() =>
        {
            EnsureCleanDirectory(RootDirectory / "src/NetVips/bin" / Parameters.Configuration);
            EnsureCleanDirectory(RootDirectory / "src/NetVips.Extensions/bin" / Parameters.Configuration);
            EnsureCleanDirectory(RootDirectory / "tests/NetVips.Tests/bin" / Parameters.Configuration);
            EnsureCleanDirectory(Parameters.ArtifactsDir);
            EnsureCleanDirectory(Parameters.PackDir);
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetBuild(c => c
                .SetProjectFile(Parameters.BuildSolution)
                .SetConfiguration(Parameters.Configuration)
            );
        });

    Target RunTests => _ => _
        .OnlyWhenStatic(() => !Parameters.SkipTests)
        .DependsOn(Compile)
        .Executes(() =>
        {
            // Only test with the precompiled NuGet binaries if we're not on Travis.
            DotNetTest(c => c
                .SetProjectFile(Parameters.TestSolution)
                .SetConfiguration(Parameters.Configuration)
                .AddProperty("TestWithNuGetBinaries", Host != HostType.Travis));
        });

    Target DownloadBinaries => _ => _
        .OnlyWhenStatic(() => Parameters.Package)
        .After(RunTests)
        .Executes(async () =>
        {
            var client = new HttpClient();

            foreach (var architecture in Parameters.NuGetArchitectures)
            {
                var fileName = $"libvips-{Parameters.VipsVersion}-{architecture}.tar.gz";
                var tarball =
                    new Uri(
                        $"https://github.com/kleisauke/libvips-packaging/releases/download/v{Parameters.VipsVersion}/{fileName}");

                var filePath = Parameters.DownloadDir / fileName;
                if (!File.Exists(filePath))
                {
                    Information(filePath + " not in download directory. Downloading now ...");
                    EnsureExistingDirectory(Parameters.DownloadDir);
                    var response = await client.GetAsync(tarball);
                    using (var fs = new FileStream(filePath, FileMode.CreateNew))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                var tempDir = Parameters.PackDir / "temp";

                Information($"Uncompressing {fileName} ...");
                ExtractTarball(filePath, tempDir);

                var dllPackDir = Parameters.PackDir / architecture;
                EnsureExistingDirectory(dllPackDir);

                tempDir.GlobFiles("lib/*.dll", "lib/*.so.*", "lib/*.dylib", "THIRD-PARTY-NOTICES.md", "versions.json")
                    .ForEach(f => CopyFileToDirectory(f, dllPackDir));

                DeleteDirectory(tempDir);
            }
        });

    Target CreateNetVipsNugetPackage => _ => _
        .OnlyWhenStatic(() => Parameters.Package)
        .After(RunTests)
        .Executes(() =>
        {
            // Need to build the OSX and Linux DLL first.
            DotNetBuild(c => c
                .SetProjectFile(Parameters.BuildSolution)
                .SetConfiguration(Parameters.Configuration)
                .SetFramework("netstandard2.0")
                .AddProperty("Platform", "AnyCPU")
                .AddProperty("TargetOS", "OSX")
            );

            DotNetBuild(c => c
                .SetProjectFile(Parameters.BuildSolution)
                .SetConfiguration(Parameters.Configuration)
                .SetFramework("netstandard2.0")
                .AddProperty("Platform", "AnyCPU")
                .AddProperty("TargetOS", "Linux")
            );

            DotNetPack(c => c
                .SetProject(Parameters.BuildSolution)
                .SetConfiguration(Parameters.Configuration)
                .SetOutputDirectory(Parameters.ArtifactsDir)
                .AddProperty("TargetOS", "Windows")
            );
        });

    Target CreateNetVipsExtensionsNugetPackage => _ => _
        .OnlyWhenStatic(() => Parameters.Package)
        .After(RunTests)
        .Executes(() =>
        {
            DotNetPack(c => c
                .SetProject(Parameters.BuildSolutionExtensions)
                .SetConfiguration(Parameters.Configuration)
                .SetOutputDirectory(Parameters.ArtifactsDir)
            );
        });

    Target CreateNativeNuGetPackages => _ => _
        .OnlyWhenStatic(() => Parameters.Package)
        .DependsOn(DownloadBinaries)
        .Executes(() =>
        {
            // Build the architecture specific packages.
            foreach (var architecture in Parameters.NuGetArchitectures)
            {
                NuGetPack(c => c
                    .SetTargetPath(RootDirectory / "build/native/NetVips.Native." + architecture + ".nuspec")
                    .SetVersion(Parameters.VipsVersion)
                    .SetOutputDirectory(Parameters.ArtifactsDir)
                    .AddProperty("NoWarn", "NU5128"));
            }

            // Build the all-in-one package, which depends on the previous packages.
            NuGetPack(c => c
                .SetTargetPath(RootDirectory / "build/native/NetVips.Native.nuspec")
                .SetVersion(Parameters.VipsVersion)
                .SetOutputDirectory(Parameters.ArtifactsDir)
                .AddProperty("NoWarn", "NU5128"));
        });

    Target All => _ => _
        .DependsOn(RunTests)
        .DependsOn(CreateNetVipsNugetPackage)
        .DependsOn(CreateNetVipsExtensionsNugetPackage)
        .DependsOn(CreateNativeNuGetPackages);

    public static int Main() => Execute<Build>(x => x.All);
}