#tool "nuget:?package=xunit.runner.console"

// Arguments
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

// Global variables
var artifactsDirectory = Directory("./artifacts");
var buildDir = Directory("./src/NetVips/bin") + Directory(configuration);
var isOnAppVeyorAndNotPR = AppVeyor.IsRunningOnAppVeyor && !AppVeyor.Environment.PullRequest.IsPullRequest;
var vipsHome = EnvironmentVariable("VIPS_HOME");

const string downloadDir = "./download/";
const string dllPackDir = "./pack/";

// Tasks
Task("Clean")
    .Does(() =>
    {
        CleanDirectory(buildDir);
    });

Task("Install-Libvips")
    .WithCriteria(IsRunningOnWindows())
    .IsDependentOn("Clean")
    .Does(() =>
    {
        var version = EnvironmentVariable("VIPS_VERSION");
        var preVersion = EnvironmentVariable("VIPS_PRE_VERSION") ?? "";

        var zipVersion = EnvironmentVariable("VIPS_ZIP_VERSION");

        var fileName = $"vips-{zipVersion}.zip";
        var vipsZip = $"https://github.com/kleisauke/build-win64-mxe/releases/download/v{version}{preVersion}/{fileName}";

        var outputPath = new DirectoryPath(downloadDir).CombineWithFilePath(fileName);
        if (!FileExists(outputPath))
        {
            Information("libvips zip file not in download directory. Downloading now ...");
            EnsureDirectoryExists(downloadDir);
            DownloadFile(vipsZip, outputPath);
        }
        
        if (DirectoryExists(vipsHome))
        {
            Information("Removing old libvips");
            DeleteDirectory(vipsHome, new DeleteDirectorySettings {
                Recursive = true,
                Force = true
            });
        }

        Information("Uncompressing zip file ...");
        Unzip(outputPath, vipsHome);

        // Need to remove toplevel dir from zip container
        var containerDir = GetDirectories(vipsHome + "/*").First(x => x.GetDirectoryName().StartsWith("vips-"));
        CopyDirectory(containerDir, vipsHome);
        DeleteDirectory(containerDir, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    });

// Run dotnet restore to restore all package references.
Task("Restore")  
    .IsDependentOn("Install-Libvips")
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
        if (DirectoryExists(dllPackDir))
        {
            Information("Removing old packaging directory");
            DeleteDirectory(dllPackDir, new DeleteDirectorySettings {
                Recursive = true,
                Force = true
            });
        }

        EnsureDirectoryExists(dllPackDir);
    
        // Copy binaries to packaging directory
        CopyFiles(vipsHome + "/bin/*.dll", dllPackDir);

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
            ArgumentCustomization = (args) => {
                return args
                    .Append("/p:TargetOS={0}", "Windows")
                    .Append("/p:IncludeLibvips={0}", "true");
            }
        });
    });

Task("LeakTest")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCoreTest("./tests/NetVips.Tests/NetVips.Tests.csproj", new DotNetCoreTestSettings
        {
            Configuration = configuration,
            Filter = "Category=Leak",
        });
    });

Task("Test")
    .IsDependentOn("LeakTest")
    .Does(() =>
    {
        DotNetCoreTest("./tests/NetVips.Tests/NetVips.Tests.csproj", new DotNetCoreTestSettings
        {
            Configuration = configuration,
            Filter = "Category!=Leak",
        });
    });

// Task targets
Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

// Execution
RunTarget(target);