using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Service.Test.Core;

namespace Bolt.Service.Test
{
    public class TestContractStateFullChannel : RecoverableStatefullChannel<TestContractStateFullProxy, TestContractStateFullDescriptor>
    {
        public TestContractStateFullChannel(IServerProvider serverProvider, string sessionHeaderName, IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
            : base(TestContractStateFullDescriptor.Default, serverProvider, sessionHeaderName, requestForwarder, endpointProvider)
        {
        }

        protected override void OnProxyOpening(TestContractStateFullProxy contract)
        {
            contract.Init();
        }

        protected override void OnProxyClosing(TestContractStateFullProxy contract)
        {
            contract.Destroy();
        }
    }
}
