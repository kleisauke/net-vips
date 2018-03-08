using System;
using System.Collections.Generic;
using System.IO;

namespace NetVips.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Base.VipsInit())
            {
                Console.WriteLine("Unable to init libvips");
                Console.ReadLine();
                return;
            }

            // File.WriteAllText("functions.txt", Operation.GenerateAllFunctions());

            Console.WriteLine("libvips " + Base.Version(0) + "." + Base.Version(1) + "." + Base.Version(2));

            Console.WriteLine("Test example program");

            var im = Image.NewFromFile("lichtenstein.jpg");

            // put im at position (100, 100) in a 3000 x 3000 pixel image, 
            // make the other pixels in the image by mirroring im up / down / 
            // left / right, see
            // https://jcupitt.github.io/libvips/API/current/libvips-conversion.html#vips-embed
            im = im.Embed(100, 100, 3000, 3000, new Dictionary<string, object>
            {
                {"extend", Enums.Extend.Mirror}
            });

            // multiply the green (middle) band by 2, leave the other two alone
            im *= new[] {1, 2, 1};

            // make an image from an array constant, convolve with it
            var mask = Image.NewFromArray(new[]
            {
                new[] {-1, -1, -1},
                new[] {-1, 16, -1},
                new[] {-1, -1, -1}
            }, 8);
            im = im.Conv(mask, new Dictionary<string, object>
            {
                {"precision", Enums.Precision.Integer}
            });

            // finally, write the result back to a file on disk
            im.WriteToFile("output.jpg");

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            Console.WriteLine("Test thumbnail");

            var lichtenstein = Image.NewFromFile("lichtenstein.jpg", new Dictionary<string, object>
            {
                {"access", Enums.Access.Sequential}
            });
            Console.WriteLine(lichtenstein.ToString());

            var thumbnail = lichtenstein.ThumbnailImage(200);
            Console.WriteLine(thumbnail.ToString());
            thumbnail.WriteToFile("lichtenstein-thumb.jpg");

            Console.WriteLine("All done!");
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}