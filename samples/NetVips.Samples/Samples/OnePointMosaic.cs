namespace NetVips.Samples
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// From: https://github.com/libvips/nip2/tree/master/share/nip2/data/examples/1_point_mosaic
    /// </summary>
    public class OnePointMosaic : ISample
    {
        public string Name => "1 Point Mosaic";
        public string Category => "Mosaicing";

        public struct Point
        {
            public int X, Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public List<string> Images = new List<string>
        {
            "images/cd1.1.jpg",
            "images/cd1.2.jpg",
            "images/cd2.1.jpg",
            "images/cd2.2.jpg",
            "images/cd3.1.jpg",
            "images/cd3.2.jpg",
            "images/cd4.1.jpg",
            "images/cd4.2.jpg"
        };

        public List<Point> HorizontalMarks = new List<Point>
        {
            new Point(489, 140),
            new Point(66, 141),
            new Point(453, 40),
            new Point(15, 43),
            new Point(500, 122),
            new Point(65, 121),
            new Point(495, 58),
            new Point(40, 57)
        };

        public List<Point> VerticalMarks = new List<Point>
        {
            new Point(364, 346),
            new Point(388, 44),
            new Point(385, 629),
            new Point(384, 17),
            new Point(503, 959),
            new Point(527, 42)
        };

        public void Execute(string[] args)
        {
            Image mosaicedImage = null;
            for (var i = 0; i < Images.Count; i += 2)
            {
                using var image = Image.NewFromFile(Images[i]);
                using var secondaryImage = Image.NewFromFile(Images[i + 1]);

                if (mosaicedImage == null)
                {
                    mosaicedImage = image.Mosaic(secondaryImage, Enums.Direction.Horizontal,
                        HorizontalMarks[i].X, HorizontalMarks[i].Y,
                        HorizontalMarks[i + 1].X, HorizontalMarks[i + 1].Y);
                }
                else
                {
                    using var horizontalPart = image.Mosaic(secondaryImage, Enums.Direction.Horizontal,
                        HorizontalMarks[i].X, HorizontalMarks[i].Y,
                        HorizontalMarks[i + 1].X, HorizontalMarks[i + 1].Y);

                    using (mosaicedImage)
                    {
                        mosaicedImage = mosaicedImage.Mosaic(horizontalPart, Enums.Direction.Vertical,
                            VerticalMarks[i - 2].X, VerticalMarks[i - 2].Y,
                            VerticalMarks[i - 2 + 1].X, VerticalMarks[i - 2 + 1].Y);
                    }
                }
            }

            using (mosaicedImage)
            {
                using var balanced = mosaicedImage.Globalbalance();
                balanced.WriteToFile("1-pt-mosaic.jpg");
            }

            Console.WriteLine("See 1-pt-mosaic.jpg");
        }
    }
}