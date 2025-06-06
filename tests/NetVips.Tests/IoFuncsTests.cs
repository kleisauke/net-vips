using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace NetVips.Tests;

[Collection(nameof(NonParallelTestCollection))]
public class IoFuncsTests : IClassFixture<TestsFixture>
{
    public IoFuncsTests(TestsFixture testsFixture, ITestOutputHelper output)
    {
        testsFixture.SetUpLogging(output);
    }

    /// <summary>
    /// test the vips7 filename splitter ... this is very fragile and annoying
    /// code with lots of cases
    /// </summary>
    [SkippableFact]
    public void TestSplit7()
    {
        var ex = Record.Exception(() => NetVips.PathFilename7(""));
        Skip.IfNot(ex == null, "vips configured with --disable-deprecated, skipping test");

        string[] Split(string path)
        {
            var filename7 = NetVips.PathFilename7(path);
            var mode7 = NetVips.PathMode7(path);
            return [filename7, mode7];
        }

        var cases = new Dictionary<string, string[]>
        {
            {
                @"c:\silly:dir:name\fr:ed.tif:jpeg:95,,,,c:\icc\srgb.icc",
                [
                    @"c:\silly:dir:name\fr:ed.tif",
                    @"jpeg:95,,,,c:\icc\srgb.icc"
                ]
            },
            {
                "I180:",
                [
                    "I180",
                    ""
                ]
            },
            {
                @"c:\silly:",
                [
                    @"c:\silly",
                    ""
                ]
            },
            {
                @"c:\program files\x:hello",
                [
                    @"c:\program files\x",
                    "hello"
                ]
            },
            {
                @"C:\fixtures\2569067123_aca715a2ee_o.jpg",
                [
                    @"C:\fixtures\2569067123_aca715a2ee_o.jpg",
                    ""
                ]
            }
        };

        foreach (var entry in cases)
        {
            Assert.Equal(entry.Value, Split(entry.Key));
        }
    }

    [Fact]
    public void TestNewFromImage()
    {
        var im = Image.MaskIdeal(100, 100, 0.5, reject: true, optical: true);

        var im2 = im.NewFromImage(12);

        Assert.Equal(im.Width, im2.Width);
        Assert.Equal(im.Height, im2.Height);
        Assert.Equal(im.Interpretation, im2.Interpretation);
        Assert.Equal(im.Format, im2.Format);
        Assert.Equal(im.Xres, im2.Xres);
        Assert.Equal(im.Yres, im2.Yres);
        Assert.Equal(im.Xoffset, im2.Xoffset);
        Assert.Equal(im.Yoffset, im2.Yoffset);
        Assert.Equal(1, im2.Bands);
        Assert.Equal(12, im2.Avg());

        im2 = im.NewFromImage(1, 2, 3);
        Assert.Equal(3, im2.Bands);
        Assert.Equal(2, im2.Avg());
    }

    [Fact]
    public void TestNewFromMemory()
    {
        var s = new byte[200];
        var im = Image.NewFromMemory(s, 20, 10, 1, Enums.BandFormat.Uchar);
        Assert.Equal(20, im.Width);
        Assert.Equal(10, im.Height);
        Assert.Equal(Enums.BandFormat.Uchar, im.Format);
        Assert.Equal(1, im.Bands);
        Assert.Equal(0, im.Avg());

        im += 10;
        Assert.Equal(10, im.Avg());
    }

    [Fact]
    public void TestNewFromMemoryPtr()
    {
        const int sizeInBytes = 100;
        var memory = Marshal.AllocHGlobal(sizeInBytes);

        // Zero the memory
        // Unsafe.InitBlockUnaligned((byte*)memory, 0, sizeInBytes);
        Marshal.Copy(new byte[sizeInBytes], 0, memory, sizeInBytes);

        // Avoid reusing the image after subsequent use
        var prevMax = Cache.Max;
        Cache.Max = 0;

        using (var im = Image.NewFromMemory(memory, sizeInBytes, 10, 10, 1, Enums.BandFormat.Uchar))
        {
            im.OnPostClose += () => Marshal.FreeHGlobal(memory);

            Assert.Equal(0, im.Avg());
        } // OnPostClose

        Cache.Max = prevMax;
    }

    [Fact]
    public void TestNewFromMemoryReadOnly()
    {
        ReadOnlyMemory<byte> s = new byte[200];
        var im = Image.NewFromMemory(s, 20, 10, 1, Enums.BandFormat.Uchar);
        Assert.Equal(20, im.Width);
        Assert.Equal(10, im.Height);
        Assert.Equal(Enums.BandFormat.Uchar, im.Format);
        Assert.Equal(1, im.Bands);
        Assert.Equal(0, im.Avg());

        im += 10;
        Assert.Equal(10, im.Avg());
    }

