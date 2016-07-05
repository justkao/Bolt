using System;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Moq;
using Xunit;

namespace Bolt.Client.Proxy.Test
{
    public class CustomBaseProxyTest
    {
        [Fact]
        public void CreatWithCustomProxy_Ok()
        {
            ProxyFactory factory = new ProxyFactory(typeof (CustomProxy));
        }

        [Fact]
        public void CreatWithInvalidCustomProxy_Ok()
        {
            Assert.Throws<ArgumentException>(()=>new ProxyFactory(typeof(string)));
        }

        [Fact]
        public void CreateProxy_EnsureIsCustomProxy()
        {
            ProxyFactory factory = new ProxyFactory(typeof(CustomProxy));
            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>();

            IDummyInterface proxy = factory.CreateProxy<IDummyInterface>(pipeline.Object);
            Assert.NotNull(proxy as CustomProxy);
        }

        [Fact]
        public void ExecuteMethodOnCustomProxyThatIsAlsoInInterface()
        {
            ProxyFactory factory = new ProxyFactory(typeof(CustomProxy));
            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>();

            IDummyInterface proxy = factory.CreateProxy<IDummyInterface>(pipeline.Object);
            Assert.Equal(10, proxy.Execute(false));
            Assert.True(((CustomProxy) proxy).Executed);
        }

        [Fact]
        public void ExecuteMethodOnCustomProxyThatIsAlsoInInterface_Throws_Ok()
        {
            ProxyFactory factory = new ProxyFactory(typeof(CustomProxy));
            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>();

            IDummyInterface proxy = factory.CreateProxy<IDummyInterface>(pipeline.Object);
            Assert.Throws<InvalidOperationException>(() => proxy.Execute(true));
        }

        [Fact]
        public void ExecuteInterfaceMethod_Ok()
        {
            ProxyFactory factory = new ProxyFactory(typeof(CustomProxy));
            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>(MockBehavior.Strict);
            pipeline.Setup(p => p.Instance).Returns((ctxt) =>
            {
                ctxt.ActionResult = 5;
                return Task.FromResult(true);
            });
            IDummyInterface proxy = factory.CreateProxy<IDummyInterface>(pipeline.Object);
            Assert.Equal(5, proxy.Execute2());
        }

        public interface IDummyInterface
        {
            int Execute(bool throwError);

            int Execute2();
        }

        public class CustomProxy : ProxyBase
        {
            public virtual int Execute(bool throwError)
            {
                if (throwError)
                {
                    throw new InvalidOperationException();
                }
                Executed = true;
                return 10;
            }

            public bool Executed { get; set; }
        } 
    }
}