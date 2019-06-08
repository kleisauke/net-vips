namespace NetVips.Samples
{
    /// <summary>
    /// From: https://github.com/lovell/sharp/issues/1235#issuecomment-390907151
    /// </summary>
    public class DuotoneFilter : ISample
    {
        public string Name => "Duotone filter";
        public string Category => "Other";

        public const string Filename = "images/equus_quagga.jpg";

        // #C83658 as CIELAB triple
        public double[] Start = { 46.479, 58.976, 15.052 };

        // #D8E74F as CIELAB triple
        public double[] Stop = { 88.12, -23.952, 69.178 };

        public string Execute(string[] args)
        {
            // makes a lut which is a smooth gradient from start colour to stop colour,
            // with start and stop in CIELAB
            var lut = Image.Identity() / 255;
            lut = lut * Stop + (1 - lut) * Start;
            lut = lut.Colourspace("srgb", sourceSpace: "lab");

            var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);

            // the first step to implement a duotone filter is to convert the image
            // to greyscale
            im = im.Colourspace("b-w");

            // loops over the image looking up every pixel value in lut and replacing
            // it with the pre - calculated result
            im = im.Maplut(lut);

            // finally, write the result back to a file on disk
            im.WriteToFile("duotone.jpg");

            return "See duotone.jpg";
        }
    }
}