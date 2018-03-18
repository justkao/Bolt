using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;
using System;
using System.IO;

namespace Bolt.Performance.Core.Benchmark
{
    internal class PerformanceContractBenchmarkConfig : ManualConfig
    {
        public PerformanceContractBenchmarkConfig(bool quick, string path)
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
            }
            else
            {
                Add(new DummyEporter());
            }

            Add(MemoryDiagnoser.Default);
            Add(StatisticColumn.OperationsPerSecond);
            Add(DefaultColumnProviders.Instance);
            Add(JitOptimizationsValidator.FailOnError);
            Add(new TagColumn("Version", name => version.ToString()));

            if (quick)
            {
                Add(new DummyLogger());

                Add(Job.Core
                .WithLaunchCount(1)
                .WithIterationTime(TimeInterval.FromMilliseconds(10))
                .WithWarmupCount(1)
                .WithTargetCount(1));
            }
            else
            {
                Add(ConsoleLogger.Default);

                Add(Job.Core
                    .With(CsProjCoreToolchain.From(NetCoreAppSettings.NetCoreApp20))
                    .WithRemoveOutliers(false)
                    .With(new GcMode { Server = true })
                    .With(RunStrategy.Throughput)
                    .WithLaunchCount(2)
                    .WithWarmupCount(5)
                    .WithTargetCount(10));
                /*
                Add(Job.Core
                    .With(CsProjCoreToolchain.From(NetCoreAppSettings.NetCoreApp21))
                    .WithRemoveOutliers(false)
                    .With(new GcMode { Server = true })
                    .With(RunStrategy.Throughput)
                    .WithLaunchCount(2)
                    .WithWarmupCount(5)
                    .WithTargetCount(10));
                */
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
