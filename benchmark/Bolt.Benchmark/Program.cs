using System.Linq;
using System.Net;
using BenchmarkDotNet.Running;
using Bolt.Benchmark.Benchmarks;
using Bolt.Performance.Core.Benchmark;
using Microsoft.Extensions.CommandLineUtils;

namespace Bolt.Benchmark
{
    public static class Program
    {
        private static AnsiConsole Console = AnsiConsole.GetOutput(true);

        public static int Main(params string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;		
            ServicePointManager.MaxServicePoints = 1000;
 
            var app = new CommandLineApplication();
            app.Name = "bolt";
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 2;
            });

            app.Command("benchmark", c =>
            {
                var output = c.Option("--output <PATH>", "Output directory where performance report will be stored.", CommandOptionType.SingleValue);
                var quick = c.Option("--quick", "Generates asynchronous version of methods.", CommandOptionType.NoValue);
                var silent = c.Option("--silent", "Generates asynchronous version of methods.", CommandOptionType.NoValue);

                c.OnExecute(() =>
                {
                    var summary = BenchmarkRunner.Run<PerformanceContractBenchmark>(new PerformanceContractBenchmarkConfig(quick.HasValue(), silent.HasValue(), output.Value()));

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
                    return 0;
                });
            });

            return app.Execute(args);
        }

        private static int Fail(object o, string message)
        {
            System.Console.Error.WriteLine("'{0}' failed, reason: '{1}'", o, message);
            return 1;
        }
    }
}
