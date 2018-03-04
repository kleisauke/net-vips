using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace NetVips.Tests
{
    class ForeignTests
    {
        private string _tempDir;

        private Image _colour;
        private Image _mono;
        private Image _rad;
        private Image _cmyk;
        private Image _oneBit;

        [SetUp]
        public void Init()
        {
            Base.VipsInit();

            _tempDir = Helper.GetTemporaryDirectory();

            _colour = Image.Jpegload(Helper.JpegFile);
            _mono = _colour[0];

            // we remove the ICC profile: the RGB one will no longer be appropriate
            _mono.Remove("icc-profile-data");
            _rad = _colour.Float2rad();
            _rad.Remove("icc-profile-data");
            _cmyk = _colour.Bandjoin(_mono);
            _cmyk = _cmyk.Copy(new Dictionary<string, object>
            {
                {"interpretation", Enums.Interpretation.Cmyk}
            });
            _cmyk.Remove("icc-profile-data");
            var im = Image.NewFromFile(Helper.GifFile);
            _oneBit = im > 128;
        }

        [TearDown]
        public void Dispose()
        {
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void FileLoader(string loader, string testFile, Action<Image> validate)
        {
            var im = Operation.Call(loader, testFile) as Image;
            validate(im);
            im = Image.NewFromFile(testFile);
            validate(im);
        }

        public void BufferLoader(string loader, string testFile, Action<Image> validate)
        {
            var buf = File.ReadAllBytes(testFile);
            var im = Operation.Call(loader, buf) as Image;
            validate(im);
            im = Image.NewFromBuffer(buf);
            validate(im);
        }

        public void SaveLoad(string format, Image im)
        {
            var x = Image.NewTempFile(format);
            im.Write(x);

            Assert.AreEqual(x.Width, im.Width);
            Assert.AreEqual(x.Height, im.Height);
            Assert.AreEqual(x.Bands, im.Bands);
            var maxDiff = (im - x).Abs().Max();
            Assert.AreEqual(0, maxDiff);
        }

        public void SaveLoadFile(string format, string options, Image im, int thresh)
        {
            // yuk!
            // but we can't set format parameters for Image.NewTempFile()
            var filename = Helper.GetTemporaryFile(_tempDir, format);

            im.WriteToFile(filename + options);
            var x = Image.NewFromFile(filename);

            Assert.AreEqual(x.Width, im.Width);
            Assert.AreEqual(x.Height, im.Height);
            Assert.AreEqual(x.Bands, im.Bands);
            Assert.LessOrEqual((double) (im - x).Abs().Max(), thresh);
            x = null;
        }


        public void SaveLoadBuffer(string saver, string loader, Image im, int maxDiff = 0)
        {
            var buf = Operation.Call(saver, im) as byte[];
            var x = Operation.Call(loader, buf) as Image;

            Assert.AreEqual(x.Width, im.Width);
            Assert.AreEqual(x.Height, im.Height);
            Assert.AreEqual(x.Bands, im.Bands);
            Assert.LessOrEqual((double) (im - x).Abs().Max(), maxDiff);
        }

        public void SaveBufferTempFile(string saver, string suf, Image im, int maxDiff = 0)
        {
            var filename = Helper.GetTemporaryFile(_tempDir, suf);

            var buf = Operation.Call(saver, im) as byte[];
            File.WriteAllBytes(filename, buf);

            var x = Image.NewFromFile(filename);

            Assert.AreEqual(x.Width, im.Width);
            Assert.AreEqual(x.Height, im.Height);
            Assert.AreEqual(x.Bands, im.Bands);
            Assert.LessOrEqual((double) (im - x).Abs().Max(), maxDiff);
        }

        [Test]
        public void TestVips()
        {
            SaveLoadFile(".v", "", _colour, 0);

            // check we can save and restore metadata
            var filename = Helper.GetTemporaryFile(_tempDir, ".v");
            _colour.WriteToFile(filename);
            var x = Image.NewFromFile(filename);
            var beforeExif = _colour.Get("exif-data") as byte[];
            var afterExif = x.Get("exif-data") as byte[];

            Assert.AreEqual(beforeExif.Length, afterExif.Length);
            Assert.AreEqual(beforeExif, afterExif);
            x = null;
        }

        [Test]
        public void TestJpeg()
        {
            if (Base.TypeFind("VipsForeign", "jpegload") == 0)
            {
                Console.WriteLine("no jpeg support in this vips, skipping test");
                return;
            }

            void JpegValid(Image im)
            {
                var a = im.Getpoint(10, 10);
                CollectionAssert.AreEqual(new[] {6, 5, 3}, a);
                var profile = im.Get("icc-profile-data") as byte[];

                Assert.AreEqual(1352, profile.Length);
                Assert.AreEqual(1024, im.Width);
                Assert.AreEqual(768, im.Height);
                Assert.AreEqual(3, im.Bands);
            }

            FileLoader("jpegload", Helper.JpegFile, JpegValid);
            SaveLoad("%s.jpg", _mono);
            SaveLoad("%s.jpg", _colour);

            BufferLoader("jpegload_buffer", Helper.JpegFile, JpegValid);
            SaveLoadBuffer("jpegsave_buffer", "jpegload_buffer", _colour, 80);

            // see if we have exif parsing: our test image has this field
            var x = Image.NewFromFile(Helper.JpegFile);
            if (x.GetTypeOf("exif-ifd0-Orientation") != 0)
            {
                // we need a copy of the image to set the new metadata on
                // otherwise we get caching problems

                // can set, save and load new orientation
                x = Image.NewFromFile(Helper.JpegFile);
                x = x.Copy();
                x.Set("orientation", 2);
                var filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
                x.WriteToFile(filename);
                x = Image.NewFromFile(filename);
                var y = x.Get("orientation");
                Assert.AreEqual(2, y);

                // can remove orientation, save, load again, orientation
                // has reset
                x.Remove("orientation");
                filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
                x.WriteToFile(filename);
                x = Image.NewFromFile(filename);
                y = x.Get("orientation");
                Assert.AreEqual(1, y);

                // autorotate load works
                filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
                x = Image.NewFromFile(Helper.JpegFile);
                x = x.Copy();
                x.Set("orientation", 6);
                x.WriteToFile(filename);
                var x1 = Image.NewFromFile(filename);
                var x2 = Image.NewFromFile(filename, new Dictionary<string, object>
                {
                    {"autorotate", true}
                });
                Assert.AreEqual(x1.Width, x2.Height);
                Assert.AreEqual(x1.Height, x2.Width);
            }
        }

        [Test]
        public void TestPng()
        {
            if (Base.TypeFind("VipsForeign", "pngload") == 0 || !File.Exists(Helper.PngFile))
            {
                Console.WriteLine("no png support, skipping test");
                return;
            }

            void PngValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                CollectionAssert.AreEqual(new[]
                {
                    38671.0,
                    33914.0,
                    26762.0
                }, a);
                Assert.AreEqual(290, im.Width);
                Assert.AreEqual(442, im.Height);
                Assert.AreEqual(3, im.Bands);
            }

            FileLoader("pngload", Helper.PngFile, PngValid);
            BufferLoader("pngload_buffer", Helper.PngFile, PngValid);
            SaveLoadBuffer("pngsave_buffer", "pngload_buffer", _colour);
            SaveLoad("%s.png", _mono);
            SaveLoad("%s.png", _colour);
        }

        [Test]
        public void TestTiff()
        {
            if (Base.TypeFind("VipsForeign", "tiffload") == 0 || !File.Exists(Helper.TifFile))
            {
                Console.WriteLine("no tiff support, skipping test");
                return;
            }

            void TiffValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                CollectionAssert.AreEqual(new[]
                {
                    38671.0,
                    33914.0,
                    26762.0
                }, a);
                Assert.AreEqual(290, im.Width);
                Assert.AreEqual(442, im.Height);
                Assert.AreEqual(3, im.Bands);
            }

            FileLoader("tiffload", Helper.TifFile, TiffValid);
            BufferLoader("tiffload_buffer", Helper.TifFile, TiffValid);
            if (Base.AtLeastLibvips(8, 5))
            {
                SaveLoadBuffer("tiffsave_buffer", "tiffload_buffer", _colour);
            }

            SaveLoad("%s.tif", _mono);
            SaveLoad("%s.tif", _colour);
            SaveLoad("%s.tif", _cmyk);

            SaveLoad("%s.tif", _oneBit);
            SaveLoadFile(".tif", "[squash]", _oneBit, 0);
            SaveLoadFile(".tif", "[miniswhite]", _oneBit, 0);
            SaveLoadFile(".tif", "[squash,miniswhite]", _oneBit, 0);

            SaveLoadFile(".tif", $"[profile={Helper.SrgbFile}]", _colour, 0);
            SaveLoadFile(".tif", "[tile]", _colour, 0);
            SaveLoadFile(".tif", "[tile,pyramid]", _colour, 0);
            SaveLoadFile(".tif", "[tile,pyramid,compression=jpeg]", _colour, 80);
            SaveLoadFile(".tif", "[bigtiff]", _colour, 0);
            SaveLoadFile(".tif", "[compression=jpeg]", _colour, 80);
            SaveLoadFile(".tif", "[tile,tile-width=256]", _colour, 10);

            var filename = Helper.GetTemporaryFile(_tempDir, ".tif");
            var x = Image.NewFromFile(Helper.TifFile);
            x = x.Copy();
            x.Set("orientation", 2);
            x.WriteToFile(filename);
            x = Image.NewFromFile(filename);
            var y = x.Get("orientation");
            Assert.AreEqual(2, y);

            filename = Helper.GetTemporaryFile(_tempDir, ".tif");
            x = Image.NewFromFile(Helper.TifFile);
            x = x.Copy();
            x.Set("orientation", 2);
            x.WriteToFile(filename);
            x = Image.NewFromFile(filename);
            y = x.Get("orientation");
            Assert.AreEqual(2, y);
            x.Remove("orientation");


            filename = Helper.GetTemporaryFile(_tempDir, ".tif");
            x.WriteToFile(filename);
            x = Image.NewFromFile(filename);
            y = x.Get("orientation");
            Assert.AreEqual(1, y);

            filename = Helper.GetTemporaryFile(_tempDir, ".tif");
            x = Image.NewFromFile(Helper.TifFile);
            x = x.Copy();
            x.Set("orientation", 6);
            x.WriteToFile(filename);
            var x1 = Image.NewFromFile(filename);
            var x2 = Image.NewFromFile(filename, new Dictionary<string, object>
            {
                {"autorotate", true}
            });
            Assert.AreEqual(x1.Width, x2.Height);
            Assert.AreEqual(x1.Height, x2.Width);

            // OME support in 8.5
            if (Base.AtLeastLibvips(8, 5))
            {
                x = Image.NewFromFile(Helper.OmeFile);
                Assert.AreEqual(439, x.Width);
                Assert.AreEqual(167, x.Height);
                var pageHeight = x.Height;

                x = Image.NewFromFile(Helper.OmeFile, new Dictionary<string, object>
                {
                    {"n", -1}
                });
                Assert.AreEqual(439, x.Width);
                Assert.AreEqual(pageHeight * 15, x.Height);

                x = Image.NewFromFile(Helper.OmeFile, new Dictionary<string, object>
                {
                    {"page", 1},
                    {"n", -1}
                });
                Assert.AreEqual(439, x.Width);
                Assert.AreEqual(pageHeight * 14, x.Height);

                x = Image.NewFromFile(Helper.OmeFile, new Dictionary<string, object>
                {
                    {"page", 1},
                    {"n", 2}
                });
                Assert.AreEqual(439, x.Width);
                Assert.AreEqual(pageHeight * 2, x.Height);


                x = Image.NewFromFile(Helper.OmeFile, new Dictionary<string, object>
                {
                    {"n", -1}
                });
                Assert.AreEqual(96, x.Getpoint(0, 166)[0]);
                Assert.AreEqual(0, x.Getpoint(0, 167)[0]);
                Assert.AreEqual(1, x.Getpoint(0, 168)[0]);

                filename = Helper.GetTemporaryFile(_tempDir, ".tif");
                x.WriteToFile(filename);

                x = Image.NewFromFile(filename, new Dictionary<string, object>
                {
                    {"n", -1}
                });
                Assert.AreEqual(439, x.Width);
                Assert.AreEqual(pageHeight * 15, x.Height);
                Assert.AreEqual(96, x.Getpoint(0, 166)[0]);
                Assert.AreEqual(0, x.Getpoint(0, 167)[0]);
                Assert.AreEqual(1, x.Getpoint(0, 168)[0]);
            }

            // pyr save to buffer added in 8.6
            if (Base.AtLeastLibvips(8, 6))
            {
                x = Image.NewFromFile(Helper.TifFile);
                var buf = x.TiffsaveBuffer(new Dictionary<string, object>
                {
                    {"tile", true},
                    {"pyramid", true}
                });
                filename = Helper.GetTemporaryFile(_tempDir, ".tif");
                x.Tiffsave(filename, new Dictionary<string, object>
                {
                    {"tile", true},
                    {"pyramid", true}
                });
                var buf2 = File.ReadAllBytes(filename);
                Assert.AreEqual(buf.Length, buf2.Length);

                var a = Image.NewFromBuffer(buf, "", new Dictionary<string, object>
                {
                    {"page", 2},
                });
                var b = Image.NewFromBuffer(buf2, "", new Dictionary<string, object>
                {
                    {"page", 2},
                });
                Assert.AreEqual(a.Width, b.Width);
                Assert.AreEqual(a.Height, b.Height);
                Assert.AreEqual(a.Avg(), b.Avg());
            }
        }

        [Test]
        public void TestMagickLoad()
        {
            if (Base.TypeFind("VipsForeign", "magickload") == 0 || !File.Exists(Helper.GifFile))
            {
                Console.WriteLine("no magick support, skipping test");
                return;
            }

            void GifValid(Image im)
            {
                // some libMagick produce an RGB for this image, some a mono, some
                // rgba, some have a valid alpha, some don't :-(
                // therefore ... just test channel 0
                var a = im.Getpoint(10, 10)[0];

                Assert.AreEqual(33, a);
                Assert.AreEqual(159, im.Width);
                Assert.AreEqual(203, im.Height);
            }

            FileLoader("magickload", Helper.GifFile, GifValid);
            BufferLoader("magickload_buffer", Helper.GifFile, GifValid);

            // we should have rgba for svg files
            var x = Image.Magickload(Helper.SvgFile);
            Assert.AreEqual(4, x.Bands);

            // density should change size of generated svg
            x = Image.Magickload(Helper.SvgFile, new Dictionary<string, object>
            {
                {"density", "100"},
            });
            var width = x.Width;
            var height = x.Height;
            x = Image.Magickload(Helper.SvgFile, new Dictionary<string, object>
            {
                {"density", "200"},
            });

            // This seems to fail on travis, no idea why, some problem in their IM
            // perhaps
            //Assert.AreEqual(width * 2, x.Width);
            //Assert.AreEqual(height * 2, x.Height);

            // all-frames should load every frame of the animation
            // (though all-frames is deprecated)
            x = Image.Magickload(Helper.GifAnimFile);
            width = x.Width;
            height = x.Height;
            x = Image.Magickload(Helper.GifAnimFile, new Dictionary<string, object>
            {
                {"all_frames", true},
            });
            Assert.AreEqual(width, x.Width);
            Assert.AreEqual(height * 5, x.Height);

            // page/n let you pick a range of pages
            // 'n' param added in 8.5
            if (Base.AtLeastLibvips(8, 5))
            {
                x = Image.Magickload(Helper.GifAnimFile);
                width = x.Width;
                height = x.Height;
                x = Image.Magickload(Helper.GifAnimFile, new Dictionary<string, object>
                {
                    {"page", 1},
                    {"n", 2}
                });
                Assert.AreEqual(width, x.Width);
                Assert.AreEqual(height * 2, x.Height);

                var pageHeight = x.Get("page-height");
                Assert.AreEqual(height, pageHeight);
            }

            // should work for dicom
            x = Image.Magickload(Helper.DicomFile);
            Assert.AreEqual(128, x.Width);
            Assert.AreEqual(128, x.Height);

            // some IMs are 3 bands, some are 1, can't really test
            // Assert.AreEqual(1, x.Bands);
        }

        [Test]
        public void TestWebp()
        {
            if (Base.TypeFind("VipsForeign", "webpload") == 0 || !File.Exists(Helper.WebpFile))
            {
                Console.WriteLine("no webp support, skipping test");
                return;
            }

            void WebpValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                CollectionAssert.AreEqual(new[]
                {
                    71,
                    166,
                    236
                }, a);
                Assert.AreEqual(550, im.Width);
                Assert.AreEqual(368, im.Height);
                Assert.AreEqual(3, im.Bands);
            }

            FileLoader("webpload", Helper.WebpFile, WebpValid);
            BufferLoader("webpload_buffer", Helper.WebpFile, WebpValid);
            SaveLoadBuffer("webpsave_buffer", "webpload_buffer", _colour, 60);
            SaveLoad("%s.webp", _colour);

            // test lossless mode
            var x = Image.NewFromFile(Helper.WebpFile);
            var buf = x.WebpsaveBuffer(new Dictionary<string, object>
            {
                {"lossless", true}
            });

            var im2 = Image.NewFromBuffer(buf);
            Assert.AreEqual(x.Avg(), im2.Avg());

            // higher Q should mean a bigger buffer
            var b1 = x.WebpsaveBuffer(new Dictionary<string, object>
            {
                {"Q", 10}
            });
            var b2 = x.WebpsaveBuffer(new Dictionary<string, object>
            {
                {"Q", 90}
            });
            Assert.Greater(b2.Length, b1.Length);

            // try saving an image with an ICC profile and reading it back ... if we
            // can do it, our webp supports metadata load/save
            buf = _colour.WebpsaveBuffer();
            x = Image.NewFromBuffer(buf);
            if (x.GetTypeOf("icc-profile-data") != 0)
            {
                // verify that the profile comes back unharmed
                var p1 = _colour.Get("icc-profile-data");
                var p2 = _colour.Get("icc-profile-data");
                Assert.AreEqual(p1, p2);

                // add tests for exif, xmp, ipct
                // the exif test will need us to be able to walk the header,
                // we can't just check exif-data

                // we can test that exif changes change the output of webpsave
                // first make sure we have exif support
                var z = Image.NewFromFile(Helper.JpegFile);
                if (z.GetTypeOf("exif-ifd0-Orientation") != 0)
                {
                    var w = _colour.Copy();
                    w.Set("orientation", 6);
                    buf = x.WebpsaveBuffer();
                    var y = Image.NewFromBuffer(buf);
                    Assert.AreEqual(6, y.Get("orientation"));
                }
            }
        }

        [Test]
        public void TestAnalyzeLoad()
        {
            if (Base.TypeFind("VipsForeign", "analyzeload") == 0 || !File.Exists(Helper.AnalyzeFile))
            {
                Console.WriteLine("no analyze support, skipping test");
                return;
            }

            void AnalyzeValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Assert.AreEqual(3335, a[0]);
                Assert.AreEqual(128, im.Width);
                Assert.AreEqual(8064, im.Height);
                Assert.AreEqual(1, im.Bands);
            }

            FileLoader("analyzeload", Helper.AnalyzeFile, AnalyzeValid);
        }

        [Test]
        public void TestMatLoad()
        {
            if (Base.TypeFind("VipsForeign", "matload") == 0 || !File.Exists(Helper.MatlabFile))
            {
                Console.WriteLine("no matlab support, skipping test");
                return;
            }

            void MatlabValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                CollectionAssert.AreEqual(new[]
                {
                    38671.0,
                    33914.0,
                    26762.0
                }, a);
                Assert.AreEqual(290, im.Width);
                Assert.AreEqual(442, im.Height);
                Assert.AreEqual(3, im.Bands);
            }

            FileLoader("matload", Helper.MatlabFile, MatlabValid);
        }

        [Test]
        public void TestOpenexrLoad()
        {
            if (Base.TypeFind("VipsForeign", "openexrload") == 0 || !File.Exists(Helper.ExrFile))
            {
                Console.WriteLine("no openexr support, skipping test");
                return;
            }

            void ExrValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Helper.AssertAlmostEqualObjects(new[]
                {
                    0.124512,
                    0.159668,
                    0.040375,
                    1.0
                }, a, "", 0.00001);
                Assert.AreEqual(610, im.Width);
                Assert.AreEqual(406, im.Height);
                Assert.AreEqual(4, im.Bands);
            }

            FileLoader("openexrload", Helper.ExrFile, ExrValid);
        }

        [Test]
        public void TestsFitsLoad()
        {
            if (Base.TypeFind("VipsForeign", "fitsload") == 0 || !File.Exists(Helper.FitsFile))
            {
                Console.WriteLine("no fits support, skipping test");
                return;
            }


            void FitsValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Helper.AssertAlmostEqualObjects(new[]
                {
                    -0.165013,
                    -0.148553,
                    1.09122,
                    -0.942242
                }, a, "", 0.00001);
                Assert.AreEqual(200, im.Width);
                Assert.AreEqual(200, im.Height);
                Assert.AreEqual(4, im.Bands);
            }

            FileLoader("fitsload", Helper.FitsFile, FitsValid);
            SaveLoad("%s.fits", _mono);
        }

        [Test]
        public void TestOpenslideLoad()
        {
            if (Base.TypeFind("VipsForeign", "openslideload") == 0 || !File.Exists(Helper.OpenslideFile))
            {
                Console.WriteLine("no openslide support, skipping test");
                return;
            }

            void OpenslideValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                CollectionAssert.AreEqual(new[]
                {
                    244,
                    250,
                    243,
                    255
                }, a);
                Assert.AreEqual(2220, im.Width);
                Assert.AreEqual(2967, im.Height);
                Assert.AreEqual(4, im.Bands);
            }

            FileLoader("openslideload", Helper.OpenslideFile, OpenslideValid);
        }

        [Test]
        public void TestPdfLoad()
        {
            if (Base.TypeFind("VipsForeign", "pdfload") == 0 || !File.Exists(Helper.PdfFile))
            {
                Console.WriteLine("no pdf support, skipping test");
                return;
            }


            void PdfValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                CollectionAssert.AreEqual(new[]
                {
                    35,
                    31,
                    32,
                    255
                }, a);
                Assert.AreEqual(1133, im.Width);
                Assert.AreEqual(680, im.Height);
                Assert.AreEqual(4, im.Bands);
            }

            FileLoader("pdfload", Helper.PdfFile, PdfValid);
            BufferLoader("pdfload_buffer", Helper.PdfFile, PdfValid);

            var x = Image.NewFromFile(Helper.PdfFile);
            var y = Image.NewFromFile(Helper.PdfFile, new Dictionary<string, object>
            {
                {"scale", 2.0}
            });
            Assert.Less(Math.Abs(x.Width * 2 - y.Width), 2);
            Assert.Less(Math.Abs(x.Height * 2 - y.Height), 2);

            x = Image.NewFromFile(Helper.PdfFile);
            y = Image.NewFromFile(Helper.PdfFile, new Dictionary<string, object>
            {
                {"dpi", 144.0}
            });
            Assert.Less(Math.Abs(x.Width * 2 - y.Width), 2);
            Assert.Less(Math.Abs(x.Height * 2 - y.Height), 2);
        }

        [Test]
        public void TestGifLoad()
        {
            if (Base.TypeFind("VipsForeign", "gifload") == 0 || !File.Exists(Helper.GifFile))
            {
                Console.WriteLine("no gif support, skipping test");
                return;
            }

            void GifValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                CollectionAssert.AreEqual(new[]
                {
                    33
                }, a);
                Assert.AreEqual(159, im.Width);
                Assert.AreEqual(203, im.Height);
                Assert.AreEqual(1, im.Bands);
            }

            FileLoader("gifload", Helper.GifFile, GifValid);
            BufferLoader("gifload_buffer", Helper.GifFile, GifValid);

            // 'n' param added in 8.5
            if (Base.AtLeastLibvips(8, 5))
            {
                var x1 = Image.NewFromFile(Helper.GifAnimFile);
                var x2 = Image.NewFromFile(Helper.GifAnimFile, new Dictionary<string, object>
                {
                    {"n", 2}
                });
                Assert.AreEqual(2 * x1.Height, x2.Height);
                var pageHeight = x2.Get("page-height");
                Assert.AreEqual(x1.Height, pageHeight);

                x2 = Image.NewFromFile(Helper.GifAnimFile, new Dictionary<string, object>
                {
                    {"n", -1}
                });
                Assert.AreEqual(5 * x1.Height, x2.Height);

                x2 = Image.NewFromFile(Helper.GifAnimFile, new Dictionary<string, object>
                {
                    {"page", 1},
                    {"n", -1}
                });
                Assert.AreEqual(4 * x1.Height, x2.Height);
            }
        }

        [Test]
        public void TestSvgLoad()
        {
            if (Base.TypeFind("VipsForeign", "svgload") == 0 || !File.Exists(Helper.SvgFile))
            {
                Console.WriteLine("no svg support, skipping test");
                return;
            }

            void SvgValid(Image im)
            {
                var a = im.Getpoint(10, 10);


                // some old rsvg versions are way, way off
                Assert.Less(Math.Abs(a[0] - 79), 2);
                Assert.Less(Math.Abs(a[1] - 79), 2);
                Assert.Less(Math.Abs(a[2] - 132), 2);
                Assert.Less(Math.Abs(a[3] - 255), 2);

                Assert.AreEqual(288, im.Width);
                Assert.AreEqual(470, im.Height);
                Assert.AreEqual(4, im.Bands);
            }

            FileLoader("svgload", Helper.SvgFile, SvgValid);
            BufferLoader("svgload_buffer", Helper.SvgFile, SvgValid);

            FileLoader("svgload", Helper.SvgzFile, SvgValid);
            BufferLoader("svgload_buffer", Helper.SvgzFile, SvgValid);

            FileLoader("svgload", Helper.SvgGzFile, SvgValid);

            var x = Image.NewFromFile(Helper.SvgFile);
            var y = Image.NewFromFile(Helper.SvgFile, new Dictionary<string, object>
            {
                {"scale", 2.0}
            });

            Assert.Less(Math.Abs(x.Width * 2 - y.Width), 2);
            Assert.Less(Math.Abs(x.Height * 2 - y.Height), 2);

            x = Image.NewFromFile(Helper.SvgFile);
            y = Image.NewFromFile(Helper.SvgFile, new Dictionary<string, object>
            {
                {"dpi", 144.0}
            });
            Assert.Less(Math.Abs(x.Width * 2 - y.Width), 2);
            Assert.Less(Math.Abs(x.Height * 2 - y.Height), 2);
        }

        [Test]
        public void TestCsv()
        {
            SaveLoad("%s.csv", _mono);
        }

        [Test]
        public void TestMatrix()
        {
            SaveLoad("%s.mat", _mono);
        }

        [Test]
        public void TestPpm()
        {
            if (Base.TypeFind("VipsForeign", "ppmload") == 0)
            {
                Console.WriteLine("no PPM support, skipping test");
                return;
            }

            SaveLoad("%s.ppm", _mono);
            SaveLoad("%s.ppm", _colour);
        }

        [Test]
        public void TestRad()
        {
            if (Base.TypeFind("VipsForeign", "radload") == 0)
            {
                Console.WriteLine("no Radiance support, skipping test");
                return;
            }

            SaveLoad("%s.hdr", _colour);
            SaveBufferTempFile("radsave_buffer", ".hdr", _rad);
        }

        [Test]
        public void TestDzSave()
        {
            if (Base.TypeFind("VipsForeign", "dzsave") == 0)
            {
                Console.WriteLine("no dzsave support, skipping test");
                return;
            }

            // dzsave is hard to test, there are so many options
            // test each option separately and hope they all function together
            // correctly

            // default deepzoom layout ... we must use png here, since we want to
            // test the overlap for equality
            var filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, new Dictionary<string, object>
            {
                {"suffix", ".png"}
            });

            // test horizontal overlap ... expect 256 step, overlap 1
            var x = Image.NewFromFile(filename + "_files/10/0_0.png");
            Assert.AreEqual(255, x.Width);
            var y = Image.NewFromFile(filename + "_files/10/1_0.png");
            Assert.AreEqual(256, y.Width);

            // the right two columns of x should equal the left two columns of y
            var left = x.ExtractArea(x.Width - 2, 0, 2, x.Height);
            var right = y.ExtractArea(0, 0, 2, y.Height);
            Assert.AreEqual(0, (left - right).Abs().Max());

            // test vertical overlap
            Assert.AreEqual(255, x.Height);
            y = Image.NewFromFile(filename + "_files/10/0_1.png");
            Assert.AreEqual(256, y.Height);

            // the bottom two rows of x should equal the top two rows of y
            var top = x.ExtractArea(0, x.Height - 2, x.Width, 2);
            var bottom = y.ExtractArea(0, 0, y.Width, 2);
            Assert.AreEqual(0, (top - bottom).Abs().Max());

            // there should be a bottom layer
            x = Image.NewFromFile(filename + "_files/0/0_0.png");
            Assert.AreEqual(1, x.Width);
            Assert.AreEqual(1, x.Height);

            // 10 should be the final layer
            Assert.IsFalse(Directory.Exists(filename + "_files/11"));

            // default google layout
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, new Dictionary<string, object>
            {
                {"layout", "google"}
            });

            // test bottom-right tile ... default is 256x256 tiles, overlap 0
            x = Image.NewFromFile(filename + "/2/2/3.jpg");
            Assert.AreEqual(256, x.Width);
            Assert.AreEqual(256, x.Height);
            Assert.IsFalse(Directory.Exists(filename + "/2/2/4.jpg"));
            Assert.IsFalse(Directory.Exists(filename + "/3"));
            x = Image.NewFromFile(filename + "/blank.png");
            Assert.AreEqual(256, x.Width);
            Assert.AreEqual(256, x.Height);

            // google layout with overlap ... verify that we clip correctly

            // overlap 1, 510x510 pixels, 256 pixel tiles, should be exactly 2x2
            // tiles, though in fact the bottom and right edges will be white
            filename = Helper.GetTemporaryFile(_tempDir, "");

            _colour.ExtractArea(0, 0, 510, 510).Dzsave(filename, new Dictionary<string, object>
            {
                {"layout", "google"},
                {"overlap", 1},
                {"depth", "one"}
            });

            x = Image.NewFromFile(filename + "/0/1/1.jpg");
            Assert.AreEqual(256, x.Width);
            Assert.AreEqual(256, x.Height);
            Assert.IsFalse(Directory.Exists(filename + "/0/2/2.jpg"));

            // with 511x511, it'll fit exactly into 2x2 -- we we actually generate
            // 3x3, since we output the overlaps
            // 8.6 revised the rules on overlaps, so don't test earlier than that
            if (Base.AtLeastLibvips(8, 6))
            {
                filename = Helper.GetTemporaryFile(_tempDir, "");
                _colour.ExtractArea(0, 0, 511, 511).Dzsave(filename, new Dictionary<string, object>
                {
                    {"layout", "google"},
                    {"overlap", 1},
                    {"depth", "one"}
                });

                x = Image.NewFromFile(filename + "/0/2/2.jpg");
                Assert.AreEqual(256, x.Width);
                Assert.AreEqual(256, x.Height);
                Assert.IsFalse(Directory.Exists(filename + "/0/3/3.jpg"));
            }

            // default zoomify layout
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, new Dictionary<string, object>
            {
                {"layout", "zoomify"}
            });

            // 256x256 tiles, no overlap
            Assert.IsTrue(File.Exists(filename + "/ImageProperties.xml"));
            x = Image.NewFromFile(filename + "/TileGroup0/2-3-2.jpg");
            Assert.AreEqual(256, x.Width);
            Assert.AreEqual(256, x.Height);

            // test zip output
            filename = Helper.GetTemporaryFile(_tempDir, ".zip");
            _colour.Dzsave(filename);
            // before 8.5.8, you needed a gc on pypy to flush small zip output to
            // disc
            // TODO Is this needed for C#?
            if (!Base.AtLeastLibvips(8, 6))
            {
                GC.Collect();
            }

            Assert.IsTrue(File.Exists(filename));
            Assert.IsFalse(Directory.Exists(filename + "_files"));
            Assert.IsFalse(File.Exists(filename + ".dzi"));

            // test compressed zip output
            var filename2 = Helper.GetTemporaryFile(_tempDir, ".zip");
            _colour.Dzsave(filename2, new Dictionary<string, object>
            {
                {"compression", -1}
            });
            // before 8.5.8, you needed a gc on pypy to flush small zip output to
            // disc
            // TODO Is this needed for C#?
            if (!Base.AtLeastLibvips(8, 6))
            {
                GC.Collect();
            }

            Assert.IsTrue(File.Exists(filename2));
            Assert.Less(new FileInfo(filename2).Length, new FileInfo(filename).Length);

            // test suffix
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, new Dictionary<string, object>
            {
                {"suffix", ".png"}
            });

            x = Image.NewFromFile(filename + "_files/10/0_0.png");
            Assert.AreEqual(255, x.Width);

            // test overlap
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, new Dictionary<string, object>
            {
                {"overlap", 200}
            });

            x = Image.NewFromFile(filename + "_files/10/1_1.jpeg");
            Assert.AreEqual(654, x.Width);

            // test tile-size
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, new Dictionary<string, object>
            {
                {"tile_size", 512}
            });

            y = Image.NewFromFile(filename + "_files/10/0_0.jpeg");
            Assert.AreEqual(513, y.Width);
            Assert.AreEqual(513, y.Height);

            // test save to memory buffer
            if (Base.TypeFind("VipsForeign", "dzsave_buffer") != 0)
            {
                filename = Helper.GetTemporaryFile(_tempDir, ".zip");
                var baseName = Path.GetFileNameWithoutExtension(filename);

                _colour.Dzsave(filename);
                // before 8.5.8, you needed a gc on pypy to flush small zip
                // output to disc
                // TODO Is this needed for C#?
                if (!Base.AtLeastLibvips(8, 6))
                {
                    GC.Collect();
                }

                var buf1 = File.ReadAllBytes(filename);
                var buf2 = _colour.DzsaveBuffer(new Dictionary<string, object>
                {
                    {"basename", baseName}
                });
                Assert.AreEqual(buf1.Length, buf2.Length);

                // we can't test the bytes are exactly equal -- the timestamps will
                // be different
            }
        }
    }
}