using BenchmarkDotNet.Loggers;

namespace Bolt.Benchmark.Benchmarks.Helpers
{
    public class DummyLogger : ILogger
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
