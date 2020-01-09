namespace NetVips.Samples
{
    using Extensions;
    using System.Drawing.Imaging;

    public class GdiConvert : ISample
    {
        public string Name => "GDI Convert";
        public string Category => "Utils";

        public const string Filename = "images/PNG_transparency_demonstration_1.png";

        public string Execute(string[] args)
        {
            var image = Image.NewFromFile(Filename, access: Enums.Access.Sequential);
            var bitmap = image.ToBitmap();
            bitmap.Save("vips-convert.png", ImageFormat.Png);
            var image2 = bitmap.ToVips();
            image2.WriteToFile("gdi-convert.png");

            return "See gdi-convert.jpg";
        }
    }
}