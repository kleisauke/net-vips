namespace NetVips.Samples
{
    using System;
    using System.IO;

    /// <summary>
    /// From: https://github.com/kleisauke/net-vips/issues/26
    /// </summary>
    public class LeakTest : ISample
    {
        public string Name => "Leak test";
        public string Category => "Internal";

        public const string Filename = "images/equus_quagga.jpg";

        /// <summary>
        /// Load from memory buffer 10000 times.
        ///
        /// It runs in a fairly steady 60MB of ram for me. Watching the output, you see
        /// stuff like:
        ///
        /// memory processing &lt;NetVips.Image 4120x2747 uchar, 3 bands, srgb&gt;
        /// 24 vips objects known to net-vips
        /// memory processing &lt;NetVips.Image 4120x2747 uchar, 3 bands, srgb&gt;
        /// 21 vips objects known to net-vips
        /// memory processing &lt;NetVips.Image 4120x2747 uchar, 3 bands, srgb&gt;
        /// 23 vips objects known to net-vips
        /// memory processing &lt;NetVips.Image 4120x2747 uchar, 3 bands, srgb&gt;
        /// 16 vips objects known to net-vips
        /// memory processing &lt;NetVips.Image 4120x2747 uchar, 3 bands, srgb&gt;
        /// 7 vips objects known to net-vips
        /// memory processing &lt;NetVips.Image 4120x2747 uchar, 3 bands, srgb&gt;
        /// 7 vips objects known to net-vips
        /// memory processing &lt;NetVips.Image 4120x2747 uchar, 3 bands, srgb&gt;
        /// 9 vips objects known to net-vips
        /// memory processing &lt;NetVips.Image 4120x2747 uchar, 3 bands, srgb&gt;
        /// 4 vips objects known to net-vips
        /// memory processing &lt;NetVips.Image 4120x2747 uchar, 3 bands, srgb&gt;
        ///
        /// So when around 25 vips objects are alive, the C# gc runs and they all get
        /// flushed.
        ///
        /// If you want it to run in less ram than that, you'll need to expose the GC and
        /// trigger it manually every so often.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>Result.</returns>
        public void Execute(string[] args)
        {
            NetVips.Leak = true;

            Cache.Max = 0;

            var imageBytes = File.ReadAllBytes(Filename);

            for (var i = 0; i < 10000; i++)
            {
                using var img = Image.NewFromBuffer(imageBytes);
                Console.WriteLine($"memory processing {img}");
                // uncomment this line together with the `NObjects` variable in GObject
                // Console.WriteLine($"{GObject.NObjects} vips objects known to net-vips");
            }
        }
    }
}