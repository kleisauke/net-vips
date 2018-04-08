using System;
using BenchmarkDotNet.Attributes;
using ImageMagick;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Convolution;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace NetVips.Benchmarks
{
    [Config(typeof(Config))]
    public class Benchmark
    {
        [GlobalSetup]
        public void GlobalSetup()
        {
            // Turn off OpenCL acceleration
            OpenCL.IsEnabled = false;
        }

        [Benchmark(Description = "NetVips", Baseline = true)]
        [Arguments("t.tif", "t2.tif")]
        [Arguments("t.jpg", "t2.jpg")]
        public void NetVips(string input, string output)
        {
            var im = Image.NewFromFile(input, access: Enums.Access.Sequential);

            im = im.Crop(100, 100, im.Width - 200, im.Height - 200);
            im = im.Reduce(1.0 / 0.9, 1.0 / 0.9, kernel: Enums.Kernel.Linear);
            var mask = Image.NewFromArray(new[,]
            {
                {-1, -1, -1},
                {-1, 16, -1},
                {-1, -1, -1}
            }, 8);
            im = im.Conv(mask, precision: Enums.Precision.Integer);

            im.WriteToFile(output);
        }

        [Benchmark(Description = "Magick.NET")]
        [Arguments("t.tif", "t2.tif")]
        [Arguments("t.jpg", "t2.jpg")]
        public void MagickNet(string input, string output)
        {
            using (var im = new MagickImage(input))
            {
                im.Shave(100, 100);
                im.Resize(new Percentage(90.0));

                // All values in the kernel are divided by 8 (to match libvips scale behavior)
                var kernel = new ConvolveMatrix(3, -0.125, -0.125, -0.125, -0.125, 2, -0.125, -0.125, -0.125, -0.125);
                im.Convolve(kernel);

                im.Write(output);
            }
        }

        [Benchmark(Description = "ImageSharp")]
        [Arguments("t.jpg", "t2.jpg")] // ImageSharp doesn't have TIFF support
        public void ImageSharp(string input, string output)
        {
            using (var image = SixLabors.ImageSharp.Image.Load(input))
            {
                image.Mutate(x => x
                    .Crop(new Rectangle(100, 100, image.Width - 200, image.Height - 200))
                    .Resize(new Size((int) Math.Round(image.Width * .9F), (int) Math.Round(image.Height * .9F)))
                    .GaussianSharpen(.75f));

                image.Save(output);
            }
        }
    }
}