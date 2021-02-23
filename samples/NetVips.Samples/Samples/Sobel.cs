namespace NetVips.Samples
{
    public class Sobel : ISample
    {
        public string Name => "Sobel";
        public string Category => "Edge detection";

        public const string Filename = "images/lichtenstein.jpg";

        public string Execute(string[] args)
        {
            var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);

            // Optionally, convert to greyscale
            // im = im.Colourspace(Enums.Interpretation.Bw);

            // Apply sobel operator
            im = im.Sobel();

            im.WriteToFile("sobel.jpg");

            return "See sobel.jpg";
        }
    }
}