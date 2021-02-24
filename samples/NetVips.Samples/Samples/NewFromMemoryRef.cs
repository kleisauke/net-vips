namespace NetVips.Samples
{
    using System;
    using System.Linq;

    /// <summary>
    /// See: https://github.com/libvips/pyvips/pull/104#issuecomment-554632653
    /// </summary>
    public class NewFromMemoryRef : ISample
    {
        public string Name => "NewFromMemory reference test";
        public string Category => "Internal";

        public void Execute(string[] args)
        {
            Cache.Max = 0;

            using var a = Image.NewFromMemory(Enumerable.Repeat((byte)255, 200).ToArray(),
                20, 10, 1, Enums.BandFormat.Uchar);
            using var b = a / 2;

            Console.WriteLine($"Reference count b: {b.RefCount}");

            var average = b.Avg();

            Console.WriteLine($"Before GC: {average}");

            GC.Collect();
            GC.WaitForPendingFinalizers();

            average = b.Avg();

            Console.WriteLine($"After GC: {average}");
        }
    }
}