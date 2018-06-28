using System;
using System.IO;
using Xunit;

namespace NetVips.Tests
{
    public class ForeignTests : IClassFixture<TestsFixture>, IDisposable
    {
        private string _tempDir;

        private Image _colour;
        private Image _mono;
        private Image _rad;
        private Image _cmyk;
        private Image _oneBit;

        public ForeignTests()
        {
            _tempDir = Helper.GetTemporaryDirectory();

            _colour = Image.Jpegload(Helper.JpegFile);
            _mono = _colour[0];

            // we remove the ICC profile: the RGB one will no longer be appropriate
            _mono.Remove("icc-profile-data");
            _rad = _colour.Float2rad();
            _rad.Remove("icc-profile-data");
            _cmyk = _colour.Bandjoin(_mono);
            _cmyk = _cmyk.Copy(interpretation: Enums.Interpretation.Cmyk);
            _cmyk.Remove("icc-profile-data");
            var im = Image.NewFromFile(Helper.GifFile);
            _oneBit = im > 128;
        }

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

        #region helpers

        internal void FileLoader(string loader, string testFile, Action<Image> validate)
        {
            var im = Operation.Call(loader, testFile) as Image;
            validate(im);
            im = Image.NewFromFile(testFile);
            validate(im);
        }

        internal void BufferLoader(string loader, string testFile, Action<Image> validate)
        {
            var buf = File.ReadAllBytes(testFile);
            var im = Operation.Call(loader, buf) as Image;
            validate(im);
            im = Image.NewFromBuffer(buf);
            validate(im);
        }

        internal void SaveLoad(string format, Image im)
        {
            var x = Image.NewTempFile(format);
            im.Write(x);

            Assert.Equal(x.Width, im.Width);
            Assert.Equal(x.Height, im.Height);
            Assert.Equal(x.Bands, im.Bands);
            var maxDiff = (im - x).Abs().Max();
            Assert.Equal(0, maxDiff);
        }

        internal void SaveLoadFile(string format, string options, Image im, int thresh)
        {
            // yuk!
            // but we can't set format parameters for Image.NewTempFile()
            var filename = Helper.GetTemporaryFile(_tempDir, format);

            im.WriteToFile(filename + options);
            var x = Image.NewFromFile(filename);

            Assert.Equal(x.Width, im.Width);
            Assert.Equal(x.Height, im.Height);
            Assert.Equal(x.Bands, im.Bands);
            Assert.True((im - x).Abs().Max() <= thresh);
        }

        internal void SaveLoadBuffer(string saver, string loader, Image im, int maxDiff = 0, VOption kwargs = null)
        {
            var buf = Operation.Call(saver, kwargs, im) as byte[];
            var x = Operation.Call(loader, buf) as Image;

            Assert.Equal(x.Width, im.Width);
            Assert.Equal(x.Height, im.Height);
            Assert.Equal(x.Bands, im.Bands);
            Assert.True((im - x).Abs().Max() <= maxDiff);
        }

        internal void SaveBufferTempFile(string saver, string suf, Image im, int maxDiff = 0)
        {
            var filename = Helper.GetTemporaryFile(_tempDir, suf);

            var buf = Operation.Call(saver, im) as byte[];
            File.WriteAllBytes(filename, buf);

            var x = Image.NewFromFile(filename);

            Assert.Equal(x.Width, im.Width);
            Assert.Equal(x.Height, im.Height);
            Assert.Equal(x.Bands, im.Bands);
            Assert.True((im - x).Abs().Max() <= maxDiff);
        }

        #endregion

        [Fact]
        public void TestVips()
        {
            SaveLoadFile(".v", "", _colour, 0);

            // check we can save and restore metadata
            var filename = Helper.GetTemporaryFile(_tempDir, ".v");
            _colour.WriteToFile(filename);
            var x = Image.NewFromFile(filename);
            var beforeExif = _colour.Get("exif-data") as byte[];
            var afterExif = x.Get("exif-data") as byte[];

            Assert.Equal(beforeExif.Length, afterExif.Length);
            Assert.Equal(beforeExif, afterExif);
        }

