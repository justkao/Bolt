using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using Bolt.Benchmark.Contracts;
using Bolt.Client;
using Bolt.Serialization;
using Bolt.Serialization.MessagePack;
using Bolt.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bolt.Benchmark.Benchmarks
{
    [RankColumn]
    public class PerformanceContractBenchmark
    {
        private TestServer _runningServer;
        private Person _person;
        private List<Person> _large;
        private List<Person> _veryLarge;
        private DateTime _dateTime;

        public ClientConfiguration ClientConfiguration { get; private set; }

        public IPerformanceContract Proxy { get; private set; }

        [Params(true, false)]
        public bool UseMessagePack { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _person = Person.Create(10);
            _large = Enumerable.Repeat(_person, 100).ToList();
            _veryLarge = Enumerable.Repeat(_person, 10000).ToList();

            _dateTime = DateTime.UtcNow;

            _runningServer = new TestServer(new WebHostBuilder().ConfigureLogging(ConfigureLog).Configure(Configure).ConfigureServices(ConfigureServices));

            ClientConfiguration = new ClientConfiguration();
            if (UseMessagePack)
            {
                ClientConfiguration.Serializer = new MessagePackSerializer();
            }

            HttpMessageHandler handler = _runningServer.CreateHandler();
            ClientConfiguration.HttpMessageHandler = handler;
            Proxy = ClientConfiguration.CreateProxy<IPerformanceContract>(new Uri("http://localhost"));
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _runningServer.Dispose();
        }

        [Benchmark]
        public Task Method_Async()
        {
            return Proxy.Method_Async();
        }

        [Benchmark]
        public Task Method_Int_Async()
        {
            return Proxy.Method_Int_Async(55);
        }

        [Benchmark]
        public Task Method_Large_Async()
        {
            return Proxy.Method_Large_Async(_large);
        }

        [Benchmark]
        public Task Method_Many_Async()
        {
            return Proxy.Method_Many_Async(10, "Xxxx", _dateTime, _person);
        }

        [Benchmark]
        public Task Method_Object_Async()
        {
            return Proxy.Method_Object_Async(_person);
        }

        [Benchmark]
        public Task Method_String_Async()
        {
            return Proxy.Method_String_Async("xxxx");
        }

        [Benchmark]
        public Task Return_Ints_Async()
        {
            return Proxy.Return_Ints_Async();
        }

        [Benchmark]
        public Task Return_Int_Async()
        {
            return Proxy.Return_Int_Async();
        }

        [Benchmark]
        public Task Return_Large_Async()
        {
            return Proxy.Return_Large_Async();
        }

        [Benchmark]
        public Task Return_Objects_Async()
        {
            return Proxy.Return_Objects_Async();
        }

        [Benchmark]
        public Task Return_Object_Async()
        {
            return Proxy.Return_Object_Async();
        }

        [Benchmark]
        public Task Return_Strings_Async()
        {
            return Proxy.Return_Strings_Async();
        }

        [Benchmark]
        public Task Return_String_Async()
        {
            return Proxy.Return_String_Async();
        }

        [Benchmark]
        public async Task Method_ThrowsErrorAsync()
        {
            try
            {
                await Proxy.Method_ThrowsErrorAsync();
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Benchmark]
        public Task Return_Very_Large_Async()
        {
            return Proxy.Return_Large_Cached_Async(50000);
        }

        [Benchmark]
        public Task Method_Very_Large_Async()
        {
            return Proxy.Method_Large_Async(_veryLarge);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddLogging();
            services.AddOptions();
            services.AddBolt();
        }

        private void Configure(IApplicationBuilder app)
        {
            app.UseBolt(
                b =>
                {
                    if (UseMessagePack)
                    {
                        b.Configuration.DefaultSerializer = new MessagePackSerializer();
                        b.Configuration.AvailableSerializers = new[] { b.Configuration.DefaultSerializer };
                    }

                    b.Use<IPerformanceContract, PerformanceContractImplementation>();
                });
        }

        private void ConfigureLog(ILoggingBuilder builder)
        {
            builder.SetMinimumLevel(LogLevel.Error);
        }
    }
}
