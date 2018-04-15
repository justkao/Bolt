using System;
using Xunit;

namespace Bolt.Server.Test
{
    public class ContractInvokerSelectorTest
    {
        [InlineData("contract")]
        [InlineData("Contract")]
        [InlineData("contractAsync")]
        [InlineData("ContractAsync")]
        [InlineData("contractasync")]
        [InlineData("Contractasync")]
        [Theory]
        public void Resolve_Ok(string contractName)
        {
            IContractInvokerSelector resolver = new ContractInvokerSelector();

            ContractInvoker invoker = new ContractInvoker(new ServerRuntimeConfiguration());
            invoker.Contract = BoltFramework.GetContract(typeof(IContract));

            Assert.NotNull(resolver.Resolve(new[] { invoker }, contractName.AsSpan()));
        }

        private interface IContract
        {
        }
    }
}