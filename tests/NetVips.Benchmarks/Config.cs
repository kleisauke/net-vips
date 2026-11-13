using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.NetCoreApp;

namespace NetVips.Benchmarks;

public class Config : ManualConfig
{
    public Config()
    {
        // Only support LTS and latest releases
        // https://endoflife.date/dotnet
        AddJob(Job.Default
#if NET10_0
                .WithToolchain(CsProjCoreToolchain.NetCoreApp10_0)
                .WithId(".NET 10.0")
#elif NET11_0
                .WithToolchain(CsProjCoreToolchain.NetCoreApp11_0)
                .WithId(".NET 11.0")
#endif
#if GLOBAL_VIPS
                .WithArguments(new Argument[]
                {
                    new MsBuildArgument("/p:UseGlobalLibvips=true")
                })
#endif
        );
    }
}