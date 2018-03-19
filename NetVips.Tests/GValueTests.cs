using System.Collections;
using System.IO;
using NetVips.Internal;
using Xunit;

namespace NetVips.Tests
{
    public class GValueTests : IClassFixture<NetVipsFixture>
    {
        [Fact]
        public void TestBool()
        {
            var gv = new GValue();
            gv.SetType(GValue.GBoolType);
            gv.Set(true);
            var value = gv.Get();
            Assert.True(value as bool?);

            gv.Set(false);
            value = gv.Get();
            Assert.False(value as bool?);
        }

        [Fact]
        public void TestInt()
        {
            var gv = new GValue();
            gv.SetType(GValue.GIntType);
            gv.Set(12);
            var value = gv.Get();
            Assert.Equal(12, value);
        }

        [Fact]
        public void TestDouble()
        {
            var gv = new GValue();
            gv.SetType(GValue.GDoubleType);
            gv.Set(3.1415);
            var value = gv.Get();
            Assert.Equal(3.1415, value);
        }

        [Fact]
        public void TestEnum()
        {
            // the Interpretation enum is created when the first image is made --
            // make it ourselves in case we are run before the first image
            Base.VipsInterpretationGetType();
            var interpretationGtype = Base.TypeFromName("VipsInterpretation");
            var gv = new GValue();
            gv.SetType(interpretationGtype);
            gv.Set("xyz");
            var value = gv.Get();
            Assert.Equal("xyz", value);
        }

        [Fact]
        public void TestFlags()
        {
            // the OperationFlags enum is created when the first op is made --
            // make it ourselves in case we are run before that
            Base.VipsOperationFlagsGetType();
            var operationflagsGtype = Base.TypeFromName("VipsOperationFlags");
            var gv = new GValue();
            gv.SetType(operationflagsGtype);
            gv.Set(12);
            var value = gv.Get();
            Assert.Equal(12u, value);
        }

        [Fact]
        public void TestString()
        {
            var gv = new GValue();
            gv.SetType(GValue.GStrType);
            gv.Set("banana");
            var value = gv.Get();
            Assert.Equal("banana", value);
        }

        [Fact]
        public void TestArrayInt()
        {
            var gv = new GValue();
            gv.SetType(GValue.ArrayIntType);
            gv.Set(new[] {1, 2, 3});
            var value = gv.Get();
            Assert.Equal(new[] {1, 2, 3}, value as IEnumerable);
        }

        [Fact]
        public void TestArrayDouble()
        {
            var gv = new GValue();
            gv.SetType(GValue.ArrayDoubleType);
            gv.Set(new[] {1.1, 2.1, 3.1});
            var value = gv.Get();
            Assert.Equal(new[] {1.1, 2.1, 3.1}, value as IEnumerable);
        }

        [Fact]
        public void TestImage()
        {
            var image = Image.NewFromFile(Helper.JpegFile);
            var gv = new GValue();
            gv.SetType(GValue.ImageType);
            gv.Set(image);
            var value = gv.Get();
            Assert.Equal(image, value);
        }

        [Fact]
        public void TestArrayImage()
        {
            var image = Image.NewFromFile(Helper.JpegFile);
            var values = image.Bandsplit();
            var r = values[0];
            var g = values[1];
            var b = values[2];

            var gv = new GValue();
            gv.SetType(GValue.ArrayImageType);
            gv.Set(new[] {r, g, b});
            var value = gv.Get();

            Assert.Equal(new[] {r, g, b}, value as IEnumerable);
        }

        [Fact]
        public void TestBlob()
        {
            var blob = File.ReadAllBytes(Helper.JpegFile);
            var gv = new GValue();
            gv.SetType(GValue.BlobType);
            gv.Set(blob);
            var value = gv.Get();
            Assert.Equal(blob, value);
        }
    }
}