using NetVips.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetVips.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (vips.VipsInit("NetVips") != 0)
            {
                Console.WriteLine("Unable to init libvips");
                Console.ReadLine();
                return;
            }

            // File.WriteAllText("functions.txt", Operation.GenerateAllFunctions());

            Console.WriteLine("libvips " + Base.Version(0) + "." + Base.Version(1) + "." + Base.Version(2));

            var array = Enumerable.Repeat(0, 200).ToArray();
            var memory = Image.NewFromMemory(array, 20, 10, 1, "uchar");
            Console.WriteLine(memory.ToString());
            Console.WriteLine(memory.Avg());

            // TODO Doesn't work (memory corruption)
            /*memory += 10;
            Console.WriteLine(memory.Avg());*/

            Console.WriteLine("Test load image");

            var lichtenstein = Image.NewFromFile("lichtenstein.jpg", new Dictionary<string, object>
            {
                {"access", "sequential"}
            });
            Console.WriteLine(lichtenstein.ToString());

            // TODO: Doesn't work, hangs on `VipsCacheOperationBuild` (due to GC?)
            lichtenstein[0].WriteToFile("lichtenstein-first-band.jpg");
            lichtenstein.WriteToFile("lichtenstein-original.jpg");

            var thumbnail = lichtenstein.ThumbnailImage(200);

            Console.WriteLine(thumbnail.ToString());

            // TODO: Doesn't work, hangs on `VipsCacheOperationBuild` (due to GC?)
            thumbnail.WriteToFile("lichtenstein-thumb.jpg");

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}