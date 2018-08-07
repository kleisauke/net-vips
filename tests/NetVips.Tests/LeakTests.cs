using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace NetVips.Tests
{
    // Note: Make sure to run this test separately, as this will check memory allocations.
    [Trait("Category", "leak")]
    public class LeakTests : IDisposable
    {
        public LeakTests()
        {
            // Enable libvips leak checking.
            Base.LeakSet(1);

            // Disable operations cache.
            Operation.VipsCacheSetMax(0);
        }

        public void Dispose()
        {
            // Disable libvips leak checking.
            Base.LeakSet(0);

            // Enable operations cache.
            Operation.VipsCacheSetMax(1000);
        }

        [Fact]
        public void TestDelete()
        {
            const string filename = "lichtenstein";
            const string extension = "jpg";

            // Make sure the thumbnail is disposed correctly.
            using (var thumb = Image.Thumbnail(Path.Combine(Helper.Images, $"{filename}.{extension}"), 250))
            {
                //var buf = thumb.WriteToBuffer($".{extension}");
                thumb.WriteToFile(Path.Combine(Helper.Images, $"{filename}.thumbnail.{extension}"));
            }

            var memStats = Base.MemoryStats();
            var activeAllocs = memStats[0];
            var currentAllocs = memStats[1];
            var files = memStats[2];

            // No bytes may be still allocated.
            Assert.Equal(0, activeAllocs);
            Assert.Equal(0, currentAllocs);

            // No files may still be open.
            Assert.Equal(0, files);

            // In order to remove the file successfully; an immediate garbage collection is
            // required on Windows (for an unknown reason).
            // See: https://github.com/kleisauke/net-vips/issues/12
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            var ex = Record.Exception(() => File.Delete(Path.Combine(Helper.Images, $"{filename}.{extension}")));
            Assert.Null(ex);
        }
    }
}