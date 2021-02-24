namespace NetVips.Samples
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// See: https://github.com/kleisauke/net-vips/issues/58
    /// </summary>
    public class RandomCropper : ISample
    {
        public string Name => "Random cropper";
        public string Category => "Internal";

        public const int TileSize = 256;

        public const string Filename = "images/equus_quagga.jpg";

        public static readonly Random Rnd = new Random();

        public Image RandomCrop(Image image, int tileSize)
        {
            var x = Rnd.Next(0, image.Width);
            var y = Rnd.Next(0, image.Height);

            var width = Math.Min(tileSize, image.Width - x);
            var height = Math.Min(tileSize, image.Height - y);

            return image.Crop(x, y, width, height);
        }

        public void Execute(string[] args)
        {
            using var fileStream = File.OpenRead(Filename);
            using var image = Image.NewFromStream(fileStream);

            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = NetVips.Concurrency},
                i =>
                {
                    using var crop = RandomCrop(image, TileSize);
                    crop.WriteToFile($"x_{i}.png");
                });
        }
    }
}