using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public abstract class IntegrationTestBase : IClassFixture<BoltServer>
    {
        private BoltServer _runningServer;

        public IntegrationTestBase(BoltServer server)
        {
            server.Start(ConfigureDefaultServer);
        }

        protected abstract void ConfigureDefaultServer(Microsoft.AspNet.Builder.IApplicationBuilder appBuilder);

        protected virtual void Destroy()
        {
            _runningServer.Dispose();
        }
    }
}
