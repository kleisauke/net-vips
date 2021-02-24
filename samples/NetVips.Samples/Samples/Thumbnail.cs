namespace NetVips.Samples
{
    using System;

    public class Thumbnail : ISample
    {
        public string Name => "Thumbnail";
        public string Category => "Resample";

        public const string Filename = "images/lichtenstein.jpg";

        public void Execute(string[] args)
        {
            using var image = Image.Thumbnail(Filename, 300, height: 300);
            image.WriteToFile("thumbnail.jpg");

            Console.WriteLine("See thumbnail.jpg");
        }
    }
}