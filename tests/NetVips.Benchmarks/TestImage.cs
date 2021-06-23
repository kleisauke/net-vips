namespace NetVips.Benchmarks
{
    using System;
    using System.IO;

    public class TestImage
    {
        // Much larger than this and im falls over with cache resources exhausted
        public const int TargetDimension = 5000;

        public static void BuildTestImages(string outputDir)
        {
            var targetTiff = Path.Combine(outputDir, "t.tif");
            var targetJpeg = Path.Combine(outputDir, "t.jpg");

            // Do not build test images if they are already present
            if (File.Exists(targetTiff) && File.Exists(targetJpeg))
            {
                return;
            }

            var outputFile = Path.Combine(outputDir, "t.v");

            // Build test image
            var im = Image.NewFromFile(Path.Combine(outputDir, "sample2.v"));
            im = im.Replicate((int)Math.Ceiling((double)TargetDimension / im.Width),
                (int)Math.Ceiling((double)TargetDimension / im.Height));
            im = im.ExtractArea(0, 0, TargetDimension, TargetDimension);
            im.WriteToFile(outputFile);

            // Make tiff and jpeg derivatives
            im = Image.NewFromFile(outputFile);
            im.Tiffsave(targetTiff, tile: true);

            im = Image.NewFromFile(outputFile);
            im.Jpegsave(targetJpeg);
        }
    }
}