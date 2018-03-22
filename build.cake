#tool "nuget:?package=xunit.runner.console"

// Arguments
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

// Variables
// Define directories.
var buildDir = Directory("./src/NetVips/bin") + Directory(configuration);

const string downloadDir = "./download/";

// Setup / teardown
Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

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
    var vipsZip = $"https://github.com/jcupitt/libvips/releases/download/v{version}{preVersion}/{fileName}";

    var outputPath = File(downloadDir + fileName);

    if (!FileExists(outputPath))
    {
        Information("libvips zip file not in download directory. Downloading now ...");
        EnsureDirectoryExists(downloadDir);
        DownloadFile(vipsZip, outputPath);
    }

    var vipsHome = EnvironmentVariable("VIPS_HOME");
    
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
    DotNetCoreRestore("./src/NetVips/NetVips.csproj", new DotNetCoreRestoreSettings
    {
        NoDependencies = true
    });
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetCoreBuild("./src/NetVips/NetVips.csproj", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        NoDependencies = true,
        NoRestore = true
    });
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
    .IsDependentOn("Test");

// Execution
RunTarget(target);