using Bolt.Client;
using Microsoft.Framework.DependencyInjection;
using System;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public abstract class IntegrationTestBase : IClassFixture<BoltServer>
    {
        private BoltServer _runningServer;
        public Uri ServerUrl { get; private set; }

        public IntegrationTestBase(BoltServer server)
        {
            ServerUrl = new Uri("http://localhost");
            _runningServer = server;
            server.Start(Configure,ConfigureServices);
            ClientConfiguration = new ClientConfiguration();
            ClientConfiguration.RequestHandler = new RequestHandler(ClientConfiguration.DataHandler, new ClientErrorProvider(ClientConfiguration.Options.ServerErrorHeader), server.GetHandler());
        }

        protected ClientConfiguration ClientConfiguration { get; private set; }

        protected abstract void Configure(Microsoft.AspNet.Builder.IApplicationBuilder appBuilder);

        [Fact]
        public void RequestBoltRoot_EnsureOk()
        {
            // TODO:
        }

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
