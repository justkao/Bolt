using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Service.Test.Core;
using System;
using System.Threading.Tasks;

namespace Bolt.Service.Test
{
    public class TestContractStateFullChannel : RecoverableStatefullChannel<TestContractStateFullProxy>
    {
        public TestContractStateFullChannel(Uri server, ClientConfiguration clientConfiguration)
            : base(server, clientConfiguration)
        {
            Retries = 1;
        }

        public override bool IsRecoverable
        {
            get { return true; }
        }

        protected override bool HandleError(ClientActionContext context, Exception error)
        {
            if (error is TestContractProxyFailedException)
            {
                CloseConnection();
                return true;
            }

            return base.HandleError(context, error);
        }

        protected override Task OnProxyClosingAsync(TestContractStateFullProxy contract)
        {
            return contract.DestroyAsync();
        }

        protected override Task OnProxyOpeningAsync(TestContractStateFullProxy contract)
        {
            return contract.InitAsync();
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
