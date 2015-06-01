using System;
using System.Threading.Tasks;
using Bolt.Server.InstanceProviders;
using Microsoft.AspNet.Http.Internal;
using Moq;
using Xunit;

namespace Bolt.Server.Test
{
    public class MemorySessionFactoryTest
    {
        public MemorySessionFactoryTest()
        {
            SesionHandler = new Mock<IServerSessionHandler>(MockBehavior.Loose);
            Subject = new MemorySessionFactory(new BoltServerOptions(), SesionHandler.Object);
            Instance = new InstanceInternal();
        }

        public Mock<IServerSessionHandler> SesionHandler { get; set; }

        public MemorySessionFactory Subject { get; set; }

        private InstanceInternal Instance { get; set; }

        [Fact]
        public async Task Create_EnsureContractSesion()
        {
            var ctxt = new DefaultHttpContext();
            var sessionId = "testSessionId";

            SesionHandler.Setup(v => v.Initialize(ctxt)).Returns(sessionId).Verifiable();
            SesionHandler.Setup(v => v.GetIdentifier(ctxt)).Returns(sessionId).Verifiable();

            var result = await Subject.CreateAsync(ctxt, Instance);
            Assert.NotNull(result);

            SesionHandler.Verify();
        }


        private class InstanceInternal : IDisposable
        {
            public void Dispose()
            {
                Disposed = true;
            }

            public bool Disposed { get; set; }
        }
    }
}
