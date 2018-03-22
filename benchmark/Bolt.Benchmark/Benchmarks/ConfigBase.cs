using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Loggers;
using Bolt.Benchmark.Benchmarks.Helpers;

namespace Bolt.Performance.Core.Benchmark
{
    public class ConfigBase : ManualConfig
    {
        public ConfigBase()
        {
            var version = typeof(BoltFramework).Assembly.GetName().Version;
            Add(DefaultColumnProviders.Instance);
            Add(new TagColumn("Version", name => version.ToString()));
            Add(MemoryDiagnoser.Default);
            Add(StatisticColumn.OperationsPerSecond);

            ArtifactsPath = Path.Combine("Reports", version.ToString(), Environment.MachineName);

            if (Environment.GetCommandLineArgs().Contains("--silent"))
            {
                Add(new DummyLogger());
            }
            else
            {
                Add(ConsoleLogger.Default);
                Add(MarkdownExporter.GitHub);
                Add(JsonExporter.Brief);
                Add(CsvMeasurementsExporter.Default);
                Add(RPlotExporter.Default);
            }
        }
    }
}
