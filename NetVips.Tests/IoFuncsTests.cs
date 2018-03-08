using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NetVips.Tests
{
    [TestFixture]
    class IoFuncsTests
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

        /// <summary>
        // test the vips7 filename splitter ... this is very fragile and annoying
        // code with lots of cases
        /// </summary>
        [Test]
        public void TestSplit7()
        {
            string[] Split(string path)
            {
                var filename7 = Base.PathFilename7(path);
                var mode7 = Base.PathMode7(path);
                return new[] {filename7, mode7};
            }

            var cases = new Dictionary<string, string[]> {
                {
                    "c:\\silly:dir:name\\fr:ed.tif:jpeg:95,,,,c:\\icc\\srgb.icc",
                    new []{
                        "c:\\silly:dir:name\\fr:ed.tif",
                        "jpeg:95,,,,c:\\icc\\srgb.icc"
                    }
                },
                {
                    "I180:",
                new [] {
                        "I180",
                        ""
                    }
                },
                 {
                    "c:\\silly:",
                    new [] {
                        "c:\\silly",
                        ""
                    }
                },
               {
                    "c:\\program files\\x:hello",
                    new [] {
                        "c:\\program files\\x",
                        "hello"
                    }
                },
                 {
                    "C:\\fixtures\\2569067123_aca715a2ee_o.jpg",
                    new [] {
                        "C:\\fixtures\\2569067123_aca715a2ee_o.jpg",
                        ""
                    }
                }
            };

            foreach (var entry in cases)
            {
                CollectionAssert.AreEqual(entry.Value, Split(entry.Key));
            }
        }

        [Test]
        public void TestNewFromImage()
        {
            var im = Image.MaskIdeal(100, 100, 0.5, new VOption
            {
                {"reject", true},
                {"optical", true}
            });

            var im2 = im.NewFromImage(12);

            Assert.AreEqual(im.Width, im2.Width);
            Assert.AreEqual(im.Height, im2.Height);
            Assert.AreEqual(im.Interpretation, im2.Interpretation);
            Assert.AreEqual(im.Format, im2.Format);
            Assert.AreEqual(im.Xres, im2.Xres);
            Assert.AreEqual(im.Yres, im2.Yres);
            Assert.AreEqual(im.Xoffset, im2.Xoffset);
            Assert.AreEqual(im.Yoffset, im2.Yoffset);
            Assert.AreEqual(1, im2.Bands);
            Assert.AreEqual(12, im2.Avg());

            im2 = im.NewFromImage(new[]{
                1,
                2,
                3
            });
            Assert.AreEqual(3, im2.Bands);
            Assert.AreEqual(2, im2.Avg());
        }

        [Test]
        public void TestNewFromMemory()
        {
            var s = Enumerable.Repeat((byte) 0, 200).ToArray();
            var im = Image.NewFromMemory(s, 20, 10, 1, "uchar");
            Assert.AreEqual(20, im.Width);
            Assert.AreEqual(10, im.Height);
            Assert.AreEqual("uchar", im.Format);
            Assert.AreEqual(1, im.Bands);
            Assert.AreEqual(0, im.Avg());

            im += 10;
            Assert.AreEqual(im.Avg(), 10);
        }

        [Test]
        public void TestGetFields()
        {
            if (Base.AtLeastLibvips(8, 5))
            {
                var im = Image.Black(10, 10);
                var fields = im.GetFields();

                // we might add more fields later
                Assert.IsTrue(fields.Length > 10);

                Assert.AreEqual("width", fields[0]);
            }
        }

        [Test]
        public void TestWriteToMemory()
        {
            var s = Enumerable.Repeat((byte) 0, 200).ToArray();
            var im = Image.NewFromMemory(s, 20, 10, 1, "uchar");
            var t = im.WriteToMemory();
            CollectionAssert.AreEqual(s, t);
        }
    }
}