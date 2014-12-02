using Bolt.Client;
using NUnit.Framework;
using System;

namespace Bolt.Service.Test
{
    [TestFixture]
    public class MultipleServersProviderTest
    {
        public Uri Server1 { get; set; }
        public Uri Server2 { get; set; }
        public Uri Server3 { get; set; }

        public IServerProvider Provider { get; set; }

        [Test]
        public void GetServer_EnsureNotNull()
        {
            Assert.IsNotNull(Provider.GetServer());
        }

        [Test]
        public void GetServer_MultipleTimes_SameValue()
        {
            Uri initial = Provider.GetServer();
            Assert.AreEqual(initial, Provider.GetServer());
        }

        [Test]
        public void GetServer_ThenServerUnavailable_EnsureNewServer()
        {
            Uri initial = Provider.GetServer();
            Provider.OnServerUnavailable(initial);

            Assert.AreNotEqual(initial, Provider.GetServer());
        }

        [SetUp]
        public void BeforeTest()
        {
            Server1 = new Uri("http://localhost:5001");
            Server2 = new Uri("http://localhost:5002");
            Server3 = new Uri("http://localhost:5003");

            Provider = new MultipleServersProvider(Server1, Server2, Server3);
        }
    }
}
