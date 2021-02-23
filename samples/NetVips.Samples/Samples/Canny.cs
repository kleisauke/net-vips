namespace NetVips.Samples
{
    public class Canny : ISample
    {
        public string Name => "Canny";
        public string Category => "Edge detection";

        public const string Filename = "images/lichtenstein.jpg";

        public string Execute(string[] args)
        {
            var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);

            // Optionally, convert to greyscale
            // im = im.Colourspace(Enums.Interpretation.Bw);

            // Canny edge detector
            im = im.Canny(1.4, precision: Enums.Precision.Integer);

            // Canny makes a float image, scale the output up to make it visible.
            im *= 64;

            im.WriteToFile("canny.jpg");

            return "See canny.jpg";
        }
    }
}