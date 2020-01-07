namespace NetVips.Samples
{
    /// <summary>
    /// From: https://github.com/lovell/sharp/issues/1235#issuecomment-390907151
    /// </summary>
    public class Duotone : ISample
    {
        public string Name => "Duotone";
        public string Category => "Filter";

        public const string Filename = "images/equus_quagga.jpg";

        // #C83658 as CIELAB triple
        public double[] Start = { 46.479, 58.976, 15.052 };

        // #D8E74F as CIELAB triple
        public double[] Stop = { 88.12, -23.952, 69.178 };

        public string Execute(string[] args)
        {
            // Makes a lut which is a smooth gradient from start colour to stop colour,
            // with start and stop in CIELAB
            var lut = Image.Identity() / 255;
            lut = lut * Stop + (1 - lut) * Start;
            lut = lut.Colourspace("srgb", sourceSpace: "lab");

            var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);

            // The first step to implement a duotone filter is to convert the
            // image to greyscale. The image is then mapped through the lut.
            // Mapping is done by looping over the image and looking up each
            // pixel value in the lut and replacing it with the pre-calculated
            // result.
            if (im.HasAlpha())
            {
                // Separate alpha channel
                var withoutAlpha = im.ExtractBand(0, im.Bands - 1);
                var alpha = im[im.Bands - 1];
                im = withoutAlpha.Colourspace(Enums.Interpretation.Bw)
                    .Maplut(lut)
                    .Bandjoin(alpha);
            }
            else
            {
                im = im.Colourspace(Enums.Interpretation.Bw).Maplut(lut);
            }

            // Finally, write the result back to a file on disk
            im.WriteToFile("duotone.jpg");

            return "See duotone.jpg";
        }
    }
}