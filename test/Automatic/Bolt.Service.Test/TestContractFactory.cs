using Bolt.Client;

namespace Bolt.Service.Test.Core
{
    public class TestContractFactory : ChannelFactory<TestContractChannel, TestContractDescriptor>
    {
        public TestContractFactory()
        {
            ContractDescriptor = TestContractDescriptor.Default;
            ContractDefinition = Contracts.TestContract;
            Prefix = "test";
        }
    }
}