namespace NetVips.Tests
{
    using Xunit;
    using Xunit.Abstractions;

    public class DrawTests : IClassFixture<TestsFixture>
    {
        public DrawTests(TestsFixture testsFixture, ITestOutputHelper output)
        {
            testsFixture.SetUpLogging(output);
        }

        [Fact]
        public void TestDrawCircle()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawCircle(new double[] { 100 }, 50, 50, 25));
            var pixel = im[25, 50];
            Assert.Single(pixel);
            Assert.Equal(100, pixel[0]);
            pixel = im[26, 50];
            Assert.Single(pixel);
            Assert.Equal(0, pixel[0]);

            im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawCircle(new double[] { 100 }, 50, 50, 25, fill: true));
            pixel = im[25, 50];
            Assert.Single(pixel);
            Assert.Equal(100, pixel[0]);
            pixel = im[26, 50];
            Assert.Equal(100, pixel[0]);
            pixel = im[24, 50];
            Assert.Equal(0, pixel[0]);
        }

        [Fact]
        public void TestDrawFlood()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x =>
            {
                x.DrawCircle(new double[] { 100 }, 50, 50, 25);
                x.DrawFlood(new double[] { 100 }, 50, 50);
            });

            var im2 = Image.Black(100, 100);
            im2 = im2.Mutate(x => x.DrawCircle(new double[] { 100 }, 50, 50, 25, fill: true));

            var diff = (im - im2).Abs().Max();
            Assert.Equal(0, diff);
        }

        [Fact]
        public void TestDrawImage()
        {
            var im = Image.Black(51, 51);
            im = im.Mutate(x => x.DrawCircle(new double[] { 100 }, 25, 25, 25, fill: true));

            var im2 = Image.Black(100, 100);
            im2 = im2.Mutate(x => x.DrawImage(im, 25, 25));

            var im3 = Image.Black(100, 100);
            im3 = im3.Mutate(x => x.DrawCircle(new double[] { 100 }, 50, 50, 25, fill: true));

            var diff = (im2 - im3).Abs().Max();
            Assert.Equal(0, diff);
        }

        [Fact]
        public void TestDrawLine()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawLine(new double[] { 100 }, 0, 0, 100, 0));
            var pixel = im[0, 0];
            Assert.Single(pixel);
            Assert.Equal(100, pixel[0]);
            pixel = im[0, 1];
            Assert.Single(pixel);
            Assert.Equal(0, pixel[0]);
        }

        [Fact]
        public void TestDrawMask()
        {
            var mask = Image.Black(51, 51);
            mask = mask.Mutate(x => x.DrawCircle(new double[] { 128 }, 25, 25, 25, fill: true));

            var im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawMask(new double[] { 200 }, mask, 25, 25));

            var im2 = Image.Black(100, 100);
            im2 = im2.Mutate(x => x.DrawCircle(new double[] { 100 }, 50, 50, 25, fill: true));

            var diff = (im - im2).Abs().Max();
            Assert.Equal(0, diff);
        }

        [Fact]
        public void TestDrawRect()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawRect(new double[] { 100 }, 25, 25, 50, 50, fill: true));

            var im2 = Image.Black(100, 100);
            im2 = im2.Mutate(x =>
            {
                for (var y = 25; y < 75; y++)
                {
                    x.DrawLine(new double[] { 100 }, 25, y, 74, y);
                }
            });


            var diff = (im - im2).Abs().Max();
            Assert.Equal(0, diff);
        }

        [Fact]
        public void TestDrawSmudge()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawCircle(new double[] { 100 }, 50, 50, 25, fill: true));

            var im2 = im.ExtractArea(10, 10, 50, 50);

            var im3 = im.Mutate(x =>
            {
                x.DrawSmudge(10, 10, 50, 50);
                x.DrawImage(im2, 10, 10);
            });

            var diff = (im3 - im).Abs().Max();
            Assert.Equal(0, diff);
        }
    }
}