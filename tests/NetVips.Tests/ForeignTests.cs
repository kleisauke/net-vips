using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace NetVips.Tests;

public class ForeignTests : IClassFixture<TestsFixture>, IDisposable
{
    private readonly string _tempDir;

    private Image _colour;
    private Image _mono;
    private Image _rad;
    private Image _cmyk;
    private Image _oneBit;

    public ForeignTests(TestsFixture testsFixture, ITestOutputHelper output)
    {
        testsFixture.SetUpLogging(output);

        _tempDir = Helper.GetTemporaryDirectory();

        _colour = Image.Jpegload(Helper.JpegFile);
        _mono = _colour[0];

        // we remove the ICC profile: the RGB one will no longer be appropriate
        _mono = _mono.Mutate(x => x.Remove("icc-profile-data"));

        _rad = _colour.Float2rad();
        _rad = _rad.Mutate(x => x.Remove("icc-profile-data"));

        _cmyk = _colour.Bandjoin(_mono);
        _cmyk = _cmyk.Copy(interpretation: Enums.Interpretation.Cmyk);
        _cmyk = _cmyk.Mutate(x => x.Remove("icc-profile-data"));
        var im = Image.NewFromFile(Helper.GifFile);
        _oneBit = im[1] > 128;
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
        var im = (Image)Operation.Call(loader, testFile);
        validate(im);
        im = Image.NewFromFile(testFile);
        validate(im);
    }

    internal void BufferLoader(string loader, string testFile, Action<Image> validate)
    {
        var buf = File.ReadAllBytes(testFile);
        var im = (Image)Operation.Call(loader, buf);
        validate(im);
        im = Image.NewFromBuffer(buf);
        validate(im);
    }

    internal void SaveLoad(string format, Image im)
    {
        var x = Image.NewTempFile(format);
        im.Write(x);

        Assert.Equal(im.Width, x.Width);
        Assert.Equal(im.Height, x.Height);
        Assert.Equal(im.Bands, x.Bands);
        var maxDiff = (im - x).Abs().Max();
        Assert.Equal(0, maxDiff);
    }

    internal void SaveLoadFile(string format, string options, Image im, int maxDiff = 0)
    {
        // yuk!
        // but we can't set format parameters for Image.NewTempFile()
        var filename = Helper.GetTemporaryFile(_tempDir, format);

        im.WriteToFile(filename + options);
        var x = Image.NewFromFile(filename);

        Assert.Equal(im.Width, x.Width);
        Assert.Equal(im.Height, x.Height);
        Assert.Equal(im.Bands, x.Bands);
        Assert.True((im - x).Abs().Max() <= maxDiff);
    }

    internal void SaveLoadBuffer(string saver, string loader, Image im, int maxDiff = 0, VOption kwargs = null)
    {
        var buf = (byte[])Operation.Call(saver, kwargs, im);
        var x = (Image)Operation.Call(loader, buf);

        Assert.Equal(im.Width, x.Width);
        Assert.Equal(im.Height, x.Height);
        Assert.Equal(im.Bands, x.Bands);
        Assert.True((im - x).Abs().Max() <= maxDiff);
    }

    internal void SaveLoadStream(string format, string options, Image im, int maxDiff = 0)
    {
        using var stream = new MemoryStream();
        im.WriteToStream(stream, format + options);

        // Reset to start position
        stream.Seek(0, SeekOrigin.Begin);

        var x = Image.NewFromStream(stream);

        Assert.Equal(im.Width, x.Width);
        Assert.Equal(im.Height, x.Height);
        Assert.Equal(im.Bands, x.Bands);
        Assert.True((im - x).Abs().Max() <= maxDiff);
    }

    internal void SaveBufferTempFile(string saver, string suf, Image im, int maxDiff = 0)
    {
        var filename = Helper.GetTemporaryFile(_tempDir, suf);

        var buf = (byte[])Operation.Call(saver, matchImage: im);
        File.WriteAllBytes(filename, buf);

        var x = Image.NewFromFile(filename);

        Assert.Equal(im.Width, x.Width);
        Assert.Equal(im.Height, x.Height);
        Assert.Equal(im.Bands, x.Bands);
        Assert.True((im - x).Abs().Max() <= maxDiff);
    }

    #endregion

    [Fact]
    public void TestVips()
    {
        SaveLoadFile(".v", "", _colour);

        // check we can save and restore metadata
        var filename = Helper.GetTemporaryFile(_tempDir, ".v");
        _colour.WriteToFile(filename);
        var x = Image.NewFromFile(filename);
        var beforeExif = (byte[])_colour.Get("exif-data");
        var afterExif = (byte[])x.Get("exif-data");

        Assert.Equal(beforeExif.Length, afterExif.Length);
        Assert.Equal(beforeExif, afterExif);
    }

