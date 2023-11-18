namespace NetVips.Benchmarks
{
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Environments;
    using BenchmarkDotNet.Exporters;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Toolchains.CsProj;
    using System.Reflection;

    public class Config : ManualConfig
    {
        public Config()
        {
            // Only support LTS and latest releases
            // https://endoflife.date/dotnet
            AddJob(Job.Default
#if NET6_0
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp60)
                    .WithId(".NET 6.0 CLI")
#elif NET7_0
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp70)
                    .WithRuntime(NativeAotRuntime.Net70)
                    .WithId(".NET 7.0 CLI (NativeAOT)")
#elif NET8_0       
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp80)
                    .WithRuntime(NativeAotRuntime.Net80)
                    .WithId(".NET 8.0 CLI (NativeAOT)")
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
            githubExporter.GetType().GetField("EscapeHtml", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(githubExporter, false);
        }
    }
}