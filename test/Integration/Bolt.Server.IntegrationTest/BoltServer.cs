using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using System;
using System.Net.Http;

namespace Bolt.Server.IntegrationTest
{
    public class BoltServer : IDisposable
    {
        private TestServer _server;

        public BoltServer()
        {
        }

        public void Start(Action<IApplicationBuilder> action, Action<IServiceCollection> configureServices)
        {
            _server = TestServer.Create(action, configureServices);
        }

        public HttpMessageHandler GetHandler()
        {
            return _server?.CreateHandler();
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
