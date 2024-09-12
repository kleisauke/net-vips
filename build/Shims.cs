using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Nuke.Common.CI;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.GitHubActions;

public partial class Build
{
    static void Information(string info)
    {
        Serilog.Log.Information(info);
    }

    static void Information(string info, params object[] args)
    {
        Serilog.Log.Information(info, args);
    }

    public static string GetVersion()
    {
        var xdoc = XDocument.Load(RootDirectory / "build/common.props");
        var major = xdoc.Descendants().First(x => x.Name.LocalName == "Major").Value;
        var minor = xdoc.Descendants().First(x => x.Name.LocalName == "Minor").Value;
        var revision = xdoc.Descendants().First(x => x.Name.LocalName == "Revision").Value;

        long buildNumber;
        switch (Host)
        {
            case GitHubActions gitHubActions:
                buildNumber = gitHubActions.RunNumber;
                break;
            case AppVeyor appVeyor:
                buildNumber = appVeyor.BuildNumber;
                break;
            case IBuildServer ci:
                throw new NotImplementedException(
                    $"Could not extract build number; CI provider '{ci}' not implemented.");
            default /*IsLocalBuild*/:
                var prerelease = xdoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "PrereleaseLabel" &&
                                                                        !x.HasAttributes)?.Value ?? string.Empty;
                return major + "." + minor + "." + revision + prerelease;
        }

        return major + "." + minor + "." + revision + "." + buildNumber + "-develop";
    }

    public void ExtractTarball(string gzArchiveName, string destFolder)
    {
        using var inStream = File.OpenRead(gzArchiveName);
        using var gzipStream = new GZipInputStream(inStream);
        using var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
        tarArchive.ExtractContents(destFolder);
    }
}