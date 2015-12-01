using System;
using System.Net.Http;
using Bolt.Client;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;

namespace Bolt.Server.IntegrationTest
{
    public abstract class IntegrationTestBase : IDisposable
    {
        private readonly TestServer _runningServer;

        protected IntegrationTestBase()
        {
            ServerUrl = new Uri("http://localhost");
            _runningServer = TestServer.Create(Configure, s =>
            {
                s.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                ConfigureServices(s);
            });

            ClientConfiguration = new ClientConfiguration();
            HttpMessageHandler handler = _runningServer.CreateHandler();
            ClientConfiguration.HttpMessageHandler = handler;
        }

        public Uri ServerUrl { get; private set; }

        protected ClientConfiguration ClientConfiguration { get; }

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

        public void Dispose()
        {
            _runningServer.Dispose();
        }
    }
}