    [Fact]
    public void TestNewFromMemoryCopySpan()
    {
        ReadOnlySpan<byte> s = stackalloc byte[200];
        var im = Image.NewFromMemoryCopy(s, 20, 10, 1, Enums.BandFormat.Uchar);
        Assert.Equal(20, im.Width);
        Assert.Equal(10, im.Height);
        Assert.Equal(Enums.BandFormat.Uchar, im.Format);
        Assert.Equal(1, im.Bands);
        Assert.Equal(0, im.Avg());

        im += 10;
        Assert.Equal(10, im.Avg());
    }

    [Fact]
    public void TestGetFields()
    {
        var im = Image.Black(10, 10);
        var fields = im.GetFields();

        // we might add more fields later
        Assert.True(fields.Length > 10);

        Assert.Equal("width", fields[0]);
    }

    [Fact]
    public void TestGetSuffixes()
    {
        var suffixes = NetVips.GetSuffixes();

        // vips supports these file types by default
        // (without being dependent on external dependencies):
        // - Native file format (`*.v`, `*.vips`).
        // - PPM images (`*.ppm`, `*.pgm`, `*.pbm`, `*.pfm`).
        // - Analyze images (`*.hdr`).
        Assert.True(suffixes.Length >= 7);
    }

    [Fact]
    public void TestFindLoadUtf8()
    {
        Assert.Equal("VipsForeignLoadJpegFile", Image.FindLoad(Helper.JpegFile));
    }

    [Fact]
    public void TestWriteToMemory()
    {
        var s = new byte[200];
        var im = Image.NewFromMemory(s, 20, 10, 1, Enums.BandFormat.Uchar);
        var t = im.WriteToMemory();
        Assert.True(s.SequenceEqual(t));
    }

    [Fact]
    public void TestRegion()
    {
        var im = Image.Black(100, 100);
        var region = Region.New(im);
        var data = region.Fetch(0, 0, 10, 10);

        Assert.Equal(10, region.Width);
        Assert.Equal(10, region.Height);
        Assert.Equal(100, data.Length);
        Assert.True(data.All(p => p == 0));

        data = region.Fetch(0, 0, 20, 10);

        Assert.Equal(20, region.Width);
        Assert.Equal(10, region.Height);
        Assert.Equal(200, data.Length);
        Assert.True(data.All(p => p == 0));
    }

    [Fact]
    public void TestInvalidate()
    {
        byte[] data = [0];

        var im = Image.NewFromMemory(data, 1, 1, 1, Enums.BandFormat.Uchar);
        var point = im[0, 0];
        Assert.Equal(data[0], point[0]);

        data[0] = 1;

        point = im[0, 0];
        Assert.True(point[0] <= data[0]); // can be either 0 or 1

        im.Invalidate();
        point = im[0, 0];
        Assert.Equal(data[0], point[0]);
    }

    [SkippableFact]
    public void TestSetProgress()
    {
        Skip.IfNot(Helper.Have("dzsave"), "no dzsave support, skipping test");

        var im = Image.NewFromFile(Helper.JpegFile, access: Enums.Access.Sequential);

        var lastPercent = 0;

        var progress = new Progress<int>(percent => lastPercent = percent);
        im.SetProgress(progress);

        var buf = im.DzsaveBuffer("image-pyramid");
        Assert.True(buf.Length > 0);
        Assert.True(lastPercent <= 100);
    }

    [Fact]
    public void TestModuleInitializer()
    {
        // vips should have been initialized when this assembly was loaded.
        Assert.True(ModuleInitializer.VipsInitialized);
    }

    [SkippableFact]
    public void TestRevalidate()
    {
        Skip.If(Cache.Max == 0, "requires a functional operation cache");

        var tempDir = Helper.GetTemporaryDirectory();
        var filename = Helper.GetTemporaryFile(tempDir, ".v");

        var im1 = Image.Black(10, 10);
        im1.WriteToFile(filename);

        var load1 = Image.NewFromFile(filename);
        Assert.Equal(im1.Width, load1.Width);

        var im2 = Image.Black(20, 20);
        im2.WriteToFile(filename);

        // this will use the old, cached load
        var load2 = Image.NewFromFile(filename);
        Assert.Equal(im1.Width, load2.Width);

        // load again with "revalidate" and we should see the new image
        load2 = Image.NewFromFile(filename, revalidate: true);
        Assert.Equal(im2.Width, load2.Width);

        // load once more without revalidate and we should see the cache
        // new image
        load2 = Image.NewFromFile(filename);
        Assert.Equal(im2.Width, load2.Width);
    }
}