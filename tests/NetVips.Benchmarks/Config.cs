using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

namespace NetVips.Benchmarks;

public class Config : ManualConfig
{
    public Config()
    {
        // Only support LTS and latest releases
        // https://endoflife.date/dotnet
        AddJob(Job.Default
#if NET8_0
                .WithToolchain(CsProjCoreToolchain.NetCoreApp80)
                .WithRuntime(NativeAotRuntime.Net80)
                .WithId(".NET 8.0 CLI (NativeAOT)")
#elif NET9_0
                .WithToolchain(CsProjCoreToolchain.NetCoreApp90)
                .WithRuntime(NativeAotRuntime.Net90)
                .WithId(".NET 9.0 CLI (NativeAOT)")
#endif
#if GLOBAL_VIPS
                .WithArguments(new Argument[]
                {
                    new MsBuildArgument("/p:UseGlobalLibvips=true")
                })
#endif
        );

        // Don't escape HTML within the GitHub Markdown exporter,
        // to support <pre>-tags within the "Method" column.
        // Ouch, this is quite hackish.
        var githubExporter = MarkdownExporter.GitHub;
        githubExporter.GetType().GetField("EscapeHtml", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(
            githubExporter, false);
    }
}