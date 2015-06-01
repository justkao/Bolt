using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Server.IntegrationTest.Core;
using System;
using System.Threading.Tasks;

namespace Bolt.Server.IntegrationTest
{
    public class TestContractStateFullChannel : RecoverableStatefullChannel<TestContractStateFullProxy>
    {
        public TestContractStateFullChannel(Uri server, ClientConfiguration clientConfiguration)
            : base(server, clientConfiguration)
        {
            Retries = 1;
        }

        public bool ExtendedInitialization { get; set; }

        public bool FailExtendedInitialization { get; set; }

        public override bool IsRecoverable => true;

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

        protected override async Task OnProxyOpeningAsync(TestContractStateFullProxy contract)
        {
            await contract.InitAsync();
            if (ExtendedInitialization)
            {
                await contract.InitExAsync(FailExtendedInitialization);
            }
        }
    }
}
