using System;
using System.Reflection;
using Bolt.Client;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server.IntegrationTest
{
    using Microsoft.AspNet.TestHost;

    public abstract class IntegrationTestBase : IDisposable
    {
        private readonly TestServer _runningServer;

        protected IntegrationTestBase()
        {
            ServerUrl = new Uri("http://localhost");
            _runningServer = TestServer.Create(Configure, ConfigureServices);
            ClientConfiguration = new ClientConfiguration();
            var handler = _runningServer.CreateHandler();

            ClientConfiguration.RequestHandler = new RequestHandler(
                ClientConfiguration.DataHandler,
                new ClientErrorProvider(ClientConfiguration.Options.ServerErrorHeader),
                handler);
        }

        public Uri ServerUrl { get; private set; }

        protected ClientConfiguration ClientConfiguration { get; }

        protected abstract void Configure(Microsoft.AspNet.Builder.IApplicationBuilder appBuilder);

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
