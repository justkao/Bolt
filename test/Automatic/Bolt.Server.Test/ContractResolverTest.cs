using Xunit;

namespace Bolt.Server.Test
{
    public class ContractResolverTest
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
            ContractResolver resolver = new ContractResolver();
            Assert.NotNull(resolver.Resolve(new[] {typeof (Contract)}, contractName));
        }

        private class Contract
        {
        }
    }
}