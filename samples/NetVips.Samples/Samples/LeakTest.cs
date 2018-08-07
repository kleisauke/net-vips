using System;
using System.IO;

namespace NetVips.Samples
{
    /// <summary>
    /// From: https://raw.githubusercontent.com/jcupitt/node-vips/master/example/thumb.js
    /// </summary>
    public class LeakTest : ISample
    {
        public string Name => "Leak test";
        public string Category => "Internal";

        /// <summary>
        /// Thumbnail many images, either from a file source or memory buffer.
        /// 
        /// It runs in a fairly steady 1gb of ram for me. Watching the output, you see
        /// stuff like:
        /// 
        /// memory processing /images/sample/7350.jpg
        /// (115 vips objects known to net-vips)
        /// memory processing /images/sample/7351.jpg
        /// (6 vips objects known to net-vips)
        /// memory processing /images/sample/7352.jpg
        /// (11 vips objects known to net-vips)
        /// memory processing /images/sample/7353.jpg
        /// (16 vips objects known to net-vips)
        /// 
        /// So when around 100 vips objects are alive, the C# gc runs and they all get
        /// flushed.
        /// 
        /// If you want it to run in less ram than that, you'll need to expose the gc and
        /// trigger it manually every so often.
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Result</returns>
        public string Execute(string[] args)
        {
            if (args.Length == 0)
            {
                return "No directory given";
            }

            Base.LeakSet(1);

            foreach (var file in Directory.GetFiles(args[0]))
            {
                Console.WriteLine($"memory processing {file}");
                ViaMemory(file, 500);
                // uncomment this line together with the `NObjects` variable in GObject
                // Console.WriteLine($"{GObject.NObjects} vips objects known to net-vips");
            }

            return "All done!";
        }

        /// <summary>
        /// Benchmark thumbnail via memory
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="thumbnailWidth"></param>
        public void ViaMemory(string filename, int thumbnailWidth)
        {
            var data = File.ReadAllBytes(filename);
            var thumb = Image.ThumbnailBuffer(data, thumbnailWidth, crop: Enums.Align.Centre);

            // don't do anything with the result, this is just a test
            thumb.WriteToBuffer(".jpg");
        }

        /// <summary>
        /// Benchmark thumbnail via files
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="thumbnailWidth"></param>
        public void ViaFiles(string filename, int thumbnailWidth)
        {
            var thumb = Image.Thumbnail(filename, thumbnailWidth, crop: Enums.Align.Centre);

            // don't do anything with the result, this is just a test
            thumb.WriteToBuffer(".jpg");
        }
    }
}