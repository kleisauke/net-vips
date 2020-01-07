namespace NetVips.Samples
{
    public class Emboss : ISample
    {
        public string Name => "Emboss";
        public string Category => "Filter";

        public const string Filename = "images/lichtenstein.jpg";

        public string Execute(string[] args)
        {
            var im = Image.NewFromFile(Filename);

            // Optionally, Convert the image to greyscale
            // im = im.Colourspace("b-w");

            // The four primary emboss kernels.
            // Offset the pixel values by 128 to achieve the emboss effect.
            var kernel1 = Image.NewFromArray(new[,]
            {
                {0, 1, 0},
                {0, 0, 0},
                {0, -1, 0}
            }, offset: 128);
            var kernel2 = Image.NewFromArray(new[,]
            {
                {1, 0, 0},
                {0, 0, 0},
                {0, 0, -1}
            }, offset: 128);
            var kernel3 = kernel1.Rot270();
            var kernel4 = kernel2.Rot90();

            var images = new[]
            {
                // Apply the emboss kernels
                im.Conv(kernel1, precision: Enums.Precision.Float),
                im.Conv(kernel2, precision: Enums.Precision.Float),
                im.Conv(kernel3, precision: Enums.Precision.Float),
                im.Conv(kernel4, precision: Enums.Precision.Float)
            };

            Image.Arrayjoin(images, across: 2).WriteToFile("emboss.jpg");

            return "See emboss.jpg";
        }
    }
}