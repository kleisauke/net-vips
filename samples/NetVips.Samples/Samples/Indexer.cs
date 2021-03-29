namespace NetVips.Samples
{
    using System.Diagnostics;

    /// <summary>
    /// From: https://github.com/libvips/php-vips/blob/d3cf61bd087bd9c00cfc5ae32f6ded7165963058/tests/ShortcutTest.php#L202
    /// </summary>
    public class Indexer : ISample
    {
        public string Name => "Indexer";
        public string Category => "Create";

        public void Execute(string[] args)
        {
            using var r = Image.NewFromArray(new[] { 1, 2, 3 });
            using var g = r + 1;
            using var b = r + 2;
            using var image = r.Bandjoin(g, b);

            // replace band with image
            using var array = image.NewFromImage(12, 13);
            using var test = image.Mutate(x => x[1] = array);
            using var band1 = test[0];
            using var band2 = test[1];
            using var band3 = test[2];
            using var band4 = test[3];

            Debug.Assert(test.Bands == 4);
            Debug.Assert(band1.Avg() == 2);
            Debug.Assert(band2.Avg() == 12);
            Debug.Assert(band3.Avg() == 13);
            Debug.Assert(band4.Avg() == 4);
        }
    }
}