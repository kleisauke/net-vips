using System.Collections.Generic;
using NUnit.Framework;

namespace NetVips.Tests
{
    [TestFixture]
    class MorphologyTests
    {
        [SetUp]
        public void Init()
        {
            Base.VipsInit();
        }

        [TearDown]
        public void Dispose()
        {
        }

        [Test]
        public void TestCountlines()
        {
            var im = Image.Black(100, 100);
            im = im.DrawLine(new double[] {255}, 0, 50, 100, 50);
            var nLines = im.Countlines(Enums.Direction.Horizontal);
            Assert.AreEqual(1, nLines);
        }

        [Test]
        public void TestLabelregions()
        {
            var im = Image.Black(100, 100);
            im = im.DrawCircle(new double[] {255}, 50, 50, 25, new Dictionary<string, object>
            {
                {"fill", true}
            });
            var mask = im.Labelregions(out var segments);

            Assert.AreEqual(3, segments);
            Assert.AreEqual(2, mask.Max());
        }

        [Test]
        public void TestErode()
        {
            var im = Image.Black(100, 100);
            im = im.DrawCircle(new double[] {255}, 50, 50, 25, new Dictionary<string, object>
            {
                {"fill", true}
            });
            var im2 = im.Erode(Image.NewFromArray(new[]
            {
                new[] {128, 255, 128},
                new[] {255, 255, 255},
                new[] {128, 255, 128}
            }));
            Assert.AreEqual(im.Width, im2.Width);
            Assert.AreEqual(im.Height, im2.Height);
            Assert.AreEqual(im.Bands, im2.Bands);
            Assert.IsTrue(im.Avg() > im2.Avg());
        }

        [Test]
        public void TestDilate()
        {
            var im = Image.Black(100, 100);
            im = im.DrawCircle(new double[] {255}, 50, 50, 25, new Dictionary<string, object>
            {
                {"fill", true}
            });
            var im2 = im.Dilate(Image.NewFromArray(new[]
            {
                new[] {128, 255, 128},
                new[] {255, 255, 255},
                new[] {128, 255, 128}
            }));
            Assert.AreEqual(im.Width, im2.Width);
            Assert.AreEqual(im.Height, im2.Height);
            Assert.AreEqual(im.Bands, im2.Bands);
            Assert.IsTrue(im2.Avg() > im.Avg());
        }

        [Test]
        public void TestRank()
        {
            var im = Image.Black(100, 100);
            im = im.DrawCircle(new double[] {255}, 50, 50, 25, new Dictionary<string, object>
            {
                {"fill", true}
            });
            var im2 = im.Rank(3, 3, 8);
            Assert.AreEqual(im.Width, im2.Width);
            Assert.AreEqual(im.Height, im2.Height);
            Assert.AreEqual(im.Bands, im2.Bands);
            Assert.IsTrue(im2.Avg() > im.Avg());
        }
    }
}