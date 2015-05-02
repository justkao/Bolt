using Bolt.Client;
using Bolt.Client.Channels;
using Bolt.Server.IntegrationTest;
using Bolt.Service.Test.Core;
using Bolt.Test.Common;
using Microsoft.AspNet.Builder;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class StateLessTest : IntegrationTestBase
    {
        public StateLessTest(BoltServer server)
            : base(server)
        {
        }

        [Fact]
        public void Fact_Dummy()
        {
        }

        /*
        [Fact]
        public void ClientCallsAsyncMethod_AsyncOnClientAndServer_EnsureExecutedOnServer()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethodExAsync()).Returns(Task.FromResult(true));
            client.SimpleMethodExAsync().GetAwaiter().GetResult();
            server.Verify(v => v.SimpleMethodExAsync(), Times.Once);
        }

        [Fact]
        public void Client_CallsComplexFunction_EnsureValidDataReturned()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            CompositeType serverData = CompositeType.CreateRandom();
            server.Setup(v => v.ComplexFunction()).Returns(serverData);
            CompositeType clientData = client.ComplexFunction();

            Assert.Equal(serverData, clientData);
        }

        [Fact]
        public void Client_CallsVoidMethod_EnsureExecutedOnServer()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethod());
            client.SimpleMethod();
            server.Verify(v => v.SimpleMethod(), Times.Once);
        }

        [Fact]
        public async Task Client_CallsAsyncFunction_EnsureCalledAsyncOnServer()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleAsyncFunction()).Returns(async () =>
            {
                await Task.Delay(1);
                return 10;
            });

            int result = await client.SimpleAsyncFunction();
            Assert.Equal(10, result);
        }

        [Fact]
        public void Client_SimpleParameter_EnsureSameOnServer()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();
            int arg = 999;

            server.Setup(v => v.SimpleMethodWithSimpleArguments(arg));
            client.SimpleMethodWithSimpleArguments(arg);
        }

        [Fact]
        public void Client_ManyComplexParameters_EnsureSameOnServer()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            CompositeType arg1 = CompositeType.CreateRandom();
            CompositeType arg2 = CompositeType.CreateRandom();
            DateTime arg3 = DateTime.UtcNow;

            server.Setup(v => v.MethodWithManyArguments(arg1, arg2, arg3));
            client.MethodWithManyArguments(arg1, arg2, arg3);
        }

        [Fact]
        public void Server_ReturnsNumber_EnsureSameValueOnClient()
        {
            ITestContract client = CreateChannel();
            Mock<ITestContract> server = Server();

            int value = 99;

            server.Setup(v => v.SimpleFunction()).Returns(value);
            Assert.Equal(value, client.SimpleFunction());
        }

        [Fact]
        public void Client_ComplexParameters_EnsureSameOnServer()
        {
            ITestContract client = CreateChannel();
            Mock<ITestContract> server = Server();
            CompositeType arg1 = CompositeType.CreateRandom();

            server.Setup(v => v.SimpleMethodWithComplexParameter(arg1)).Callback<CompositeType>((serverArg) =>
            {
                Assert.NotSame(serverArg, arg1);
                Assert.Equal(serverArg, arg1);
            });

            client.SimpleMethodWithComplexParameter(arg1);
        }

        [Fact]
        public void Server_Throws_EnsureSameExceptionOnClient()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethod()).Throws<CustomException>();
            Assert.Throws<CustomException>(client.SimpleMethod);
        }


        [Fact]
        public void Client_NotSerializableReturnValue_EnsureBoltServerException()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();
            var arg = new NotSerializableType(10);

            server.Setup(v => v.FunctionWithNotSerializableType()).Returns(arg);

            try
            {
                client.FunctionWithNotSerializableType();
            }
            catch (BoltServerException e)
            {
                if (e.Error != ServerErrorCode.Serialization)
                {
                    throw;
                }
            }
        }

        [Fact]
        public async Task Async_Client_NotSerializableReturnValue_EnsureBoltServerException()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();
            var arg = new NotSerializableType(10);

            server.Setup(v => v.FunctionWithNotSerializableType()).Returns(arg);

            try
            {
                await client.FunctionWithNotSerializableTypeAsync();
            }
            catch (BoltServerException e)
            {
                if (e.Error != ServerErrorCode.Serialization)
                {
                    throw;
                }
            }
        }

        [Fact]
        public void Client_NotSerializableParameters_EnsureBoltSerializationException()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();
            var arg = new NotSerializableType(10);

            server.Setup(v => v.MethodWithNotSerializableType(It.IsAny<NotSerializableType>()));

            try
            {
                client.MethodWithNotSerializableType(arg);
            }
            catch (SerializeParametersException)
            {
                // ok
            }
        }

        [Fact]
        public async Task Async_Client_NotSerializableParameters_EnsureBoltSerializationException()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();
            var arg = new NotSerializableType(10);

            server.Setup(v => v.MethodWithNotSerializableType(It.IsAny<NotSerializableType>()));

            try
            {
                await client.MethodWithNotSerializableTypeAsync(arg);
            }
            catch (SerializeParametersException)
            {
                // ok
            }
        }

        [Fact]
        public void Server_Throws_EnsureCorrectMessageOnClient()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethod()).Throws(new CustomException("test message"));

            try
            {
                client.SimpleMethod();
            }
            catch (CustomException e)
            {
                Assert.Equal("test message", e.Message);
            }
        }

        [Fact]
        public void CreateProxyPerformance()
        {
            int cnt = 10000;

            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < cnt; i++)
            {
                using (ClientConfiguration.CreateProxy<TestContractProxy>(ServerUrl))
                {
                }
            }

            Console.WriteLine("Creating {0} proxies by helpers has taken {1}ms", 10000, watch.ElapsedMilliseconds);

            watch.Restart();
            for (int i = 0; i < cnt; i++)
            {
                using (new TestContractProxy(new RecoverableChannel(new UriServerProvider(ServerUrl), ClientConfiguration)))
                {
                }
            }

            Console.WriteLine("Creating {0} proxies manually has taken {1}ms", 10000, watch.ElapsedMilliseconds);
        }

        [Fact]
        public void Server_ThrowsWithInner_EnsureInnerReceivedOnClient()
        {
            ITestContract client = CreateChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethod()).Throws(new CustomException("test message", new CustomException()));

            try
            {
                client.SimpleMethod();
            }
            catch (CustomException e)
            {
                Assert.IsNotNull(e.InnerException as CustomException);
            }
        }

        [Fact]
        public void Server_ReturnsNull_EnsureNullReceivedOnClient()
        {
            ITestContract client = CreateChannel();
            Mock<ITestContract> server = Server();
            server.Setup(v => v.ComplexFunction()).Returns<CompositeType>(null);
            Assert.IsNull(client.ComplexFunction());
        }

        [Fact]
        public void Server_ThrowsWithInner_EnsureInnerwithCorrectMessageReceivedOnClient()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethod()).Throws(new CustomException("test message", new CustomException("inner message")));

            try
            {
                client.SimpleMethod();
            }
            catch (CustomException e)
            {
                Assert.IsNotNull(e.InnerException as CustomException);
                Assert.Equal("inner message", e.InnerException.Message);
            }
        }

        [Fact]
        public void LongOperation_TimeoutSet_EnsureCallTimeouted()
        {
            TestContractProxy client = CreateChannel();
            ((ChannelBase)client.Channel).DefaultResponseTimeout = TimeSpan.FromSeconds(0.1);
            CompositeType arg = CompositeType.CreateRandom();

            Mock<ITestContract> server = Server();
            server.Setup(v => v.SimpleMethodWithComplexParameter(arg)).Callback(() => Thread.Sleep(TimeSpan.FromSeconds(1)));
            Assert.Throws<TimeoutException>(() => client.SimpleMethodWithComplexParameter(arg));
        }

        [Fact]
        public void Request_Recoverable_EnsureExecuted()
        {
            Uri server1 = new Uri("http://localhost:1111");
            Mock<ITestContract> server = Server();
            server.Setup(m => m.SimpleMethod());
            TestContractProxy channel = CreateChannel(10, TimeSpan.FromSeconds(0.2), server1);
            Task ongoing = channel.SimpleMethodAsync();

            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            IDisposable running = StartServer(server1, ConfigureDefaultServer);

            ongoing.GetAwaiter().GetResult();
            running.Dispose();
        }


        [Fact]
        public void ServerReturnsBigData_EnsureReceivedOnClient()
        {
            TestContractProxy channel = CreateChannel();
            Server()
                .Setup(v => v.FunctionReturningHugeData())
                .Returns(() => Enumerable.Repeat(0, 1000).Select(_ => CompositeType.CreateRandom()).ToList());

            List<CompositeType> result = channel.FunctionReturningHugeData();
            Assert.Equal(1000, result.Count);

        }

        [Fact]
        public void ClientSendsBigData_EnsureReceivedOnServer()
        {
            TestContractProxy channel = CreateChannel();
            var data = Enumerable.Repeat(0, 1000).Select(_ => CompositeType.CreateRandom()).ToList();

            Mock<ITestContract> server = Server();
            server.Setup(v => v.MethodTakingHugeData(It.IsAny<List<CompositeType>>())).Callback<List<CompositeType>>(
                v => Assert.Equal(1000, v.Count));

            channel.MethodTakingHugeData(data);

            server.Verify(v => v.MethodTakingHugeData(It.IsAny<List<CompositeType>>()));
        }

        public MockInstanceProvider InstanceProvider = new MockInstanceProvider();

        public Mock<ITestContract> Server()
        {
            Mock<ITestContract> mock = new Mock<ITestContract>(MockBehavior.Strict);
            InstanceProvider.CurrentInstance = mock.Object;
            return mock;
        }

        public virtual TestContractProxy CreateChannel(int retries = 0)
        {
            return ClientConfiguration.CreateProxy<TestContractProxy>(ServerUrl);
        }

        public virtual TestContractProxy CreateChannel(int retries, TimeSpan retryDelay, params Uri[] servers)
        {
            TestContractProxy proxy = ClientConfiguration.CreateProxy<TestContractProxy>(new MultipleServersProvider(servers));
            proxy.WithRetries(retries, retryDelay);
            return proxy;
        }
        */

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt((h) => { });
        }
    }
}
