using System.Collections.Generic;
using System.Linq;

namespace NetVips.Samples
{
    /// <summary>
    /// From: https://github.com/libvips/nip2/tree/master/share/nip2/data/examples/1_point_mosaic
    /// </summary>
    public class OnePointMosaic : ISample
    {
        public string Name => "1 Point Mosaic";
        public string Category => "Mosaicing";

        public Dictionary<string, int[]> ImagesMarksDict = new Dictionary<string, int[]>
        {
            {"images/cd1.1.jpg", new[] {489, 140}},
            {"images/cd1.2.jpg", new[] {66, 141}},
            {"images/cd2.1.jpg", new[] {453, 40}},
            {"images/cd2.2.jpg", new[] {15, 43}},
            {"images/cd3.1.jpg", new[] {500, 122}},
            {"images/cd3.2.jpg", new[] {65, 121}},
            {"images/cd4.1.jpg", new[] {495, 58}},
            {"images/cd4.2.jpg", new[] {40, 57}}
        };

        public List<int[]> VerticalMarks = new List<int[]>
        {
            new[] {388, 44},
            new[] {364, 346},
            new[] {384, 17},
            new[] {385, 629},
            new[] {527, 42},
            new[] {503, 959}
        };

        public string Execute(string[] args)
        {
            Image mosaicedImage = null;

            for (var i = 0; i < ImagesMarksDict.Count; i += 2)
            {
                var items = ImagesMarksDict.Skip(i).Take(2).ToList();

                var firstItem = items[0];
                var secondItem = items[1];

                var image = Image.NewFromFile(firstItem.Key);
                var secondaryImage = Image.NewFromFile(secondItem.Key);
                var horizontalPart = image.Mosaic(secondaryImage, Enums.Direction.Horizontal, firstItem.Value[0],
                    firstItem.Value[1], secondItem.Value[0], secondItem.Value[1]);

                if (mosaicedImage == null)
                {
                    mosaicedImage = horizontalPart;
                }
                else
                {
                    var verticalMarks = VerticalMarks.Skip(i - 2).Take(2).ToList();
                    mosaicedImage = mosaicedImage.Mosaic(horizontalPart, Enums.Direction.Vertical, verticalMarks[1][0],
                        verticalMarks[1][1], verticalMarks[0][0], verticalMarks[0][1]);
                }

                mosaicedImage = mosaicedImage.Globalbalance();
            }

            mosaicedImage.WriteToFile("1-pt-mosaic.jpg");

            return "See 1-pt-mosaic.jpg";
        }
    }
}