        [SkippableFact]
        public void TestJpeg()
        {
            Skip.IfNot(Helper.Have("jpegload"), "no jpeg support in this vips, skipping test");

            void JpegValid(Image im)
            {
                var a = im.Getpoint(10, 10);
                Assert.Equal(new double[] {6, 5, 3}, a);
                var profile = im.Get("icc-profile-data") as byte[];

                Assert.Equal(1352, profile.Length);
                Assert.Equal(1024, im.Width);
                Assert.Equal(768, im.Height);
                Assert.Equal(3, im.Bands);
            }

            FileLoader("jpegload", Helper.JpegFile, JpegValid);
            SaveLoad("%s.jpg", _mono);
            SaveLoad("%s.jpg", _colour);

            BufferLoader("jpegload_buffer", Helper.JpegFile, JpegValid);
            SaveLoadBuffer("jpegsave_buffer", "jpegload_buffer", _colour, 80);

            // see if we have exif parsing: our test image has this field
            var x = Image.NewFromFile(Helper.JpegFile);
            if (x.Contains("exif-ifd0-Orientation"))
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
                Assert.Equal(2, y);

                // can remove orientation, save, load again, orientation
                // has reset
                x.Remove("orientation");
                filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
                x.WriteToFile(filename);
                x = Image.NewFromFile(filename);
                y = x.Get("orientation");
                Assert.Equal(1, y);

                // autorotate load works
                filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
                x = Image.NewFromFile(Helper.JpegFile);
                x = x.Copy();
                x.Set("orientation", 6);
                x.WriteToFile(filename);
                var x1 = Image.NewFromFile(filename);
                var x2 = Image.NewFromFile(filename, kwargs: new VOption
                {
                    {"autorotate", true}
                });
                Assert.Equal(x1.Width, x2.Height);
                Assert.Equal(x1.Height, x2.Width);
            }
        }

        [SkippableFact]
        public void TestPng()
        {
            Skip.IfNot(Helper.Have("pngload") && File.Exists(Helper.PngFile), "no png support, skipping test");

            void PngValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Assert.Equal(new[] {38671.0, 33914.0, 26762.0}, a);
                Assert.Equal(290, im.Width);
                Assert.Equal(442, im.Height);
                Assert.Equal(3, im.Bands);
            }

