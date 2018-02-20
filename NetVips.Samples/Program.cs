using NetVips;
using System;
using static NetVips.vips;
using static NetVips.header;
using static NetVips.image;

namespace NetVips_Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            // This sample application demonstrates how to use the generated libvips PInvoke layer to execute a sample script.
            // Although a higher level object model in .NET can be easily created that eliminates the need for boilerplate code,
            // this sample focuses on direct, low-level usage of the libvips API.

            if (VipsInit("NetVips") != 0)
            {
                Console.WriteLine("Unable to init libvips");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("libvips " + VipsVersionString());
            Console.WriteLine("Test load image");

            VipsImage image = VipsImageNewFromFile("lichtenstein.jpg");

            if (image == null)
            {
                Console.WriteLine("Could not load image from file: " + error.VipsErrorBuffer());
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Image.Bands = " + image.Bands);
            Console.WriteLine("Image.Width = " + VipsImageGetWidth(image)); // TODO: Fix image.Width
            Console.WriteLine("Image.Height = " + VipsImageGetHeight(image)); // TODO: Fix image.Height
            Console.WriteLine();

            Console.WriteLine("Test write image");
            VipsImageWriteToFile(image, "lichtenstein2.jpg");

            VipsShutdown();

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}