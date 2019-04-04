namespace NetVips.Benchmarks
{
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Toolchains.CsProj;
    using BenchmarkDotNet.Toolchains.DotNetCli;

    public class Config : ManualConfig
    {
        public Config()
        {
            Add(Job.Default.With(CsProjCoreToolchain.From(
                new NetCoreAppSettings(
                    targetFrameworkMoniker: "netcoreapp2.2",
                    runtimeFrameworkVersion: "2.2.0",
                    name: ".NET Core 2.2.0"))));
        }
    }
}