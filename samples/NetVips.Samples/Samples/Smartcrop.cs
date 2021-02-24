namespace NetVips.Samples
{
    using System;

    public class Smartcrop : ISample
    {
        public string Name => "Smartcrop";
        public string Category => "Conversion";

        public const string Filename = "images/equus_quagga.jpg";

        public void Execute(string[] args)
        {
            using var image = Image.Thumbnail(Filename, 300, height: 300, crop: Enums.Interesting.Attention);
            image.WriteToFile("smartcrop.jpg");

            Console.WriteLine("See smartcrop.jpg");
        }
    }
}