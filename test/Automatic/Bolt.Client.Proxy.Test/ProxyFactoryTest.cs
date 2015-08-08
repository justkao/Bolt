using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client.Channels;
using Bolt.Core;
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
                new Type[] {typeof (string), typeof (int), typeof (CancellationToken)});

        [Fact]
        public void Create_EnsureDerivesFromContractProxy()
        {
            ProxyFactory factory = Create();

            Mock<IChannel> contract = new Mock<IChannel>();

            ITestContract proxy = factory.CreateProxy<ITestContract>(contract.Object);
            Assert.True(proxy is ContractProxy);
        }

        [Fact]
        public void Create_EnsureIsChannel()
        {
            ProxyFactory factory = Create();

            Mock<IChannel> contract = new Mock<IChannel>();

            ITestContract proxy = factory.CreateProxy<ITestContract>(contract.Object);
            Assert.True(proxy is IChannel);
        }

        [Fact]
        public void ExecuteAction_EnsureChannelCalled()
        {
            ProxyFactory factory = Create();
            
            Mock<IChannel> contract = new Mock<IChannel>();
            var contractType = typeof (ITestContract);
            var resultType = typeof(void);

            contract.Setup(c => c.Serializer).Returns(new JsonSerializer());
            contract.Setup(
                c => c.Send(contractType, Action, resultType, null, CancellationToken.None))
                .Verifiable();

            ITestContract proxy = factory.CreateProxy<ITestContract>(contract.Object);
            proxy.Action();

            contract.Verify();
        }

        [Fact]
        public async Task ExecuteAsyncMethod_EnsureChannelCalled()
        {
            ProxyFactory factory = Create();

            Mock<IChannel> contract = new Mock<IChannel>();
            var contractType = typeof(ITestContract);
            var resultType = typeof(void);

            contract.Setup(c => c.Serializer).Returns(new JsonSerializer());
            contract.Setup(
                c => c.SendAsync(contractType, AsyncMethod, resultType, null, CancellationToken.None)).Returns(Task.FromResult((object)Empty.Instance))
                .Verifiable();

            ITestContract proxy = factory.CreateProxy<ITestContract>(contract.Object);
            await proxy.AsyncMethod();

            contract.Verify();
        }

        [Fact]
        public void ExecuteFunction_EnsureChannelCalled()
        {
            ProxyFactory factory = Create();

            Mock<IChannel> contract = new Mock<IChannel>();
            var contractType = typeof(ITestContract);
            var resultType = typeof(string);

            contract.Setup(c => c.Serializer).Returns(new JsonSerializer());
            contract.Setup(
                c => c.Send(contractType, Function, resultType, null, CancellationToken.None))
                .Returns("some value")
                .Verifiable();

            ITestContract proxy = factory.CreateProxy<ITestContract>(contract.Object);
            string result = proxy.Function();
            contract.Verify();

            Assert.Equal("some value", result);
        }

        [Fact]
        public async Task ExecuteAsyncFunction_EnsureChannelCalled()
        {
            ProxyFactory factory = Create();

            Mock<IChannel> contract = new Mock<IChannel>(MockBehavior.Strict);
            var contractType = typeof(ITestContract);
            var resultType = typeof(int);

            contract.Setup(c => c.Serializer).Returns(new JsonSerializer());
            contract.Setup(
                c => c.SendAsync(contractType, AsyncFunction, resultType, null, CancellationToken.None))
                .Returns(() => Task.FromResult((object) 99))
                .Verifiable();

            ITestContract proxy = factory.CreateProxy<ITestContract>(contract.Object);
            var result = await proxy.AsyncFunction();
            contract.Verify();

            Assert.Equal(99, result);
        }

        [Fact]
        public async Task ExecuteMethodWithParameters_EnsureSerialized()
        {
            ProxyFactory factory = Create();

            Mock<IObjectSerializer> objectSerializer = new Mock<IObjectSerializer>(MockBehavior.Strict);
            Mock<ISerializer> jsonSerializer = new Mock<ISerializer>(MockBehavior.Strict);
            jsonSerializer.Setup(s => s.CreateSerializer()).Returns(objectSerializer.Object);

            Mock<IChannel> contract = new Mock<IChannel>(MockBehavior.Strict);

            var contractType = typeof (ITestContract);
            var resultType = typeof (void);

            string param1 = "val";
            int param2 = 10;
            CancellationToken cancellationToken = new CancellationTokenSource().Token;

            objectSerializer.Setup(s => s.Write(nameof(param1), typeof (string), param1)).Verifiable();
            objectSerializer.Setup(s => s.Write(nameof(param2), typeof (int), param2)).Verifiable();

            contract.Setup(c => c.Serializer).Returns(jsonSerializer.Object);
            contract.Setup(
                c =>
                    c.Send(contractType, MethodWithParameters, resultType, objectSerializer.Object,
                        cancellationToken)).Returns(true)
                .Verifiable();

            ITestContract proxy = factory.CreateProxy<ITestContract>(contract.Object);

            proxy.MethodWithParameters(param1, param2, cancellationToken);

            contract.Verify();
            objectSerializer.Verify();
            jsonSerializer.Verify();
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
            return new ProxyFactory();
        }
    }
}
