using System;
using BenchmarkDotNet.Running;

namespace NetVips.Benchmarks;

public class Runner
{
    public static void Main(string[] args)
    {
        TestImage.BuildTestImages(AppDomain.CurrentDomain.BaseDirectory);
        BenchmarkRunner.Run<Benchmark>();
    }
}