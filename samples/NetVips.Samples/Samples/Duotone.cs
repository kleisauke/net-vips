namespace NetVips.Samples
{
    using System;

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

        public void Execute(string[] args)
        {
            // Makes a lut which is a smooth gradient from start colour to stop colour,
            // with start and stop in CIELAB
            using var identity = Image.Identity();
            using var index = identity / 255;
            using var stop = index * Stop;
            using var inverse = 1 - index;
            using var start = inverse * Start;
            using var gradient = stop + start;
            using var lut = gradient.Colourspace(Enums.Interpretation.Srgb, sourceSpace: Enums.Interpretation.Lab);

            var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);

            // The first step to implement a duotone filter is to convert the
            // image to greyscale. The image is then mapped through the lut.
            // Mapping is done by looping over the image and looking up each
            // pixel value in the lut and replacing it with the pre-calculated
            // result.
            if (im.HasAlpha())
            {
                // Separate alpha channel
                using var withoutAlpha = im.ExtractBand(0, im.Bands - 1);
                using var alpha = im[im.Bands - 1];
                using var mono = withoutAlpha.Colourspace(Enums.Interpretation.Bw);
                using var mapped = mono.Maplut(lut);
                using (im)
                {
                    im = mapped.Bandjoin(alpha);
                }
            }
            else
            {
                using var mono = im.Colourspace(Enums.Interpretation.Bw);
                using (im)
                {
                    im = mono.Maplut(lut);
                }
            }

            using (im)
            {
                // Finally, write the result back to a file on disk
                im.WriteToFile("duotone.jpg");
            }

            Console.WriteLine("See duotone.jpg");
        }
    }
}