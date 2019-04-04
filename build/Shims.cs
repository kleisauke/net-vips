using System.IO;
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
        Stream inStream = File.OpenRead(gzArchiveName);
        Stream gzipStream = new GZipInputStream(inStream);

        var tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
        tarArchive.ExtractContents(destFolder);
        tarArchive.Close();

        gzipStream.Close();
        inStream.Close();
    }
}