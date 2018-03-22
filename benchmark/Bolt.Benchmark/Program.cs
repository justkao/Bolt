using System.Linq;
using System.Net;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Bolt.Benchmark.Benchmarks;
using Bolt.Performance.Core.Benchmark;
using Microsoft.Extensions.CommandLineUtils;

namespace Bolt.Benchmark
{
    public static class Program
    {
        private static readonly AnsiConsole Console = AnsiConsole.GetOutput(true);

        public static int Main(params string[] args)
        {
            var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args, new ManualConfig());
            foreach (var summary in summaries)
            {
                if (summary.HasCriticalValidationErrors)
                {
                    return Fail(summary, nameof(summary.HasCriticalValidationErrors));
                }

                foreach (var report in summary.Reports)
                {
                    if (!report.BuildResult.IsGenerateSuccess)
                    {
                        return Fail(report, nameof(report.BuildResult.IsGenerateSuccess));
                    }

                    if (!report.BuildResult.IsBuildSuccess)
                    {
                        return Fail(report, nameof(report.BuildResult.IsBuildSuccess));
                    }

                    if (!report.AllMeasurements.Any())
                    {
                        return Fail(report, nameof(report.AllMeasurements));
                    }
                }

                Console.WriteLine(summary.ToString());
            }

            return 0;
        }

        private static int Fail(object o, string message)
        {
            System.Console.Error.WriteLine("'{0}' failed, reason: '{1}'", o, message);
            return 1;
        }
    }
}
