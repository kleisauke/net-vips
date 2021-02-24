namespace NetVips.Samples
{
    using System;

    public class Sobel : ISample
    {
        public string Name => "Sobel";
        public string Category => "Edge detection";

        public const string Filename = "images/lichtenstein.jpg";

        public void Execute(string[] args)
        {
            using var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);

            // Optionally, convert to greyscale
            //using var mono = im.Colourspace(Enums.Interpretation.Bw);

            // Apply sobel operator
            using var sobel = /*mono*/im.Sobel();
            sobel.WriteToFile("sobel.jpg");

            Console.WriteLine("See sobel.jpg");
        }
    }
}