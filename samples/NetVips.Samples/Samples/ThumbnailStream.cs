namespace NetVips.Samples
{
    using System;
    using System.IO;

    public class ThumbnailStream : ISample
    {
        public string Name => "Thumbnail a file stream";
        public string Category => "Streaming";

        public const string Filename = "images/lichtenstein.jpg";

        public void Execute(string[] args)
        {
            using var input = File.OpenRead(Filename);
            using var thumbnail = Image.ThumbnailStream(input, 300, height: 300);
            Console.WriteLine(thumbnail.ToString());

            using var output = File.OpenWrite("thumbnail-stream.jpg");
            thumbnail.WriteToStream(output, ".jpg");

            Console.WriteLine("See thumbnail-stream.jpg");
        }
    }
}