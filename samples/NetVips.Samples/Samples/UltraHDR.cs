using System;

namespace NetVips.Samples;

public class UltraHDR : ISample
{
    public string Name => "Ultra HDR cropping";
    public string Category => "Conversion";

    public const string Filename = "images/ultra-hdr.jpg";

    public void Execute(string[] args)
    {
        const int left = 60;
        const int top = 1560;
        const int width = 128;
        const int height = 128;

        using var im = Image.NewFromFile(Filename);
        using var cropped = im.Crop(left, top, width, height);
        using var final = cropped.Mutate(mutable =>
        {
            // Also crop the gainmap, if there is one
            using var gainmap = im.Gainmap;
            if (gainmap != null)
            {
                // The gainmap can be smaller than the image, we must scale the crop area
                double hscale = (double)gainmap.Width / im.Width;
                double vscale = (double)gainmap.Height / im.Height;

                using var x = gainmap.Crop((int)Math.Round(left * hscale), (int)Math.Round(top * vscale),
                    (int)Math.Round(width * hscale), (int)Math.Round(height * vscale));

                // Update the gainmap
                mutable.Set(GValue.ImageType, "gainmap", x);
            }
        });
        final.WriteToFile("ultra-hdr-crop.jpg");

        Console.WriteLine("See ultra-hdr-crop.jpg");
    }
}