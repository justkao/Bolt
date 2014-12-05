using Bolt.Client;
using Bolt.Helpers;
using Bolt.Server;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using Owin;
using System;

namespace Bolt.Service.Test
{

    public abstract class TestBase : SerializerTestBase
    {
        private IDisposable _runningServer;

        protected TestBase(SerializerType serializerType)
            : base(serializerType)
        {
        }


        public Uri ServerUrl = new Uri("http://localhost:9999");

        public ServerConfiguration ServerConfiguration { get; set; }

        public ClientConfiguration ClientConfiguration { get; set; }

        [TestFixtureSetUp]
        protected override void Init()
        {
            base.Init();

            JsonExceptionSerializer jsonExceptionSerializer = new JsonExceptionSerializer(Serializer);

            ServerConfiguration = new ServerConfiguration(Serializer, jsonExceptionSerializer);
            ClientConfiguration = new ClientConfiguration(Serializer, jsonExceptionSerializer, new DefaultWebRequestHandlerEx());

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
