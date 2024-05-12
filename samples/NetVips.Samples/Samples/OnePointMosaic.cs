using System;
using System.Collections.Generic;

namespace NetVips.Samples;

/// <summary>
/// From: https://github.com/libvips/nip2/tree/master/share/nip2/data/examples/1_point_mosaic
/// </summary>
public class OnePointMosaic : ISample
{
    public string Name => "1 Point Mosaic";
    public string Category => "Mosaicing";

    public struct Point(int x, int y)
    {
        public int X = x, Y = y;
    }

    public List<string> Images =
    [
        "images/cd1.1.jpg",
        "images/cd1.2.jpg",
        "images/cd2.1.jpg",
        "images/cd2.2.jpg",
        "images/cd3.1.jpg",
        "images/cd3.2.jpg",
        "images/cd4.1.jpg",
        "images/cd4.2.jpg"
    ];

    public List<Point> HorizontalMarks =
    [
        new(489, 140),
        new(66, 141),
        new(453, 40),
        new(15, 43),
        new(500, 122),
        new(65, 121),
        new(495, 58),
        new(40, 57)
    ];

    public List<Point> VerticalMarks =
    [
        new(364, 346),
        new(388, 44),
        new(385, 629),
        new(384, 17),
        new(503, 959),
        new(527, 42)
    ];

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