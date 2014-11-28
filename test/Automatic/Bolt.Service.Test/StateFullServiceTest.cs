using Bolt.Client;
using Bolt.Core.Serialization;
using Bolt.Server;
using Bolt.Service.Test.Core;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Service.Test
{
    [TestFixture(SerializerType.Json)]
    [TestFixture(SerializerType.Proto)]
    public class StateFullServiceTest
    {
        private readonly SerializerType _serializerType;

        public StateFullServiceTest(SerializerType serializerType)
        {
            _serializerType = serializerType;
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

            StateFullInstanceProvider instanceProvider = (StateFullInstanceProvider)((ContractInvoker)BoltExecutor.Get(TestContractStateFullDescriptor.Default)).InstanceProvider;
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

            StateFullInstanceProvider instanceProvider = (StateFullInstanceProvider)((ContractInvoker)BoltExecutor.Get(TestContractStateFullDescriptor.Default)).InstanceProvider;
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

            StateFullInstanceProvider instanceProvider = (StateFullInstanceProvider)((ContractInvoker)BoltExecutor.Get(TestContractStateFullDescriptor.Default)).InstanceProvider;
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

            StateFullInstanceProvider instanceProvider = (StateFullInstanceProvider)((ContractInvoker)BoltExecutor.Get(TestContractStateFullDescriptor.Default)).InstanceProvider;
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
            StateFullInstanceProvider instanceProvider = (StateFullInstanceProvider)((ContractInvoker)BoltExecutor.Get(TestContractStateFullDescriptor.Default)).InstanceProvider;
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
            StateFullInstanceProvider instanceProvider = (StateFullInstanceProvider)((ContractInvoker)BoltExecutor.Get(TestContractStateFullDescriptor.Default)).InstanceProvider;
            Assert.IsFalse(instanceProvider.ReleaseInstance(session));
        }

        [Test]
        public async Task Async_Request_ClosedProxy_EnsureSessionClosedException()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            await client.GetStateAsync();
            await (client as IChannel).CloseAsync();

            Assert.Throws<SessionClosedException>(() => client.GetState());
        }

        [Test]
        public void Request_ClosedProxy_EnsureSessionClosedException()
        {
            TestContractStateFullProxy client = GetChannel();
            ((TestContractStateFullChannel)client.Channel).Retries = 0;
            client.GetState();
            (client as IChannel).Close();

            Assert.Throws<SessionClosedException>(() => client.GetState());
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

        private IDisposable _runningServer;

        public Uri ServerUrl = new Uri("http://localhost:9999");

        public ServerConfiguration ServerConfiguration { get; set; }

        public ClientConfiguration ClientConfiguration { get; set; }

        public virtual TestContractStateFullProxy GetChannel()
        {
            return
                ClientConfiguration.CreateProxy<TestContractStateFullProxy>(
                    new TestContractStateFullChannel(ServerUrl, ClientConfiguration));
        }

        public IBoltExecutor BoltExecutor { get; set; }

        [TestFixtureSetUp]
        protected virtual void Init()
        {
            ISerializer serializer = null;

            switch (_serializerType)
            {
                case SerializerType.Proto:
                    serializer = new ProtocolBufferSerializer();
                    break;
                case SerializerType.Json:
                    serializer = new JsonSerializer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            JsonExceptionSerializer jsonExceptionSerializer = new JsonExceptionSerializer();

            ServerConfiguration = new ServerConfiguration(serializer, jsonExceptionSerializer);
            ClientConfiguration = new ClientConfiguration(serializer, jsonExceptionSerializer, new DefaultWebRequestHandlerEx());

            _runningServer = WebApp.Start(
                ServerUrl.ToString(),
                (appBuilder) =>
                {
                    appBuilder.UseBolt(ServerConfiguration);
                    appBuilder.UseStateFullTestContractStateFull<TestContractStateFull>();
                    BoltExecutor = appBuilder.GetBolt();
                });
        }

        [TestFixtureTearDown]
        protected virtual void Destroy()
        {
            _runningServer.Dispose();
        }

    }
}
