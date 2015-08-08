using System.Threading.Tasks;
using Xunit;

namespace Bolt.Server.Test
{
    public class ActionResolverTest
    {
        [InlineData("Method")]
        [InlineData("MethodAsync")]
        [InlineData("methodAsync")]
        [InlineData("methodasync")]
        [InlineData("method")]
        [Theory]
        public void Resolve_Ok(string actionName)
        {
            ActionResolver resolver = new ActionResolver();

            Assert.NotNull(resolver.Resolve(typeof (Contract1), actionName));
        }

        [InlineData("Method")]
        [InlineData("MethodAsync")]
        [InlineData("methodAsync")]
        [InlineData("methodasync")]
        [InlineData("method")]
        [Theory]
        public void PreferAsyncMethod_Ok(string actionName)
        {
            ActionResolver resolver = new ActionResolver();

            Assert.NotNull(resolver.Resolve(typeof(Contract2), actionName));
            Assert.Equal(nameof(Contract2.MethodAsync), resolver.Resolve(typeof (Contract2), actionName).Name);
        }

        private class Contract1
        {
            public void Method()
            {
                
            }
        }

        private class Contract2
        {
            public void Method()
            {
            }

            public Task MethodAsync()
            {
                return Task.FromResult(true);
            }
        }
    }
}
