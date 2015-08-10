using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Server.InstanceProviders;
using Bolt.Server.IntegrationTest.Core;
using Microsoft.AspNet.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Server.Session;
using Bolt.Session;
using Bolt.Test.Common;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class SessionTest : IntegrationTestBase, ITestState
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

            await (client as IChannel).CloseAsync();
        }

        [Fact]
        public async Task Async_RecoverProxy_EnsureNewSession()
        {
            var client = GetChannel();
            await client.GetStateAsync();
            string sessionId1 = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;
            await client.NextCallWillFailProxyAsync();
            await client.GetStateAsync();
            string sessionId2 = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;
            Assert.NotEqual(sessionId1, sessionId2);
        }

        [Fact]
        public async Task Async_SessionNotFound_EnsureBoltServerExceptionIsThrown()
        {
            var client = GetChannel();
            ((TestContractSessionChannel)GetInnerChannel(client)).Retries = 0;

            await client.GetStateAsync();
            string sessionId1 = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;
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
            var client = GetChannel();
            ((TestContractSessionChannel)GetInnerChannel(client)).Retries = 1;

            await client.GetStateAsync();
            string session = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;

            SessionInstanceProvider instanceProvider = InstanceProvider;
            Factory.Destroy(session);

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
            var client = GetChannel();
            client.GetState();
            string sessionId1 = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;
            client.NextCallWillFailProxy();
            client.GetState();
            string sessionId2 = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;
            Assert.NotEqual(sessionId1, sessionId2);
        }

        [Fact]
        public void SessionNotFound_EnsureBoltServerExceptionIsThrown()
        {
            var client = GetChannel();
            ((TestContractSessionChannel)GetInnerChannel(client)).Retries = 0;

            client.GetState();
            string sessionId1 = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;
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
            var client = GetChannel();
            ((TestContractSessionChannel)GetInnerChannel(client)).Retries = 1;

            client.GetState();
            string session = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;

            SessionInstanceProvider instanceProvider = InstanceProvider;
            Factory.Destroy(session);

            client.GetState();
        }

        [Fact]
        public void CloseSession_EnsureInstanceReleasedOnServer()
        {
            var client = GetChannel();
            ((TestContractSessionChannel)GetInnerChannel(client)).Retries = 0;
            client.GetState();
            string session = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;
            (client as IDisposable).Dispose();
            SessionInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(Factory.Destroy(session));
        }

        [Fact]
        public async Task Async_Request_EnsureInstanceReleasedOnServer()
        {
            var client = GetChannel();
            ((TestContractSessionChannel)GetInnerChannel(client)).Retries = 0;
            await client.GetStateAsync();
            string session = ((TestContractSessionChannel)GetInnerChannel(client)).SessionId;
            await (client as IChannel).CloseAsync();
            SessionInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(Factory.Destroy(session));
        }

        [Fact]
        public async Task Async_Request_ClosedProxy_EnsureSessionClosedException()
        {
            var client = GetChannel();
            ((TestContractSessionChannel)GetInnerChannel(client)).Retries = 0;
            await client.GetStateAsync();
            await (client as IChannel).CloseAsync();

            Assert.Throws<ChannelClosedException>(() => client.GetState());
        }

        [Fact]
        public void Request_ClosedProxy_EnsureSessionClosedException()
        {
            var client = GetChannel();
            ((TestContractSessionChannel)GetInnerChannel(client)).Retries = 0;
            client.GetState();
            (client as IChannel).Close();

            Assert.Throws<ChannelClosedException>(() => client.GetState());
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
            ((IChannel) channel).Dispose();
            Assert.Equal(before, Factory.Count);
        }

        [Fact]
        public async Task Async_ExecuteManyRequests_SingleChannel_EnsureOnlyOneSessionCreated()
        {
            var channel = GetChannel();
            int before = Factory.Count;
            await Task.WhenAll(Enumerable.Repeat(0, 100).Select(_ => channel.GetStateAsync()));
            Assert.Equal(before + 1, Factory.Count);
            await (channel as IChannel).CloseAsync();
            Assert.Equal(before, Factory.Count);
        }


        [Fact]
        public void CloseSession_EnsureNextRequestFails()
        {
            var channel = GetChannel();
            (channel as ContractProxy).WithRetries(0, TimeSpan.FromMilliseconds(1));
            channel.GetState();
            ((IChannel) channel).Close();

            Assert.Throws<ChannelClosedException>(() => channel.GetState());
        }

        [Fact]
        public async Task Async_CloseSession_EnsureNextRequestFails()
        {
            var channel = GetChannel();
            (channel as ContractProxy).WithRetries(0, TimeSpan.FromMilliseconds(1));
            await channel.GetStateAsync();
            await ((IChannel)channel).CloseAsync();

            await Assert.ThrowsAsync<ChannelClosedException>(() => channel.GetStateAsync());
        }

        [Fact]
        public async Task OpenSession_EnsureCallbackCalled()
        {
            ITestContractStateFullAsync channel = GetChannel();
            SessionChannel recoverable = GetInnerChannel(channel);

            recoverable.InitSessionParameters = new InitSessionParameters();
            recoverable.InitSessionParameters.UserData["temp"] = "temp";

            SessionCallback = new Mock<ISessionCallback>();
            SessionCallback.Setup(c => c.InitSessionAsync(It.IsAny<InitSessionParameters>(), It.IsAny<ActionContextBase>(), It.IsAny<CancellationToken>()))
                .Returns(
                    () => Task.FromResult(new InitSessionResult())).Verifiable();


            (channel as ContractProxy).WithRetries(0, TimeSpan.FromMilliseconds(1));
            await channel.GetStateAsync();

            SessionCallback.Verify();
        }

        [Fact]
        public async Task CloseSession_EnsureCallbackCalled()
        {
            ITestContractStateFullAsync channel = GetChannel();
            SessionChannel recoverable = GetInnerChannel(channel);


            SessionCallback = new Mock<ISessionCallback>();
            SessionCallback.Setup(c => c.DestroySessionAsync(It.IsAny<DestroySessionParameters>(), It.IsAny<ActionContextBase>(), It.IsAny<CancellationToken>()))
                .Returns(
                    () => Task.FromResult(new DestroySessionResult())).Verifiable();


            (channel as ContractProxy).WithRetries(0, TimeSpan.FromMilliseconds(1));
            await channel.GetStateAsync();
            await recoverable.CloseAsync();

            SessionCallback.Verify();
        }

        [Fact]
        public async Task OpenSession_EnsureProperResult()
        {
            ITestContractStateFullAsync channel = GetChannel();
            SessionChannel recoverable = GetInnerChannel(channel);

            recoverable.InitSessionParameters = new InitSessionParameters();
            recoverable.InitSessionParameters.UserData["temp"] = "temp";

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


            (channel as ContractProxy).WithRetries(0, TimeSpan.FromMilliseconds(1));
            await channel.GetStateAsync();
            SessionCallback.Verify();

            Assert.Equal("temp", recoverable.InitSessionResult.UserData["temp"]);
        }

        [Fact]
        public async Task OpenSession_WriteCustomValues_EnsureProperResult()
        {
            ITestContractStateFullAsync channel = GetChannel();
            SessionChannel recoverable = GetInnerChannel(channel);
            CompositeType obj = CompositeType.CreateRandom();
            recoverable.ConfigureSession((ctxt) =>
            {
                ctxt.Write("composite", obj);
            });

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


            (channel as ContractProxy).WithRetries(0, TimeSpan.FromMilliseconds(1));
            await channel.GetStateAsync();
            SessionCallback.Verify();

            Assert.Equal(obj, recoverable.InitSessionResult.Read<CompositeType>(recoverable.Serializer, "composite"));
        }

        [Fact]
        public async Task CloseSession_EnsureProperResult()
        {
            ITestContractStateFullAsync channel = GetChannel();
            SessionChannel recoverable = GetInnerChannel(channel);

            recoverable.DestroySessionParameters = new DestroySessionParameters();
            recoverable.DestroySessionParameters.UserData["temp"] = "temp";

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


            (channel as ContractProxy).WithRetries(0, TimeSpan.FromMilliseconds(1));
            await channel.GetStateAsync();
            await recoverable.CloseAsync();

            SessionCallback.Verify();

            Assert.Equal("temp", recoverable.DestroySessionResult.UserData["temp"]);
        }

        [Fact]
        public async Task Async_InitSession_Explicitely_EnsureInitialized()
        {
            ITestContractStateFullAsync channel = GetChannel();
            await ((IChannel) channel).OpenAsync();
            await channel.GetStateAsync();
            await (channel as IChannel).CloseAsync();
        }

        [Fact]
        public async Task Async_GetSessionId_EnsureCorrect()
        {
            var channel = GetChannel();
            var sesionId = await channel.GetSessionIdAsync();
            Assert.NotNull(sesionId);

            Assert.Equal(((ISessionProvider)GetInnerChannel(channel)).SessionId, sesionId);
        }

        [Fact]
        public void GetSessionId_EnsureCorrect()
        {
            var channel = GetChannel();
            var sesionId = channel.GetSessionId();
            Assert.NotNull(sesionId);

            Assert.Equal(((ISessionProvider)GetInnerChannel(channel)).SessionId, sesionId);
        }

        [Fact]
        public void CloseSession_EnsureSessionIdNull()
        {
            var proxy = GetChannel();
            proxy.GetState();
            ((IChannel) proxy).Dispose();

            Assert.Null(((SessionChannel)(((ContractProxy)proxy).Channel)).SessionId);
        }

        [Fact]
        public async Task Async_CloseSession_EnsureSessionIdNull()
        {
            var channel = GetChannel();
            await channel.GetStateAsync();
            await (channel as ICloseable).CloseAsync();

            Assert.Null(((SessionChannel)GetInnerChannel(channel)).SessionId);
        }

        public virtual ITestContractStateFullAsync GetChannel()
        {
            return
                ClientConfiguration.CreateProxy<TestContractStateFullProxy>(
                    new TestContractSessionChannel(ServerUrl, ClientConfiguration));
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddInstance<ITestState>(this);

            base.ConfigureServices(services);
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

        private SessionChannel GetInnerChannel(object proxy)
        {
            return (SessionChannel) ((IChannelProvider) proxy).Channel;
        }

        public Mock<ISessionCallback> SessionCallback { get; set; }
    }
}
