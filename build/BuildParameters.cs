using System;
using System.Linq;
using System.Xml.Linq;
using Nuke.Common;
using static Nuke.Common.IO.PathConstruction;

public partial class Build
{
    [Parameter("configuration")]
    public string Configuration { get; set; }

    [Parameter("skip-tests")]
    public bool SkipTests { get; set; }

    [Parameter("package")]
    public bool Package { get; set; }

    public class BuildParameters
    {
        public string Configuration { get; }
        public bool SkipTests { get; }
        public bool Package { get; }
        public string BuildSolution { get; }
        public string TestSolution { get; }
        public string[] NuGetArchitectures { get; }
        public string Version { get; }
        public string VipsVersion { get; }
        public AbsolutePath ArtifactsDir { get; }
        public AbsolutePath PackDir { get; }
        public AbsolutePath DownloadDir { get; }

        public BuildParameters(Build b)
        {
            // ARGUMENTS
            Configuration = b.Configuration ?? "Release";
            SkipTests = b.SkipTests;
            Package = b.Package;

            // CONFIGURATION
            BuildSolution = RootDirectory / "src/NetVips/NetVips.csproj";
            TestSolution = RootDirectory / "tests/NetVips.Tests/NetVips.Tests.csproj";

            // PARAMETERS
            NuGetArchitectures = new[] { "win-x64", "win-x86", "linux-x64", "linux-musl-x64", "osx-x64" };

            // VERSION
            Version = GetVersion();
            VipsVersion = Environment.GetEnvironmentVariable("VIPS_VERSION");

            var vipsPreVersion = Environment.GetEnvironmentVariable("VIPS_PRE_VERSION");
            if (!string.IsNullOrWhiteSpace(vipsPreVersion))
            {
                VipsVersion += "-" + vipsPreVersion;
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

            return major + "." + minor + "." + revision;
        }
    }
}