namespace NetVips.Benchmarks
{
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Exporters;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Toolchains.CsProj;
    using System.Reflection;

    public class Config : ManualConfig
    {
        public Config()
        {
            // Only support LTS and latest releases
            AddJob(Job.Default
#if NETCOREAPP2_1
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp21)
                    .WithId(".Net Core 2.1 CLI")
#elif NETCOREAPP3_1
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp31)
                    .WithId(".Net Core 3.1 CLI")
#elif NET5_0
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp50)
                    .WithId(".Net 5.0 CLI")
#elif NET6_0
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp60)
                    .WithId(".Net 6.0 CLI")
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