using System;
using System.IO;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;

namespace Bolt.Performance.Core.Benchmark
{
    internal class PerformanceContractBenchmarkConfig : ManualConfig
    {
        public PerformanceContractBenchmarkConfig(bool quick, bool silent, string path)
        {
            ArtifactsPath = null;
            var version = typeof(BoltFramework).Assembly.GetName().Version;

            if (!string.IsNullOrEmpty(path))
            {
                ArtifactsPath = Path.Combine(path, typeof(BoltFramework).Assembly.GetName().Version.ToString(), Environment.MachineName);
                if (!Directory.Exists(ArtifactsPath))
                {
                    Directory.CreateDirectory(ArtifactsPath);
                }

                Add(MarkdownExporter.GitHub);
                Add(JsonExporter.Brief);
            }
            else
            {
                Add(new DummyEporter());
            }

            Add(StatisticColumn.OperationsPerSecond);
            Add(DefaultColumnProviders.Instance);
            Add(new TagColumn("Version", name => version.ToString()));
            if (!silent)
            {
                Add(ConsoleLogger.Default);
            }
            else
            {
                Add(new DummyLogger());
            }

            if (quick)
            {
                Add(Job.Core
                .WithLaunchCount(1)
                .With(RunStrategy.ColdStart)
                .WithWarmupCount(1)
                .WithTargetCount(1));
            }
            else
            {
                Add(JitOptimizationsValidator.FailOnError);
                Add(MemoryDiagnoser.Default);

                Add(Job.Core
                    .With(CsProjCoreToolchain.From(NetCoreAppSettings.NetCoreApp20))
                    .WithRemoveOutliers(false)
                    .With(new GcMode { Server = true })
                    .With(RunStrategy.Throughput)
                    .WithLaunchCount(2)
                    .WithWarmupCount(5)
                    .WithTargetCount(10));
            }
        }

        private class DummyEporter : ExporterBase
        {
            public override void ExportToLog(Summary summary, ILogger logger)
            {
            }
        }

        private class DummyLogger : ILogger
        {
            public void Write(LogKind logKind, string text)
            {
            }

            public void WriteLine()
            {
            }

            public void WriteLine(LogKind logKind, string text)
            {
            }
        }
    }
}