            FileLoader("pngload", Helper.PngFile, PngValid);
            BufferLoader("pngload_buffer", Helper.PngFile, PngValid);
            SaveLoadBuffer("pngsave_buffer", "pngload_buffer", _colour);
            SaveLoad("%s.png", _mono);
            SaveLoad("%s.png", _colour);
        }

        [SkippableFact]
        public void TestBufferOverload()
        {
            Skip.IfNot(Helper.Have("pngload"), "no png support, skipping test");

            var buf = _colour.WriteToBuffer(".png");
            var x = Image.NewFromBuffer(buf);

            Assert.Equal(x.Width, _colour.Width);
            Assert.Equal(x.Height, _colour.Height);
            Assert.Equal(x.Bands, _colour.Bands);
            Assert.True((_colour - x).Abs().Max() <= 0);
        }

        [SkippableFact]
        public void TestTiff()
        {
            Skip.IfNot(Helper.Have("tiffload") && File.Exists(Helper.TifFile), "no tiff support, skipping test");

            void TiffValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Assert.Equal(new[] {38671.0, 33914.0, 26762.0}, a);
                Assert.Equal(290, im.Width);
                Assert.Equal(442, im.Height);
                Assert.Equal(3, im.Bands);
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
            Assert.Equal(2, y);

            filename = Helper.GetTemporaryFile(_tempDir, ".tif");
            x = Image.NewFromFile(Helper.TifFile);
            x = x.Copy();
            x.Set("orientation", 2);
            x.WriteToFile(filename);
            x = Image.NewFromFile(filename);
            y = x.Get("orientation");
            Assert.Equal(2, y);
            x.Remove("orientation");


            filename = Helper.GetTemporaryFile(_tempDir, ".tif");
            x.WriteToFile(filename);
            x = Image.NewFromFile(filename);
            y = x.Get("orientation");
            Assert.Equal(1, y);

            filename = Helper.GetTemporaryFile(_tempDir, ".tif");
            x = Image.NewFromFile(Helper.TifFile);
            x = x.Copy();
            x.Set("orientation", 6);
            x.WriteToFile(filename);
            var x1 = Image.NewFromFile(filename);
            var x2 = Image.NewFromFile(filename, kwargs: new VOption
            {
                {"autorotate", true}
            });
            Assert.Equal(x1.Width, x2.Height);
            Assert.Equal(x1.Height, x2.Width);

            // OME support in 8.5
            if (Base.AtLeastLibvips(8, 5))
            {
                x = Image.NewFromFile(Helper.OmeFile);
                Assert.Equal(439, x.Width);
                Assert.Equal(167, x.Height);
                var pageHeight = x.Height;

                x = Image.NewFromFile(Helper.OmeFile, kwargs: new VOption
                {
                    {"n", -1}
                });
                Assert.Equal(439, x.Width);
                Assert.Equal(pageHeight * 15, x.Height);

                x = Image.NewFromFile(Helper.OmeFile, kwargs: new VOption
                {
                    {"page", 1},
                    {"n", -1}
                });
                Assert.Equal(439, x.Width);
                Assert.Equal(pageHeight * 14, x.Height);

                x = Image.NewFromFile(Helper.OmeFile, kwargs: new VOption
                {
                    {"page", 1},
                    {"n", 2}
                });
                Assert.Equal(439, x.Width);
                Assert.Equal(pageHeight * 2, x.Height);


                x = Image.NewFromFile(Helper.OmeFile, kwargs: new VOption
                {
                    {"n", -1}
                });
                Assert.Equal(96, x.Getpoint(0, 166)[0]);
                Assert.Equal(0, x.Getpoint(0, 167)[0]);
                Assert.Equal(1, x.Getpoint(0, 168)[0]);

                filename = Helper.GetTemporaryFile(_tempDir, ".tif");
                x.WriteToFile(filename);

                x = Image.NewFromFile(filename, kwargs: new VOption
                {
                    {"n", -1}
                });
                Assert.Equal(439, x.Width);
                Assert.Equal(pageHeight * 15, x.Height);
                Assert.Equal(96, x.Getpoint(0, 166)[0]);
                Assert.Equal(0, x.Getpoint(0, 167)[0]);
                Assert.Equal(1, x.Getpoint(0, 168)[0]);
            }

            // pyr save to buffer added in 8.6
            if (Base.AtLeastLibvips(8, 6))
            {
                x = Image.NewFromFile(Helper.TifFile);
                var buf = x.TiffsaveBuffer(tile: true, pyramid: true);
                filename = Helper.GetTemporaryFile(_tempDir, ".tif");
                x.Tiffsave(filename, tile: true, pyramid: true);
                var buf2 = File.ReadAllBytes(filename);
                Assert.Equal(buf.Length, buf2.Length);

                var a = Image.NewFromBuffer(buf, "", kwargs: new VOption
                {
                    {"page", 2}
                });
                var b = Image.NewFromBuffer(buf2, "", kwargs: new VOption
                {
                    {"page", 2}
                });
                Assert.Equal(a.Width, b.Width);
                Assert.Equal(a.Height, b.Height);
                Assert.Equal(a.Avg(), b.Avg());
            }
        }

        [SkippableFact]
        public void TestMagickLoad()
        {
            Skip.IfNot(Helper.Have("magickload") && File.Exists(Helper.BmpFile), "no magick support, skipping test");

            void BmpValid(Image im)
            {
                var a = im.Getpoint(100, 100);

                Helper.AssertAlmostEqualObjects(new double[] {227, 216, 201}, a);
                Assert.Equal(1419, im.Width);
                Assert.Equal(1001, im.Height);
            }

            FileLoader("magickload", Helper.BmpFile, BmpValid);
            BufferLoader("magickload_buffer", Helper.BmpFile, BmpValid);

            // we should have rgba for svg files
            var x = Image.Magickload(Helper.SvgFile);
            Assert.Equal(4, x.Bands);

            // density should change size of generated svg
            x = Image.Magickload(Helper.SvgFile, density: "100");
            var width = x.Width;
            var height = x.Height;
            x = Image.Magickload(Helper.SvgFile, density: "200");

            // This seems to fail on travis, no idea why, some problem in their IM
            // perhaps
            //Assert.Equal(width * 2, x.Width);
            //Assert.Equal(height * 2, x.Height);

            // page/n let you pick a range of pages
            // 'n' param added in 8.5
            if (Base.AtLeastLibvips(8, 5))
            {
                x = Image.Magickload(Helper.GifAnimFile);
                width = x.Width;
                height = x.Height;
                x = Image.Magickload(Helper.GifAnimFile, page: 1, n: 2);
                Assert.Equal(width, x.Width);
                Assert.Equal(height * 2, x.Height);

                var pageHeight = x.Get("page-height");
                Assert.Equal(height, pageHeight);
            }

            // should work for dicom
            x = Image.Magickload(Helper.DicomFile);
            Assert.Equal(128, x.Width);
            Assert.Equal(128, x.Height);

            // some IMs are 3 bands, some are 1, can't really test
            // Assert.Equal(1, x.Bands);

            // added in 8.7
            if (Helper.Have("magicksave"))
            {
                SaveLoadFile(".bmp", "", _colour, 0);
                SaveLoadBuffer("magicksave_buffer", "magickload_buffer", _colour, 0, new VOption
                {
                    {"format", "BMP"}
                });
                SaveLoad("%s.bmp", _colour);
            }
        }

        [SkippableFact]
        public void TestWebp()
        {
            Skip.IfNot(Helper.Have("webpload") && File.Exists(Helper.WebpFile), "no webp support, skipping test");

            void WebpValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Assert.Equal(new double[] {71, 166, 236}, a);
                Assert.Equal(550, im.Width);
                Assert.Equal(368, im.Height);
                Assert.Equal(3, im.Bands);
            }

            FileLoader("webpload", Helper.WebpFile, WebpValid);
            BufferLoader("webpload_buffer", Helper.WebpFile, WebpValid);
            SaveLoadBuffer("webpsave_buffer", "webpload_buffer", _colour, 60);
            SaveLoad("%s.webp", _colour);

            // test lossless mode
            var x = Image.NewFromFile(Helper.WebpFile);
            var buf = x.WebpsaveBuffer(lossless: true);
            var im2 = Image.NewFromBuffer(buf);
            Assert.Equal(x.Avg(), im2.Avg());

            // higher Q should mean a bigger buffer
            var b1 = x.WebpsaveBuffer(q: 10);
            var b2 = x.WebpsaveBuffer(q: 90);
            Assert.True(b2.Length > b1.Length);

            // try saving an image with an ICC profile and reading it back ... if we
            // can do it, our webp supports metadata load/save
            buf = _colour.WebpsaveBuffer();
            x = Image.NewFromBuffer(buf);
            if (x.Contains("icc-profile-data"))
            {
                // verify that the profile comes back unharmed
                var p1 = _colour.Get("icc-profile-data");
                var p2 = x.Get("icc-profile-data");
                Assert.Equal(p1, p2);

                // add tests for exif, xmp, ipct
                // the exif test will need us to be able to walk the header,
                // we can't just check exif-data

                // we can test that exif changes change the output of webpsave
                // first make sure we have exif support
                var z = Image.NewFromFile(Helper.JpegFile);
                if (z.Contains("exif-ifd0-Orientation"))
                {
                    x = _colour.Copy();
                    x.Set("orientation", 6);
                    buf = x.WebpsaveBuffer();
                    var y = Image.NewFromBuffer(buf);
                    Assert.Equal(6, y.Get("orientation"));
                }
            }
        }

        [SkippableFact]
        public void TestAnalyzeLoad()
        {
            Skip.IfNot(Helper.Have("analyzeload") && File.Exists(Helper.AnalyzeFile),
                "no analyze support, skipping test");

            void AnalyzeValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Assert.Equal(3335, a[0]);
                Assert.Equal(128, im.Width);
                Assert.Equal(8064, im.Height);
                Assert.Equal(1, im.Bands);
            }

            FileLoader("analyzeload", Helper.AnalyzeFile, AnalyzeValid);
        }

        [SkippableFact]
        public void TestMatLoad()
        {
            Skip.IfNot(Helper.Have("matload") && File.Exists(Helper.MatlabFile), "no matlab support, skipping test");

            void MatlabValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Assert.Equal(new[] {38671.0, 33914.0, 26762.0}, a);
                Assert.Equal(290, im.Width);
                Assert.Equal(442, im.Height);
                Assert.Equal(3, im.Bands);
            }

            FileLoader("matload", Helper.MatlabFile, MatlabValid);
        }

        [SkippableFact]
        public void TestOpenexrLoad()
        {
            Skip.IfNot(Helper.Have("openexrload") && File.Exists(Helper.ExrFile), "no openexr support, skipping test");

            void ExrValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Helper.AssertAlmostEqualObjects(new[]
                {
                    0.124512,
                    0.159668,
                    0.040375,
                    1.0
                }, a, 0.00001);
                Assert.Equal(610, im.Width);
                Assert.Equal(406, im.Height);
                Assert.Equal(4, im.Bands);
            }

            FileLoader("openexrload", Helper.ExrFile, ExrValid);
        }

        [SkippableFact]
        public void TestsFitsLoad()
        {
            Skip.IfNot(Helper.Have("fitsload") && File.Exists(Helper.FitsFile), "no fits support, skipping test");

            void FitsValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Helper.AssertAlmostEqualObjects(new[]
                {
                    -0.165013,
                    -0.148553,
                    1.09122,
                    -0.942242
                }, a, 0.00001);
                Assert.Equal(200, im.Width);
                Assert.Equal(200, im.Height);
                Assert.Equal(4, im.Bands);
            }

            FileLoader("fitsload", Helper.FitsFile, FitsValid);
            SaveLoad("%s.fits", _mono);
        }

        [SkippableFact]
        public void TestOpenslideLoad()
        {
            Skip.IfNot(Helper.Have("openslideload") && File.Exists(Helper.OpenslideFile),
                "no openslide support, skipping test");

            void OpenslideValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Assert.Equal(new double[] {244, 250, 243, 255}, a);
                Assert.Equal(2220, im.Width);
                Assert.Equal(2967, im.Height);
                Assert.Equal(4, im.Bands);
            }

            FileLoader("openslideload", Helper.OpenslideFile, OpenslideValid);
        }

        [SkippableFact]
        public void TestPdfLoad()
        {
            Skip.IfNot(Helper.Have("pdfload") && File.Exists(Helper.PdfFile), "no pdf support, skipping test");

            void PdfValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Assert.Equal(new double[] {35, 31, 32, 255}, a);
                Assert.Equal(1133, im.Width);
                Assert.Equal(680, im.Height);
                Assert.Equal(4, im.Bands);
            }

            FileLoader("pdfload", Helper.PdfFile, PdfValid);
            BufferLoader("pdfload_buffer", Helper.PdfFile, PdfValid);

            var x = Image.NewFromFile(Helper.PdfFile);
            var y = Image.NewFromFile(Helper.PdfFile, kwargs: new VOption
            {
                {"scale", 2}
            });
            Assert.True(Math.Abs(x.Width * 2 - y.Width) < 2);
            Assert.True(Math.Abs(x.Height * 2 - y.Height) < 2);

            x = Image.NewFromFile(Helper.PdfFile);
            y = Image.NewFromFile(Helper.PdfFile, kwargs: new VOption
            {
                {"dpi", 144}
            });
            Assert.True(Math.Abs(x.Width * 2 - y.Width) < 2);
            Assert.True(Math.Abs(x.Height * 2 - y.Height) < 2);
        }

        [SkippableFact]
        public void TestGifLoad()
        {
            Skip.IfNot(Helper.Have("gifload") && File.Exists(Helper.GifFile), "no gif support, skipping test");

            void GifValid(Image im)
            {
                var a = im.Getpoint(10, 10);

                Assert.Equal(new double[] {33}, a);
                Assert.Equal(159, im.Width);
                Assert.Equal(203, im.Height);
                Assert.Equal(1, im.Bands);
            }

            FileLoader("gifload", Helper.GifFile, GifValid);
            BufferLoader("gifload_buffer", Helper.GifFile, GifValid);

            // 'n' param added in 8.5
            if (Base.AtLeastLibvips(8, 5))
            {
                var x1 = Image.NewFromFile(Helper.GifAnimFile);
                var x2 = Image.NewFromFile(Helper.GifAnimFile, kwargs: new VOption
                {
                    {"n", 2}
                });
                Assert.Equal(2 * x1.Height, x2.Height);
                var pageHeight = x2.Get("page-height");
                Assert.Equal(x1.Height, pageHeight);

                x2 = Image.NewFromFile(Helper.GifAnimFile, kwargs: new VOption
                {
                    {"n", -1}
                });
                Assert.Equal(5 * x1.Height, x2.Height);

                x2 = Image.NewFromFile(Helper.GifAnimFile, kwargs: new VOption
                {
                    {"page", 1},
                    {"n", -1}
                });
                Assert.Equal(4 * x1.Height, x2.Height);
            }
        }

        [SkippableFact]
        public void TestSvgLoad()
        {
            Skip.IfNot(Helper.Have("svgload") && File.Exists(Helper.SvgFile), "no svg support, skipping test");

            void SvgValid(Image im)
            {
                var a = im.Getpoint(10, 10);


                // some old rsvg versions are way, way off
                Assert.True(Math.Abs(a[0] - 79) < 2);
                Assert.True(Math.Abs(a[1] - 79) < 2);
                Assert.True(Math.Abs(a[2] - 132) < 2);
                Assert.True(Math.Abs(a[3] - 255) < 2);

                Assert.Equal(288, im.Width);
                Assert.Equal(470, im.Height);
                Assert.Equal(4, im.Bands);
            }

            FileLoader("svgload", Helper.SvgFile, SvgValid);
            BufferLoader("svgload_buffer", Helper.SvgFile, SvgValid);

            FileLoader("svgload", Helper.SvgzFile, SvgValid);
            BufferLoader("svgload_buffer", Helper.SvgzFile, SvgValid);

            FileLoader("svgload", Helper.SvgGzFile, SvgValid);

            var x = Image.NewFromFile(Helper.SvgFile);
            var y = Image.NewFromFile(Helper.SvgFile, kwargs: new VOption
            {
                {"scale", 2}
            });

            Assert.True(Math.Abs(x.Width * 2 - y.Width) < 2);
            Assert.True(Math.Abs(x.Height * 2 - y.Height) < 2);

            x = Image.NewFromFile(Helper.SvgFile);
            y = Image.NewFromFile(Helper.SvgFile, kwargs: new VOption
            {
                {"dpi", 144}
            });
            Assert.True(Math.Abs(x.Width * 2 - y.Width) < 2);
            Assert.True(Math.Abs(x.Height * 2 - y.Height) < 2);
        }

        [Fact]
        public void TestCsv()
        {
            SaveLoad("%s.csv", _mono);
        }

        [Fact]
        public void TestMatrix()
        {
            SaveLoad("%s.mat", _mono);
        }

        [SkippableFact]
        public void TestPpm()
        {
            Skip.IfNot(Helper.Have("ppmload"), "no PPM support, skipping test");

            SaveLoad("%s.ppm", _mono);
            SaveLoad("%s.ppm", _colour);
        }

        [SkippableFact]
        public void TestRad()
        {
            Skip.IfNot(Helper.Have("radload"), "no Radiance support, skipping test");

            SaveLoad("%s.hdr", _colour);
            SaveBufferTempFile("radsave_buffer", ".hdr", _rad);
        }

        [SkippableFact]
        public void TestDzSave()
        {
            Skip.IfNot(Helper.Have("dzsave"), "no dzsave support, skipping test");

            // dzsave is hard to test, there are so many options
            // test each option separately and hope they all function together
            // correctly

            // default deepzoom layout ... we must use png here, since we want to
            // test the overlap for equality
            var filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, suffix: ".png");

            // test horizontal overlap ... expect 256 step, overlap 1
            var x = Image.NewFromFile(filename + "_files/10/0_0.png");
            Assert.Equal(255, x.Width);
            var y = Image.NewFromFile(filename + "_files/10/1_0.png");
            Assert.Equal(256, y.Width);

            // the right two columns of x should equal the left two columns of y
            var left = x.ExtractArea(x.Width - 2, 0, 2, x.Height);
            var right = y.ExtractArea(0, 0, 2, y.Height);
            Assert.Equal(0, (left - right).Abs().Max());

            // test vertical overlap
            Assert.Equal(255, x.Height);
            y = Image.NewFromFile(filename + "_files/10/0_1.png");
            Assert.Equal(256, y.Height);

            // the bottom two rows of x should equal the top two rows of y
            var top = x.ExtractArea(0, x.Height - 2, x.Width, 2);
            var bottom = y.ExtractArea(0, 0, y.Width, 2);
            Assert.Equal(0, (top - bottom).Abs().Max());

            // there should be a bottom layer
            x = Image.NewFromFile(filename + "_files/0/0_0.png");
            Assert.Equal(1, x.Width);
            Assert.Equal(1, x.Height);

            // 10 should be the final layer
            Assert.False(Directory.Exists(filename + "_files/11"));

            // default google layout
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, layout: "google");

            // test bottom-right tile ... default is 256x256 tiles, overlap 0
            x = Image.NewFromFile(filename + "/2/2/3.jpg");
            Assert.Equal(256, x.Width);
            Assert.Equal(256, x.Height);
            Assert.False(Directory.Exists(filename + "/2/2/4.jpg"));
            Assert.False(Directory.Exists(filename + "/3"));
            x = Image.NewFromFile(filename + "/blank.png");
            Assert.Equal(256, x.Width);
            Assert.Equal(256, x.Height);

            // google layout with overlap ... verify that we clip correctly

            // overlap 1, 510x510 pixels, 256 pixel tiles, should be exactly 2x2
            // tiles, though in fact the bottom and right edges will be white
            filename = Helper.GetTemporaryFile(_tempDir, "");

            _colour.ExtractArea(0, 0, 510, 510).Dzsave(filename, layout: "google", overlap: 1, depth: "one");

            x = Image.NewFromFile(filename + "/0/1/1.jpg");
            Assert.Equal(256, x.Width);
            Assert.Equal(256, x.Height);
            Assert.False(Directory.Exists(filename + "/0/2/2.jpg"));

            // with 511x511, it'll fit exactly into 2x2 -- we we actually generate
            // 3x3, since we output the overlaps
            // 8.6 revised the rules on overlaps, so don't test earlier than that
            if (Base.AtLeastLibvips(8, 6))
            {
                filename = Helper.GetTemporaryFile(_tempDir, "");
                _colour.ExtractArea(0, 0, 511, 511).Dzsave(filename, layout: "google", overlap: 1, depth: "one");

                x = Image.NewFromFile(filename + "/0/2/2.jpg");
                Assert.Equal(256, x.Width);
                Assert.Equal(256, x.Height);
                Assert.False(Directory.Exists(filename + "/0/3/3.jpg"));
            }

            // default zoomify layout
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, layout: "zoomify");

            // 256x256 tiles, no overlap
            Assert.True(File.Exists(filename + "/ImageProperties.xml"));
            x = Image.NewFromFile(filename + "/TileGroup0/2-3-2.jpg");
            Assert.Equal(256, x.Width);
            Assert.Equal(256, x.Height);

            // test zip output
            filename = Helper.GetTemporaryFile(_tempDir, ".zip");
            _colour.Dzsave(filename);

            Assert.True(File.Exists(filename));
            Assert.False(Directory.Exists(filename + "_files"));
            Assert.False(File.Exists(filename + ".dzi"));

            // test compressed zip output
            var filename2 = Helper.GetTemporaryFile(_tempDir, ".zip");
            _colour.Dzsave(filename2, compression: -1);

            Assert.True(File.Exists(filename2));
            Assert.True(new FileInfo(filename2).Length < new FileInfo(filename).Length);

            // test suffix
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, suffix: ".png");

            x = Image.NewFromFile(filename + "_files/10/0_0.png");
            Assert.Equal(255, x.Width);

            // test overlap
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, overlap: 200);

            x = Image.NewFromFile(filename + "_files/10/1_1.jpeg");
            Assert.Equal(654, x.Width);

            // test tile-size
            filename = Helper.GetTemporaryFile(_tempDir, "");
            _colour.Dzsave(filename, tileSize: 512);

            y = Image.NewFromFile(filename + "_files/10/0_0.jpeg");
            Assert.Equal(513, y.Width);
            Assert.Equal(513, y.Height);

            // test save to memory buffer
            if (Helper.Have("dzsave_buffer"))
            {
                filename = Helper.GetTemporaryFile(_tempDir, ".zip");
                var baseName = Path.GetFileNameWithoutExtension(filename);

                _colour.Dzsave(filename);

                var buf1 = File.ReadAllBytes(filename);
                var buf2 = _colour.DzsaveBuffer(basename: baseName);
                Assert.Equal(buf1.Length, buf2.Length);

                // we can't test the bytes are exactly equal -- the timestamps will
                // be different
            }
        }
    }
}