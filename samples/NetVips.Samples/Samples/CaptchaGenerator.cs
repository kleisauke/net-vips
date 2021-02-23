namespace NetVips.Samples
{
    using System;
    using System.Linq;

    /// <summary>
    /// From: https://github.com/libvips/libvips/issues/898
    /// </summary>
    public class CaptchaGenerator : ISample
    {
        public string Name => "Captcha generator";
        public string Category => "Create";

        public const string Text = "Hello World";

        /// <summary>
        /// A warp image is a 2D grid containing the new coordinates of each pixel with
        /// the new x in band 0 and the new y in band 1
        ///
        /// you can also use a complex image
        ///
        /// start from a low-res XY image and distort it
        /// </summary>
        /// <param name="image"><see cref="Image"/> to wobble.</param>
        /// <returns>A new <see cref="Image"/>.</returns>
        public Image Wobble(Image image)
        {
            var xy = Image.Xyz(image.Width / 20, image.Height / 20);
            var xDistort = Image.Gaussnoise(xy.Width, xy.Height);
            var yDistort = Image.Gaussnoise(xy.Width, xy.Height);
            xy += xDistort.Bandjoin(yDistort) / 150;
            xy = xy.Resize(20);
            xy *= 20;

            // apply the warp
            return image.Mapim(xy);
        }

        public string Execute(string[] args)
        {
            var random = new Random();

            var textLayer = Image.Black(1, 1);
            var xPosition = 0;

            foreach (var c in Text)
            {
                if (c == ' ')
                {
                    xPosition += 50;
                    continue;
                }

                var letter = Image.Text(c.ToString(), dpi: 600);

                var image = letter.Gravity(Enums.CompassDirection.Centre, letter.Width + 50, letter.Height + 50);

                // random scale and rotate
                image = image.Similarity(scale: random.NextDouble(0, 0.2) + 0.8, angle: random.Next(0, 40) - 20);

                // random wobble
                image = Wobble(image);

                // random colour
                var colour = Enumerable.Range(1, 3).Select(i => random.Next(0, 255)).ToArray();
                image = image.Ifthenelse(colour, 0, blend: true);

                // tag as 9-bit srgb
                image = image.Copy(interpretation: Enums.Interpretation.Srgb).Cast(Enums.BandFormat.Uchar);

                // position at our write position in the image
                image = image.Embed(xPosition, 0, image.Width + xPosition, image.Height);

                textLayer += image;
                textLayer = textLayer.Cast(Enums.BandFormat.Uchar);

                xPosition += letter.Width;
            }

            // remove any unused edges
            var trim = textLayer.FindTrim(background: new double[] { 0 });
            textLayer = textLayer.Crop((int)trim[0], (int)trim[1], (int)trim[2], (int)trim[3]);

            // make an alpha for the text layer: just a mono version of the image, but scaled
            // up so the letters themselves are not transparent
            var alpha = (textLayer.Colourspace(Enums.Interpretation.Bw) * 3).Cast(Enums.BandFormat.Uchar);
            textLayer = textLayer.Bandjoin(alpha);

            //  make a white background with random speckles
            var speckles = Image.Gaussnoise(textLayer.Width, textLayer.Height, mean: 400, sigma: 200);
            var background = Enumerable.Range(1, 2).Aggregate(speckles,
                    (a, b) => a.Bandjoin(Image.Gaussnoise(textLayer.Width, textLayer.Height, mean: 400, sigma: 200)))
                .Copy(interpretation: Enums.Interpretation.Srgb)
                .Cast(Enums.BandFormat.Uchar);

            // composite the text over the background
            var final = background.Composite(textLayer, Enums.BlendMode.Over);

            final.WriteToFile("captcha.jpg");

            return "See captcha.jpg";
        }
    }
}