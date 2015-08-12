using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Client;
using Bolt.Client.Pipeline;
using Bolt.Pipeline;
using Bolt.Server.InstanceProviders;
using Bolt.Server.IntegrationTest.Core;
using Bolt.Server.Session;
using Bolt.Test.Common;

using Microsoft.AspNet.Builder;

using Moq;

using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class SessionTest : IntegrationTestBase
    {
        private SessionInstanceProvider InstanceProvider { get; set; }

        private MemorySessionFactory Factory { get; set; }

        [Fact]
        public async Task Async_EnsureStatePersistedBetweenCalls()
        {
            var client = GetChannel();

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
            var client = GetChannel(pipeline);

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
            var client = GetChannel(pipeline);

            await client.GetStateAsync();
            string sessionId1 = session.GetSession(client).SessionId;
            Factory.Destroy(sessionId1);

            try
            {
                await client.GetStateAsync();
            }
            catch (BoltServerException e)
            {
                Assert.Equal(e.Error, ServerErrorCode.SessionNotFound);
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
            var client = GetChannel(pipeline);

            await client.GetStateAsync();
            string sessionId = session.GetSession(client).SessionId;

            SessionInstanceProvider instanceProvider = InstanceProvider;
            Factory.Destroy(sessionId);

            await client.GetStateAsync();
        }

        [Fact]
        public void EnsureStatePersistedBetweenCalls()
        {
            var client = GetChannel();

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
            var client = GetChannel(pipeline);

            client.GetState();
            string sessionId1 = session.GetSession(client).SessionId;
            client.NextCallWillFailProxy();
            client.GetState();
            string sessionId2 = session.GetSession(client).SessionId;
            Assert.NotEqual(sessionId1, sessionId2);
        }

        [Fact]
        public void SessionNotFound_EnsureBoltServerExceptionIsThrown()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            client.GetState();
            string sessionId1 = session.GetSession(client).SessionId;
            Factory.Destroy(sessionId1);

            try
            {
                client.GetState();
            }
            catch (BoltServerException e)
            {
                Assert.Equal(e.Error, ServerErrorCode.SessionNotFound);
            }
        }

        [Fact]
        public void SessionNotFound_RetriesEnabled_EnsureNewSession()
        {
            var pipeline = CreatePipeline(1);
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            client.GetState();
            string sessionId = session.GetSession(client).SessionId;

            SessionInstanceProvider instanceProvider = InstanceProvider;
            Factory.Destroy(sessionId);

            client.GetState();
        }

        [Fact]
        public void CloseSession_EnsureInstanceReleasedOnServer()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            client.GetState();
            string sessionId = session.GetSession(client).SessionId;
            (client as IDisposable).Dispose();
            SessionInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(Factory.Destroy(sessionId));
        }

        [Fact]
        public async Task Async_Request_EnsureInstanceReleasedOnServer()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            await client.GetStateAsync();
            string sessionId = session.GetSession(client).SessionId;
            await (client as IProxy).CloseAsync();
            SessionInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(Factory.Destroy(sessionId));
        }

        [Fact]
        public async Task Async_Request_ClosedProxy_EnsureSessionClosedException()
        {
            var pipeline = CreatePipeline();
            var client = GetChannel(pipeline);

            await client.GetStateAsync();
            await (client as IProxy).CloseAsync();

            Assert.Throws<ProxyClosedException>(() => client.GetState());
        }

        [Fact]
        public void Request_ClosedProxy_EnsureSessionClosedException()
        {
            var pipeline = CreatePipeline();
            var client = GetChannel(pipeline);

            client.GetState();
            (client as IProxy).Close();

            Assert.Throws<ProxyClosedException>(() => client.GetState());
        }

        [Fact]
        public void ManySessions_EnsureStateSaved()
        {
            List<ITestContractStateFullAsync> proxies =
                Enumerable.Repeat(100, 100).Select(c => GetChannel()).ToList();

            for (int index = 0; index < proxies.Count; index++)
            {
                var proxy = proxies[index];
                proxy.SetState(index.ToString());
            }

            for (int index = 0; index < proxies.Count; index++)
            {
                var proxy = proxies[index];
                Assert.Equal(index.ToString(), proxy.GetState());
            }

            foreach (IDisposable proxy in proxies)
            {
                proxy.Dispose();
            }
        }

        [Fact]
        public void ExecuteManyRequests_SingleChannel_EnsureOnlyOneSessionCreated()
        {
            var channel = GetChannel();
            int before = Factory.Count;
            Task.WaitAll(Enumerable.Repeat(0, 5).Select(_ => Task.Run(() => channel.GetState())).ToArray());
            Assert.Equal(before + 1, Factory.Count);
            ((IProxy) channel).Dispose();
            Assert.Equal(before, Factory.Count);
        }

        [Fact]
        public async Task Async_ExecuteManyRequests_SingleChannel_EnsureOnlyOneSessionCreated()
        {
            var channel = GetChannel();
            int before = Factory.Count;
            await Task.WhenAll(Enumerable.Repeat(0, 100).Select(_ => channel.GetStateAsync()));
            Assert.Equal(before + 1, Factory.Count);
            await (channel as IProxy).CloseAsync();
            Assert.Equal(before, Factory.Count);
        }


        [Fact]
        public void CloseSession_EnsureNextRequestFails()
        {
            var pipeline = CreatePipeline();
            var client = GetChannel(pipeline);

            client.GetState();
            ((IProxy)client).Close();

            Assert.Throws<ProxyClosedException>(() => client.GetState());
        }

        [Fact]
        public async Task Async_CloseSession_EnsureNextRequestFails()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            await client.GetStateAsync();
            await ((IProxy)client).CloseAsync();

            await Assert.ThrowsAsync<ProxyClosedException>(() => client.GetStateAsync());
        }

        /*
        [Fact]
        public async Task OpenSession_EnsureCallbackCalled()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            session.InitSessionParameters = new InitSessionParameters();
            session.InitSessionParameters.UserData["temp"] = "temp";

            SessionCallback = new Mock<ISessionCallback>();
            SessionCallback.Setup(c => c.InitSessionAsync(It.IsAny<InitSessionParameters>(), It.IsAny<ActionContextBase>(), It.IsAny<CancellationToken>()))
                .Returns(
                    () => Task.FromResult(new InitSessionResult())).Verifiable();

            await client.GetStateAsync();

            SessionCallback.Verify();
        }

        [Fact]
        public async Task CloseSession_EnsureCallbackCalled()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            SessionCallback = new Mock<ISessionCallback>();
            SessionCallback.Setup(c => c.DestroySessionAsync(It.IsAny<DestroySessionParameters>(), It.IsAny<ActionContextBase>(), It.IsAny<CancellationToken>()))
                .Returns(
                    () => Task.FromResult(new DestroySessionResult())).Verifiable();


            await client.GetStateAsync();
            await (client as IProxy).CloseAsync();

            SessionCallback.Verify();
        }

        [Fact]
        public async Task OpenSession_EnsureProperResult()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            session.InitSessionParameters = new InitSessionParameters();
            session.InitSessionParameters.UserData["temp"] = "temp";

            SessionCallback = new Mock<ISessionCallback>();
            SessionCallback.Setup(
                c =>
                    c.InitSessionAsync(It.IsAny<InitSessionParameters>(), It.IsAny<ActionContextBase>(),
                        It.IsAny<CancellationToken>()))
                .Returns<InitSessionParameters, ActionContextBase, CancellationToken>(
                    (p, ctxt, c) =>
                    {
                        Assert.NotNull(ctxt);
                        InitSessionResult result = new InitSessionResult();
                        result.UserData["temp"] = p.UserData["temp"];
                        return Task.FromResult(result);
                    }).Verifiable();


            await client.GetStateAsync();
            SessionCallback.Verify();

            Assert.Equal("temp", session.InitSessionResult.UserData["temp"]);
        }

        [Fact]
        public async Task OpenSession_WriteCustomValues_EnsureProperResult()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            session.InitSessionParameters = new InitSessionParameters(); 
            var client = GetChannel(pipeline);

            CompositeType obj = CompositeType.CreateRandom();
            ConfigureSessionContext context= new ConfigureSessionContext(ClientConfiguration.Serializer, session.InitSessionParameters);

            context.Write("composite", obj);

            SessionCallback = new Mock<ISessionCallback>();
            SessionCallback.Setup(
                c =>
                    c.InitSessionAsync(It.IsAny<InitSessionParameters>(), It.IsAny<ActionContextBase>(),
                        It.IsAny<CancellationToken>()))
                .Returns<InitSessionParameters, ActionContextBase, CancellationToken>(
                    (p, ctxt, c) =>
                    {
                        CompositeType res = p.Read<CompositeType>(ctxt, "composite");
                        Assert.NotNull(res);
                        Assert.Equal(obj, res);
                        InitSessionResult result = new InitSessionResult();
                        result.Write(ctxt, "composite", res);
                        return Task.FromResult(result);
                    }).Verifiable();


            await client.GetStateAsync();
            SessionCallback.Verify();

            Assert.Equal(obj, session.InitSessionResult.Read<CompositeType>(ClientConfiguration.Serializer, "composite"));
        }

        [Fact]
        public async Task CloseSession_EnsureProperResult()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            session.DestroySessionParameters = new DestroySessionParameters();
            session.DestroySessionParameters.UserData["temp"] = "temp";

            SessionCallback = new Mock<ISessionCallback>();
            SessionCallback.Setup(
                c =>
                    c.DestroySessionAsync(It.IsAny<DestroySessionParameters>(), It.IsAny<ActionContextBase>(),
                        It.IsAny<CancellationToken>()))
                .Returns<DestroySessionParameters, ActionContextBase, CancellationToken>(
                    (p, ctxt, c) =>
                    {
                        Assert.NotNull(ctxt);
                        DestroySessionResult result = new DestroySessionResult();
                        result.UserData["temp"] = p.UserData["temp"];
                        return Task.FromResult(result);
                    }).Verifiable();


            await client.GetStateAsync();
            await (client as IProxy).CloseAsync();

            SessionCallback.Verify();

            Assert.Equal("temp", session.DestroySessionResult.UserData["temp"]);
        }
        */

        [Fact]
        public async Task Async_InitSession_Explicitely_EnsureInitialized()
        {
            ITestContractStateFullAsync channel = GetChannel();
            await ((IProxy) channel).OpenAsync();
            await channel.GetStateAsync();
            await (channel as IProxy).CloseAsync();
        }

        [Fact]
        public async Task Async_GetSessionId_EnsureCorrect()
        {
            var pipeline = CreatePipeline();
            var client = GetChannel(pipeline);

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
            var client = GetChannel(pipeline);

            var sesionId = client.GetSessionId();

            Assert.NotNull(sesionId);
            Assert.Equal(session.GetSession(client).SessionId, sesionId);
        }

        [Fact]
        public void CloseSession_EnsureSessionIdNull()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            client.GetState();
            ((IProxy)client).Dispose();

            Assert.Null(session.GetSession(client)?.SessionId);
        }

        [Fact]
        public async Task Async_CloseSession_EnsureSessionIdNull()
        {
            var pipeline = CreatePipeline();
            var session = pipeline.Find<SessionMiddleware>();
            var client = GetChannel(pipeline);

            await client.GetStateAsync();
            await (client as IProxy).CloseAsync();

            Assert.Null(session.GetSession(client)?.SessionId);
        }

        public virtual ITestContractStateFullAsync GetChannel(IPipeline<ClientActionContext> pipeline = null)
        {
            return new TestContractStateFullProxy(pipeline ?? CreatePipeline());
        }

        protected IPipeline<ClientActionContext> CreatePipeline(int recoveries = 0, IErrorHandling errorHandling = null)
        {
            var builder = ClientConfiguration.ProxyBuilder().Url(ServerUrl).UseSession(errorHandling: errorHandling);
            if (recoveries > 0)
            {
                builder.Recoverable(recoveries, TimeSpan.FromMilliseconds(10), errorHandling);
            }

            return builder.BuildPipeline<ITestContractStateFull>();
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt((h) =>
            {
                Factory = new MemorySessionFactory(h.Configuration.Options);
                IContractInvoker contract = h.UseSession<ITestContractStateFull, TestContractStateFull>(Factory);
                InstanceProvider = (SessionInstanceProvider)contract.InstanceProvider;
            });
        }
    }
}
