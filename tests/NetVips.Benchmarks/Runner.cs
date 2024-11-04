using System.IO;
using BenchmarkDotNet.Running;

namespace NetVips.Benchmarks;

public class Runner
{
    public static void Main(string[] args)
    {
        TestImage.BuildTestImages(Path.Combine(Directory.GetCurrentDirectory(), "images"));
        BenchmarkRunner.Run<Benchmark>();
    }
}