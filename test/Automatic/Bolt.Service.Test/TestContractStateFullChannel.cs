using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Service.Test.Core;
using System;

namespace Bolt.Service.Test
{
    public class TestContractStateFullChannel : RecoverableStatefullChannel<TestContractStateFullProxy, TestContractStateFullDescriptor>
    {
        public TestContractStateFullChannel(Uri server, ClientConfiguration clientConfiguration)
            : base(TestContractStateFullDescriptor.Default, server, clientConfiguration)
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
