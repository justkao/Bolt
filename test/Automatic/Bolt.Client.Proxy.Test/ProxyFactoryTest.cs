using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Client.Pipeline;
using Bolt.Pipeline;
using Moq;
using Xunit;

namespace Bolt.Client.Proxy.Test
{
    public class ProxyFactoryTest
    {
        private static readonly MethodInfo Action = typeof (ITestContract).GetRuntimeMethod(nameof(ITestContract.Action), new Type[0]);
        private static readonly MethodInfo Function = typeof(ITestContract).GetRuntimeMethod(nameof(ITestContract.Function), new Type[0]);
        private static readonly MethodInfo AsyncMethod = typeof(ITestContract).GetRuntimeMethod(nameof(ITestContract.AsyncMethod), new Type[0]);
        private static readonly MethodInfo AsyncFunction = typeof(ITestContract).GetRuntimeMethod(nameof(ITestContract.AsyncFunction), new Type[0]);

        private static readonly MethodInfo MethodWithParameters =
            typeof (ITestContract).GetRuntimeMethod(nameof(ITestContract.MethodWithParameters),
                new[] {typeof (string), typeof (int), typeof (CancellationToken)});

        [Fact]
        public void Create_EnsureDerivesFromContractProxy()
        {
            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>();
            ProxyFactory factory = Create();

            ITestContract proxy = factory.CreateProxy<ITestContract>(pipeline.Object);
            Assert.True(proxy is ProxyBase);
        }

        [Fact]
        public void Create_EnsureIsChannel()
        {
            ProxyFactory factory = Create();
            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>();
            ITestContract proxy = factory.CreateProxy<ITestContract>(pipeline.Object);

            Assert.True(proxy is IProxy);
        }

        [Fact]
        public void ExecuteMethod_EnsureChannelCalled()
        {
            ProxyFactory factory = Create();

            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>();
            pipeline.Setup(p => p.Instance).Returns(ctxt =>
            {
                ctxt.ActionResult = "some value";
                Assert.Equal(MethodWithParameters, ctxt.Action);
                Assert.Equal("val", ctxt.Parameters[0]);
                Assert.Equal(10, ctxt.Parameters[1]);
                Assert.Equal(CancellationToken.None, ctxt.Parameters[2]);

                return Task.FromResult(0);
            });

            ITestContract proxy = factory.CreateProxy<ITestContract>(pipeline.Object);
            proxy.MethodWithParameters("val", 10, CancellationToken.None);
            pipeline.Verify();
        }

        [Fact]
        public async Task ExecuteAsyncMethod_EnsureChannelCalled()
        {
            ProxyFactory factory = Create();

            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>();
            pipeline.Setup(p => p.Instance).Returns(ctxt =>
            {
                ctxt.ActionResult = "some value";
                Assert.Equal(AsyncMethod, ctxt.Action);
                return Task.FromResult(0);
            });

            ITestContract proxy = factory.CreateProxy<ITestContract>(pipeline.Object);
            await proxy.AsyncMethod();
            pipeline.Verify();
        }

        [Fact]
        public void ExecuteFunction_EnsureChannelCalled()
        {
            ProxyFactory factory = Create();

            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>();
            pipeline.Setup(p => p.Instance).Returns(ctxt =>
            {
                ctxt.ActionResult = "some value";
                Assert.Equal(Function, ctxt.Action);
                return Task.FromResult(0);
            });

            ITestContract proxy = factory.CreateProxy<ITestContract>(pipeline.Object);
            string result = proxy.Function();
            pipeline.Verify();

            Assert.Equal("some value", result);
        }

        [Fact]
        public async Task ExecuteAsyncFunction_EnsureChannelCalled()
        {
            ProxyFactory factory = Create();

            Mock<IClientPipeline> pipeline = new Mock<IClientPipeline>();
            pipeline.Setup(p => p.Instance).Returns(ctxt =>
            {
                ctxt.ActionResult = 10;
                Assert.Equal(AsyncFunction, ctxt.Action);
                return Task.FromResult(0);
            });

            ITestContract proxy = factory.CreateProxy<ITestContract>(pipeline.Object);
            int result = await proxy.AsyncFunction();
            pipeline.Verify();

            Assert.Equal(10, result);
        }

        public interface ITestContract
        {
            void Action();

            string Function();

            Task<int> AsyncFunction();

            Task AsyncMethod();

            void MethodWithParameters(string param1, int param2, CancellationToken cancellationToken);
        }

        private ProxyFactory Create()
        {
            return new DynamicProxyFactory();
        }
    }
}
