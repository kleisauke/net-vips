namespace NetVips.Samples
{
    using System;

    public class Emboss : ISample
    {
        public string Name => "Emboss";
        public string Category => "Filter";

        public const string Filename = "images/lichtenstein.jpg";

        public void Execute(string[] args)
        {
            using var im = Image.NewFromFile(Filename);

            // Optionally, Convert the image to greyscale
            //using var mono = im.Colourspace(Enums.Interpretation.Bw);

            // The four primary emboss kernels.
            // Offset the pixel values by 128 to achieve the emboss effect.
            using var kernel1 = Image.NewFromArray(new[,]
            {
                {0, 1, 0},
                {0, 0, 0},
                {0, -1, 0}
            }, offset: 128);
            using var kernel2 = Image.NewFromArray(new[,]
            {
                {1, 0, 0},
                {0, 0, 0},
                {0, 0, -1}
            }, offset: 128);
            using var kernel3 = kernel1.Rot270();
            using var kernel4 = kernel2.Rot90();

            // Apply the emboss kernels
            using var conv1 = /*mono*/im.Conv(kernel1, precision: Enums.Precision.Float);
            using var conv2 = /*mono*/im.Conv(kernel2, precision: Enums.Precision.Float);
            using var conv3 = /*mono*/im.Conv(kernel3, precision: Enums.Precision.Float);
            using var conv4 = /*mono*/im.Conv(kernel4, precision: Enums.Precision.Float);

            var images = new[]
            {
                conv1,
                conv2,
                conv3,
                conv4
            };

            using var joined = Image.Arrayjoin(images, across: 2);
            joined.WriteToFile("emboss.jpg");

            Console.WriteLine("See emboss.jpg");
        }
    }
}