namespace NetVips.Samples
{
    /// <summary>
    /// From: https://github.com/libvips/lua-vips/blob/master/example/combine.lua
    /// </summary>
    public class Combine : ISample
    {
        public string Name => "Combine";
        public string Category => "Conversion";

        public const string MainFilename = "images/Gugg_coloured.jpg";
        public const string WatermarkFilename = "images/PNG_transparency_demonstration_1.png";

        public const int Left = 100;
        public const int Top = 100;

        public string Execute(string[] args)
        {
            var main = Image.NewFromFile(MainFilename);
            var watermark = Image.NewFromFile(WatermarkFilename);

            var width = watermark.Width;
            var height = watermark.Height;

            // extract related area from main image
            var baseImage = main.Crop(Left, Top, width, height);

            // composite the two areas using the PDF "over" mode
            var composite = baseImage.Composite(watermark, Enums.BlendMode.Over);

            // the result will have an alpha, and our base image does not..we must flatten
            // out the alpha before we can insert it back into a plain RGB JPG image
            composite = composite.Flatten();

            // insert composite back in to main image on related area
            var combined = main.Insert(composite, Left, Top);

            combined.WriteToFile("combine.jpg");

            return "See combine.jpg";
        }
    }
}