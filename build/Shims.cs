using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Nuke.Common;

public partial class Build
{
    static void Information(string info)
    {
        Logger.Info(info);
    }

    static void Information(string info, params object[] args)
    {
        Logger.Info(info, args);
    }

    public void ExtractTarball(string gzArchiveName, string destFolder)
    {
        using var inStream = File.OpenRead(gzArchiveName);
        using var gzipStream = new GZipInputStream(inStream);
        using var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
        tarArchive.ExtractContents(destFolder);
    }
}