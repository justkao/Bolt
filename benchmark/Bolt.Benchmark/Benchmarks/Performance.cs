using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;
using Bolt.Benchmark.Benchmarks;

namespace Bolt.Performance.Core.Benchmark
{
    [Config(typeof(Config))]
    public class Performance : PerformanceBase
    {
        private class Config : ConfigBase
        {
            public Config()
            {
                Add(JitOptimizationsValidator.FailOnError);

                AddJob(Job.Core, CsProjCoreToolchain.NetCoreApp20);
                AddJob(Job.Core, CsProjCoreToolchain.NetCoreApp21);
                AddJob(Job.Clr, CsProjClassicNetToolchain.Net461);
            }

            private void AddJob(Job job, IToolchain toolchain)
            {
                Add(job.With(toolchain)
                    .WithRemoveOutliers(false)
                    .With(new GcMode { Server = true })
                    .With(RunStrategy.Throughput)
                    .WithLaunchCount(2)
                    .WithWarmupCount(5)
                    .WithTargetCount(10));
            }
        }
    }
}
