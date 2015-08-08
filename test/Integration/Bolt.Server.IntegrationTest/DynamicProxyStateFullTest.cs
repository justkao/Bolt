using Bolt.Client;
using Bolt.Client.Proxy;
using Bolt.Server.IntegrationTest.Core;

namespace Bolt.Server.IntegrationTest
{
    public class DynamicProxyStateFullTest: StateFullTest
    {
        public DynamicProxyStateFullTest()
        {
            ClientConfiguration.ProxyFactory = new DynamicProxyFactory();
        }

        public override ITestContractStateFullAsync GetChannel()
        {
            return ClientConfiguration.CreateProxy<ITestContractStateFullAsync>(new TestContractStateFullChannel(ServerUrl, ClientConfiguration));
        }
    }
}