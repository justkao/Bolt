using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Server.InstanceProviders;
using Bolt.Server.IntegrationTest.Core;
using Microsoft.AspNet.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Core;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class StateFullTest : IntegrationTestBase
    {
        private StateFullInstanceProvider InstanceProvider { get; set; }

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
            string sessionId1 = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;
            await client.NextCallWillFailProxyAsync();
            await client.GetStateAsync();
            string sessionId2 = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;
            Assert.NotEqual(sessionId1, sessionId2);
        }

        [Fact]
        public async Task Async_SessionNotFound_EnsureBoltServerExceptionIsThrown()
        {
            var client = GetChannel();
            ((TestContractStateFullChannel)GetInnerChannel(client)).Retries = 0;

            await client.GetStateAsync();
            string sessionId1 = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;
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
            ((TestContractStateFullChannel)GetInnerChannel(client)).Retries = 1;

            await client.GetStateAsync();
            string session = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;

            StateFullInstanceProvider instanceProvider = InstanceProvider;
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
            string sessionId1 = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;
            client.NextCallWillFailProxy();
            client.GetState();
            string sessionId2 = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;
            Assert.NotEqual(sessionId1, sessionId2);
        }

        [Fact]
        public void SessionNotFound_EnsureBoltServerExceptionIsThrown()
        {
            var client = GetChannel();
            ((TestContractStateFullChannel)GetInnerChannel(client)).Retries = 0;

            client.GetState();
            string sessionId1 = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;
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
            ((TestContractStateFullChannel)GetInnerChannel(client)).Retries = 1;

            client.GetState();
            string session = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;

            StateFullInstanceProvider instanceProvider = InstanceProvider;
            Factory.Destroy(session);

            client.GetState();
        }

        [Fact]
        public void CloseSession_EnsureInstanceReleasedOnServer()
        {
            var client = GetChannel();
            ((TestContractStateFullChannel)GetInnerChannel(client)).Retries = 0;
            client.GetState();
            string session = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;
            (client as IDisposable).Dispose();
            StateFullInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(Factory.Destroy(session));
        }

        [Fact]
        public async Task Async_Request_EnsureInstanceReleasedOnServer()
        {
            var client = GetChannel();
            ((TestContractStateFullChannel)GetInnerChannel(client)).Retries = 0;
            await client.GetStateAsync();
            string session = ((TestContractStateFullChannel)GetInnerChannel(client)).SessionId;
            await (client as IChannel).CloseAsync();
            StateFullInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(Factory.Destroy(session));
        }

        [Fact]
        public async Task Async_Request_ClosedProxy_EnsureSessionClosedException()
        {
            var client = GetChannel();
            ((TestContractStateFullChannel)GetInnerChannel(client)).Retries = 0;
            await client.GetStateAsync();
            await (client as IChannel).CloseAsync();

            Assert.Throws<ChannelClosedException>(() => client.GetState());
        }

        [Fact]
        public void Request_ClosedProxy_EnsureSessionClosedException()
        {
            var client = GetChannel();
            ((TestContractStateFullChannel)GetInnerChannel(client)).Retries = 0;
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
            channel.Destroy();

            try
            {
                channel.GetState();
                Assert.True(false, "BoltServerException was not thrown.");
            }
            catch (BoltServerException e)
            {
                Assert.Equal(ServerErrorCode.SessionNotFound, e.Error.Value);
            }
        }

        [Fact]
        public async Task Async_CloseSession_EnsureNextRequestFails()
        {
            var channel = GetChannel();
            (channel as ContractProxy).WithRetries(0, TimeSpan.FromMilliseconds(1));
            await channel.GetStateAsync();
            await channel.DestroyAsync();

            try
            {
                await channel.GetStateAsync();
                Assert.True(false, "BoltServerException was not thrown.");
            }
            catch (BoltServerException e)
            {
                Assert.Equal(ServerErrorCode.SessionNotFound, e.Error.Value);
            }
        }

        [Fact]
        public void InitSession_Explicitely_EnsureInitialized()
        {
            var channel = GetChannel();
            channel.Init();
            channel.GetState();
            ((IChannel) channel).Dispose();
        }

        [Fact]
        public void InitSessionEx_EnsureInitialized()
        {
            var channel = GetChannel();
            TestContractStateFullChannel statefull = GetInnerChannel(channel) as TestContractStateFullChannel;
            statefull.ExtendedInitialization = true;

            int before = Factory.Count;

            channel.GetState();
            (channel as IChannel).Dispose();

            Assert.Equal(before, Factory.Count);
        }

        [Fact]
        public void InitSessionEx_Fails_EnsureSessionDestroyed()
        {
            var channel = GetChannel();
            TestContractStateFullChannel statefull = GetInnerChannel(channel) as TestContractStateFullChannel;
            statefull.ExtendedInitialization = true;
            statefull.FailExtendedInitialization = true;

            int before = Factory.Count;

            try
            {
                channel.GetState();
            }
            catch (Exception)
            {
            }

            Assert.Equal(before, Factory.Count);
        }

        [Fact]
        public async Task Async_InitSession_Explicitely_EnsureInitialized()
        {
            var channel = GetChannel();
            await channel.InitAsync();
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

            Assert.Null(((RecoverableStatefullChannel<TestContractStateFullProxy>)(((ContractProxy)proxy).Channel)).SessionId);
        }

        [Fact]
        public async Task Async_CloseSession_EnsureSessionIdNull()
        {
            var channel = GetChannel();
            await channel.GetStateAsync();
            await (channel as ICloseable).CloseAsync();

            Assert.Null(((RecoverableStatefullChannel<TestContractStateFullProxy>)GetInnerChannel(channel)).SessionId);
        }

        public virtual ITestContractStateFullAsync GetChannel()
        {
            return
                ClientConfiguration.CreateProxy<TestContractStateFullProxy>(
                    new TestContractStateFullChannel(ServerUrl, ClientConfiguration));
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt((h) =>
            {
                Factory = new MemorySessionFactory(h.Configuration.Options);
                var contract = h.UseStateFull<ITestContractStateFull, TestContractStateFull>(Factory);
                InstanceProvider = (StateFullInstanceProvider)contract.InstanceProvider;
            });
        }

        private IChannel GetInnerChannel(object proxy)
        {
            return ((ContractProxy) proxy).Channel;
        }
    }
}
