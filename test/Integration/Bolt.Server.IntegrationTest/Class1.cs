using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using System;

namespace Bolt.Server.IntegrationTest
{
    public class BoltServer : IDisposable
    {
        private TestServer _server;

        public BoltServer()
        {
        }

        public void Start(Action<IApplicationBuilder> action)
        {
            _server = TestServer.Create(action);
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
