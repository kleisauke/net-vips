namespace NetVips.Samples
{
    using System;

    /// <summary>
    /// From: https://github.com/libvips/ruby-vips#example
    /// </summary>
    public class EmbedMultiplyConv : ISample
    {
        public string Name => "Embed / Multiply / Convolution";
        public string Category => "Other";

        public const string Filename = "images/lichtenstein.jpg";

        public void Execute(string[] args)
        {
            using var im = Image.NewFromFile(Filename);

            // put im at position (100, 100) in a 3000 x 3000 pixel image,
            // make the other pixels in the image by mirroring im up / down /
            // left / right, see
            // https://www.libvips.org/API/current/libvips-conversion.html#vips-embed
            using var embed = im.Embed(100, 100, 3000, 3000, extend: Enums.Extend.Mirror);

            // multiply the green (middle) band by 2, leave the other two alone
            using var multiply = embed * new[] { 1, 2, 1 };

            // make an image from an array constant, convolve with it
            using var mask = Image.NewFromArray(new[,]
            {
                {-1, -1, -1},
                {-1, 16, -1},
                {-1, -1, -1}
            }, 8);
            using var convolve = multiply.Conv(mask, precision: Enums.Precision.Integer);

            // finally, write the result back to a file on disk
            convolve.WriteToFile("embed-multiply-conv.jpg");

            Console.WriteLine("See embed-multiply-conv.jpg");
        }
    }
}