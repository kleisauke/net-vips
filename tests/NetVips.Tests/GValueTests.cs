namespace NetVips.Tests
{
    using System.IO;
    using Xunit;
    using Xunit.Abstractions;

    public class GValueTests : IClassFixture<TestsFixture>
    {
        public GValueTests(TestsFixture testsFixture, ITestOutputHelper output)
        {
            testsFixture.SetUpLogging(output);
        }

        [Fact]
        public void TestBool()
        {
            bool actual;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.GBoolType);
                gv.Set(true);
                actual = (bool)gv.Get();
            }

            Assert.True(actual);
        }

        [Fact]
        public void TestInt()
        {
            int actual;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.GIntType);
                gv.Set(12);
                actual = (int)gv.Get();
            }

            Assert.Equal(12, actual);
        }

        [Fact]
        public void TestUint64()
        {
            ulong actual;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.GUint64Type);
                gv.Set(ulong.MaxValue);
                actual = (ulong)gv.Get();
            }

            Assert.Equal(ulong.MaxValue, actual);
        }

        [Fact]
        public void TestDouble()
        {
            double actual;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.GDoubleType);
                gv.Set(3.1415);
                actual = (double)gv.Get();
            }

            Assert.Equal(3.1415, actual);
        }

        [Fact]
        public void TestEnum()
        {
            // the Interpretation enum is created when the first image is made --
            // make it ourselves in case we are run before the first image
            NetVips.VipsInterpretationGetType();
            var gtype = NetVips.TypeFromName("VipsInterpretation");

            Enums.Interpretation actual;
            using (var gv = new GValue())
            {
                gv.SetType(gtype);
                gv.Set(Enums.Interpretation.Xyz);
                actual = (Enums.Interpretation)gv.Get();
            }

            Assert.Equal(Enums.Interpretation.Xyz, actual);
        }

        [Fact]
        public void TestFlags()
        {
            // the OperationFlags enum is created when the first op is made --
            // make it ourselves in case we are run before that
            NetVips.VipsOperationFlagsGetType();
            var gtype = NetVips.TypeFromName("VipsOperationFlags");

            Enums.OperationFlags actual;
            using (var gv = new GValue())
            {
                gv.SetType(gtype);
                gv.Set(Enums.OperationFlags.DEPRECATED);
                actual = (Enums.OperationFlags)gv.Get();
            }

            Assert.Equal(Enums.OperationFlags.DEPRECATED, actual);
        }

        [Fact]
        public void TestString()
        {
            string actual;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.GStrType);
                gv.Set("banana");
                actual = (string)gv.Get();
            }

            Assert.Equal("banana", actual);
        }

        [Fact]
        public void TestRefString()
        {
            string actual;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.RefStrType);
                gv.Set("banana");
                actual = (string)gv.Get();
            }

            Assert.Equal("banana", actual);
        }

        [Fact]
        public void TestArrayInt()
        {
            int[] actual;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.ArrayIntType);
                gv.Set(new[] { 1, 2, 3 });
                actual = (int[])gv.Get();
            }

            Assert.Equal(new[] { 1, 2, 3 }, actual);
        }

        [Fact]
        public void TestArrayDouble()
        {
            double[] actual;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.ArrayDoubleType);
                gv.Set(new[] { 1.1, 2.1, 3.1 });
                actual = (double[])gv.Get();
            }

            Assert.Equal(new[] { 1.1, 2.1, 3.1 }, actual);
        }

        [Fact]
        public void TestImage()
        {
            var image = Image.NewFromFile(Helper.JpegFile);

            Image actual;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.ImageType);
                gv.Set(image);
                actual = (Image)gv.Get();
            }


            Assert.Equal(image, actual);
        }

        [Fact]
        public void TestArrayImage()
        {
            var images = Image.NewFromFile(Helper.JpegFile).Bandsplit();

            Image[] actualImages;
            using (var gv = new GValue())
            {
                gv.SetType(GValue.ArrayImageType);
                gv.Set(images);
                actualImages = (Image[])gv.Get();
            }

            for (var i = 0; i < actualImages.Length; i++)
            {
                var actual = actualImages[i];
                var expected = images[i];

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void TestBlob()
        {
            var blob = File.ReadAllBytes(Helper.JpegFile);
            byte[] actual;

            using (var gv = new GValue())
            {
                gv.SetType(GValue.BlobType);
                gv.Set(blob);
                actual = (byte[])gv.Get();
            }

            Assert.Equal(blob, actual);
        }
    }
}