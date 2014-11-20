using Bolt.Client;
using Bolt.Server;
using Bolt.Service.Test.Core;
using Microsoft.Owin.Hosting;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Service.Test
{
    [TestFixture]
    public class ServiceTest
    {
        [Test]
        public void ClientCallsAsyncMethod_AsyncOnClientAndServer_EnsureExecutedOnServer()
        {
            ITestContractAsync client = GetChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethodExAsync()).Returns(Task.FromResult(true));
            client.SimpleMethodExAsync().GetAwaiter().GetResult();
            server.Verify(v => v.SimpleMethodExAsync(), Times.Once);
        }

        [Test]
        public void Client_CallsComplexFunction_EnsureValidDataReturned()
        {
            ITestContractAsync client = GetChannel();
            Mock<ITestContract> server = Server();

            CompositeType serverData = CompositeType.CreateRandom();
            server.Setup(v => v.ComplexFunction()).Returns(serverData);
            CompositeType clientData = client.ComplexFunction();

            Assert.AreEqual(serverData, clientData);
        }

        [Test]
        public void Client_CallsVoidMethodAsync_EnsureExecutedOnServer()
        {
            ITestContractAsync client = GetChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethod());
            client.SimpleMethodAsync().GetAwaiter().GetResult();
            server.Verify(v => v.SimpleMethod(), Times.Once);
        }

        [Test]
        public void Client_CallsVoidMethod_EnsureExecutedOnServer()
        {
            ITestContractAsync client = GetChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethod());
            client.SimpleMethod();
            server.Verify(v => v.SimpleMethod(), Times.Once);
        }

        [Test]
        public void Client_CancelsRequest_EnsureCancelledOnServer()
        {
            Mock<ITestContract> server = Server();
            ITestContractAsync client = GetChannel();

            CancellationTokenSource cancellation = new CancellationTokenSource();

            CancellationToken serverToken = CancellationToken.None;

            EventWaitHandle waitHandle = new ManualResetEvent(false);
            EventWaitHandle called = new ManualResetEvent(false);

            server.Setup(v => v.SimpleMethodWithCancellation(It.IsAny<CancellationToken>())).Callback<CancellationToken>((t) =>
            {
                serverToken = t;
                called.Set();
                Assert.Throws<OperationCanceledException>(() => Task.Delay(TimeSpan.FromSeconds(30), t).Wait(t));
                serverToken = t;
                waitHandle.Set();
            });

            Task task = Task.Run(() =>
            {
                Assert.Throws<OperationCanceledException>(() => client.SimpleMethodWithCancellation(cancellation.Token));
            });

            called.WaitOne(1000);
            cancellation.Cancel();

            task.GetAwaiter().GetResult();
            if (!waitHandle.WaitOne(1000))
            {
                if (!serverToken.IsCancellationRequested)
                {
                    Assert.Fail("Request was not cancelled on server.");
                }
            }
        }

        [Test]
        public void Client_SimpleParameter_EnsureSameOnServer()
        {
            ITestContractAsync client = GetChannel();
            Mock<ITestContract> server = Server();
            int arg = 999;

            server.Setup(v => v.SimpleMethodWithSimpleArguments(arg));
            client.SimpleMethodWithSimpleArguments(arg);
        }

        [Test]
        public void Server_ReturnsNumber_EnsureSameValueOnClient()
        {
            ITestContractAsync client = GetChannel();
            Mock<ITestContract> server = Server();

            int value = 99;

            server.Setup(v => v.SimpleFunction()).Returns(value);
            Assert.AreEqual(value, client.SimpleFunction());
        }

        [Test]
        public void Client_ComplexParameters_EnsureSameOnServer()
        {
            ITestContractAsync client = GetChannel();
            Mock<ITestContract> server = Server();
            CompositeType arg1 = CompositeType.CreateRandom();

            server.Setup(v => v.SimpleMethodWithComplexParameter(arg1)).Callback<CompositeType>((serverArg) =>
            {
                Assert.AreNotSame(serverArg, arg1);
                Assert.AreEqual(serverArg, arg1);
            });

            client.SimpleMethodWithComplexParameter(arg1);
        }

        [Test]
        public void Server_Throws_EnsureSameExceptionOnClient()
        {
            ITestContractAsync client = GetChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethod()).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(client.SimpleMethod);
        }

        private IDisposable _runningServer;

        public Uri ServerUrl = new Uri("http://localhost:9999");

        public ServerConfiguration ServerConfiguration { get; set; }

        public ClientConfiguration ClientConfiguration { get; set; }

        public ContractDefinition TestContract { get; set; }

        public string Prefix { get; set; }

        public MockInstanceProvider InstanceProvider = new MockInstanceProvider();

        public Mock<ITestContract> Server()
        {
            Mock<ITestContract> mock = new Mock<ITestContract>(MockBehavior.Strict);
            InstanceProvider.CurrentInstance = mock.Object;
            return mock;
        }

        public virtual ITestContractAsync GetChannel()
        {
            TestContractChannelFactory factory = new TestContractChannelFactory(Contracts.TestContract);
            factory.ClientConfiguration = ClientConfiguration;
            factory.Prefix = Prefix;

            return factory.Create(ServerUrl);
        }

        [TestFixtureSetUp]
        protected virtual void Init()
        {
            var serializer = new JsonSerializer();

            ServerConfiguration = new ServerConfiguration(serializer);
            ClientConfiguration = new ClientConfiguration(serializer);

            TestContract = Contracts.TestContract;
            Prefix = "test";

            _runningServer = WebApp.Start(ServerUrl.ToString(),
                (b) =>
                    b.RegisterEndpoint(ServerConfiguration, TestContract, Prefix,
                        (endpointBuilder) => endpointBuilder.UseExecutor<TestContractExecutor>(ServerConfiguration,
                            TestContractDescriptor.Default, InstanceProvider)));
        }

        [TestFixtureTearDown]
        protected virtual void Destroy()
        {
            _runningServer.Dispose();
        }

    }
}
