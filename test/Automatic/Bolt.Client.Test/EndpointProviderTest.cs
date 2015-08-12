using System;
using System.Reflection;

using Xunit;

namespace Bolt.Client.Test
{
    public class EndpointProviderTest
    {
        [InlineData("http://localhost/", "http://localhost/test/EndpointProviderTest/Method")]
        [InlineData("http://localhost", "http://localhost/test/EndpointProviderTest/Method")]
        [Theory]
        public void ValidateEndpoint(string server, string expectedResult)
        {
            EndpointProvider endpointProvider = new EndpointProvider(new BoltOptions {Prefix = "test"});

            Assert.Equal<string>(expectedResult, endpointProvider.GetEndpoint(new Uri(server), GetType(), MethodInfo).ToString());
        }

        [InlineData("http://localhost/", "http://localhost/test/EndpointProviderTest/Method")]
        [InlineData("http://localhost", "http://localhost/test/EndpointProviderTest/Method")]
        [Theory]
        public void ValidateEndpointOnAsyncPostfix(string server, string expectedResult)
        {
            EndpointProvider endpointProvider = new EndpointProvider(new BoltOptions { Prefix = "test" });

            Assert.Equal<string>(expectedResult, endpointProvider.GetEndpoint(new Uri(server), GetType(), MethodAsyncInfo).ToString());
        }

        public void Method()
        {
        }

        public void MethodAsync()
        {
        }

        private static readonly MethodInfo MethodInfo = typeof (EndpointProviderTest).GetRuntimeMethod("Method", new Type[0]);

        private static readonly MethodInfo MethodAsyncInfo = typeof(EndpointProviderTest).GetRuntimeMethod("Method", new Type[0]);
    }
}
