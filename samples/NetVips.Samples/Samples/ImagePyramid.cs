namespace NetVips.Samples
{
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

            // Save image pyramid
            im.Dzsave("images/image-pyramid");

            return "See images/image-pyramid.dzi";
        }
    }
}