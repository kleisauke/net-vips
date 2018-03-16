#tool nuget:?package=NUnit.ConsoleRunner&version=3.8.0

// Arguments
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

// Variables

// Define directories.
var buildDir = Directory("./NetVips/bin") + Directory(configuration);

const string downloadDir = "./download/";

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

    var fileName = "vips-" + zipVersion + ".zip";
    var vipsZip = "https://github.com/jcupitt/libvips/releases/download/v" + version + preVersion + "/" + fileName;

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

Task("Restore-NuGet-Packages")
    .IsDependentOn("Install-Libvips")
    .Does(() =>
{
    NuGetRestore("./NetVips.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./NetVips.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./NetVips.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        NoResults = true
    });
});

// Task targets

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

// Execution

RunTarget(target);