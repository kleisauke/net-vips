namespace NetVips.Samples
{
    using System;
    using System.IO;

    public class Stream : ISample
    {
        public string Name => "File stream";
        public string Category => "Streaming";

        public const string Filename = "images/PNG_transparency_demonstration_1.png";

        public void Execute(string[] args)
        {
            using var input = File.OpenRead(Filename);

            using var image = Image.NewFromStream(input, access: Enums.Access.Sequential);
            Console.WriteLine(image.ToString());

            using var output = File.OpenWrite("stream.png");
            image.WriteToStream(output, ".png");

            Console.WriteLine("See stream.png");
        }
    }
}