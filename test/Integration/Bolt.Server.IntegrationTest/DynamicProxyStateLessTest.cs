using Bolt.Client;
using Bolt.Client.Proxy;
using Bolt.Server.IntegrationTest.Core;

namespace Bolt.Server.IntegrationTest
{
    public class DynamicProxyStateLessTest : StateLessTest
    {
        public DynamicProxyStateLessTest()
        {
            ClientConfiguration.ProxyFactory = new DynamicProxyFactory(); 
        }

        public override ITestContractAsync CreateChannel(int retries = 0)
        {
            return ClientConfiguration.CreateProxy<ITestContractAsync>(ServerUrl);
        }
    }
}