    [SkippableFact]
    public void TestJpeg()
    {
        Skip.IfNot(Helper.Have("jpegload"), "no jpeg support, skipping test");

        void JpegValid(Image im)
        {
            var a = im[10, 10];
            Assert.Equal([6, 5, 3], a);
            var profile = (byte[])im.Get("icc-profile-data");

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
        SaveLoadStream(".jpg", "", _colour, 80);

        _ = Image.Jpegload(Helper.JpegFile, out var flags);
        Assert.Equal(Enums.ForeignFlags.SEQUENTIAL, flags);

        // see if we have exif parsing: our test image has this field
        var x = Image.NewFromFile(Helper.JpegFile);
        if (x.Contains("exif-ifd0-Orientation"))
        {
            // we need a copy of the image to set the new metadata on
            // otherwise we get caching problems

            // can set, save and load new orientation
            x = Image.NewFromFile(Helper.JpegFile);
            x = x.Mutate(im => im.Set("orientation", 2));

            var filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
            x.WriteToFile(filename);

            x = Image.NewFromFile(filename);
            var y = x.Get("orientation");
            Assert.Equal(2, y);

            // can remove orientation, save, load again, orientation
            // has reset
            x = x.Mutate(im => im.Remove("orientation"));

            filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
            x.WriteToFile(filename);

            x = Image.NewFromFile(filename);
            y = x.Get("orientation");
            Assert.Equal(1, y);

            // autorotate load works
            x = Image.NewFromFile(Helper.JpegFile);
            x = x.Mutate(im => im.Set("orientation", 6));

            filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
            x.WriteToFile(filename);
            var x1 = Image.NewFromFile(filename);
            var x2 = Image.NewFromFile(filename, kwargs: new VOption
            {
                {"autorotate", true}
            });
            Assert.Equal(x1.Width, x2.Height);
            Assert.Equal(x1.Height, x2.Width);

            // can set, save and reload ASCII string fields
            x = Image.NewFromFile(Helper.JpegFile);
            x = x.Mutate(im => im.Set(GValue.GStrType, "exif-ifd0-ImageDescription", "hello world"));

            filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
            x.WriteToFile(filename);

            x = Image.NewFromFile(filename);
            y = x.Get("exif-ifd0-ImageDescription");

            // can't use Assert.Equal since the string will have an extra " (xx, yy, zz)"
            // format area at the end
            Assert.StartsWith("hello world", (string)y);

            // can set, save and reload UTF16 string fields ... NetVips is
            // utf8, but it will be coded as utf16 and back for the XP* fields
            x = Image.NewFromFile(Helper.JpegFile);
            x = x.Mutate(im => im.Set(GValue.GStrType, "exif-ifd0-XPComment", "йцук"));

            filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
            x.WriteToFile(filename);

            x = Image.NewFromFile(filename);
            y = x.Get("exif-ifd0-XPComment");

            // can't use Assert.Equal since the string will have an extra " (xx, yy, zz)"
            // format area at the end
            Assert.StartsWith("йцук", (string)y);

            // can set/save/load UserComment, a tag which has the
            // encoding in the first 8 bytes ... though libexif only supports
            // ASCII for this
            x = Image.NewFromFile(Helper.JpegFile);
            x = x.Mutate(im => im.Set(GValue.GStrType, "exif-ifd2-UserComment", "hello world"));

            filename = Helper.GetTemporaryFile(_tempDir, ".jpg");
            x.WriteToFile(filename);

            x = Image.NewFromFile(filename);
            y = x.Get("exif-ifd2-UserComment");

            // can't use Assert.Equal since the string will have an extra " (xx, yy, zz)"
            // format area at the end
            Assert.StartsWith("hello world", (string)y);
        }
    }

    [SkippableFact]
    public void TestJpegSave()
    {
        Skip.IfNot(Helper.Have("jpegsave"), "no jpeg support, skipping test");

        var im = Image.NewFromFile(Helper.JpegFile);

        var q10 = im.JpegsaveBuffer(q: 10);
        var q10SubsampleAuto = im.JpegsaveBuffer(q: 10, subsampleMode: Enums.ForeignSubsample.Auto);
        var q10SubsampleOn = im.JpegsaveBuffer(q: 10, subsampleMode: Enums.ForeignSubsample.On);
        var q10SubsampleOff = im.JpegsaveBuffer(q: 10, subsampleMode: Enums.ForeignSubsample.Off);

        var q90 = im.JpegsaveBuffer(q: 90);
        var q90SubsampleAuto = im.JpegsaveBuffer(q: 90, subsampleMode: Enums.ForeignSubsample.Auto);
        var q90SubsampleOn = im.JpegsaveBuffer(q: 90, subsampleMode: Enums.ForeignSubsample.On);
        var q90SubsampleOff = im.JpegsaveBuffer(q: 90, subsampleMode: Enums.ForeignSubsample.Off);

        // higher Q should mean a bigger buffer
        Assert.True(q90.Length > q10.Length);

        Assert.Equal(q10.Length, q10SubsampleAuto.Length);
        Assert.Equal(q10SubsampleAuto.Length, q10SubsampleOn.Length);
        Assert.True(q10SubsampleOff.Length > q10.Length);

        Assert.Equal(q90SubsampleAuto.Length, q90.Length);
        Assert.True(q90SubsampleOn.Length < q90.Length);
        Assert.Equal(q90SubsampleAuto.Length, q90SubsampleOff.Length);

        // A non-zero restart_interval should result in a bigger file.
        // Otherwise, smaller restart intervals will have more restart markers
        // and therefore be larger
        var r0 = im.JpegsaveBuffer(restartInterval: 0);
        var r10 = im.JpegsaveBuffer(restartInterval: 10);
        var r2 = im.JpegsaveBuffer(restartInterval: 2);
        Assert.True(r10.Length > r0.Length);
        Assert.True(r2.Length > r10.Length);

        // we should be able to reload jpegs with extra MCU markers
        var im0 = Image.JpegloadBuffer(r0);
        var im10 = Image.JpegloadBuffer(r10);
        Assert.Equal(im10.Avg(), im0.Avg());
    }

    [SkippableFact]
    public void TestTruncated()
    {
        Skip.IfNot(Helper.Have("jpegload"), "no jpeg support, skipping test");

        // This should open (there's enough there for the header)
        var im = Image.NewFromFile(Helper.TruncatedFile);

        // but this should fail with a warning, and knock TRUNCATED_FILE out of
        // the cache
        _ = im.Avg();

        // now we should open again, but it won't come from cache, it'll reload
        im = Image.NewFromFile(Helper.TruncatedFile);

        // and this should fail with a warning once more
        _ = im.Avg();
    }

    [SkippableFact]
    public void TestPng()
    {
        Skip.IfNot(Helper.Have("pngload"), "no png support, skipping test");

        void PngValid(Image im)
        {
            var a = im[10, 10];

            Assert.Equal(new[] { 38671.0, 33914.0, 26762.0 }, a);
            Assert.Equal(290, im.Width);
            Assert.Equal(442, im.Height);
            Assert.Equal(3, im.Bands);
            Assert.Equal(16, im.Get("bits-per-sample"));
            Assert.False(im.Contains("palette"));
        }

        FileLoader("pngload", Helper.PngFile, PngValid);
        BufferLoader("pngload_buffer", Helper.PngFile, PngValid);
        SaveLoadBuffer("pngsave_buffer", "pngload_buffer", _colour);
        SaveLoad("%s.png", _mono);
        SaveLoad("%s.png", _colour);
        SaveLoadFile(".png", "[interlace]", _colour);
        SaveLoadFile(".png", "[interlace]", _mono);
        SaveLoadStream(".png", "", _colour);

        // size of a regular mono PNG
        var lenMono = _mono.PngsaveBuffer().Length;

        // 4-bit should be smaller
        var lenMono4 = _mono.PngsaveBuffer(bitdepth: 4).Length;
        Assert.True(lenMono4 < lenMono);

        var lenMono2 = _mono.PngsaveBuffer(bitdepth: 2).Length;
        Assert.True(lenMono2 < lenMono4);

        var lenMono1 = _mono.PngsaveBuffer(bitdepth: 1).Length;
        Assert.True(lenMono1 < lenMono2);

        // take a 1-bit image to png and back
        var onebit = _mono > 128;
        var data = onebit.PngsaveBuffer(bitdepth: 1);
        var after = Image.NewFromBuffer(data);
        Assert.Equal(0, (onebit - after).Abs().Max());
        Assert.Equal(1, after.Get("bits-per-sample"));

        // we can't test palette save since we can't be sure libimagequant is
        // available and there's no easy test for its presence

        // Add EXIF to new PNG
        var im1 = Image.Black(8, 8);
        im1 = im1.Mutate(im => im.Set(GValue.GStrType,
            "exif-ifd0-ImageDescription", "test description"));
        var im2 = Image.NewFromBuffer(im1.WriteToBuffer(".png"));
        Assert.StartsWith("test description", (string)im2.Get("exif-ifd0-ImageDescription"));

        // we should be able to save an 8-bit image as a 16-bit PNG
        data = _colour.PngsaveBuffer(bitdepth: 16);
        var rgb16 = Image.PngloadBuffer(data);
        Assert.Equal(Enums.BandFormat.Ushort, rgb16.Format);

        // we should be able to save a 16-bit image as an 8-bit PNG
        data = rgb16.PngsaveBuffer(bitdepth: 8);
        var rgb = Image.PngloadBuffer(data);
        Assert.Equal(Enums.BandFormat.Uchar, rgb.Format);
    }

    [SkippableFact]
    public void TestBufferOverload()
    {
        Skip.IfNot(Helper.Have("pngload"), "no png support, skipping test");

        var buf = _colour.WriteToBuffer(".png");
        var x = Image.NewFromBuffer(buf);

        Assert.Equal(_colour.Width, x.Width);
        Assert.Equal(_colour.Height, x.Height);
        Assert.Equal(_colour.Bands, x.Bands);
        Assert.Equal(0, (_colour - x).Abs().Max());
    }

    [SkippableFact]
    public void TestStreamOverload()
    {
        Skip.IfNot(Helper.Have("jpegload"), "no jpeg support, skipping test");

        // Set the beginning of the stream to an arbitrary but carefully chosen number.
        using var stream = new MemoryStream { Position = 42 };
        _colour.WriteToStream(stream, ".jpg");

        // Set the current position of the stream to the chosen number.
        stream.Position = 42;

        // We should be able to read from this stream, even if it starts at any position.
        var x = Image.NewFromStream(stream, access: Enums.Access.Sequential);

        Assert.Equal(_colour.Width, x.Width);
        Assert.Equal(_colour.Height, x.Height);
        Assert.Equal(_colour.Bands, x.Bands);
        Assert.True((_colour - x).Abs().Max() <= 80);
    }

    [SkippableFact]
    public void TestTiff()
    {
        Skip.IfNot(Helper.Have("tiffload"), "no tiff support, skipping test");

        void TiffValid(Image im)
        {
            var a = im[10, 10];

            Assert.Equal(new[] { 38671.0, 33914.0, 26762.0 }, a);
            Assert.Equal(290, im.Width);
            Assert.Equal(442, im.Height);
            Assert.Equal(3, im.Bands);
            Assert.Equal(16, im.Get("bits-per-sample"));
        }

        void Tiff1Valid(Image im)
        {
            var a = im[127, 0];
            Assert.Equal(new[] { 0.0 }, a);
            a = im[128, 0];
            Assert.Equal(new[] { 255.0 }, a);
            Assert.Equal(256, im.Width);
            Assert.Equal(4, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(1, im.Get("bits-per-sample"));
        }

        FileLoader("tiffload", Helper.Tif1File, Tiff1Valid);

        void Tiff2Valid(Image im)
        {
            var a = im[127, 0];
            Assert.Equal(new[] { 85.0 }, a);
            a = im[128, 0];
            Assert.Equal(new[] { 170.0 }, a);
            Assert.Equal(256, im.Width);
            Assert.Equal(4, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(2, im.Get("bits-per-sample"));
        }

        FileLoader("tiffload", Helper.Tif2File, Tiff2Valid);

        void Tiff4Valid(Image im)
        {
            var a = im[127, 0];
            Assert.Equal(new[] { 119.0 }, a);
            a = im[128, 0];
            Assert.Equal(new[] { 136.0 }, a);
            Assert.Equal(256, im.Width);
            Assert.Equal(4, im.Height);
            Assert.Equal(1, im.Bands);
            Assert.Equal(4, im.Get("bits-per-sample"));
        }

        FileLoader("tiffload", Helper.Tif4File, Tiff4Valid);

        FileLoader("tiffload", Helper.TifFile, TiffValid);
        BufferLoader("tiffload_buffer", Helper.TifFile, TiffValid);
        SaveLoadBuffer("tiffsave_buffer", "tiffload_buffer", _colour);

        SaveLoad("%s.tif", _mono);
        SaveLoad("%s.tif", _colour);
        SaveLoad("%s.tif", _cmyk);

        SaveLoad("%s.tif", _oneBit);
        SaveLoadFile(".tif", "[bitdepth=1]",
            _oneBit);
        SaveLoadFile(".tif", "[miniswhite]", _oneBit);
        SaveLoadFile(".tif", "[bitdepth=1,miniswhite]",
            _oneBit);

        SaveLoadFile(".tif", $"[profile={Helper.SrgbFile}]", _colour);
        SaveLoadFile(".tif", "[tile]", _colour);
        SaveLoadFile(".tif", "[tile,pyramid]", _colour);
        SaveLoadFile(".tif", "[tile,pyramid,subifd]", _colour);
        SaveLoadFile(".tif", "[tile,pyramid,compression=jpeg]", _colour, 80);
        SaveLoadFile(".tif", "[tile,pyramid,subifd,compression=jpeg]", _colour, 80);
        SaveLoadFile(".tif", "[bigtiff]", _colour);
        SaveLoadFile(".tif", "[compression=jpeg]", _colour, 80);
        SaveLoadFile(".tif", "[tile,tile-width=256]", _colour, 10);

        var x = Image.NewFromFile(Helper.Tif2File);
        SaveLoadFile(".tif", "[bitdepth=2]", x);
        x = Image.NewFromFile(Helper.Tif4File);
        SaveLoadFile(".tif", "[bitdepth=4]", x);

        SaveLoadStream(".tif", "", _colour);

        // Test Read/Seek in TargetCustom
        using var input = File.OpenRead(Helper.GifAnimFile);
        using var im = Image.NewFromStream(input, kwargs: new VOption
        {
            {"n", -1}
        });

        var tmpFile = Helper.GetTemporaryFile(_tempDir, ".tif");
        using var output = File.Open(tmpFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        im.TiffsaveStream(output);

        using var im2 = Image.NewFromFile(tmpFile, kwargs: new VOption
        {
            {"n", -1}
        });
        Assert.Equal(im.Width, im2.Width);
        Assert.Equal(im.Height, im2.Height);
        Assert.Equal(im.Bands, im2.Bands);
        var maxDiff = (im - im2).Abs().Max();
        Assert.Equal(0, maxDiff);

        var filename = Helper.GetTemporaryFile(_tempDir, ".tif");
        x = Image.NewFromFile(Helper.TifFile);
        x = x.Mutate(m => m.Set("orientation", 2));
        x.WriteToFile(filename);
        x = Image.NewFromFile(filename);
        var y = x.Get("orientation");
        Assert.Equal(2, y);

        filename = Helper.GetTemporaryFile(_tempDir, ".tif");
        x = Image.NewFromFile(Helper.TifFile);
        x = x.Mutate(m => m.Set("orientation", 2));
        x.WriteToFile(filename);
        x = Image.NewFromFile(filename);
        y = x.Get("orientation");
        Assert.Equal(2, y);
        x = x.Mutate(m => m.Remove("orientation"));

        filename = Helper.GetTemporaryFile(_tempDir, ".tif");
        x.WriteToFile(filename);
        x = Image.NewFromFile(filename);
        y = x.Get("orientation");
        Assert.Equal(1, y);

        filename = Helper.GetTemporaryFile(_tempDir, ".tif");
        x = Image.NewFromFile(Helper.TifFile);
        x = x.Mutate(m => m.Set("orientation", 6));
        x.WriteToFile(filename);
        var x1 = Image.NewFromFile(filename);
        var x2 = Image.NewFromFile(filename, kwargs: new VOption
        {
            {"autorotate", true}
        });
        Assert.Equal(x1.Width, x2.Height);
        Assert.Equal(x1.Height, x2.Width);

        // OME support in 8.5
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
        Assert.Equal(96, x[0, 166][0]);
        Assert.Equal(0, x[0, 167][0]);
        Assert.Equal(1, x[0, 168][0]);

        filename = Helper.GetTemporaryFile(_tempDir, ".tif");
        x.WriteToFile(filename);

        x = Image.NewFromFile(filename, kwargs: new VOption
        {
            {"n", -1}
        });
        Assert.Equal(439, x.Width);
        Assert.Equal(pageHeight * 15, x.Height);
        Assert.Equal(96, x[0, 166][0]);
        Assert.Equal(0, x[0, 167][0]);
        Assert.Equal(1, x[0, 168][0]);

        // pyr save to buffer added in 8.6
        x = Image.NewFromFile(Helper.TifFile);
        var buf = x.TiffsaveBuffer(tile: true, pyramid: true);
        filename = Helper.GetTemporaryFile(_tempDir, ".tif");
        x.Tiffsave(filename, tile: true, pyramid: true);
        var buf2 = File.ReadAllBytes(filename);
        Assert.Equal(buf.Length, buf2.Length);

        var a = Image.NewFromBuffer(buf, kwargs: new VOption
        {
            {"page", 2}
        });
        var b = Image.NewFromBuffer(buf2, kwargs: new VOption
        {
            {"page", 2}
        });
        Assert.Equal(a.Width, b.Width);
        Assert.Equal(a.Height, b.Height);
        Assert.Equal(a.Avg(), b.Avg());

        // region-shrink added in 8.7
        x = Image.NewFromFile(Helper.TifFile);
        _ = x.TiffsaveBuffer(tile: true, pyramid: true, regionShrink: Enums.RegionShrink.Mean);
        _ = x.TiffsaveBuffer(tile: true, pyramid: true, regionShrink: Enums.RegionShrink.Mode);
        _ = x.TiffsaveBuffer(tile: true, pyramid: true, regionShrink: Enums.RegionShrink.Median);

        // region-shrink max/min/nearest added in 8.10
        _ = x.TiffsaveBuffer(tile: true, pyramid: true, regionShrink: Enums.RegionShrink.Max);
        _ = x.TiffsaveBuffer(tile: true, pyramid: true, regionShrink: Enums.RegionShrink.Min);
        _ = x.TiffsaveBuffer(tile: true, pyramid: true, regionShrink: Enums.RegionShrink.Nearest);
    }

    [SkippableFact]
    public void TestTiffJp2K()
    {
        Skip.IfNot(Helper.Have("tiffload") && Helper.Have("jp2kload"), "no tiff or jp2k support, skipping test");

        SaveLoadFile(".tif", "[tile,compression=jp2k]", _colour, 80);
        SaveLoadFile(".tif",
            "[tile,pyramid,compression=jp2k]", _colour, 80);
        SaveLoadFile(".tif",
            "[tile,pyramid,subifd,compression=jp2k]",
            _colour, 80);
    }

    [SkippableFact]
    public void TestMagickLoad()
    {
        Skip.IfNot(Helper.Have("magickload"), "no magick support, skipping test");

        void BmpValid(Image im)
        {
            var a = im[100, 100];

            Helper.AssertAlmostEqualObjects(new double[] { 227, 216, 201 }, a);
            Assert.Equal(1419, im.Width);
            Assert.Equal(1001, im.Height);
            Assert.Equal(8, im.Get("bits-per-sample"));
        }

        FileLoader("magickload", Helper.BmpFile, BmpValid);
        BufferLoader("magickload_buffer", Helper.BmpFile, BmpValid);

        // we should have rgb or rgba for svg files ... different versions of
        // IM handle this differently. GM even gives 1 band.
        var x = Image.Magickload(Helper.SvgFile);
        Assert.True(x.Bands is 3 or 4 or 1);

        // density should change size of generated svg
        x = Image.Magickload(Helper.SvgFile, density: "100");
        var width = x.Width;
        var height = x.Height;

        // This seems to fail on travis, no idea why, some problem in their IM
        // perhaps
        //x = Image.Magickload(Helper.SvgFile, density: "200");
        //Assert.Equal(width * 2, x.Width);
        //Assert.Equal(height * 2, x.Height);

        // page/n let you pick a range of pages
        x = Image.Magickload(Helper.GifAnimFile);
        width = x.Width;
        height = x.Height;
        x = Image.Magickload(Helper.GifAnimFile, page: 1, n: 2);
        Assert.Equal(width, x.Width);
        Assert.Equal(height * 2, x.Height);

        var pageHeight = x.Get("page-height");
        Assert.Equal(height, pageHeight);

        // should work for dicom
        x = Image.Magickload(Helper.DicomFile);
        Assert.Equal(128, x.Width);
        Assert.Equal(128, x.Height);

        // some IMs are 3 bands, some are 1, can't really test
        // Assert.Equal(1, x.Bands);

        // libvips has its own sniffer for ICO, test that
        var buf = File.ReadAllBytes(Helper.IcoFile);
        var im = Image.NewFromBuffer(buf);
        Assert.Equal(16, im.Width);
        Assert.Equal(16, im.Height);
    }

    [SkippableFact]
    public void TestMagickSave()
    {
        Skip.IfNot(Helper.Have("magicksave"), "no magick support, skipping test");

        // save to a file and load again ... we can't use SaveLoadFile since
        // we want to make sure we use magickload/save
        // don't use BMP - GraphicsMagick always adds an alpha
        // don't use TIF - IM7 will save as 16-bit
        var filename = Helper.GetTemporaryFile(_tempDir, ".jpg");

        _colour.Magicksave(filename);
        var x = Image.Magickload(filename);

        Assert.Equal(_colour.Width, x.Width);
        Assert.Equal(_colour.Height, x.Height);
        Assert.Equal(_colour.Bands, x.Bands);
        Assert.Equal(_colour.Height, x.Height);

        var maxDiff = (_colour - x).Abs().Max();
        Assert.True(maxDiff <= 60);

        SaveLoadBuffer("magicksave_buffer", "magickload_buffer", _colour, 60, new VOption
        {
            {"format", "JPG"}
        });

        // try an animation
        if (Helper.Have("gifload"))
        {
            var x1 = Image.NewFromFile(Helper.GifAnimFile, kwargs: new VOption
            {
                {"n", -1}
            });
            var w1 = x1.MagicksaveBuffer(format: "GIF");
            var x2 = Image.NewFromBuffer(w1, kwargs: new VOption
            {
                {"n", -1}
            });

            Assert.Equal(x2.Get("delay"), x1.Get("delay"));
            Assert.Equal(x2.Get("page-height"), x1.Get("page-height"));
            // magicks vary in how they handle this ... just pray we are close
            Assert.Equal((int)x1.Get("gif-loop"), (int)x2.Get("gif-loop"), 5.0);
        }
    }

    [SkippableFact]
    public void TestWebp()
    {
        Skip.IfNot(Helper.Have("webpload"), "no webp support, skipping test");

        void WebpValid(Image im)
        {
            var a = im[10, 10];

            // different webp versions use different rounding systems leading
            // to small variations
            Helper.AssertAlmostEqualObjects(new double[] { 71, 166, 236 }, a, 2);
            Assert.Equal(550, im.Width);
            Assert.Equal(368, im.Height);
            Assert.Equal(3, im.Bands);
        }

        FileLoader("webpload", Helper.WebpFile, WebpValid);
        BufferLoader("webpload_buffer", Helper.WebpFile, WebpValid);
        SaveLoadBuffer("webpsave_buffer", "webpload_buffer", _colour, 60);
        SaveLoad("%s.webp", _colour);
        SaveLoadStream(".webp", "", _colour, 80);

        // test lossless mode
        var x = Image.NewFromFile(Helper.WebpFile);
        var buf = x.WebpsaveBuffer(lossless: true);
        var im2 = Image.NewFromBuffer(buf);
        Assert.Equal(0, (x - im2).Abs().Max());

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

            // add tests for exif, xmp, iptc
            // the exif test will need us to be able to walk the header,
            // we can't just check exif-data

            // we can test that exif changes change the output of webpsave
            // first make sure we have exif support
            var z = Image.NewFromFile(Helper.JpegFile);
            if (z.Contains("exif-ifd0-Orientation"))
            {
                x = _colour.Mutate(im => im.Set("orientation", 6));
                buf = x.WebpsaveBuffer();
                var y = Image.NewFromBuffer(buf);
                Assert.Equal(6, y.Get("orientation"));
            }
        }

        // try converting an animated gif to webp
        if (Helper.Have("gifload"))
        {
            var x1 = Image.NewFromFile(Helper.GifAnimFile, kwargs: new VOption
            {
                {"n", -1}
            });
            var w1 = x1.WebpsaveBuffer(q: 10);

            var expectedDelay = (int[])x1.Get("delay");

            // our test gif has delay 0 for the first frame set in error,
            // when converting to WebP this should result in a 100ms delay.
            for (var i = 0; i < expectedDelay.Length; i++)
            {
                expectedDelay[i] = expectedDelay[i] <= 10 ? 100 : expectedDelay[i];
            }

            var x2 = Image.NewFromBuffer(w1, kwargs: new VOption
            {
                {"n", -1}
            });
            Assert.Equal(x1.Width, x2.Width);
            Assert.Equal(x1.Height, x2.Height);

            Assert.Equal(expectedDelay, (int[])x2.Get("delay"));
            Assert.Equal(x1.Get("page-height"), x2.Get("page-height"));
            Assert.Equal(x1.Get("gif-loop"), x2.Get("gif-loop"));
        }

        //  target_size should reasonably work, +/- 2% is fine
        x = Image.NewFromFile(Helper.WebpFile);
        buf = x.WebpsaveBuffer(targetSize: 20_000, keep: Enums.ForeignKeep.None);
        Assert.InRange(buf.Length, 19600, 20400);
    }

    [SkippableFact]
    public void TestAnalyzeLoad()
    {
        Skip.IfNot(Helper.Have("analyzeload"), "no analyze support, skipping test");

        void AnalyzeValid(Image im)
        {
            var a = im[10, 10];

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
        Skip.IfNot(Helper.Have("matload"), "no matlab support, skipping test");

        void MatlabValid(Image im)
        {
            var a = im[10, 10];

            Assert.Equal(new[] { 38671.0, 33914.0, 26762.0 }, a);
            Assert.Equal(290, im.Width);
            Assert.Equal(442, im.Height);
            Assert.Equal(3, im.Bands);
        }

        FileLoader("matload", Helper.MatlabFile, MatlabValid);
    }

    [SkippableFact]
    public void TestOpenexrLoad()
    {
        Skip.IfNot(Helper.Have("openexrload"), "no openexr support, skipping test");

        void ExrValid(Image im)
        {
            var a = im[10, 10];

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
    public void TestFitsLoad()
    {
        Skip.IfNot(Helper.Have("fitsload"), "no fits support, skipping test");

        void FitsValid(Image im)
        {
            var a = im[10, 10];

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
    public void TestNiftiLoad()
    {
        Skip.IfNot(Helper.Have("niftiload"), "no nifti support, skipping test");

        void NiftiValid(Image im)
        {
            var a = im[30, 26];

            Helper.AssertAlmostEqualObjects(new[]
            {
                131
            }, a);
            Assert.Equal(91, im.Width);
            Assert.Equal(9919, im.Height);
            Assert.Equal(1, im.Bands);
        }

        FileLoader("niftiload", Helper.NiftiFile, NiftiValid);
        SaveLoad("%s.nii.gz", _mono);
    }

    [SkippableFact]
    public void TestOpenslideLoad()
    {
        Skip.IfNot(Helper.Have("openslideload"), "no openslide support, skipping test");

        void OpenslideValid(Image im)
        {
            var a = im[10, 10];

            Assert.Equal([244, 250, 243, 255], a);
            Assert.Equal(2220, im.Width);
            Assert.Equal(2967, im.Height);
            Assert.Equal(4, im.Bands);
        }

        FileLoader("openslideload", Helper.OpenslideFile, OpenslideValid);
    }

    [SkippableFact]
    public void TestPdfLoad()
    {
        Skip.IfNot(Helper.Have("pdfload"), "no pdf support, skipping test");

        void PdfValid(Image im)
        {
            var a = im[10, 10];

            Assert.Equal([35, 31, 32, 255], a);
            Assert.Equal(1134, im.Width);
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
        Assert.Equal(x.Width * 2, y.Width, 2.0);
        Assert.Equal(x.Height * 2, y.Height, 2.0);

        x = Image.NewFromFile(Helper.PdfFile);
        y = Image.NewFromFile(Helper.PdfFile, kwargs: new VOption
        {
            {"dpi", 144}
        });
        Assert.Equal(x.Width * 2, y.Width, 2.0);
        Assert.Equal(x.Height * 2, y.Height, 2.0);
    }

    [SkippableFact]
    public void TestGifLoad()
    {
        Skip.IfNot(Helper.Have("gifload"), "no gif support, skipping test");

        void GifValid(Image im)
        {
            var a = im[10, 10];

            Assert.Equal([33, 33, 33], a);
            Assert.Equal(159, im.Width);
            Assert.Equal(203, im.Height);
            Assert.Equal(3, im.Bands);
        }

        FileLoader("gifload", Helper.GifFile, GifValid);
        BufferLoader("gifload_buffer", Helper.GifFile, GifValid);

        // file-based loader fallback
        using (var input = Source.NewFromFile(Helper.GifFile))
        {
            var img = Image.NewFromSource(input, access: Enums.Access.Sequential);
            GifValid(img);
        }

        // buffer-based loader fallback
        using (var input = File.OpenRead(Helper.GifFile))
        {
            var img = Image.NewFromStream(input, access: Enums.Access.Sequential);
            GifValid(img);
        }

        // test page handling
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

        // our test gif has delay 0 for the first frame set in error
        Assert.Equal(new[] { 0, 50, 50, 50, 50 }, x2.Get("delay"));

        x2 = Image.NewFromFile(Helper.GifAnimFile, kwargs: new VOption
        {
            {"page", 1},
            {"n", -1}
        });
        Assert.Equal(4 * x1.Height, x2.Height);
    }

    [SkippableFact]
    public void TestSvgLoad()
    {
        Skip.IfNot(Helper.Have("svgload"), "no svg support, skipping test");

        void SvgValid(Image im)
        {
            var a = im[10, 10];

            Helper.AssertAlmostEqualObjects(new[]
            {
                0, 0, 0, 0
            }, a);
            Assert.Equal(736, im.Width);
            Assert.Equal(552, im.Height);
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

        Assert.Equal(x.Width * 2, y.Width, 2.0);
        Assert.Equal(x.Height * 2, y.Height, 2.0);

        x = Image.NewFromFile(Helper.SvgFile);
        y = Image.NewFromFile(Helper.SvgFile, kwargs: new VOption
        {
            {"dpi", 144}
        });
        Assert.Equal(x.Width * 2, y.Width, 2.0);
        Assert.Equal(x.Height * 2, y.Height, 2.0);

        Assert.Throws<VipsException>(() => Image.NewFromBuffer(
            """<svg xmlns="http://www.w3.org/2000/svg" width="0" height="0"></svg>"""));

        // recognize dimensions for SVGs without width/height
        var svg = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"></svg>""";
        var im = Image.NewFromBuffer(svg);
        Assert.Equal(100, im.Width);
        Assert.Equal(100, im.Height);

        svg = """<svg xmlns="http://www.w3.org/2000/svg"><rect width="100" height="100" /></svg>""";
        im = Image.NewFromBuffer(svg);
        Assert.Equal(100, im.Width);
        Assert.Equal(100, im.Height);

        // width and height of 0.5 is valid
        svg = """<svg xmlns="http://www.w3.org/2000/svg" width="0.5" height="0.5"></svg>""";
        im = Image.NewFromBuffer(svg);
        Assert.Equal(1, im.Width);
        Assert.Equal(1, im.Height);

        // scale up
        var svgSpan = """<svg xmlns="http://www.w3.org/2000/svg" width="1" height="1"></svg>"""u8;
        im = Image.NewFromBuffer(svgSpan, kwargs: new VOption
        {
            {"scale", 10000}
        });
        Assert.Equal(10000, im.Width);
        Assert.Equal(10000, im.Height);

        svgSpan = """<svg xmlns="http://www.w3.org/2000/svg" width="100000" height="100000"></svg>"""u8;
        im = Image.NewFromBuffer(svgSpan, kwargs: new VOption
        {
            {"scale", 0.0001}
        });
        Assert.Equal(10, im.Width);
        Assert.Equal(10, im.Height);

        // Custom CSS stylesheet
        im = Image.NewFromFile(Helper.SvgFile);
        Assert.True(im.Avg() < 5);

        im = Image.Svgload(Helper.SvgFile, stylesheet: "path{stroke:#f00;stroke-width:1em;}");
        Assert.True(im.Avg() > 5);
    }

    [Fact]
    public void TestCsv()
    {
        SaveLoad("%s.csv", _mono);

        var x = Target.NewToMemory();
        _mono.CsvsaveTarget(x);

        var y = Source.NewFromMemory(x.Blob);
        var im = Image.CsvloadSource(y);

        Assert.Equal(0, (im - _mono).Abs().Max());
    }

    [Fact]
    public void TestMatrix()
    {
        SaveLoad("%s.mat", _mono);

        var x = Target.NewToMemory();
        _mono.MatrixsaveTarget(x);

        var y = Source.NewFromMemory(x.Blob);
        var im = Image.MatrixloadSource(y);

        Assert.Equal(0, (im - _mono).Abs().Max());
    }

    [SkippableFact]
    public void TestPpm()
    {
        Skip.IfNot(Helper.Have("ppmload"), "no PPM support, skipping test");

        SaveLoadFile(".pgm", "[ascii]", _mono);
        SaveLoadFile(".ppm", "[ascii]", _colour);

        SaveLoadFile(".pbm", "[ascii]", _oneBit);

        var rgb16 = _colour.Colourspace(Enums.Interpretation.Rgb16);
        var grey16 = _mono.Colourspace(Enums.Interpretation.Rgb16);

        SaveLoad("%s.ppm", rgb16);

        SaveLoadFile(".ppm", "[ascii]", grey16);
        SaveLoadFile(".ppm", "[ascii]", rgb16);

        var x = Target.NewToMemory();
        _mono.PpmsaveTarget(x);

        var y = Source.NewFromMemory(x.Blob);
        var im = Image.PpmloadSource(y);

        Assert.Equal(0, (im - _mono).Abs().Max());
    }

    [SkippableFact]
    public void TestRad()
    {
        Skip.IfNot(Helper.Have("radload"), "no Radiance support, skipping test");

        SaveLoad("%s.hdr", _rad);
        SaveLoad("%s.pfm", _rad);
        SaveLoad("%s.tif", _rad);
        SaveBufferTempFile("radsave_buffer", ".hdr", _rad);
        SaveLoadStream(".hdr", "", _rad);
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
        var filename = Helper.GetTemporaryFile(_tempDir);
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
        filename = Helper.GetTemporaryFile(_tempDir);
        _colour.Dzsave(filename, layout: Enums.ForeignDzLayout.Google);

        // test bottom-right tile ... default is 256x256 tiles, overlap 0
        x = Image.NewFromFile(filename + "/2/2/3.jpg");
        Assert.Equal(256, x.Width);
        Assert.Equal(256, x.Height);
        Assert.False(File.Exists(filename + "/2/2/4.jpg"));
        Assert.False(Directory.Exists(filename + "/3"));
        x = Image.NewFromFile(filename + "/blank.png");
        Assert.Equal(256, x.Width);
        Assert.Equal(256, x.Height);

        // google layout with overlap ... verify that we clip correctly

        // overlap 1, 510x510 pixels, 256 pixel tiles, should be exactly 2x2
        // tiles, though in fact the bottom and right edges will be white
        filename = Helper.GetTemporaryFile(_tempDir);

        _colour.ExtractArea(0, 0, 510, 510).Dzsave(filename, layout: Enums.ForeignDzLayout.Google, overlap: 1,
            depth: Enums.ForeignDzDepth.One);

        x = Image.NewFromFile(filename + "/0/1/1.jpg");
        Assert.Equal(256, x.Width);
        Assert.Equal(256, x.Height);
        Assert.False(File.Exists(filename + "/0/2/2.jpg"));

        // with 511x511, it'll fit exactly into 2x2 -- we we actually generate
        // 3x3, since we output the overlaps
        filename = Helper.GetTemporaryFile(_tempDir);
        _colour.ExtractArea(0, 0, 511, 511).Dzsave(filename, layout: Enums.ForeignDzLayout.Google, overlap: 1,
            depth: Enums.ForeignDzDepth.One);

        x = Image.NewFromFile(filename + "/0/2/2.jpg");
        Assert.Equal(256, x.Width);
        Assert.Equal(256, x.Height);
        Assert.False(File.Exists(filename + "/0/3/3.jpg"));

        // default zoomify layout
        filename = Helper.GetTemporaryFile(_tempDir);
        _colour.Dzsave(filename, layout: Enums.ForeignDzLayout.Zoomify);

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

        var buf1 = File.ReadAllBytes(filename);
        var buf2 = File.ReadAllBytes(filename2);

        // compressed output should produce smaller file size
        Assert.True(buf2.Length < buf1.Length);

        // check whether the *.dzi file is Deflate-compressed
        Assert.Contains("http://schemas.microsoft.com/deepzoom/2008", Encoding.ASCII.GetString(buf1));
        Assert.DoesNotContain("http://schemas.microsoft.com/deepzoom/2008", Encoding.ASCII.GetString(buf2));

        // test suffix
        filename = Helper.GetTemporaryFile(_tempDir);
        _colour.Dzsave(filename, suffix: ".png");

        x = Image.NewFromFile(filename + "_files/10/0_0.png");
        Assert.Equal(255, x.Width);

        // test overlap
        filename = Helper.GetTemporaryFile(_tempDir);
        _colour.Dzsave(filename, overlap: 200);

        x = Image.NewFromFile(filename + "_files/10/1_1.jpeg");
        Assert.Equal(654, x.Width);

        // test tile-size
        filename = Helper.GetTemporaryFile(_tempDir);
        _colour.Dzsave(filename, tileSize: 512);

        y = Image.NewFromFile(filename + "_files/10/0_0.jpeg");
        Assert.Equal(513, y.Width);
        Assert.Equal(513, y.Height);

        // test save to memory buffer
        filename = Helper.GetTemporaryFile(_tempDir, ".zip");
        var baseName = Path.GetFileNameWithoutExtension(filename);

        _colour.Dzsave(filename);

        buf1 = File.ReadAllBytes(filename);
        buf2 = _colour.DzsaveBuffer(imagename: baseName);
        Assert.Equal(buf1.Length, buf2.Length);

        // we can't test the bytes are exactly equal -- the timestamp in
        // vips-properties.xml will be different

        // added in 8.7
        _ = _colour.DzsaveBuffer(regionShrink: Enums.RegionShrink.Mean);
        _ = _colour.DzsaveBuffer(regionShrink: Enums.RegionShrink.Mode);
        _ = _colour.DzsaveBuffer(regionShrink: Enums.RegionShrink.Median);
    }

    [SkippableFact]
    public void TestHeifload()
    {
        Skip.IfNot(Helper.Have("heifload"), "no HEIF support, skipping test");

        void HeifValid(Image im)
        {
            var a = im[10, 10];

            // different versions of libheif decode have slightly different
            // rounding
            Helper.AssertAlmostEqualObjects(new[]
            {
                197.0, 181.0, 158.0
            }, a, 2);

            Assert.Equal(3024, im.Width);
            Assert.Equal(4032, im.Height);

            Assert.Equal(3, im.Bands);
        }

        FileLoader("heifload", Helper.AvifFile, HeifValid);
        BufferLoader("heifload_buffer", Helper.AvifFile, HeifValid);
    }

    [SkippableFact]
    public void TestHeifsave()
    {
        Skip.IfNot(Helper.Have("heifsave"), "no HEIF support, skipping test");

        SaveLoadBuffer("heifsave_buffer", "heifload_buffer", _colour, kwargs: new VOption
        {
            {"lossless", true},
            {"compression", Enums.ForeignHeifCompression.Av1}
        });

        SaveLoad("%s.avif", _colour);

        // test lossless mode
        var x = Image.NewFromFile(Helper.AvifFile);
        var buf = x.HeifsaveBuffer(lossless: true, compression: Enums.ForeignHeifCompression.Av1, effort: 0);
        var im2 = Image.NewFromBuffer(buf);
        Assert.Equal(0, (x - im2).Abs().Max());

        // higher Q should mean a bigger buffer, needs libheif >= v1.8.0,
        // see: https://github.com/libvips/libvips/issues/1757
        var b1 = _mono.HeifsaveBuffer(q: 10, compression: Enums.ForeignHeifCompression.Av1);
        var b2 = _mono.HeifsaveBuffer(q: 90, compression: Enums.ForeignHeifCompression.Av1);
        Assert.True(b2.Length > b1.Length);

        // try saving an image with an ICC profile and reading it back
        buf = _colour.HeifsaveBuffer(q: 10, compression: Enums.ForeignHeifCompression.Av1);
        x = Image.NewFromBuffer(buf);
        if (x.Contains("icc-profile-data"))
        {
            var p1 = _colour.Get("icc-profile-data");
            var p2 = x.Get("icc-profile-data");
            Assert.Equal(p1, p2);
        }

        // add tests for exif, xmp, iptc
        // the exif test will need us to be able to walk the header,
        // we can't just check exif-data

        // test that exif changes change the output of heifsave
        // first make sure we have exif support
        var z = Image.NewFromFile(Helper.AvifFile);
        if (z.Contains("exif-ifd0-Orientation"))
        {
            x = z.Mutate(im => im.Set("exif-ifd0-Make", "banana"));

            buf = x.HeifsaveBuffer(q: 10, compression: Enums.ForeignHeifCompression.Av1);
            var y = Image.NewFromBuffer(buf);
            Assert.StartsWith("banana", (string)y.Get("exif-ifd0-Make"));
        }
    }
}