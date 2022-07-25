namespace NetVips.Tests
{
    using System;
    using System.IO;
    using Xunit;
    using Xunit.Abstractions;

    public class ConnectionTests : IClassFixture<TestsFixture>, IDisposable
    {
        private readonly string _tempDir;

        public ConnectionTests(TestsFixture testsFixture, ITestOutputHelper output)
        {
            testsFixture.SetUpLogging(output);

            _tempDir = Helper.GetTemporaryDirectory();
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

        [SkippableFact]
        public void TestConnection()
        {
            Skip.IfNot(Helper.Have("jpegload_source"), "no jpeg source support, skipping test");

            var source = Source.NewFromFile(Helper.JpegFile);
            var image = Image.NewFromSource(source, access: Enums.Access.Sequential);
            var filename = Helper.GetTemporaryFile(_tempDir, ".png");
            var target = Target.NewToFile(filename);

            Assert.Equal(Helper.JpegFile, source.GetFileName());
            Assert.Equal(filename, target.GetFileName());

            image.WriteToTarget(target, ".png");

            image = Image.NewFromFile(Helper.JpegFile, access: Enums.Access.Sequential);
            var image2 = Image.NewFromFile(filename, access: Enums.Access.Sequential);

            Assert.True((image - image2).Abs().Max() < 10);
        }

        [SkippableFact]
        public void TestSourceCustomNoSeek()
        {
            Skip.IfNot(Helper.Have("jpegload_source"), "no jpeg source support, skipping test");

            var input = File.OpenRead(Helper.JpegFile);

            var source = new SourceCustom();
            source.OnRead += (buffer, length) => input.Read(buffer, 0, length);

            Assert.Null(source.GetFileName());
            Assert.Equal("source_custom", source.GetNick());

            var image = Image.NewFromSource(source, access: Enums.Access.Sequential);
            var image2 = Image.NewFromFile(Helper.JpegFile, access: Enums.Access.Sequential);

            Assert.True((image - image2).Abs().Max() < 10);
        }

        [SkippableFact]
        public void TestSourceCustom()
        {
            Skip.IfNot(Helper.Have("jpegload_source"), "no jpeg source support, skipping test");

            var input = File.OpenRead(Helper.JpegFile);

            var source = new SourceCustom();
            source.OnRead += (buffer, length) => input.Read(buffer, 0, length);
            source.OnSeek += (offset, origin) => input.Seek(offset, origin);

            Assert.Null(source.GetFileName());
            Assert.Equal("source_custom", source.GetNick());

            var image = Image.NewFromSource(source, access: Enums.Access.Sequential);
            var image2 = Image.NewFromFile(Helper.JpegFile, access: Enums.Access.Sequential);

            Assert.True((image - image2).Abs().Max() < 10);
        }

        [SkippableFact]
        public void TestTargetCustom()
        {
            Skip.IfNot(Helper.Have("jpegsave_target"), "no jpeg target support, skipping test");

            var filename = Helper.GetTemporaryFile(_tempDir, ".png");
            var output = File.OpenWrite(filename);

            var target = new TargetCustom();
            target.OnWrite += (buffer, length) =>
            {
                output.Write(buffer, 0, length);
                return length;
            };
            target.OnEnd += () =>
            {
                output.Close();
                return 0;
            };

            Assert.Null(target.GetFileName());
            Assert.Equal("target_custom", target.GetNick());

            var image = Image.NewFromFile(Helper.JpegFile, access: Enums.Access.Sequential);
            image.WriteToTarget(target, ".png");

            image = Image.NewFromFile(Helper.JpegFile, access: Enums.Access.Sequential);
            var image2 = Image.NewFromFile(filename, access: Enums.Access.Sequential);

            Assert.True((image - image2).Abs().Max() < 10);
        }


        [SkippableFact]
        public void TestSourceCustomWebpNoSeek()
        {
            Skip.IfNot(Helper.Have("webpload_source"), "no webp source support, skipping test");

            var input = File.OpenRead(Helper.WebpFile);

            var source = new SourceCustom();
            source.OnRead += (buffer, length) => input.Read(buffer, 0, length);

            Assert.Null(source.GetFileName());
            Assert.Equal("source_custom", source.GetNick());

            var image = Image.NewFromSource(source, access: Enums.Access.Sequential);
            var image2 = Image.NewFromFile(Helper.WebpFile, access: Enums.Access.Sequential);

            Assert.True((image - image2).Abs().Max() < 10);
        }

        [SkippableFact]
        public void TestSourceCustomWebp()
        {
            Skip.IfNot(Helper.Have("webpload_source"), "no webp source support, skipping test");

            var input = File.OpenRead(Helper.WebpFile);

            var source = new SourceCustom();
            source.OnRead += (buffer, length) => input.Read(buffer, 0, length);
            source.OnSeek += (offset, origin) => input.Seek(offset, origin);

            Assert.Null(source.GetFileName());
            Assert.Equal("source_custom", source.GetNick());

            var image = Image.NewFromSource(source, access: Enums.Access.Sequential);
            var image2 = Image.NewFromFile(Helper.WebpFile, access: Enums.Access.Sequential);

            Assert.True((image - image2).Abs().Max() < 10);
        }
    }
}