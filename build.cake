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
const string packDir = "./pack/";

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
        var version = EnvironmentVariable("VIPS_VERSION");
        if (HasEnvironmentVariable("VIPS_PRE_VERSION"))
        {
            version += "-" + EnvironmentVariable("VIPS_PRE_VERSION");
        }

        var zipVersion = EnvironmentVariable("VIPS_ZIP_VERSION");

        foreach (var architecture in new []{"win-x64","win-x86"})
        {
            var bitness = (architecture == "win-x86") ? "32" : "64";
            var fileName = $"vips-dev-w{bitness}-web-{zipVersion}.zip";
            var vipsZip = $"https://github.com/kleisauke/build-win64-mxe/releases/download/v{version}/{fileName}";

            var zipFile = new DirectoryPath(downloadDir).CombineWithFilePath(fileName);
            if (!FileExists(zipFile))
            {
                Information($"libvips {architecture} zip file not in download directory. Downloading now ...");
                EnsureDirectoryExists(downloadDir);
                DownloadFile(vipsZip, zipFile);
            }

            var outputPath = vipsHome + "/" + architecture;
            if (DirectoryExists(outputPath))
            {
                Information($"Removing old libvips {architecture}");
                DeleteDirectory(outputPath, new DeleteDirectorySettings {
                    Recursive = true,
                    Force = true
                });
            }

            Information("Uncompressing zip file ...");
            Unzip(zipFile, outputPath);

            // Need to remove toplevel dir from zip container
            var containerDir = GetDirectories(outputPath + "/*").First(x => x.GetDirectoryName().StartsWith("vips-"));
            CopyDirectory(containerDir, outputPath);
            DeleteDirectory(containerDir, new DeleteDirectorySettings {
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
        if (DirectoryExists(packDir))
        {
            Information("Removing old packaging directory");
            DeleteDirectory(packDir, new DeleteDirectorySettings {
                Recursive = true,
                Force = true
            });
        }

        EnsureDirectoryExists(packDir);
        
        foreach (var architecture in new []{"win-x64","win-x86"})
        {
            var dllPackDir = new DirectoryPath(packDir + "/" + architecture);
            EnsureDirectoryExists(dllPackDir);
            
            // Copy binaries to packaging directory
            CopyFiles(vipsHome + "/" + architecture + "/bin/*.dll", dllPackDir);

            // Clean unused DDL's
            var deleteFiles = new FilePath[] {
                dllPackDir.CombineWithFilePath("libvips-cpp-42.dll"),
                dllPackDir.CombineWithFilePath("libstdc++-6.dll")
            };
            DeleteFiles(deleteFiles);
        }

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