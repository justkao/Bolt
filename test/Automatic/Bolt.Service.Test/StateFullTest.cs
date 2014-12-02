using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Server;
using Bolt.Service.Test.Core;
using NUnit.Framework;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Service.Test
{
    public class StateFullTest : TestBase
    {
        public StateFullTest(SerializerType serializerType)
            : base(serializerType)
        {
        }

        [Test]
        public async Task Async_EnsureStatePersistedBetweenCalls()
        {
            TestContractStateFullProxy client = GetChannel();

            await client.SetStateAsync("test state");
            await client.GetStateAsync();
            Assert.AreEqual("test state", await client.GetStateAsync());

            await (client as IChannel).CloseAsync();
        }

        [Test]
        public async Task Async_RecoverProxy_EnsureNewSession()
        {
            TestContractStateFullProxy client = GetChannel();
            await client.GetStateAsync();
            string sessionId1 = ((TestContractStateFullChannel)client.Channel).SessionId;
            await client.NextCallWillFailProxyAsync();
            await client.GetStateAsync();
            string sessionId2 = ((TestContractStateFullChannel)client.Channel).SessionId;
            Assert.AreNotEqual(sessionId1, sessionId2);
        }

        [Test]
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
                Assert.AreEqual(e.Error, ServerErrorCode.SessionNotFound);
            }
        }

        [Test]
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

        [Test]
        public void EnsureStatePersistedBetweenCalls()
        {
            TestContractStateFullProxy client = GetChannel();

            client.SetState("test state");
            client.GetState();
            Assert.AreEqual("test state", client.GetState());

            client.Dispose();
        }

        [Test]
        public void RecoverProxy_EnsureNewSession()
        {
            TestContractStateFullProxy client = GetChannel();
            client.GetState();
            string sessionId1 = ((TestContractStateFullChannel)client.Channel).SessionId;
            client.NextCallWillFailProxy();
            client.GetState();
            string sessionId2 = ((TestContractStateFullChannel)client.Channel).SessionId;
            Assert.AreNotEqual(sessionId1, sessionId2);
        }

        [Test]
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
                Assert.AreEqual(e.Error, ServerErrorCode.SessionNotFound);
            }
        }

        [Test]
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

        [Test]
        public void CloseSession_EnsureInstanceReleasedOnServer()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            client.GetState();
            string session = ((TestContractStateFullChannel)client.Channel).SessionId;
            client.Dispose();
            StateFullInstanceProvider instanceProvider = InstanceProvider;
            Assert.IsFalse(instanceProvider.ReleaseInstance(session));
        }

        [Test]
        public async Task Async_Request_EnsureInstanceReleasedOnServer()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            await client.GetStateAsync();
            string session = ((TestContractStateFullChannel)client.Channel).SessionId;
            await (client as IChannel).CloseAsync();
            StateFullInstanceProvider instanceProvider = InstanceProvider;
            Assert.IsFalse(instanceProvider.ReleaseInstance(session));
        }

        private StateFullInstanceProvider InstanceProvider
        {
            get
            {
                return (StateFullInstanceProvider)((ContractInvoker)BoltExecutor.Get(TestContractStateFullDescriptor.Default)).InstanceProvider;
            }
        }

        [Test]
        public async Task Async_Request_ClosedProxy_EnsureSessionClosedException()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            await client.GetStateAsync();
            await (client as IChannel).CloseAsync();

            Assert.Throws<ChannelClosedException>(() => client.GetState());
        }

        [Test]
        public void Request_ClosedProxy_EnsureSessionClosedException()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            client.GetState();
            (client as IChannel).Close();

            Assert.Throws<ChannelClosedException>(() => client.GetState());
        }

        [Test]
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
                Assert.AreEqual(index.ToString(), proxy.GetState());
            }

            foreach (TestContractStateFullProxy proxy in proxies)
            {
                proxy.Dispose();
            }
        }

        [Test]
        public void ExecuteManyRequests_SingleChannel_EnsureOnlyOneSessionCreated()
        {
            TestContractStateFullProxy channel = GetChannel();
            int before = InstanceProvider.Count;
            Task.WaitAll(Enumerable.Repeat(0, 100).Select(_ => Task.Run(() => channel.GetState())).ToArray());
            Assert.AreEqual(before + 1, InstanceProvider.Count);
            channel.Dispose();
            Assert.AreEqual(before, InstanceProvider.Count);
        }

        [Test]
        public async Task Async_ExecuteManyRequests_SingleChannel_EnsureOnlyOneSessionCreated()
        {
            TestContractStateFullProxy channel = GetChannel();
            int before = InstanceProvider.Count;
            await Task.WhenAll(Enumerable.Repeat(0, 100).Select(_ => channel.GetStateAsync()));
            Assert.AreEqual(before + 1, InstanceProvider.Count);
            await (channel as IChannel).CloseAsync();
            Assert.AreEqual(before, InstanceProvider.Count);
        }


        [Test]
        public void CloseSession_EnsureNextRequestFails()
        {
            TestContractStateFullProxy channel = GetChannel();
            channel.WithRetries(0, TimeSpan.FromMilliseconds(1));
            channel.GetState();
            channel.Destroy();

            try
            {
                channel.GetState();
                Assert.Fail("BoltServerException was not thrown.");
            }
            catch (BoltServerException e)
            {
                Assert.AreEqual(ServerErrorCode.SessionNotFound, e.Error.Value);
            }
        }

        [Test]
        public async Task Async_CloseSession_EnsureNextRequestFails()
        {
            TestContractStateFullProxy channel = GetChannel();
            channel.WithRetries(0, TimeSpan.FromMilliseconds(1));
            await channel.GetStateAsync();
            await channel.DestroyAsync();

            try
            {
                await channel.GetStateAsync();
                Assert.Fail("BoltServerException was not thrown.");
            }
            catch (BoltServerException e)
            {
                Assert.AreEqual(ServerErrorCode.SessionNotFound, e.Error.Value);
            }
        }

        [Test]
        public void InitSession_Explicitely_EnsureInitialized()
        {
            TestContractStateFullProxy channel = GetChannel();
            channel.Init();
            channel.GetState();
            channel.Dispose();
        }

        [Test]
        public async Task Async_InitSession_Explicitely_EnsureInitialized()
        {
            TestContractStateFullProxy channel = GetChannel();
            await channel.InitAsync();
            await channel.GetStateAsync();
            await (channel as IChannel).CloseAsync();
        }

        [Test]
        public void CloseSession_EnsureSessionIdNull()
        {
            TestContractStateFullProxy channel = GetChannel();
            channel.GetState();
            channel.Dispose();

            Assert.IsNull(((RecoverableStatefullChannel<TestContractStateFullProxy>)channel.Channel).SessionId);
        }

        [Test]
        public async Task Async_CloseSession_EnsureSessionIdNull()
        {
            TestContractStateFullProxy channel = GetChannel();
            await channel.GetStateAsync();
            await (channel as ICloseable).CloseAsync();

            Assert.IsNull(((RecoverableStatefullChannel<TestContractStateFullProxy>)channel.Channel).SessionId);
        }


        public virtual TestContractStateFullProxy GetChannel()
        {
            return
                ClientConfiguration.CreateProxy<TestContractStateFullProxy>(
                    new TestContractStateFullChannel(ServerUrl, ClientConfiguration));
        }

        public IBoltExecutor BoltExecutor { get; set; }

        protected override void ConfigureDefaultServer(IAppBuilder appBuilder)
        {
            appBuilder.UseBolt(ServerConfiguration);
            appBuilder.UseStateFullTestContractStateFull<TestContractStateFull>();
            BoltExecutor = appBuilder.GetBolt();
        }
    }
}
