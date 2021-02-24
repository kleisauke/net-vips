namespace NetVips.Samples
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;

    /// <summary>
    /// See: https://github.com/kleisauke/net-vips/issues/53
    /// </summary>
    public class OperationRef : ISample
    {
        public string Name => "Operation reference test";
        public string Category => "Internal";

        public const string Filename = "images/lichtenstein.jpg";

        public void Execute(string[] args)
        {
            Cache.Max = 0;

            using var fileStream = File.OpenRead(Filename);
            using var image = Image.NewFromStream(fileStream);

            for (var i = 0; i < 1000; i++)
            {
                using var crop = image.Crop(0, 0, 256, 256);
                var _ = crop.Avg();

                Console.WriteLine($"reference count: {image.RefCount}");

                // RefCount should not increase (i.e. operation should be freed)
                Debug.Assert(image.RefCount == 2u);
            }

            var count = 0;
            var locker = new object();

            Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = NetVips.Concurrency },
                i =>
                {
                    Interlocked.Increment(ref count);

                    using var crop = image.Crop(0, 0, 256, 256);
                    lock (locker)
                    {
                        var _ = crop.Avg();

                        Console.WriteLine($"reference count: {image.RefCount} with {count} active threads");

                        // RefCount -1 must be lower than or equal to the number of active threads
                        Debug.Assert(image.RefCount - 1 <= (uint)count);
                    }

                    Interlocked.Decrement(ref count);
                });
        }
    }
}