namespace NetVips.Samples
{
    using System;
    using System.IO;
    /*using System.Threading.Tasks;*/

    public class NewFromStreamRef : ISample
    {
        public string Name => "NewFromStream reference test";
        public string Category => "Internal";

        public const string Filename = "images/equus_quagga.jpg";

        public void Execute(string[] args)
        {
            Cache.Max = 0;

            /*Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = NetVips.Concurrency },
                i =>
                {*/
            using var stream = File.OpenRead(Filename);
            var image = Image.NewFromStream(stream, access: Enums.Access.Sequential);

            using var mutated = image.Mutate(mutable => mutable.Set(GValue.GStrType, "exif-ifd0-XPComment", "This is a test"));

            Console.WriteLine($"Reference count image: {image.RefCount}");

            // Test to ensure {Read,Seek}Delegate doesn't get disposed
            image.Dispose();

            Console.WriteLine($"Reference count mutated: {mutated.RefCount}");

            var average = mutated.Avg();
            Console.WriteLine($"Average: {average}");
            /*});*/
        }
    }
}