using System;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Server.Session;
using Microsoft.AspNet.Http.Internal;
using Moq;
using Xunit;

namespace Bolt.Server.Test
{
    public class MemorySessionFactoryTest
    {
        public MemorySessionFactoryTest()
        {
            Options = new BoltServerOptions();
            SesionHandler = new Mock<IServerSessionHandler>(MockBehavior.Loose);
            Subject = new MemorySessionFactory(Options, SesionHandler.Object);
            Instance = new InstanceInternal();
        }

        public BoltServerOptions Options { get; set; }

        public Mock<IServerSessionHandler> SesionHandler { get; set; }

        public MemorySessionFactory Subject { get; set; }

        private InstanceInternal Instance { get; }

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

        [Fact]
        public async Task Create_EnsureAdded()
        {
            var ctxt = new DefaultHttpContext();
            var sessionId = "testSessionId";

            SesionHandler.Setup(v => v.Initialize(ctxt)).Returns(sessionId).Verifiable();
            SesionHandler.Setup(v => v.GetIdentifier(ctxt)).Returns(sessionId).Verifiable();

            await Subject.CreateAsync(ctxt, Instance);
            Assert.Equal(1, Subject.Count);
        }

        [Fact]
        public async Task GetExisting_DoesNotExist_ThrowsSessionNotFoundException()
        {
            var ctxt = new DefaultHttpContext();
            SesionHandler.Setup(v => v.GetIdentifier(ctxt)).Returns("anything").Verifiable();

            await Assert.ThrowsAsync<SessionNotFoundException>(() => Subject.GetExistingAsync(ctxt, null));

            SesionHandler.Verify();
        }

        [Fact]
        public async Task Create_SessionIdNotNull()
        {
            var ctxt = new DefaultHttpContext();

            var session = await CreateNew(ctxt, "Test");
            Assert.NotNull(session.SessionId);
        }

        [Fact]
        public async Task Create_InstanceNotNull()
        {
            var ctxt = new DefaultHttpContext();

            var session = await CreateNew(ctxt, "Test");
            Assert.NotNull(session.Instance);
        }

        [Fact]
        public async Task Destroy_EnsureDestroyed()
        {
            var ctxt = new DefaultHttpContext();

            var session = await CreateNew(ctxt, "Test");
            await session.DestroyAsync();

            Assert.Equal(0, Subject.Count);
        }

        [Fact]
        public async Task Destroy_EnsureInstanceDisposed()
        {
            var ctxt = new DefaultHttpContext();

            var session = await CreateNew(ctxt, "Test");
            var instance = session.Instance;
            await session.DestroyAsync();

            Assert.True(((InstanceInternal) instance).Disposed);
        }

        [Fact(Skip = "Unstable... investigate ")]
        public async Task Timeout_EnsureDisposed()
        {
            var ctxt = new DefaultHttpContext();

            var session = await CreateNew(ctxt, "Test");
            var instance = session.Instance;
            Subject.SessionTimeout = TimeSpan.FromMilliseconds(100);
            Subject.TimeoutCheckInterval = TimeSpan.FromMilliseconds(10);

            Thread.Sleep(150);

            Assert.Equal(0, Subject.Count);
            Assert.True(((InstanceInternal)instance).Disposed);
        }

        /*
        [Fact]
        public async Task Timeout_EnsureEventCalled()
        {
            var ctxt = new DefaultHttpContext();

            bool eventCalled = false;

            Subject.SessionTimeouted += (s, e) =>
            {
                eventCalled = true;
            };

            await CreateNew(ctxt, "Test");
            Subject.SessionTimeout = TimeSpan.FromMilliseconds(100);
            Subject.TimeoutCheckInterval = TimeSpan.FromMilliseconds(10);

            Thread.Sleep(150);

            Assert.True(eventCalled, "Timeouted event should be called on instance timeout.");
        }

        [Fact]
        public async Task Timeout_EnsureEventCalledWithValidSession()
        {
            var ctxt = new DefaultHttpContext();

            bool validId = false;
            string sessionId = null;
            Subject.SessionTimeouted += (s, e) =>
            {
                validId = e.Session == sessionId;
            };

            var session = await CreateNew(ctxt, "Test");
            sessionId = session.SessionId;

            Subject.SessionTimeout = TimeSpan.FromMilliseconds(100);
            Subject.TimeoutCheckInterval = TimeSpan.FromMilliseconds(10);

            Thread.Sleep(150);

            Assert.True(validId, "Timeouted event should be called with valid session id.");
        }
        */

        private async Task<IContractSession> CreateNew(DefaultHttpContext ctxt, string sessionId)
        {
            SesionHandler.Setup(v => v.Initialize(ctxt)).Returns(sessionId);
            SesionHandler.Setup(v => v.GetIdentifier(ctxt)).Returns(sessionId);

            return await Subject.CreateAsync(ctxt, Instance);
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
