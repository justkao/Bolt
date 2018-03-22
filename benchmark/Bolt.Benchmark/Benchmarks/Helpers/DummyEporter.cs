using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

namespace Bolt.Benchmark.Benchmarks.Helpers
{
    public class DummyEporter : ExporterBase
    {
        public override void ExportToLog(Summary summary, ILogger logger)
        {
        }
    }
}
