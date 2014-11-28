using System;
using Bolt.Client;
using Bolt.Core.Serialization;
using Bolt.Server;
using Bolt.Service.Test.Core;
using Microsoft.Owin.Hosting;
using NUnit.Framework;

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
        public void EnsureStatePersistedBetweenCalls()
        {
            TestContractStateFullProxy client = GetChannel();

            client.SetState("test state");

            Assert.AreEqual("test state", client.GetState());
        }

        [Test]
        public void CloseTest()
        {
            TestContractStateFullProxy client = GetChannel();
            client.SetState("test state");
            client.Dispose();
            client.GetState();
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
                });
        }

        [TestFixtureTearDown]
        protected virtual void Destroy()
        {
            _runningServer.Dispose();
        }

    }
}
