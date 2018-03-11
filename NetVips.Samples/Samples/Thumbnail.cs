namespace NetVips.Samples.Samples
{
    public class Thumbnail : ISample
    {
        public string Name => "Thumbnail";
        public string Category => "Resample";

        public const string Filename = "images/lichtenstein.jpg";

        public string Execute(string[] args)
        {
            var image = Image.Thumbnail(Filename, 300, height: 300);
            image.WriteToFile("thumbnail.jpg");

            return "See thumbnail.jpg";
        }
    }
}