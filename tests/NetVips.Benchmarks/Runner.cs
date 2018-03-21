using BenchmarkDotNet.Running;

namespace NetVips.Benchmarks
{
    public class Runner
    {
        public static void Main(string[] args)
        {
            // TestImage.BuildTestImages(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Images"));
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}