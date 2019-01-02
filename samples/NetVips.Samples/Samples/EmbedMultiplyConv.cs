namespace NetVips.Samples
{
    /// <summary>
    /// From: https://github.com/libvips/ruby-vips#example
    /// </summary>
    public class EmbedMultiplyConv : ISample
    {
        public string Name => "Embed / Multiply / Convolution";
        public string Category => "Other";

        public const string Filename = "images/lichtenstein.jpg";

        public string Execute(string[] args)
        {
            var im = Image.NewFromFile(Filename);

            // put im at position (100, 100) in a 3000 x 3000 pixel image, 
            // make the other pixels in the image by mirroring im up / down / 
            // left / right, see
            // https://libvips.github.io/libvips/API/current/libvips-conversion.html#vips-embed
            im = im.Embed(100, 100, 3000, 3000, extend: Enums.Extend.Mirror);

            // multiply the green (middle) band by 2, leave the other two alone
            im *= new[] { 1, 2, 1 };

            // make an image from an array constant, convolve with it
            var mask = Image.NewFromArray(new[,]
            {
                {-1, -1, -1},
                {-1, 16, -1},
                {-1, -1, -1}
            }, 8);
            im = im.Conv(mask, precision: Enums.Precision.Integer);

            // finally, write the result back to a file on disk
            im.WriteToFile("embed-multiply-conv.jpg");

            return "See embed-multiply-conv.jpg";
        }
    }
}