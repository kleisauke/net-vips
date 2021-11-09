namespace NetVips.Samples
{
    using System;
    using Extensions;
    using System.Drawing.Imaging;
    using System.Runtime.Versioning;

    [SupportedOSPlatform("windows")]
    public class GdiConvert : ISample
    {
        public string Name => "GDI Convert";
        public string Category => "Utils";

        public const string Filename = "images/PNG_transparency_demonstration_1.png";

        public void Execute(string[] args)
        {
            var bitmap = new System.Drawing.Bitmap(Filename);

            // 24bpp -> 32bppArgb
            /*using var bitmap32Argb = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap32Argb))
            {
                graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
            }
            using (bitmap)
            {
                bitmap = bitmap32Argb;
            }*/

            // 24bpp -> 32bppRgb
            /*using var bitmap32Rgb = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppRgb);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap32Rgb))
            {
                graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
            }
            using (bitmap)
            {
                bitmap = bitmap32Rgb;
            }*/

            // 24bpp -> 48bppRgb
            /*using var bitmap48Rgb = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format48bppRgb);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap48Rgb))
            {
                graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
            }
            using (bitmap)
            {
                bitmap = bitmap48Rgb;
            }*/

            // 24bpp -> 64bppArgb
            /*using var bitmap64Argb = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format64bppArgb);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap64Argb))
            {
                graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
            }
            using (bitmap)
            {
                bitmap = bitmap64Argb;
            }*/

            bitmap.Save("gdi-original.png", ImageFormat.Png);

            using var vipsImage = bitmap.ToVips();
            vipsImage.WriteToFile("gdi-to-vips.png");

            using (bitmap)
            {
                bitmap = vipsImage.ToBitmap();
            }

            bitmap.Save("vips-to-gdi.png", ImageFormat.Png);

            /*using var vipsImage2 = Image.NewFromFile(Filename, access: Enums.Access.Sequential);
            using (bitmap)
            {
                bitmap = vipsImage.ToBitmap();
            }
            bitmap.Save("vips-to-gdi2.png", ImageFormat.Png);
            using var image2 = bitmap.ToVips();
            image2.WriteToFile("gdi-to-vips2.png");*/

            Console.WriteLine("See gdi-to-vips.png");
        }
    }
}