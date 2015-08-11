using System;
using System.Linq;
using Bolt.Client.Filters;

using Xunit;

namespace Bolt.Client.Test
{
    public class ProxyBuilderTest
    {
        public ProxyBuilderTest()
        {
            ClientConfiguration = new ClientConfiguration();
        }

        public ClientConfiguration ClientConfiguration { get; }

        [Fact]
        public void Build_NoUrl_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => ClientConfiguration.ProxyBuilder().Build<ITestContract>());
        }

        [Fact]
        public void Recoverable_Ok()
        {
            TestContractProxy result = ClientConfiguration.ProxyBuilder()
                .Url("http://localhost:8080")
                .Recoverable(10, TimeSpan.FromSeconds(5))
                .Build<TestContractProxy>();

            Assert.Equal(10, result.GetChannel<RecoverableChannel>().Retries);
            Assert.Equal(TimeSpan.FromSeconds(5), result.GetChannel<RecoverableChannel>().RetryDelay);
        }

        [Fact]
        public void Session_Ok()
        {
            TestContractProxy result = ClientConfiguration.ProxyBuilder()
                .Url("http://localhost:8080")
                .UseSession(false)
                .Build<TestContractProxy>();

            result.GetChannel<SessionChannel>();
        }

        [Fact]
        public void Session_Distributed_Ok()
        {
            TestContractProxy result = ClientConfiguration.ProxyBuilder()
                .Url("http://localhost:8080")
                .UseSession(true)
                .Build<TestContractProxy>();

            Assert.True(result.GetChannel<SessionChannel>().UseDistributedSession);
        }

        [Fact]
        public void RecoverableSession_Ok()
        {
            TestContractProxy result =
                ClientConfiguration.ProxyBuilder()
                    .Url("http://localhost:8080")
                    .Recoverable(45, TimeSpan.FromSeconds(8))
                    .UseSession(true)
                    .Build<TestContractProxy>();

            Assert.Equal(45, result.GetChannel<SessionChannel>().Retries);
            Assert.Equal(TimeSpan.FromSeconds(8), result.GetChannel<SessionChannel>().RetryDelay);
        }

        [Fact]
        public void Filter_Ok()
        {
            TestContractProxy result =
                ClientConfiguration.ProxyBuilder()
                    .Url("http://localhost:8080")
                    .Filter<AcceptLanguageContextHandler>()
                    .Build<TestContractProxy>();

            Assert.True(result.GetChannel<ProxyBase>().Filters.Any(f=>f is AcceptLanguageContextHandler));
        }

        [Fact]
        public void Filter_Session_Ok()
        {
            TestContractProxy result =
                ClientConfiguration.ProxyBuilder()
                    .Url("http://localhost:8080")
                    .UseSession(true)
                    .Filter<AcceptLanguageContextHandler>()
                    .Build<TestContractProxy>();

            Assert.True(result.GetChannel<ProxyBase>().Filters.Any(f => f is AcceptLanguageContextHandler));
        }

        public class TestContractProxy : ContractProxy, ITestContract
        {
            public TestContractProxy(IChannel channel)
                : base(typeof(ITestContract), channel)
            {
            }

            public string Execute(string param)
            {
                throw new NotSupportedException();
            }
        }
    }
}
