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
        [InlineData("innermethod")]
        [InlineData("InnerMethod")]
        [Theory]
        public void Resolve_Ok(string actionName)
        {
            var contract= BoltFramework.GetContract(typeof(IContract1));


            ActionResolver resolver = new ActionResolver();

            Assert.NotNull(resolver.Resolve(typeof(IContract1), actionName));
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

            Assert.NotNull(resolver.Resolve(typeof(IContract2), actionName));
            Assert.Equal(nameof(IContract2.MethodAsync), resolver.Resolve(typeof(IContract2), actionName).Name);
        }

        [InlineData("initboltsession")]
        [InlineData("InitBoltSession")]
        [Theory]
        public void Init_SessionAction_ProperActionResolved(string actionName)
        {
            ActionResolver resolver = new ActionResolver();
            Assert.Equal(BoltFramework.SessionMetadata.Resolve(typeof(IContract1)).InitSession.Action, resolver.Resolve(typeof(IContract1), actionName));
        }

        [InlineData("destroyboltsession")]
        [InlineData("DestroyBoltSession")]
        [Theory]
        public void DestroySessionAction_ProperActionResolved(string actionName)
        {
            ActionResolver resolver = new ActionResolver();
            Assert.Equal(BoltFramework.SessionMetadata.Resolve(typeof(IContract1)).DestroySession.Action, resolver.Resolve(typeof(IContract1), actionName));
        }

        public interface IContract1 : IContractInner
        {
            void Method();
        }

        public interface IContractInner
        {
            void InnerMethod();
        }

        public interface IContract2 : IContractInner
        {
            void Method();

            Task MethodAsync();
        }
    }
}
