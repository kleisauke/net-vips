namespace NetVips.Samples
{
    using System;

    public class Canny : ISample
    {
        public string Name => "Canny";
        public string Category => "Edge detection";

        public const string Filename = "images/lichtenstein.jpg";

        public void Execute(string[] args)
        {
            using var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);

            // Optionally, convert to greyscale
            //using var mono = im.Colourspace(Enums.Interpretation.Bw);

            // Canny edge detector
            using var canny = /*mono*/im.Canny(1.4, precision: Enums.Precision.Integer);

            // Canny makes a float image, scale the output up to make it visible.
            using var scale = canny * 64;

            scale.WriteToFile("canny.jpg");

            Console.WriteLine("See canny.jpg");
        }
    }
}