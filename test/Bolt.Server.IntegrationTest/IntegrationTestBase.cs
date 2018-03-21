using System;
using System.Net.Http;
using Bolt.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt.Server.IntegrationTest
{
    public abstract class IntegrationTestBase : IDisposable
    {
        private readonly TestServer _runningServer;

        protected IntegrationTestBase()
        {
            ServerUrl = new Uri("http://localhost");
            _runningServer = new TestServer(new WebHostBuilder().Configure(Configure).ConfigureServices(
                s =>
                {
                    s.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    s.AddRouting();
                    ConfigureServices(s);
                }));

            ClientConfiguration = new ClientConfiguration();
            HttpMessageHandler handler = _runningServer.CreateHandler();
            ClientConfiguration.HttpMessageHandler = handler;
        }

        public Uri ServerUrl { get; private set; }

        protected ClientConfiguration ClientConfiguration { get; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _runningServer.Dispose();
        }

        protected abstract void Configure(IApplicationBuilder appBuilder);

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddBolt();
        }

        protected virtual void Destroy()
        {
            _runningServer.Dispose();
        }
    }
}
