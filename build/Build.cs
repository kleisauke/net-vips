using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.All);

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    [Parameter("Configuration to build - Default is 'Release'")]
    public Configuration Configuration { get; } = Configuration.Release;

    [Parameter("Skip unit tests")] public bool SkipTests { get; }

    [Parameter("Build and create NuGet packages")] public bool Package { get; }

    [Parameter("Test with a globally installed libvips")] public bool GlobalVips { get; }

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath PackingDirectory => RootDirectory / "build/native/pack";
    AbsolutePath DownloadDirectory => RootDirectory / "download";

    string VipsVersion => Environment.GetEnvironmentVariable("VIPS_VERSION");

    string[] NuGetArchitectures => new[]
    {
        "win-x64",
        "win-x86",
        "win-arm64",
        "linux-x64",
        "linux-musl-x64",
        "linux-musl-arm64",
        "linux-arm",
        "linux-arm64",
        "osx-x64",
        "osx-arm64"
    };

    protected override void OnBuildInitialized()
    {
        Information("Building version {0} of NetVips ({1}).", GetVersion(), Configuration);
        Information("OS: {0}", RuntimeInformation.OSDescription);
        Information("Architecture: {0}", RuntimeInformation.ProcessArchitecture);
        Information("Host type: {0}", IsServerBuild ? "Server" : "Local");
        if (!string.IsNullOrWhiteSpace(VipsVersion))
        {
            Information("Version of libvips: {0}", VipsVersion);
        }
    }

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").DeleteDirectories();
            TestsDirectory.GlobDirectories("**/bin", "**/obj").DeleteDirectories();
            PackingDirectory.CreateOrCleanDirectory();
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target RunTests => _ => _
        .OnlyWhenStatic(() => !SkipTests)
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetTest(c => c
                .SetProjectFile(Solution.NetVips_Tests)
                .SetConfiguration(Configuration)
                .AddProperty("TestWithNuGetBinaries", !GlobalVips));
        });

    Target DownloadBinaries => _ => _
        .OnlyWhenStatic(() => Package)
        .After(RunTests)
        .Executes(async () =>
        {
            var client = new HttpClient();

            foreach (var architecture in NuGetArchitectures)
            {
                var fileName = $"libvips-{VipsVersion}-{architecture}.tar.gz";
                var tarball =
                    new Uri(
                        $"https://github.com/kleisauke/libvips-packaging/releases/download/v{VipsVersion}/{fileName}");

                var filePath = DownloadDirectory / fileName;
                if (!File.Exists(filePath))
                {
                    Information(filePath + " not in download directory. Downloading now ...");
                    DownloadDirectory.CreateDirectory();
                    var response = await client.GetAsync(tarball);
                    await using var fs = new FileStream(filePath, FileMode.CreateNew);
                    await response.Content.CopyToAsync(fs);
                }

                var tempDir = PackingDirectory / "temp";

                Information($"Uncompressing {fileName} ...");
                ExtractTarball(filePath, tempDir);

                var dllPackDir = PackingDirectory / architecture;
                dllPackDir.CreateDirectory();

                // The C++ binding isn't needed.
                tempDir.GlobFiles("lib/libvips-cpp*").DeleteFiles();

                tempDir.GlobFiles("lib/*.dll", "lib/*.so*", "lib/*.dylib", "THIRD-PARTY-NOTICES.md", "versions.json")
                    .ForEach(f => f.MoveToDirectory(dllPackDir));

                tempDir.DeleteDirectory();
            }
        });

    Target CreateNetVipsNugetPackage => _ => _
        .OnlyWhenStatic(() => Package)
        .After(RunTests)
        .DependsOn(Clean)
        .Executes(() =>
        {
            // Need to build the macOS and *nix DLL first.
            DotNetBuild(c => c
                .SetProjectFile(Solution.NetVips)
                .SetConfiguration(Configuration)
                .SetFramework("netstandard2.0")
                .AddProperty("Platform", "AnyCPU")
                .CombineWith(
                    new[] { "OSX", "Unix" },
                    (_, os) => _.AddProperty("TargetOS", os)));

            DotNetPack(c => c
                .SetProject(Solution.NetVips)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .AddProperty("TargetOS", "Windows")
            );
        });

    Target CreateNetVipsExtensionsNugetPackage => _ => _
        .OnlyWhenStatic(() => Package)
        .After(RunTests)
        .Executes(() =>
        {
            DotNetPack(c => c
                .SetProject(Solution.NetVips_Extensions)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
            );
        });

    Target CreateNativeNuGetPackages => _ => _
        .OnlyWhenStatic(() => Package)
        .DependsOn(DownloadBinaries)
        .Executes(() =>
        {
            // Build the architecture specific packages
            NuGetPack(c => c
                .SetVersion(VipsVersion)
                .SetOutputDirectory(ArtifactsDirectory)
                .AddProperty("NoWarn", "NU5128")
                .CombineWith(NuGetArchitectures,
                    (_, architecture) =>
                        _.SetTargetPath(RootDirectory / "build/native/NetVips.Native." + architecture + ".nuspec")));

            // Build the all-in-one package, which depends on the previous packages.
            NuGetPack(c => c
                .SetTargetPath(RootDirectory / "build/native/NetVips.Native.nuspec")
                .SetVersion(VipsVersion)
                .SetOutputDirectory(ArtifactsDirectory)
                .AddProperty("NoWarn", "NU5128"));
        });

    Target All => _ => _
        .DependsOn(RunTests)
        .DependsOn(CreateNetVipsNugetPackage)
        .DependsOn(CreateNetVipsExtensionsNugetPackage)
        .DependsOn(CreateNativeNuGetPackages);
}