using System;
using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Service.Test.Core;

namespace Bolt.Service.Test
{
    public class TestContractStateFullChannel : RecoverableStatefullChannel<TestContractStateFullProxy>
    {
        public TestContractStateFullChannel(Uri server, ClientConfiguration clientConfiguration)
            : base(server, clientConfiguration)
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
