using Bolt.Client;
using Bolt.Core.Serialization;
using Bolt.Server;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using System;

namespace Bolt.Service.Test
{
    [TestFixture(SerializerType.Json)]
    [TestFixture(SerializerType.Proto)]
    [TestFixture(SerializerType.Xml)]
    public abstract class TestBase
    {
        private readonly SerializerType _serializerType;
        private IDisposable _runningServer;

        protected TestBase(SerializerType serializerType)
        {
            _serializerType = serializerType;
        }

        public Uri ServerUrl = new Uri("http://localhost:9999");

        public ServerConfiguration ServerConfiguration { get; set; }

        public ClientConfiguration ClientConfiguration { get; set; }

        [TestFixtureSetUp]
        protected virtual void Init()
        {
            ISerializer serializer;

            switch (_serializerType)
            {
                case SerializerType.Proto:
                    serializer = new ProtocolBufferSerializer();
                    break;
                case SerializerType.Json:
                    serializer = new JsonSerializer();
                    break;
                case SerializerType.Xml:
                    serializer = new XmlSerializer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            JsonExceptionSerializer jsonExceptionSerializer = new JsonExceptionSerializer();

            ServerConfiguration = new ServerConfiguration(serializer, jsonExceptionSerializer);
            ClientConfiguration = new ClientConfiguration(serializer, jsonExceptionSerializer, new DefaultWebRequestHandlerEx());

            _runningServer = StartServer(ServerUrl, ConfigureDefaultServer);
        }

        protected abstract void ConfigureDefaultServer(IAppBuilder appBuilder);

        protected IDisposable StartServer(Uri server, Action<IAppBuilder> configure)
        {
            return WebApp.Start(server.ToString(), configure);
        }

        [TestFixtureTearDown]
        protected virtual void Destroy()
        {
            _runningServer.Dispose();
        }
    }
}
