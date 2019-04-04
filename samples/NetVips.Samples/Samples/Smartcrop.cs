namespace NetVips.Samples
{
    public class Smartcrop : ISample
    {
        public string Name => "Smartcrop";
        public string Category => "Conversion";

        public const string Filename = "images/equus_quagga.jpg";

        public string Execute(string[] args)
        {
            var image = Image.Thumbnail(Filename, 300, height: 300, crop: "attention");
            image.WriteToFile("smartcrop.jpg");

            return "See smartcrop.jpg";
        }
    }
}