namespace NetVips.Benchmarks
{
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Toolchains.CsProj;

    public class Config : ManualConfig
    {
        public Config()
        {
            // Only support LTS releases
            AddJob(Job.Default
#if NETCOREAPP2_1
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp21)
                    .WithId(".Net Core 2.1 CLI")
#elif NETCOREAPP3_1
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp31)
                    .WithId(".Net Core 3.1 CLI")
#endif
            );
        }
    }
}