using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Client;
using Bolt.Client.Pipeline;
using Bolt.Server.InstanceProviders;
using Bolt.Server.IntegrationTest.Core;
using Bolt.Server.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class SessionTest : IntegrationTestBase
    {
        private SessionInstanceProvider InstanceProvider { get; set; }

        private MemorySessionFactory Factory { get; set; }

        [Fact]
        public void NewProxy_EnsureReady()
        {
            var client = GetProxy(null, false);

            Assert.Equal(ProxyState.Ready, ((IProxy)client).State);
        }

        [Fact]
        public async Task OpenAsync_EnsureProperResult()
        {
            var client = GetProxy(null, false);

            Assert.Equal("test", await client.OpenSessionAsync("test"));
        }

        [Fact]
        public async Task OpenAsync_EnsureOpened()
        {
            var client = GetProxy(null, false);
            await client.OpenSessionAsync("test");

            Assert.Equal(ProxyState.Open, ((IProxy)client).State);
        }

        [Fact]
        public async Task CloseAsync_EnsureClosed()
        {
            var client = GetProxy(null, false);
            await client.OpenSessionAsync("test");
            await ((IProxy)client).CloseAsync();

            Assert.Equal(ProxyState.Closed, ((IProxy)client).State);
        }

        [Fact]
        public async Task UseClosedProxy_EnsureThrows()
        {
            var client = GetProxy(null, false);
            await client.OpenSessionAsync("test");
            await ((IProxy)client).CloseAsync();

            await Assert.ThrowsAsync<ProxyClosedException>(() => client.GetStateAsync());
        }

        [Fact]
        public async Task Open_Twice_EnsureProperResult()
        {
            var client = GetProxy(null, false);

            Assert.Equal("test", await client.OpenSessionAsync("test"));
            Assert.Equal("test", await client.OpenSessionAsync("test"));
        }

        [Fact]
        public async Task Async_EnsureStatePersistedBetweenCalls()
        {
            var client = GetProxy();

            await client.SetStateAsync("test state");
            await client.GetStateAsync();
            Assert.Equal("test state", await client.GetStateAsync());

            await (client as IProxy).CloseAsync();
        }

        [Fact]
        public async Task Async_RecoverProxy_EnsureNewSession()
        {
            Mock<IErrorHandling> errorHandling = new Mock<IErrorHandling>();
            errorHandling.Setup(e => e.Handle(It.IsAny<ClientActionContext>(), It.IsAny<Exception>()))
                .Returns(ErrorHandlingResult.Recover);

            var pipeline = CreatePipeline(1, errorHandling.Object);
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            await client.GetStateAsync();
            string sessionId1 = session.GetSession(client).SessionId;
            await client.NextCallWillFailProxyAsync();
            await client.GetStateAsync();
            string sessionId2 = session.GetSession(client).SessionId;
            Assert.NotEqual(sessionId1, sessionId2);
        }

        [Fact]
        public async Task Async_SessionNotFound_EnsureBoltServerExceptionIsThrown()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            await client.GetStateAsync();
            string sessionId1 = session.GetSession(client).SessionId;
            await Factory.DestroyAsync(sessionId1);

            try
            {
                await client.GetStateAsync();
            }
            catch (BoltServerException e)
            {
                Assert.Equal(ServerErrorCode.SessionNotFound, e.ServerError);
            }
        }

        [Fact]
        public async Task Async_SessionNotFound_RetriesEnabled_EnsureNewSession()
        {
            Mock<IErrorHandling> errorHandling = new Mock<IErrorHandling>();
            errorHandling.Setup(e => e.Handle(It.IsAny<ClientActionContext>(), It.IsAny<Exception>()))
                .Returns(ErrorHandlingResult.Recover);

            var pipeline = CreatePipeline(1, errorHandling.Object);
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            await client.GetStateAsync();
            string sessionId = session.GetSession(client).SessionId;

            SessionInstanceProvider instanceProvider = InstanceProvider;
            await Factory.DestroyAsync(sessionId);

            await client.GetStateAsync();
        }

        [Fact]
        public void EnsureStatePersistedBetweenCalls()
        {
            var client = GetProxy();

            client.SetState("test state");
            client.GetState();
            Assert.Equal("test state", client.GetState());

            (client as IDisposable).Dispose();
        }

        [Fact]
        public void RecoverProxy_EnsureNewSession()
        {
            Mock<IErrorHandling> errorHandling = new Mock<IErrorHandling>();
            errorHandling.Setup(e => e.Handle(It.IsAny<ClientActionContext>(), It.IsAny<Exception>()))
                .Returns(ErrorHandlingResult.Recover);

            var pipeline = CreatePipeline(1, errorHandling.Object);
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            client.GetState();
            string sessionId1 = session.GetSession(client).SessionId;
            client.NextCallWillFailProxy();
            client.GetState();
            string sessionId2 = session.GetSession(client).SessionId;
            Assert.NotEqual(sessionId1, sessionId2);
        }

        [Fact]
        public async Task SessionNotFound_EnsureBoltServerExceptionIsThrown()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            client.GetState();
            string sessionId1 = session.GetSession(client).SessionId;
            await Factory.DestroyAsync(sessionId1);

            try
            {
                client.GetState();
            }
            catch (BoltServerException e)
            {
                Assert.Equal(ServerErrorCode.SessionNotFound, e.ServerError);
            }
        }

        [Fact]
        public async Task SessionNotFound_RetriesEnabled_EnsureNewSession()
        {
            var pipeline = CreatePipeline(1);
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            client.GetState();
            string sessionId = session.GetSession(client).SessionId;

            SessionInstanceProvider instanceProvider = InstanceProvider;
            await Factory.DestroyAsync(sessionId);

            client.GetState();
        }

        [Fact]
        public async Task CloseSession_EnsureInstanceReleasedOnServer()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            client.GetState();
            string sessionId = session.GetSession(client).SessionId;
            (client as IDisposable).Dispose();
            SessionInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(await Factory.DestroyAsync(sessionId));
        }

        [Fact]
        public async Task Async_Request_EnsureInstanceReleasedOnServer()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            await client.GetStateAsync();
            string sessionId = session.GetSession(client).SessionId;
            await (client as IProxy).CloseAsync();
            SessionInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(await Factory.DestroyAsync(sessionId));
        }

        [Fact]
        public async Task Async_Request_ClosedProxy_EnsureSessionClosedException()
        {
            var pipeline = CreatePipeline();
            var client = GetProxy(pipeline);

            await client.GetStateAsync();
            await (client as IProxy).CloseAsync();

            Assert.Throws<ProxyClosedException>(() => client.GetState());
        }

        [Fact]
        public void Request_ClosedProxy_EnsureSessionClosedException()
        {
            var pipeline = CreatePipeline();
            var client = GetProxy(pipeline);

            client.GetState();
            (client as IProxy).Close();

            Assert.Throws<ProxyClosedException>(() => client.GetState());
        }

        [Fact]
        public void ManySessions_EnsureStateSaved()
        {
            List<ITestContractStateFullAsync> proxies =
                Enumerable.Repeat(100, 100).Select(c => GetProxy()).ToList();

            for (int index = 0; index < proxies.Count; index++)
            {
                var proxy = proxies[index];
                proxy.SetState(index.ToString(CultureInfo.InvariantCulture));
            }

            for (int index = 0; index < proxies.Count; index++)
            {
                var proxy = proxies[index];
                Assert.Equal(index.ToString(CultureInfo.InvariantCulture), proxy.GetState());
            }

            foreach (IDisposable proxy in proxies)
            {
                proxy.Dispose();
            }
        }

        [Fact]
        public void ExecuteManyRequests_SingleChannel_EnsureOnlyOneSessionCreated()
        {
            var proxy = GetProxy(null, false);
            Task.WaitAll(Enumerable.Repeat(0, 5).Select(_ => Task.Run(() => proxy.OpenSessionAsync("test").GetAwaiter().GetResult())).ToArray());

            Assert.Equal(1, Factory.Count);
            ((IProxy)proxy).Dispose();
            Assert.Equal(0, Factory.Count);
        }

        [Fact]
        public async Task Async_ExecuteManyRequests_SingleChannel_EnsureOnlyOneSessionCreated()
        {
            var proxy = GetProxy(null, false);
            await Task.WhenAll(Enumerable.Repeat(0, 100).Select(_ => proxy.OpenSessionAsync("test")));

            Assert.Equal(1, Factory.Count);
            ((IProxy)proxy).Dispose();
            Assert.Equal(0, Factory.Count);
        }

        [Fact]
        public void CloseSession_EnsureNextRequestFails()
        {
            var pipeline = CreatePipeline();
            var client = GetProxy(pipeline);

            client.GetState();
            ((IProxy)client).Close();

            Assert.Throws<ProxyClosedException>(() => client.GetState());
        }

        [Fact]
        public async Task Async_CloseSession_EnsureNextRequestFails()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            await client.GetStateAsync();
            await ((IProxy)client).CloseAsync();

            await Assert.ThrowsAsync<ProxyClosedException>(() => client.GetStateAsync());
        }

        [Fact]
        public async Task Async_InitSession_Explicitely_EnsureInitialized()
        {
            ITestContractStateFullAsync channel = GetProxy();
            await ((IProxy)channel).OpenAsync();
            await channel.GetStateAsync();
            await (channel as IProxy).CloseAsync();
        }

        [Fact]
        public async Task Async_GetSessionId_EnsureCorrect()
        {
            var pipeline = CreatePipeline();
            var client = GetProxy(pipeline);

            var sesionId = await client.GetSessionIdAsync();
            Assert.NotNull(sesionId);

            var session = pipeline.Find<SessionMiddleware>().GetSession(client as IProxy);
            Assert.Equal(session.SessionId, sesionId);
        }

        [Fact]
        public void GetSessionId_EnsureCorrect()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            var sesionId = client.GetSessionId();

            Assert.NotNull(sesionId);
            Assert.Equal(session.GetSession(client).SessionId, sesionId);
        }

        [Fact]
        public void CloseSession_EnsureSessionIdNull()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            client.GetState();
            ((IProxy)client).Dispose();

            Assert.Null(session.GetSession(client)?.SessionId);
        }

        [Fact]
        public async Task Async_CloseSession_EnsureSessionIdNull()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetProxy(pipeline);

            await client.GetStateAsync();
            await (client as IProxy).CloseAsync();

            Assert.Null(session.GetSession(client)?.SessionId);
        }

        public virtual ITestContractStateFullAsync GetProxy(IClientPipeline pipeline = null, bool open = true)
        {
            var proxy = ClientConfiguration.ProxyFactory.CreateProxy<ITestContractStateFullAsync>(pipeline ?? CreatePipeline());
            if (open)
            {
                proxy.OpenSessionAsync("arg").GetAwaiter().GetResult();
            }

            return proxy;
        }

        protected IClientPipeline CreatePipeline(int recoveries = 0, IErrorHandling errorHandling = null)
        {
            var builder = ClientConfiguration.ProxyBuilder().Url(ServerUrl).UseSession(errorHandling: errorHandling);
            if (recoveries > 0)
            {
                builder.Recoverable(recoveries, TimeSpan.FromMilliseconds(10), errorHandling);
            }

            return builder.BuildPipeline();
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt(h =>
            {
                Factory = new MemorySessionFactory(h.Configuration.Options);
                IContractInvoker contract = h.UseSession<ITestContractStateFull, TestContractStateFull>(Factory);
                InstanceProvider = (SessionInstanceProvider)contract.InstanceProvider;
            });
        }
    }
}
