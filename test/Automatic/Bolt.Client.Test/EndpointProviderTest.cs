using Bolt.Metadata;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Bolt.Client.Test
{
    public class EndpointProviderTest
    {
        private ContractMetadata Contract => BoltFramework.GetContract(typeof(IContract));


        [InlineData(null, "/test/Contract/Method")]
        [InlineData("http://localhost/", "http://localhost/test/Contract/Method")]
        [InlineData("http://localhost", "http://localhost/test/Contract/Method")]
        [Theory]
        public void ValidateEndpoint(string server, string expectedResult)
        {
            EndpointProvider endpointProvider = new EndpointProvider(new BoltOptions { Prefix = "test" });

            Assert.Equal(expectedResult, endpointProvider.GetEndpoint(server != null ? new Uri(server) : null, Contract, Contract.GetAction(MethodInfo)).ToString());
        }

        [InlineData("http://localhost/", "http://localhost/test/Contract/Method")]
        [InlineData("http://localhost", "http://localhost/test/Contract/Method")]
        [InlineData(null, "/test/Contract/Method")]
        [Theory]
        public void ValidateEndpointOnAsyncPostfix(string server, string expectedResult)
        {
            EndpointProvider endpointProvider = new EndpointProvider(new BoltOptions { Prefix = "test" });

            Assert.Equal(expectedResult, endpointProvider.GetEndpoint(server != null ? new Uri(server) : null, Contract, Contract.GetAction(MethodAsyncInfo)).ToString());
        }

        public interface IContract
        {
            void Method();

            void MethodAsync();
        }

        private static readonly MethodInfo MethodInfo = typeof(IContract).GetRuntimeMethods().First(m => m.Name == nameof(IContract.Method));

        private static readonly MethodInfo MethodAsyncInfo = typeof(IContract).GetRuntimeMethods().First(m => m.Name == nameof(IContract.MethodAsync));
    }
}
