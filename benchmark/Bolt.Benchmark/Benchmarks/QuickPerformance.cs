using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Validators;
using Bolt.Benchmark.Benchmarks;

namespace Bolt.Performance.Core.Benchmark
{
    [Config(typeof(Config))]
    public class QuickPerformance : PerformanceBase
    {
        protected override int VeryLargeMethodParamters => 10;

        protected override int VeryLargeReturnCount => 10;

        private class Config : ConfigBase
        {
            public Config()
            {
                Add(JitOptimizationsValidator.DontFailOnError);
                Add(Job.Core
                .WithLaunchCount(1)
                .With(RunStrategy.ColdStart)
                .WithWarmupCount(0)
                .With(InProcessToolchain.Instance)
                .WithId("InProcess"));

                KeepBenchmarkFiles = false;
            }
        }
    }
}
