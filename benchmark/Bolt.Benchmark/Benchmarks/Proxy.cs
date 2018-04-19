using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Validators;
using Bolt.Client;
using Bolt.Metadata;
using Bolt.Performance.Core.Benchmark;
using Bolt.Pipeline;

namespace Bolt.Benchmark.Benchmarks
{
    [RankColumn]
    [Config(typeof(Config))]
    public class Proxy
    {
        private DummyInterface _handCoded;

        private IDummyInterface _dynamicProxy;

        private object _arg1 = new object();

        private object _arg2 = new object();

        private object _arg3 = new object();

        public interface IDummyInterface
        {
            Task MethodAsync(object arg1, object arg2, object arg3);

            Task<string> GetDataAsync(object arg1, object arg2, object arg3);

            string GetData(object ar1, object arg2, object arg3);
        }

        private class Config : ConfigBase
        {
            public Config()
            {
                Add(JitOptimizationsValidator.DontFailOnError);
                AddJob(Job.Core, InProcessToolchain.Instance);
            }

            private void AddJob(Job job, IToolchain toolchain)
            {
                Add(job.With(toolchain)
                    .WithRemoveOutliers(false)
                    .With(RunStrategy.Throughput));
            }
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var pipeline = new Pipeline();

            _handCoded = new DummyInterface()
            {
                Contract = BoltFramework.GetContract(typeof(IDummyInterface)),
                Pipeline = pipeline
            };
            _dynamicProxy = new ProxyFactory().CreateProxy<IDummyInterface>(pipeline);
        }

        [Benchmark]
        public Task ExecuteFunctionWithParametersAsync_HandCoded()
        {
            return _handCoded.GetDataAsync(_arg1, _arg2, _arg3);
        }

        [Benchmark]
        public Task ExecuteFunctionWithParametersAsync_DynamicProxy()
        {
            return _dynamicProxy.GetDataAsync(_arg1, _arg2, _arg3);
        }

        [Benchmark(Baseline = true)]
        public Task ExecuteMethodWithParametersAsync_HandCoded()
        {
            return _handCoded.MethodAsync(_arg1, _arg2, _arg3);
        }

        [Benchmark]
        public Task ExecuteMethodWithParametersAsync_DynamicProxy()
        {
            return _dynamicProxy.MethodAsync(_arg1, _arg2, _arg3);
        }

        private class Pipeline : Client.Pipeline.IClientPipeline
        {
            public Pipeline()
            {
                Instance = HandleAsync;
            }

            public ActionDelegate<ClientActionContext> Instance { get; }

            public void Dispose()
            {
            }

            public TMiddleware Find<TMiddleware>() where TMiddleware : IMiddleware<ClientActionContext>
            {
                return default(TMiddleware);
            }

            public void Validate(ContractMetadata contract)
            {
            }

            private Task HandleAsync(ClientActionContext context)
            {
                context.ActionResult = "test";
                return Task.CompletedTask;
            }
        }

        private class DummyInterface : ProxyBase, IDummyInterface
        {
            private static readonly MethodInfo GetDataAsyncMethod = typeof(IDummyInterface).GetRuntimeMethods().First(m => m.Name == nameof(GetDataAsync));

            private static readonly MethodInfo GetDataMethod = typeof(IDummyInterface).GetRuntimeMethods().First(m => m.Name == nameof(GetData));

            private static readonly MethodInfo MethodAsyncMethod = typeof(IDummyInterface).GetRuntimeMethods().First(m => m.Name == nameof(MethodAsync));

            public async Task<string> GetDataAsync(object ar1, object arg2, object arg3)
            {
                var args = System.Buffers.ArrayPool<object>.Shared.Rent(3);
                args[0] = ar1;
                args[1] = arg2;
                args[2] = arg3;

                try
                {
                    return (string)await SendAsync(GetDataAsyncMethod, args);
                }
                finally
                {
                    System.Buffers.ArrayPool<object>.Shared.Return(args);
                }
            }

            public string GetData(object arg1, object arg2, object arg3)
            {
                var args = System.Buffers.ArrayPool<object>.Shared.Rent(3);
                args[0] = arg1;
                args[1] = arg2;
                args[2] = arg3;

                try
                {
                    return (string)this.Send(GetDataMethod, args);
                }
                finally
                {
                    System.Buffers.ArrayPool<object>.Shared.Return(args);
                }
            }

            public async Task MethodAsync(object arg1, object arg2, object arg3)
            {
                var args = System.Buffers.ArrayPool<object>.Shared.Rent(3);
                args[0] = arg1;
                args[1] = arg2;
                args[2] = arg3;

                try
                {
                    await SendAsync(MethodAsyncMethod, args);
                }
                finally
                {
                    System.Buffers.ArrayPool<object>.Shared.Return(args);
                }
            }
        }
    }
}
