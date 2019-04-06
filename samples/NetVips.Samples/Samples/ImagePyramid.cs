namespace NetVips.Samples
{
    using System;
    using System.Threading;

    public class ImagePyramid : ISample
    {
        public string Name => "Image Pyramid";
        public string Category => "Create";

        public const int TileSize = 50;
        public const string Filename = "images/sample2.v";

        public string Execute(string[] args)
        {
            // Build test image
            var im = Image.NewFromFile(Filename, access: Enums.Access.Sequential);
            im = im.Replicate(TileSize, TileSize);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(5000);

            var progress = new Progress<int>(percent =>
            {
                Console.Write($"\r{percent}% complete");
            });
            // Uncomment to kill the image after 5 sec
            im.SetProgress(progress/*, cts.Token*/);

            try
            {
                // Save image pyramid
                im.Dzsave("images/image-pyramid");
            }
            catch (VipsException exception) {
                // Catch and log the VipsException, 
                // because we may block the evaluation of this image
                Console.WriteLine("\n" + exception.Message);
            }

            Console.WriteLine();
            return "See images/image-pyramid.dzi";
        }
    }
}