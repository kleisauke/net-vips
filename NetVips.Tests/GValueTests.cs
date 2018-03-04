using System.Collections;
using System.IO;
using NUnit.Framework;
using NetVips.Internal;

namespace NetVips.Tests
{
    [TestFixture]
    public class GValueTests
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
        public void TestBool()
        {
            var gv = new GValue();
            gv.SetType(GValue.GBoolType);
            gv.Set(true);
            var value = gv.Get();
            Assert.AreEqual(true, value);

            gv.Set(false);
            value = gv.Get();
            Assert.AreEqual(false, value);
        }

        [Test]
        public void TestInt()
        {
            var gv = new GValue();
            gv.SetType(GValue.GIntType);
            gv.Set(12);
            var value = gv.Get();
            Assert.AreEqual(12, value);
        }

        [Test]
        public void TestDouble()
        {
            var gv = new GValue();
            gv.SetType(GValue.GDoubleType);
            gv.Set(3.1415);
            var value = gv.Get();
            Assert.AreEqual(3.1415, value);
        }

        [Test]
        public void TestEnum()
        {
            // the Interpretation enum is created when the first image is made --
            // make it ourselves in case we are run before the first image
            Vips.VipsInterpretationGetType();
            var interpretationGtype = Base.TypeFromName("VipsInterpretation");
            var gv = new GValue();
            gv.SetType(interpretationGtype);
            gv.Set("xyz");
            var value = gv.Get();
            Assert.AreEqual("xyz", value);
        }

        [Test]
        public void TestFlags()
        {
            // the OperationFlags enum is created when the first op is made --
            // make it ourselves in case we are run before that
            Vips.VipsOperationFlagsGetType();
            var operationflagsGtype = Base.TypeFromName("VipsOperationFlags");
            var gv = new GValue();
            gv.SetType(operationflagsGtype);
            gv.Set(12);
            var value = gv.Get();
            Assert.AreEqual(12, value);
        }

        [Test]
        public void TestString()
        {
            var gv = new GValue();
            gv.SetType(GValue.GStrType);
            gv.Set("banana");
            var value = gv.Get();
            Assert.AreEqual("banana", value);
        }

        [Test]
        public void TestArrayInt()
        {
            var gv = new GValue();
            gv.SetType(GValue.ArrayIntType);
            gv.Set(new[]
            {
                1,
                2,
                3
            });
            var value = gv.Get();
            Assert.AreEqual(new[]
            {
                1,
                2,
                3
            }, value);
        }

        [Test]
        public void TestArrayDouble()
        {
            var gv = new GValue();
            gv.SetType(GValue.ArrayDoubleType);
            gv.Set(new[]
            {
                1.1,
                2.1,
                3.1
            });
            var value = gv.Get();
            Assert.AreEqual(new[]
            {
                1.1,
                2.1,
                3.1
            }, value);
        }

        [Test]
        public void TestImage()
        {
            var image = Image.NewFromFile(Helper.JpegFile);
            var gv = new GValue();
            gv.SetType(GValue.ImageType);
            gv.Set(image);
            var value = gv.Get();
            Assert.AreEqual(image, value);
        }

        [Test]
        public void TestArrayImage()
        {
            var image = Image.NewFromFile(Helper.JpegFile);
            var values = image.Bandsplit();
            var r = values[0];
            var g = values[1];
            var b = values[2];

            var gv = new GValue();
            gv.SetType(GValue.ArrayImageType);
            gv.Set(new[]
            {
                r,
                g,
                b
            });
            var value = gv.Get();

            CollectionAssert.AreEqual(new[]
            {
                r,
                g,
                b
            }, value as IEnumerable);
        }

        [Test]
        public void TestBlob()
        {
            var blob = File.ReadAllBytes(Helper.JpegFile);
            var gv = new GValue();
            gv.SetType(GValue.BlobType);
            gv.Set(blob);
            var value = gv.Get();
            Assert.AreEqual(blob, value);
        }
    }
}