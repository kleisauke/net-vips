using System;
using System.Linq;
using System.Xml.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.TravisCI;

public partial class Build
{
    [Parameter("configuration")]
    public string Configuration { get; set; }

    [Parameter("skip-tests")]
    public bool SkipTests { get; set; }

    [Parameter("package")]
    public bool Package { get; set; }

    [Parameter("global-vips")]
    public bool GlobalVips { get; set; }

    public class BuildParameters
    {
        public string Configuration { get; }
        public bool SkipTests { get; }
        public bool Package { get; }
        public bool TestWithNuGetBinaries { get; }
        public string BuildSolution { get; }
        public string BuildSolutionExtensions { get; }
        public string TestSolution { get; }
        public string[] NuGetArchitectures { get; }
        public string Version { get; }
        public string VipsVersion { get; }
        public string VipsTagVersion { get; }
        public AbsolutePath ArtifactsDir { get; }
        public AbsolutePath PackDir { get; }
        public AbsolutePath DownloadDir { get; }

        public BuildParameters(Build b)
        {
            // ARGUMENTS
            Configuration = b.Configuration ?? "Release";
            SkipTests = b.SkipTests;
            Package = b.Package;
            TestWithNuGetBinaries = !b.GlobalVips;

            // CONFIGURATION
            BuildSolution = RootDirectory / "src/NetVips/NetVips.csproj";
            BuildSolutionExtensions = RootDirectory / "src/NetVips.Extensions/NetVips.Extensions.csproj";
            TestSolution = RootDirectory / "tests/NetVips.Tests/NetVips.Tests.csproj";

            // PARAMETERS
            NuGetArchitectures = new[]
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

            // VERSION
            Version = GetVersion();
            VipsVersion = Environment.GetEnvironmentVariable("VIPS_VERSION");
            VipsTagVersion = VipsVersion;

            var vipsPreVersion = Environment.GetEnvironmentVariable("VIPS_PRE_VERSION");
            if (!string.IsNullOrWhiteSpace(vipsPreVersion))
            {
                VipsTagVersion += "-" + vipsPreVersion;
            }

            // DIRECTORIES
            ArtifactsDir = RootDirectory / "artifacts";
            PackDir = RootDirectory / "build/native/pack/";
            DownloadDir = RootDirectory / "download";
        }

        string GetVersion()
        {
            var xdoc = XDocument.Load(RootDirectory / "build/common.props");
            var major = xdoc.Descendants().First(x => x.Name.LocalName == "Major").Value;
            var minor = xdoc.Descendants().First(x => x.Name.LocalName == "Minor").Value;
            var revision = xdoc.Descendants().First(x => x.Name.LocalName == "Revision").Value;
            var prerelease = xdoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "PrereleaseLabel" &&
                                                                    !x.HasAttributes)?.Value ?? string.Empty;

            if (Host == HostType.Console)
            {
                return major + "." + minor + "." + revision + prerelease;
            }

            var buildNumber = AppVeyor.Instance?.BuildNumber ??
                              TravisCI.Instance?.BuildNumber ??
                              0;

            prerelease = "." + buildNumber + "-develop";

            return major + "." + minor + "." + revision + prerelease;
        }
    }
}