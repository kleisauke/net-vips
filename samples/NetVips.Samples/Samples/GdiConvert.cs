namespace NetVips.Samples
{
    using Extensions;

    public class GdiConvert : ISample
    {
        public string Name => "GDI Convert";
        public string Category => "Utils";

        public const string Filename = "images/equus_quagga.jpg";

        public string Execute(string[] args)
        {
            var image = Image.NewFromFile(Filename, access: Enums.Access.Sequential);
            var bitmap = image.ToBitmap();
            var image2 = bitmap.ToVips();
            image2.WriteToFile("gdi-convert.jpg");

            return "See gdi-convert.jpg";
        }
    }
}