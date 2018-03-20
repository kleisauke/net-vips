using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using ImageMagick;

namespace NetVips.Benchmarks
{
    [Config(typeof(Config))]
    public class Benchmark
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                var variables = new[]
                {
                    new EnvironmentVariable("OutputDir", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                };

                Add(Job.Core.With(variables));
            }
        }

        private readonly string _outputDir = Environment.GetEnvironmentVariable("OutputDir");

        // much larger than this and im falls over with cache resources exhausted
        public const int TileSize = 5;

        [ParamsSource(nameof(InputFiles))] public string InputFile;

        [ParamsSource(nameof(OutputFiles))] public string OutputFile;

        public IEnumerable<string> InputFiles =>
            new[] {"t.tif", "t.jpg"};

        public IEnumerable<string> OutputFiles =>
            new[] {"t2.tif", "t2.jpg"};

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Disable OpenCL
            OpenCL.IsEnabled = false;

            var tmpDir = Path.Combine(_outputDir, "tmp");
            var imageDir = Path.Combine(_outputDir, "Images");
            var outputFile = Path.Combine(tmpDir, "t.v");

            Directory.CreateDirectory(tmpDir);

            // building test image
            var im = Image.NewFromFile(Path.Combine(imageDir, "sample2.v"));
            im = im.Replicate(TileSize, TileSize);
            im.WriteToFile(outputFile);

            // making tiff and jpeg derivatives
            im = Image.NewFromFile(outputFile);
            im.WriteToFile(Path.Combine(tmpDir, "t.tif"));

            im = Image.NewFromFile(outputFile);
            im.WriteToFile(Path.Combine(tmpDir, "t.jpg"));
        }

        [Benchmark(Description = "NetVips", Baseline = true)]
        public void NetVips()
        {
            var im = Image.NewFromFile(Path.Combine(_outputDir, "tmp", InputFile), access: Enums.Access.Sequential);

            im = im.Crop(100, 100, im.Width - 200, im.Height - 200);
            im = im.Similarity(scale: 0.9);
            var mask = Image.NewFromArray(new[,]
            {
                {-1, -1, -1},
                {-1, 16, -1},
                {-1, -1, -1}
            }, 8);
            im = im.Conv(mask, precision: Enums.Precision.Integer);

            im.WriteToFile(Path.Combine(_outputDir, "tmp", OutputFile));
        }

        [Benchmark(Description = "Magick.NET")]
        public void MagickNet()
        {
            using (var im = new MagickImage(Path.Combine(_outputDir, "tmp", InputFile)))
            {
                im.Shave(100, 100);
                im.Resize(new Percentage(90.0));

                // All values in the kernel are divided by 8 (to match libvips scale behavior)
                var kernel = new ConvolveMatrix(3, -0.125, -0.125, -0.125, -0.125, 2, -0.125, -0.125, -0.125, -0.125);
                im.Convolve(kernel);

                im.Write(Path.Combine(_outputDir, "tmp", OutputFile));
            }
        }
    }
}