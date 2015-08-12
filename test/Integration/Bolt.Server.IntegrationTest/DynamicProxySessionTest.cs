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

        public override ITestContractStateFullAsync GetProxy(IPipeline<ClientActionContext> pipeline = null, bool open = true)
        {
            ITestContractStateFullAsync proxy = ClientConfiguration.ProxyFactory.CreateProxy<ITestContractStateFullAsync>(pipeline ?? CreatePipeline());
            if (open)
            {
                proxy.OpenSessionAsync("arg").GetAwaiter().GetResult();
            }

            return proxy;
        }
    }
}