namespace NetVips.Samples
{
    using System;

    public class OperationBlock : ISample
    {
        public string Name => "Operation block";
        public string Category => "Utils";

        public const string JpegFilename = "images/lichtenstein.jpg";
        public const string PngFilename = "images/PNG_transparency_demonstration_1.png";

        public void Execute(string[] args)
        {
            // Block all load operations, except JPEG
            Operation.Block("VipsForeignLoad", true);
            Operation.Block("VipsForeignLoadJpeg", false);

            // JPEG images should work
            using var image = Image.NewFromFile(JpegFilename, access: Enums.Access.Sequential);
            Console.WriteLine($"JPEG average: {image.Avg()}");

            // But PNG images should fail
            try
            {
                using var image2 = Image.NewFromFile(PngFilename, access: Enums.Access.Sequential);
                Console.WriteLine($"PNG average: {image2.Avg()}");
            }
            catch (VipsException exception)
            {
                Console.WriteLine(exception.Message);
            }

            // Re-enable all loaders
            Operation.Block("VipsForeignLoad", false);
        }
    }
}