namespace NetVips.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// From: https://github.com/weserv/images/blob/3.x/src/Manipulators/Shape.php
    /// </summary>
    public class ShapeCropping : ISample
    {
        public string Name => "Shape cropping";
        public string Category => "Conversion";

        public const string Filename = "images/lichtenstein.jpg";

        public enum Shape
        {
            Circle,
            Ellipse,
            Triangle,
            Triangle180,
            Pentagon,
            Pentagon180,
            Hexagon,
            Square,
            Star,
            Heart
        }

        public const Shape CurrentShape = Shape.Circle;

        public const bool Crop = true;

        public void Execute(string[] args)
        {
            using var image = Image.NewFromFile(Filename, access: Enums.Access.Sequential);
            var width = image.Width;
            var height = image.Height;

            var path = GetSvgShape(width, height, CurrentShape, out var xMin, out var yMin, out var maskWidth,
                out var maskHeight);

            var preserveAspectRatio = CurrentShape.Equals(Shape.Ellipse) ? "none" : "xMidYMid meet";
            var svg = new StringBuilder("<?xml version='1.0' encoding='UTF-8' standalone='no'?>");
            svg.Append($"<svg xmlns='http://www.w3.org/2000/svg' version='1.1' width='{width}' height='{height}'");
            svg.Append($" viewBox='{xMin} {yMin} {maskWidth} {maskHeight}'");
            svg.Append($" shape-rendering='geometricPrecision' preserveAspectRatio='{preserveAspectRatio}'>");
            svg.Append(path);
            svg.Append("</svg>");

            using var mask = Image.NewFromBuffer(svg.ToString(), access: Enums.Access.Sequential);

            // Cutout via dest-in
            var composite = image.Composite(mask, Enums.BlendMode.DestIn);

            // Crop the image to the mask dimensions
            if (Crop && !CurrentShape.Equals(Shape.Ellipse))
            {
                var trim = ResolveShapeTrim(width, height, maskWidth, maskHeight);
                var left = trim[0];
                var top = trim[1];
                var trimWidth = trim[2];
                var trimHeight = trim[3];

                // Crop if the trim dimensions is less than the image dimensions
                if (trimWidth < width || trimHeight < height)
                {
                    using (composite)
                    {
                        composite = composite.Crop(left, top, trimWidth, trimHeight);
                    }
                }
            }

            using (composite)
            {
                composite.WriteToFile("shape.png");
            }

            Console.WriteLine("See shape.png");
        }

        /// <summary>
        /// Get the SVG shape
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="shape">Shape in which the image is to be cropped</param>
        /// <param name="xMin">Left edge of mask</param>
        /// <param name="yMin">Top edge of mask</param>
        /// <param name="maskWidth">Mask width</param>
        /// <param name="maskHeight">Mask height</param>
        /// <returns>SVG path</returns>
        public string GetSvgShape(int width, int height, Shape shape, out int xMin, out int yMin, out int maskWidth,
            out int maskHeight)
        {
            var min = Math.Min(width, height);
            var outerRadius = min / 2.0;
            var midX = width / 2;
            var midY = height / 2;

            // 'inner' radius of the polygon/star
            var innerRadius = outerRadius;

            // Initial angle (clockwise). By default, stars and polygons are 'pointing' up.
            var initialAngle = 0.0;

            // Number of points (or number of sides for polygons)
            int points;

            switch (shape)
            {
                case Shape.Hexagon:
                    // Hexagon
                    points = 6;
                    break;
                case Shape.Pentagon:
                    // Pentagon
                    points = 5;
                    break;
                case Shape.Pentagon180:
                    // Pentagon tilted upside down
                    points = 5;
                    initialAngle = Math.PI;
                    break;
                case Shape.Star:
                    // 5 point star
                    points = 5 * 2;
                    innerRadius *= .382;
                    break;
                case Shape.Square:
                    // Square tilted 45 degrees
                    points = 4;
                    break;
                case Shape.Triangle:
                    // Triangle
                    points = 3;
                    break;
                case Shape.Triangle180:
                    // Triangle upside down
                    points = 3;
                    initialAngle = Math.PI;
                    break;
                case Shape.Circle:
                    xMin = (int)(midX - outerRadius);
                    yMin = (int)(midY - outerRadius);
                    maskWidth = min;
                    maskHeight = min;

                    // Circle
                    return $"<circle r='{outerRadius}' cx='{midX}' cy='{midY}'/>";
                case Shape.Ellipse:
                    xMin = 0;
                    yMin = 0;
                    maskWidth = width;
                    maskHeight = height;

                    // Ellipse
                    return $"<ellipse cx='{midX}' cy='{midY}' rx='{midX}' ry='{midY}'/>";
                case Shape.Heart:
                    // Heart
                    return GetSvgHeart(outerRadius, outerRadius, out xMin, out yMin, out maskWidth, out maskHeight);
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
            }

            return GetSvgMask(midX, midY, points, outerRadius, innerRadius, initialAngle, out xMin, out yMin,
                out maskWidth, out maskHeight);
        }

        /// <summary>
        /// Formula from https://mathworld.wolfram.com/HeartCurve.html
        /// </summary>
        /// <param name="midX">Image width / 2</param>
        /// <param name="midY">Image height / 2</param>
        /// <param name="xMin">Left edge of mask</param>
        /// <param name="yMin">Top edge of mask</param>
        /// <param name="maskWidth">Mask width</param>
        /// <param name="maskHeight">Mask height</param>
        /// <returns>SVG path</returns>
        public string GetSvgHeart(double midX, double midY, out int xMin, out int yMin, out int maskWidth,
            out int maskHeight)
        {
            var path = new StringBuilder();
            var xArr = new List<int>();
            var yArr = new List<int>();

            for (var t = -Math.PI; t <= Math.PI; t += 0.02)
            {
                var xPt = 16 * Math.Pow(Math.Sin(t), 3);
                var yPt = 13 * Math.Cos(t) - 5 * Math.Cos(2 * t) - 2 * Math.Cos(3 * t) - Math.Cos(4 * t);

                var x = (int)Math.Round(midX + xPt * midX);
                var y = (int)Math.Round(midY - yPt * midY);
                xArr.Add(x);
                yArr.Add(y);
                path.Append($"{x} {y} L");
            }

            xMin = xArr.Min();
            yMin = yArr.Min();
            maskWidth = xArr.Max() - xMin;
            maskHeight = yArr.Max() - yMin;

            return $"<path d='{path} Z'/>";
        }

        /// <summary>
        /// Inspired by this JSFiddle: https://jsfiddle.net/tohan/8vwjn4cx/
        /// modified to support SVG paths
        /// </summary>
        /// <param name="midX">Image width / 2</param>
        /// <param name="midY">Image height / 2</param>
        /// <param name="points">Number of points (or number of sides for polygons)</param>
        /// <param name="outerRadius">'outer' radius of the star</param>
        /// <param name="innerRadius">'inner' radius of the star (if equal to outerRadius, a polygon is drawn)</param>
        /// <param name="initialAngle">Initial angle (clockwise). By default, stars and polygons are 'pointing' up.</param>
        /// <param name="xMin">Left edge of mask</param>
        /// <param name="yMin">Top edge of mask</param>
        /// <param name="maskWidth">Mask width</param>
        /// <param name="maskHeight">Mask height</param>
        /// <returns>SVG Path</returns>
        public string GetSvgMask(
            int midX, int midY, int points, double outerRadius, double innerRadius, double initialAngle,
            out int xMin, out int yMin, out int maskWidth, out int maskHeight)
        {
            var path = new StringBuilder();
            var xArr = new List<int>();
            var yArr = new List<int>();

            for (var i = 0; i <= points; i++)
            {
                var angle = i * 2 * Math.PI / points - Math.PI / 2 + initialAngle;
                var radius = i % 2 == 0 ? outerRadius : innerRadius;
                if (i == 0)
                {
                    path.Append('M');

                    // If an odd number of points, add an additional point at the top of the polygon
                    // -- this will shift the calculated center point of the shape so that the center point
                    // of the polygon is at x,y (otherwise the center is mis-located)
                    if (points % 2 == 1)
                    {
                        path.Append($"0 {radius} M");
                    }
                }
                else
                {
                    path.Append(" L");
                }

                var x = (int)Math.Round(midX + radius * Math.Cos(angle));
                var y = (int)Math.Round(midY + radius * Math.Sin(angle));
                xArr.Add(x);
                yArr.Add(y);
                path.Append($"{x} {y} L");
            }

            xMin = xArr.Min();
            yMin = yArr.Min();
            maskWidth = xArr.Max() - xMin;
            maskHeight = yArr.Max() - yMin;

            return $"<path d='{path} Z'/>";
        }

        /// <summary>
        /// Calculate the area to extract
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="maskWidth">Mask width</param>
        /// <param name="maskHeight">Mask height</param>
        /// <returns></returns>
        public int[] ResolveShapeTrim(int width, int height, int maskWidth, int maskHeight)
        {
            var xScale = (double)width / maskWidth;
            var yScale = (double)height / maskHeight;
            var scale = Math.Min(xScale, yScale);
            var trimWidth = maskWidth * scale;
            var trimHeight = maskHeight * scale;
            var left = (int)Math.Round((width - trimWidth) / 2);
            var top = (int)Math.Round((height - trimHeight) / 2);

            return new[] { left, top, (int)Math.Round(trimWidth), (int)Math.Round(trimHeight) };
        }

        #region helpers

        /// <summary>
        /// Return the image alpha maximum. Useful for combining alpha bands. scRGB
        /// images are 0 - 1 for image data, but the alpha is 0 - 255.
        /// </summary>
        /// <param name="interpretation">The <see cref="Enums.Interpretation"/></param>
        /// <returns>the image alpha maximum</returns>
        public static int MaximumImageAlpha(Enums.Interpretation interpretation)
        {
            return Is16Bit(interpretation) ? 65535 : 255;
        }

        /// <summary>
        /// Are pixel values in this image 16-bit integer?
        /// </summary>
        /// <param name="interpretation">The <see cref="Enums.Interpretation"/></param>
        /// <returns><see langword="true"/> if the pixel values in this image are 16-bit;
        /// otherwise, <see langword="false"/></returns>
        public static bool Is16Bit(Enums.Interpretation interpretation)
        {
            return interpretation == Enums.Interpretation.Rgb16 ||
                   interpretation == Enums.Interpretation.Grey16;
        }

        #endregion
    }
}