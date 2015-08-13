using System;

using Bolt.Client.Pipeline;
using Bolt.Pipeline;

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
            var pipeline =
                ClientConfiguration.ProxyBuilder()
                    .Url("http://localhost:8080")
                    .Recoverable(10, TimeSpan.FromSeconds(5))
                    .BuildPipeline();

            var middleware = pipeline.Find<RetryRequestMiddleware>();
            Assert.NotNull(middleware);
            Assert.Equal(10, middleware.Retries);
            Assert.Equal(TimeSpan.FromSeconds(5), middleware.RetryDelay);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void Session_Ok(bool useDistributedSession)
        {
            IPipeline<ClientActionContext> pipeline =
                ClientConfiguration.ProxyBuilder()
                    .Url("http://localhost:8080")
                    .UseSession(useDistributedSession)
                    .BuildPipeline();

            var middleware = pipeline.Find<SessionMiddleware>();
            Assert.NotNull(middleware);
            Assert.Equal(useDistributedSession, middleware.UseDistributedSession);
        }

        [Fact]
        public void RecoverableSession_Ok()
        {
            IPipeline<ClientActionContext> pipeline =
                ClientConfiguration.ProxyBuilder()
                    .Url("http://localhost:8080")
                    .Recoverable(45, TimeSpan.FromSeconds(8))
                    .UseSession()
                    .BuildPipeline();

            Assert.NotNull(pipeline.Find<SessionMiddleware>());
            Assert.NotNull(pipeline.Find<RetryRequestMiddleware>());
        }
    }
}
