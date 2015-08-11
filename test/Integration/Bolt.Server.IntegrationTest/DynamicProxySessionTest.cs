using Bolt.Client;
using Bolt.Client.Proxy;
using Bolt.Pipeline;
using Bolt.Server.IntegrationTest.Core;

namespace Bolt.Server.IntegrationTest
{
    public class DynamicProxySessionTest: SessionTest
    {
        public DynamicProxySessionTest()
        {
            ClientConfiguration.ProxyFactory = new DynamicProxyFactory();
        }

        public override ITestContractStateFullAsync GetChannel(IPipeline<ClientActionContext> pipeline = null)
        {
            return ClientConfiguration.ProxyFactory.CreateProxy<ITestContractStateFullAsync>(pipeline ?? CreatePipeline());
        }
    }
}