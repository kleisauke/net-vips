using Xunit;

namespace NetVips.Tests
{
    public class DrawTests : IClassFixture<TestsFixture>
    {
        [Fact]
        public void TestDrawCircle()
        {
            var im = Image.Black(100, 100);
            im = im.DrawCircle(new double[] {100}, 50, 50, 25);
            var pixel = im.Getpoint(25, 50);
            Assert.Single(pixel);
            Assert.Equal(100, pixel[0]);
            pixel = im.Getpoint(26, 50);
            Assert.Single(pixel);
            Assert.Equal(0, pixel[0]);

            im = Image.Black(100, 100);
            im = im.DrawCircle(new double[] {100}, 50, 50, 25, fill: true);
            pixel = im.Getpoint(25, 50);
            Assert.Single(pixel);
            Assert.Equal(100, pixel[0]);
            pixel = im.Getpoint(26, 50);
            Assert.Equal(100, pixel[0]);
            pixel = im.Getpoint(24, 50);
            Assert.Equal(0, pixel[0]);
        }

        [Fact]
        public void TestDrawFlood()
        {
            var im = Image.Black(100, 100);
            im = im.DrawCircle(new double[] {100}, 50, 50, 25);
            im = im.DrawFlood(new double[] {100}, 50, 50);

            var im2 = Image.Black(100, 100);
            im2 = im2.DrawCircle(new double[] {100}, 50, 50, 25, fill: true);

            var diff = (im - im2).Abs().Max();
            Assert.Equal(0, diff);
        }

        [Fact]
        public void TestDrawImage()
        {
            var im = Image.Black(51, 51);
            im = im.DrawCircle(new double[] {100}, 25, 25, 25, fill: true);

            var im2 = Image.Black(100, 100);
            im2 = im2.DrawImage(im, 25, 25);

            var im3 = Image.Black(100, 100);
            im3 = im3.DrawCircle(new double[] {100}, 50, 50, 25, fill: true);

            var diff = (im2 - im3).Abs().Max();
            Assert.Equal(0, diff);
        }

        [Fact]
        public void TestDrawLine()
        {
            var im = Image.Black(100, 100);
            im = im.DrawLine(new double[] {100}, 0, 0, 100, 0);
            var pixel = im.Getpoint(0, 0);
            Assert.Single(pixel);
            Assert.Equal(100, pixel[0]);
            pixel = im.Getpoint(0, 1);
            Assert.Single(pixel);
            Assert.Equal(0, pixel[0]);
        }

        [Fact]
        public void TestDrawMask()
        {
            var mask = Image.Black(51, 51);
            mask = mask.DrawCircle(new double[] {128}, 25, 25, 25, fill: true);

            var im = Image.Black(100, 100);
            im = im.DrawMask(new double[] {200}, mask, 25, 25);

            var im2 = Image.Black(100, 100);
            im2 = im2.DrawCircle(new double[] {100}, 50, 50, 25, fill: true);

            var diff = (im - im2).Abs().Max();
            Assert.Equal(0, diff);
        }

        [Fact]
        public void TestDrawRect()
        {
            var im = Image.Black(100, 100);
            im = im.DrawRect(new double[] {100}, 25, 25, 50, 50, fill: true);

            var im2 = Image.Black(100, 100);
            for (var y = 25; y < 75; y++)
            {
                im2 = im2.DrawLine(new double[] {100}, 25, y, 74, y);
            }

            var diff = (im - im2).Abs().Max();
            Assert.Equal(0, diff);
        }

        [Fact]
        public void TestDrawSmudge()
        {
            var im = Image.Black(100, 100);
            im = im.DrawCircle(new double[] {100}, 50, 50, 25, fill: true);

            var im2 = im.DrawSmudge(10, 10, 50, 50);

            var im3 = im.ExtractArea(10, 10, 50, 50);

            var im4 = im2.DrawImage(im3, 10, 10);

            var diff = (im4 - im).Abs().Max();
            Assert.Equal(0, diff);
        }
    }
}