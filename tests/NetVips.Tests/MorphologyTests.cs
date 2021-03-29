namespace NetVips.Tests
{
    using Xunit;
    using Xunit.Abstractions;

    public class MorphologyTests : IClassFixture<TestsFixture>
    {
        public MorphologyTests(TestsFixture testsFixture, ITestOutputHelper output)
        {
            testsFixture.SetUpLogging(output);
        }

        [Fact]
        public void TestCountlines()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawLine(new double[] { 255 }, 0, 50, 100, 50));
            var nLines = im.Countlines(Enums.Direction.Horizontal);
            Assert.Equal(1, nLines);
        }

        [Fact]
        public void TestLabelregions()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawCircle(new double[] { 255 }, 50, 50, 25, fill: true));
            var mask = im.Labelregions(out var segments);

            Assert.Equal(3, segments);
            Assert.Equal(2, mask.Max());
        }

        [Fact]
        public void TestErode()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawCircle(new double[] { 255 }, 50, 50, 25, fill: true));
            var im2 = im.Erode(Image.NewFromArray(new[,]
            {
                {128, 255, 128},
                {255, 255, 255},
                {128, 255, 128}
            }));
            Assert.Equal(im.Width, im2.Width);
            Assert.Equal(im.Height, im2.Height);
            Assert.Equal(im.Bands, im2.Bands);
            Assert.True(im.Avg() > im2.Avg());
        }

        [Fact]
        public void TestDilate()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x => x.DrawCircle(new double[] { 255 }, 50, 50, 25, fill: true));
            var im2 = im.Dilate(Image.NewFromArray(new[,]
            {
                {128, 255, 128},
                {255, 255, 255},
                {128, 255, 128}
            }));
            Assert.Equal(im.Width, im2.Width);
            Assert.Equal(im.Height, im2.Height);
            Assert.Equal(im.Bands, im2.Bands);
            Assert.True(im2.Avg() > im.Avg());
        }

        [Fact]
        public void TestRank()
        {
            var im = Image.Black(100, 100);
            im = im.Mutate(x=> x.DrawCircle(new double[] { 255 }, 50, 50, 25, fill: true));
            var im2 = im.Rank(3, 3, 8);
            Assert.Equal(im.Width, im2.Width);
            Assert.Equal(im.Height, im2.Height);
            Assert.Equal(im.Bands, im2.Bands);
            Assert.True(im2.Avg() > im.Avg());
        }
    }
}