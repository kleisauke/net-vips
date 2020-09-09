using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.IO;

namespace NetVips.Samples;

/// <summary>
/// See:
/// https://github.com/kleisauke/net-vips/issues/96
/// https://libvips.github.io/libvips/2019/11/29/True-streaming-for-libvips.html
/// </summary>
public class ThumbnailS3 : ISample
{
    public string Name => "Thumbnail from S3";
    public string Category => "Streaming";

    private const string BucketName = "libvips-packaging";
    private const string KeyName = "zebra.jpg";

    private static readonly RegionEndpoint BucketRegion = RegionEndpoint.EUWest1;

    public async void Execute(string[] args)
    {
        var creds = new AnonymousAWSCredentials();
        using var client = new AmazonS3Client(creds, BucketRegion);

        try
        {
            using var transferUtility = new TransferUtility(client);
            await using var stream = await transferUtility.OpenStreamAsync(BucketName, KeyName);
            using var thumbnail = Image.ThumbnailStream(stream, 300, height: 300);
            await using var output = File.OpenWrite("thumbnail-s3.jpg");
            thumbnail.WriteToStream(output, ".jpg");

            Console.WriteLine("See thumbnail-s3.jpg");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine($"Error encountered. Message: '{e.Message}' when reading object");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unknown encountered on server. Message: '{e.Message}' when reading object");
        }
    }
}