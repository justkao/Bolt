using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Server.InstanceProviders;
using Bolt.Server.IntegrationTest.Core;
using Microsoft.AspNet.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class StateFullTest : IntegrationTestBase
    {
        public StateFullTest(BoltServer server)
            : base(server)
        {
        }

        [Fact]
        public async Task Async_EnsureStatePersistedBetweenCalls()
        {
            TestContractStateFullProxy client = GetChannel();

            await client.SetStateAsync("test state");
            await client.GetStateAsync();
            Assert.Equal("test state", await client.GetStateAsync());

            await (client as IChannel).CloseAsync();
        }

        [Fact]
        public async Task Async_RecoverProxy_EnsureNewSession()
        {
            TestContractStateFullProxy client = GetChannel();
            await client.GetStateAsync();
            string sessionId1 = ((TestContractStateFullChannel)client.Channel).SessionId;
            await client.NextCallWillFailProxyAsync();
            await client.GetStateAsync();
            string sessionId2 = ((TestContractStateFullChannel)client.Channel).SessionId;
            Assert.NotEqual(sessionId1, sessionId2);
        }

        [Fact]
        public async Task Async_SessionNotFound_EnsureBoltServerExceptionIsThrown()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;

            await client.GetStateAsync();
            string sessionId1 = ((TestContractStateFullChannel)client.Channel).SessionId;

            StateFullInstanceProvider instanceProvider = InstanceProvider;
            instanceProvider.ReleaseInstance(sessionId1);

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
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 1;

            await client.GetStateAsync();
            string session = ((TestContractStateFullChannel)client.Channel).SessionId;

            StateFullInstanceProvider instanceProvider = InstanceProvider;
            instanceProvider.ReleaseInstance(session);

            await client.GetStateAsync();
        }

        [Fact]
        public void EnsureStatePersistedBetweenCalls()
        {
            TestContractStateFullProxy client = GetChannel();

            client.SetState("test state");
            client.GetState();
            Assert.Equal("test state", client.GetState());

            client.Dispose();
        }

        [Fact]
        public void RecoverProxy_EnsureNewSession()
        {
            TestContractStateFullProxy client = GetChannel();
            client.GetState();
            string sessionId1 = ((TestContractStateFullChannel)client.Channel).SessionId;
            client.NextCallWillFailProxy();
            client.GetState();
            string sessionId2 = ((TestContractStateFullChannel)client.Channel).SessionId;
            Assert.NotEqual(sessionId1, sessionId2);
        }

        [Fact]
        public void SessionNotFound_EnsureBoltServerExceptionIsThrown()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;

            client.GetState();
            string sessionId1 = ((TestContractStateFullChannel)client.Channel).SessionId;

            StateFullInstanceProvider instanceProvider = InstanceProvider;
            instanceProvider.ReleaseInstance(sessionId1);

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
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 1;

            client.GetState();
            string session = ((TestContractStateFullChannel)client.Channel).SessionId;

            StateFullInstanceProvider instanceProvider = InstanceProvider;
            instanceProvider.ReleaseInstance(session);

            client.GetState();
        }

        [Fact]
        public void CloseSession_EnsureInstanceReleasedOnServer()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            client.GetState();
            string session = ((TestContractStateFullChannel)client.Channel).SessionId;
            client.Dispose();
            StateFullInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(instanceProvider.ReleaseInstance(session));
        }

        [Fact]
        public async Task Async_Request_EnsureInstanceReleasedOnServer()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            await client.GetStateAsync();
            string session = ((TestContractStateFullChannel)client.Channel).SessionId;
            await (client as IChannel).CloseAsync();
            StateFullInstanceProvider instanceProvider = InstanceProvider;
            Assert.False(instanceProvider.ReleaseInstance(session));
        }

        private StateFullInstanceProvider InstanceProvider { get; set; }

        [Fact]
        public async Task Async_Request_ClosedProxy_EnsureSessionClosedException()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            await client.GetStateAsync();
            await (client as IChannel).CloseAsync();

            Assert.Throws<ChannelClosedException>(() => client.GetState());
        }

        [Fact]
        public void Request_ClosedProxy_EnsureSessionClosedException()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            client.GetState();
            (client as IChannel).Close();

            Assert.Throws<ChannelClosedException>(() => client.GetState());
        }

        [Fact]
        public void ManySessions_EnsureStateSaved()
        {
            List<TestContractStateFullProxy> proxies =
                Enumerable.Repeat(100, 100).Select(c => GetChannel()).ToList();

            for (int index = 0; index < proxies.Count; index++)
            {
                TestContractStateFullProxy proxy = proxies[index];
                proxy.SetState(index.ToString());
            }

            for (int index = 0; index < proxies.Count; index++)
            {
                TestContractStateFullProxy proxy = proxies[index];
                Assert.Equal(index.ToString(), proxy.GetState());
            }

            foreach (TestContractStateFullProxy proxy in proxies)
            {
                proxy.Dispose();
            }
        }

        [Fact]
        public void ExecuteManyRequests_SingleChannel_EnsureOnlyOneSessionCreated()
        {
            TestContractStateFullProxy channel = GetChannel();
            int before = InstanceProvider.Count;
            Task.WaitAll(Enumerable.Repeat(0, 100).Select(_ => Task.Run(() => channel.GetState())).ToArray());
            Assert.Equal(before + 1, InstanceProvider.Count);
            channel.Dispose();
            Assert.Equal(before, InstanceProvider.Count);
        }

        [Fact]
        public async Task Async_ExecuteManyRequests_SingleChannel_EnsureOnlyOneSessionCreated()
        {
            TestContractStateFullProxy channel = GetChannel();
            int before = InstanceProvider.Count;
            await Task.WhenAll(Enumerable.Repeat(0, 100).Select(_ => channel.GetStateAsync()));
            Assert.Equal(before + 1, InstanceProvider.Count);
            await (channel as IChannel).CloseAsync();
            Assert.Equal(before, InstanceProvider.Count);
        }


        [Fact]
        public void CloseSession_EnsureNextRequestFails()
        {
            TestContractStateFullProxy channel = GetChannel();
            channel.WithRetries(0, TimeSpan.FromMilliseconds(1));
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
            TestContractStateFullProxy channel = GetChannel();
            channel.WithRetries(0, TimeSpan.FromMilliseconds(1));
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
            TestContractStateFullProxy channel = GetChannel();
            channel.Init();
            channel.GetState();
            channel.Dispose();
        }

        [Fact]
        public void InitSessionEx_EnsureInitialized()
        {
            TestContractStateFullProxy channel = GetChannel();
            TestContractStateFullChannel statefull = channel.Channel as TestContractStateFullChannel;
            statefull.ExtendedInitialization = true;

            int before = InstanceProvider.Count;

            channel.GetState();
            channel.Dispose();

            Assert.Equal(before, InstanceProvider.Count);
        }

        [Fact]
        public void InitSessionEx_Fails_EnsureSessionDestroyed()
        {
            TestContractStateFullProxy channel = GetChannel();
            TestContractStateFullChannel statefull = channel.Channel as TestContractStateFullChannel;
            statefull.ExtendedInitialization = true;
            statefull.FailExtendedInitialization = true;

            int before = InstanceProvider.Count;

            try
            {
                channel.GetState();
            }
            catch (Exception e)
            {
            }

            Assert.Equal(before, InstanceProvider.Count);
        }

        [Fact]
        public async Task Async_InitSession_Explicitely_EnsureInitialized()
        {
            TestContractStateFullProxy channel = GetChannel();
            await channel.InitAsync();
            await channel.GetStateAsync();
            await (channel as IChannel).CloseAsync();
        }



        [Fact]
        public void CloseSession_EnsureSessionIdNull()
        {
            TestContractStateFullProxy channel = GetChannel();
            channel.GetState();
            channel.Dispose();

            Assert.Null(((RecoverableStatefullChannel<TestContractStateFullProxy>)channel.Channel).SessionId);
        }

        [Fact]
        public async Task Async_CloseSession_EnsureSessionIdNull()
        {
            TestContractStateFullProxy channel = GetChannel();
            await channel.GetStateAsync();
            await (channel as ICloseable).CloseAsync();

            Assert.Null(((RecoverableStatefullChannel<TestContractStateFullProxy>)channel.Channel).SessionId);
        }

        public virtual TestContractStateFullProxy GetChannel()
        {
            return
                ClientConfiguration.CreateProxy<TestContractStateFullProxy>(
                    new TestContractStateFullChannel(ServerUrl, ClientConfiguration));
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt((h) =>
            {
                var contract = h.UseStateFullTestContractStateFull<TestContractStateFull>();
                InstanceProvider = (StateFullInstanceProvider)contract.InstanceProvider;
            });
        }
    }
}
