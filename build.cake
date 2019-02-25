// Install tools
#tool "nuget:?package=xunit.runner.console&version=2.4.1"
#tool "nuget:?package=nuget.commandline&version=4.9.3"

// Install addins
#addin "nuget:?package=SharpZipLib&version=1.1.0"
#addin "nuget:?package=Cake.Compression&version=0.2.2"

// Arguments
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

// Global variables
var artifactsDirectory = Directory("./artifacts");
var buildDir = Directory("./src/NetVips/bin") + Directory(configuration);
var isOnAppVeyorAndNotPR = AppVeyor.IsRunningOnAppVeyor && !AppVeyor.Environment.PullRequest.IsPullRequest;
var architectures = new[] { "win-x64", "win-x86", "linux-x64", "linux-musl-x64", "osx-x64" };
var vipsVersion = EnvironmentVariable("VIPS_VERSION");
if (HasEnvironmentVariable("VIPS_PRE_VERSION"))
{
    vipsVersion += "-" + EnvironmentVariable("VIPS_PRE_VERSION");
}

const string downloadDir = "./download/";
const string packDir = "./build/native/pack/";

// Tasks
Task("Clean")
    .Does(() =>
    {
        CleanDirectory(buildDir);
    });

Task("Download-Binaries")
    .WithCriteria(IsRunningOnWindows())
    .IsDependentOn("Clean")
    .Does(() =>
    {
        if (DirectoryExists(packDir))
        {
            Information("Removing old packaging directory");
            DeleteDirectory(packDir, new DeleteDirectorySettings
            {
                Recursive = true,
                Force = true
            });
        }

        EnsureDirectoryExists(packDir);

        foreach (var architecture in architectures)
        {
            var fileName = $"libvips-{vipsVersion}-{architecture}.tar.gz";
            var tarball = $"https://github.com/kleisauke/libvips-packaging/releases/download/v{vipsVersion}/{fileName}";

            var filePath = new DirectoryPath(downloadDir).CombineWithFilePath(fileName);
            if (!FileExists(filePath))
            {
                Information($"{fileName} not in download directory. Downloading now ...");
                EnsureDirectoryExists(downloadDir);
                DownloadFile(tarball, filePath);
            }

            var tempDir = new DirectoryPath(packDir).Combine("temp");

            Information($"Uncompressing {fileName} ...");
            GZipUncompress(filePath, tempDir);

            var dllPackDir = new DirectoryPath(packDir).Combine(architecture);
            EnsureDirectoryExists(dllPackDir);

            CopyFiles(tempDir + "/lib/*.{dll,so.*,dylib}", dllPackDir);   

            DeleteDirectory(tempDir, new DeleteDirectorySettings
            {
                Recursive = true,
                Force = true
            });
        }
    });

// Run dotnet restore to restore all package references.
Task("Restore")
    .IsDependentOn("Download-Binaries")
    .Does(() =>
    {
        DotNetCoreRestore("./src/NetVips/NetVips.csproj");
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild("./src/NetVips/NetVips.csproj", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoRestore = true
        });
    });

Task("Pack")
    .IsDependentOn("Build")
    .WithCriteria((isOnAppVeyorAndNotPR || string.Equals(target, "pack", StringComparison.OrdinalIgnoreCase)) && IsRunningOnWindows())
    .Does(() =>
    {
        // Need to build the OSX and Linux DLL first.
        DotNetCoreBuild("./build/NetVips.batch.csproj", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoRestore = true
        });

        DotNetCorePack("./src/NetVips/NetVips.csproj", new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDirectory,
            ArgumentCustomization = (args) =>
            {
                return args
                    .Append("/p:TargetOS={0}", "Windows")
                    .Append("/p:IncludeLibvips={0}", "true");
            }
        });

        foreach (var architecture in architectures)
        {
            NuGetPack("./build/native/NetVips.Native." + architecture + ".nuspec", new NuGetPackSettings
            {
                Version = vipsVersion,
                OutputDirectory = artifactsDirectory
            });
        }
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCoreTest("./tests/NetVips.Tests/NetVips.Tests.csproj", new DotNetCoreTestSettings
        {
            Configuration = configuration
        });
    });

// Task targets
Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

// Execution
RunTarget(target);