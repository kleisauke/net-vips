using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

namespace NetVips.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            Add(Job.Default.With(CsProjCoreToolchain.From(
                new NetCoreAppSettings(
                    targetFrameworkMoniker: "netcoreapp2.1",
                    runtimeFrameworkVersion: "2.1.0",
                    name: ".NET Core 2.1.0"))));
        }
    }
}