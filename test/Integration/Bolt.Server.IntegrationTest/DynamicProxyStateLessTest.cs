using Bolt.Client;
using Bolt.Client.Proxy;
using Bolt.Pipeline;
using Bolt.Server.IntegrationTest.Core;

namespace Bolt.Server.IntegrationTest
{
    public class DynamicProxyStateLessTest : StateLessTest
    {
        public DynamicProxyStateLessTest()
        {
            ClientConfiguration.ProxyFactory = new DynamicProxyFactory(); 
        }

        public override ITestContractAsync CreateChannel(IPipeline<ClientActionContext> pipeline = null)
        {
            return ClientConfiguration.CreateProxy<ITestContractAsync>(ServerUrl);
        }
    }
}