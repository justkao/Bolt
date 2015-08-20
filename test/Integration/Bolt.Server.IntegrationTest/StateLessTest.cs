using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client;
using Bolt.Client.Pipeline;
using Bolt.Pipeline;
using Bolt.Server.IntegrationTest.Core;
using Bolt.Test.Common;
using Microsoft.AspNet.Builder;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class StateLessTest : IntegrationTestBase
    {
        [Fact]
        public void NewProxy_EnsureReady()
        {
            var client = CreateChannel();
            Assert.Equal(ProxyState.Ready, ((IProxy)client).State);
        }

        [Fact]
        public async Task OpenAsync_EnsureOpened()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            await ((IProxy) client).OpenAsync();
            Assert.Equal(ProxyState.Open, ((IProxy)client).State);
        }

        [Fact]
        public async Task ReusePipeline_Ok()
        {
            var pipeline = CreatePipeline();

            var client1 = CreateChannel(pipeline);
            var client2 = CreateChannel(pipeline);

            Mock<ITestContract> server = Server();
            server.Setup(s => s.SimpleMethod());

            await client1.SimpleMethodAsync();
            await client2.SimpleMethodAsync();
        }

        [Fact]
        public async Task DisposeProxy_EnsurePipelineWorking()
        {
            var pipeline = CreatePipeline();

            var client1 = CreateChannel(pipeline);
            var client2 = CreateChannel(pipeline);

            Mock<ITestContract> server = Server();
            server.Setup(s => s.SimpleMethod());

            await client1.SimpleMethodAsync();
            await client2.SimpleMethodAsync();

            (client1 as IDisposable)?.Dispose();
            await client2.SimpleMethodAsync();
        }

        [Fact]
        public async Task CloseAsync_EnsureClosed()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            await ((IProxy)client).OpenAsync();
            await ((IProxy)client).CloseAsync();

            Assert.Equal(ProxyState.Closed, ((IProxy)client).State);
        }

        [Fact]
        public async Task UseClosedProxy_EnsureThrows()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            await ((IProxy)client).OpenAsync();
            await ((IProxy)client).CloseAsync();

            await Assert.ThrowsAsync<ProxyClosedException>(()=>client.ComplexFunctionAsync());
        }

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

            server.Setup(v => v.SimpleMethodWithComplexParameter(arg1)).Callback<CompositeType>(serverArg =>
            {
                Assert.NotSame(serverArg, arg1);
                Assert.Equal(serverArg, arg1);
            });

            client.SimpleMethodWithComplexParameter(arg1);
        }

        [Fact]
        public void Client_NullArgument_Ok()
        {
            Mock<ITestContract> server = Server();
            server.Setup(v => v.MethodWithNullableArguments(null)).Verifiable();

            ITestContract client = CreateChannel();
            client.MethodWithNullableArguments(null);

            server.Verify();
        }

        [Fact]
        public void Server_Throws_EnsureSameExceptionOnClient()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleMethod()).Throws<CustomException>();
            Assert.Throws<CustomException>(()=> { client.SimpleMethod(); });
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
                if (e.Error != ServerErrorCode.SerializeResponse)
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
                if (e.Error != ServerErrorCode.SerializeResponse)
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
            catch (BoltClientException e) when (e.Error == ClientErrorCode.SerializeParameters)
            {
                // ok
            }
        }

        [Fact]
        public void CallFunctionWithSingleCancellationParameter_Ok()
        {
            var client = CreateChannel();
            Mock<ITestContract> server = Server();

            server.Setup(v => v.SimpleFunctionWithCancellation(It.IsAny<CancellationToken>())).Returns(10);

            int result = client.SimpleFunctionWithCancellation(CancellationToken.None);
            Assert.Equal(10, result);
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
            catch (BoltClientException e) when (e.Error == ClientErrorCode.SerializeParameters)
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
        public virtual void CreateProxyPerformance()
        {
            for (int i = 0; i < 10; i++)
            {
                ClientConfiguration.ProxyBuilder()
                    .Url(ServerUrl)
                    .Recoverable(10, TimeSpan.FromSeconds(1))
                    .Build<TestContractProxy>().Dispose();
            }

            int cnt = 10000;

            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < cnt; i++)
            {
                ClientConfiguration.ProxyBuilder()
                    .Url(ServerUrl)
                    .Recoverable(10, TimeSpan.FromSeconds(1))
                    .Build<TestContractProxy>().Dispose();
            }

            System.Console.WriteLine("Creating {0} proxies by ProxyBuilder has taken {1}ms", 10000, watch.ElapsedMilliseconds);
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
                Assert.NotNull(e.InnerException as CustomException);
            }
        }

        [Fact]
        public void Server_ReturnsNull_EnsureNullReceivedOnClient()
        {
            ITestContract client = CreateChannel();
            Mock<ITestContract> server = Server();
            server.Setup(v => v.ComplexFunction()).Returns<CompositeType>(null);
            Assert.Null(client.ComplexFunction());
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
                Assert.NotNull(e.InnerException as CustomException);
                Assert.Equal("inner message", e.InnerException.Message);
            }
        }

        [Fact]
        public void LongOperation_TimeoutSet_EnsureCallTimeouted()
        {
            IClientPipeline pipeline = CreatePipeline();
            pipeline.Find<CommunicationMiddleware>().ResponseTimeout = TimeSpan.FromSeconds(0.1);

            ITestContractAsync client = CreateChannel(pipeline);

            CompositeType arg = CompositeType.CreateRandom();

            Mock<ITestContract> server = Server();
            server.Setup(v => v.SimpleMethodWithComplexParameter(arg)).Callback(() => Thread.Sleep(TimeSpan.FromSeconds(10)));
            Assert.Throws<TimeoutException>(() => client.SimpleMethodWithComplexParameter(arg));
        }

        [Fact]
        public void ServerReturnsBigData_EnsureReceivedOnClient()
        {
            ITestContractAsync channel = CreateChannel();
            Server()
                .Setup(v => v.FunctionReturningHugeData())
                .Returns(() => Enumerable.Repeat(0, 1000).Select(_ => CompositeType.CreateRandom()).ToList());

            List<CompositeType> result = channel.FunctionReturningHugeData();
            Assert.Equal(1000, result.Count);
        }

        [Fact]
        public void ClientSendsBigData_EnsureReceivedOnServer()
        {
            ITestContractAsync channel = CreateChannel();
            var data = Enumerable.Repeat(0, 1000).Select(_ => CompositeType.CreateRandom()).ToList();

            Mock<ITestContract> server = Server();
            server.Setup(v => v.MethodTakingHugeData(It.IsAny<List<CompositeType>>()))
                .Callback<List<CompositeType>>(v => Assert.Equal(1000, v.Count));

            channel.MethodTakingHugeData(data);

            server.Verify(v => v.MethodTakingHugeData(It.IsAny<List<CompositeType>>()));
        }

        public Mock<ITestContract> Server()
        {
            Mock<ITestContract> mock = new Mock<ITestContract>(MockBehavior.Strict);
            InstanceProvider.CurrentInstance = mock.Object;
            return mock;
        }

        public virtual ITestContractAsync CreateChannel(IClientPipeline pipeline = null)
        {
            return new TestContractProxy(pipeline ?? CreatePipeline());
        }

        protected IClientPipeline CreatePipeline(int recoveries = 0)
        {
            var builder = ClientConfiguration.ProxyBuilder().Url(ServerUrl);
            if (recoveries > 0)
            {
                builder.Recoverable(recoveries, TimeSpan.FromMilliseconds(10));
            }

            return builder.BuildPipeline();
        }

        public MockInstanceProvider InstanceProvider = new MockInstanceProvider();

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt(h =>
            {
                h.Use<ITestContract>(InstanceProvider);
            });
        }
    }
}
