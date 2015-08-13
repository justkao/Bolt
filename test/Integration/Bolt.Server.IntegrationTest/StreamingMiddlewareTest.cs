using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Client.Proxy;
using Bolt.Server.Pipeline;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class StreamingMiddlewareTest : IntegrationTestBase, ITestContext
    {
        public StreamingMiddlewareTest()
        {
            ClientConfiguration.UseDynamicProxy();
            InstanceProvider.CurrentInstance = Callback.Object;
        }

        public object Instance => this;

        public MockInstanceProvider InstanceProvider = new MockInstanceProvider();

        public Mock<IStreamingService> Callback  = new Mock<IStreamingService>();

        [Fact]
        public async Task SendHeader_EnsureReceivedOnServer()
        {
            var proxy = CreateProxy();
            var content = new StringContent("test");
            content.Headers.Add("client-test", "client-value");

            Callback.Setup(s => s.SendAsync(It.IsAny<HttpContent>())).Callback<HttpContent>(c =>
            {
                Assert.NotNull(c.Headers.GetValues("client-test").First());
                Assert.Equal("client-value", c.Headers.GetValues("client-test").First());
            }).Returns(Task.FromResult(true));

            await proxy.SendAsync(content);

            Callback.Verify();
        }

        [Fact]
        public async Task ReceiveHttpContent_EnsureHeaders()
        {
            var proxy = CreateProxy();

            Callback.Setup(s => s.ReceiveAsync()).Returns(() =>
            {
                var content = new StringContent("test");
                content.Headers.Add("server-test", "server-value");
                return Task.FromResult((HttpContent)content);
            });

            HttpContent result = await proxy.ReceiveAsync();
            Assert.NotNull(result.Headers.GetValues("server-test").First());
            Assert.Equal("server-value", result.Headers.GetValues("server-test").First());

            Callback.Verify();
        }

        [Fact]
        public async Task SendNullContent_Throws()
        {
            var proxy = CreateProxy();
            BoltClientException error = await Assert.ThrowsAsync<BoltClientException>(() => proxy.SendAsync(null));
            Assert.Equal(ClientErrorCode.SerializeParameters, error.Error);
        }


        /*
        [Fact]
        public async Task SendHttpContent_EnsureReceived()
        {
            var proxy = CreateProxy();
            var content = await proxy.DuplicateAsync(new StringContent("test"));
        }
        */

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddInstance<ITestContext>(this);
            base.ConfigureServices(services);
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt(h => h.Use<IStreamingService>(InstanceProvider, c =>
            {
                c.Use<HandleErrorMiddleware>();
                c.Use<StreamingMiddleware>();
                c.Use<SerializationMiddleware>();
                c.Use<InstanceProviderMiddleware>();
                c.Use<ActionInvokerMiddleware>();
            }));
        }

        public IStreamingService CreateProxy()
        {
            return ClientConfiguration.ProxyBuilder().UseStreaming().Url(ServerUrl).Build<IStreamingService>();
        }

        public interface IStreamingService
        {
            Task SendAsync(HttpContent content);

            Task<HttpContent> ReceiveAsync();
        }
    }
}
