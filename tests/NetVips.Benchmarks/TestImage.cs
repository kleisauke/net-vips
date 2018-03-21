using System.IO;

namespace NetVips.Benchmarks
{
    public class TestImage
    {
        // much larger than this and im falls over with cache resources exhausted
        public const int TileSize = 5;

        public static void BuildTestImages(string outputDir)
        {
            var outputFile = Path.Combine(outputDir, "t.v");

            // building test image
            var im = Image.NewFromFile(Path.Combine(outputDir, "sample2.v"));
            im = im.Replicate(TileSize, TileSize);
            im.WriteToFile(outputFile);

            // making tiff and jpeg derivatives
            im = Image.NewFromFile(outputFile);
            im.WriteToFile(Path.Combine(outputDir, "t.tif"));

            im = Image.NewFromFile(outputFile);
            im.WriteToFile(Path.Combine(outputDir, "t.jpg"));
        }
    }
}