namespace NetVips.Benchmarks
{
    using System;
    using BenchmarkDotNet.Running;

    public class Runner
    {
        public static void Main(string[] args)
        {
            TestImage.BuildTestImages(AppDomain.CurrentDomain.BaseDirectory);
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}