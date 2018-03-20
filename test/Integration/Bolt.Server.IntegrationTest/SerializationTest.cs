using System;
using System.Collections.Generic;
using Bolt.Client;
using Bolt.Serialization;
using Bolt.Server.IntegrationTest.Core;
using Bolt.Test.Common;
using Microsoft.AspNetCore.Builder;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public abstract class SerializationTest : IntegrationTestBase
    {
        public SerializationTest()
        {
            ClientConfiguration.Serializer = CreateSerializer();
        }

        public MockInstanceProvider InstanceProvider { get; } = new MockInstanceProvider();

        [Fact]
        public void WriteResponseOnServer_ReadOnClient_OK()
        {
            var data = new List<CompositeType>()
            {
                CompositeType.CreateRandom(),
                CompositeType.CreateRandom(),
                CompositeType.CreateRandom(),
                CompositeType.CreateRandom()
            };

            Server().Setup(v => v.FunctionReturningHugeData()).Returns(data);

            var returned = CreateChannel().FunctionReturningHugeData();
            Assert.Equal(data.Count, returned.Count);
            for (int i = 0; i < data.Count; i++)
            {
                Assert.Equal(returned[i], data[i]);
            }
        }

        [Fact]
        public void WriteParametersOnClient_ReadOnServer_OK()
        {
            var arg1 = CompositeType.CreateRandom();
            var arg2 = CompositeType.CreateRandom();
            var arg3 = DateTime.UtcNow;

            Server().Setup(v => v.MethodWithManyArguments(arg1, arg2, arg3)).Callback<CompositeType, CompositeType, DateTime>(
                (a1, a2, a3)
                =>
                {
                    Assert.Equal(arg1, a1);
                    Assert.Equal(arg2, a2);
                    Assert.Equal(arg3, a3);
                });

            CreateChannel().MethodWithManyArguments(arg1, arg2, arg3);
        }

        protected abstract ISerializer CreateSerializer();

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt(h =>
            {
                h.Configuration.DefaultSerializer = CreateSerializer();
                h.Configuration.AvailableSerializers = new[] { h.Configuration.DefaultSerializer };
                h.Use<ITestContract>(InstanceProvider);
            });
        }

        private Mock<ITestContract> Server()
        {
            Mock<ITestContract> mock = new Mock<ITestContract>(MockBehavior.Strict);
            InstanceProvider.CurrentInstance = mock.Object;
            return mock;
        }

        private ITestContractAsync CreateChannel()
        {
            return ClientConfiguration.CreateProxy<ITestContractAsync>(ServerUrl);
        }
    }
}
