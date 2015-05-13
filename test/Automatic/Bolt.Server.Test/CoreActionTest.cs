using Bolt.Server.Filters;
namespace Bolt.Server.Test
{
    public class CoreActionTest
    {
        public CoreActionTest()
        {
            Descriptor = new MockContractDescriptor();
            Context = new ServerActionContext()
            {
                Action = Descriptor.Action,
                ContractInvoker = new MockContractInvoker() { }
            };

            CoreAction action = new CoreAction();
        }

        public ServerActionContext Context { get; set; }

        public MockContractDescriptor Descriptor { get; set; }

        private class MockContractInvoker : ContractInvoker
        {
        }
    }
}
