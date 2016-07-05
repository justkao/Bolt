using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Server.Pipeline;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class StreamingMiddlewareTest : IntegrationTestBase, ITestContext
    {
        public StreamingMiddlewareTest()
        {
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

        [InlineData("")]
        [InlineData("test")]
        [InlineData("some other value")]
        [Theory]
        public async Task SendString_EnsureReceivedOnServer(string rawContent)
        {
            var proxy = CreateProxy();
            HttpContent content = new StringContent(rawContent);
            Callback.Setup(s => s.SendAsync(It.IsAny<HttpContent>())).Callback<HttpContent>(c =>
            {
                Assert.Equal(rawContent, c.ReadAsStringAsync().GetAwaiter().GetResult());

            }).Returns(Task.FromResult(true));

            await proxy.SendAsync(content);

            Callback.Verify();
        }


        [InlineData(0)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [Theory]
        public async Task SendByteArrayContent_EnsureReceivedOnServer(int length)
        {
            var proxy = CreateProxy();
            HttpContent content = new ByteArrayContent(new byte[length]);
            Callback.Setup(s => s.SendAsync(It.IsAny<HttpContent>())).Callback<HttpContent>(c =>
            {
                Assert.Equal(length, c.ReadAsByteArrayAsync().GetAwaiter().GetResult().Length);

            }).Returns(Task.FromResult(true));

            await proxy.SendAsync(content);

            Callback.Verify();
        }

        [InlineData(0)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [Theory]
        public async Task SendStreamContent_EnsureReceivedOnServer(int length)
        {
            var proxy = CreateProxy();
            HttpContent content = new StreamContent(new MemoryStream(new byte[length]));
            Callback.Setup(s => s.SendAsync(It.IsAny<HttpContent>())).Callback<HttpContent>(c =>
            {
                MemoryStream stream = new MemoryStream();
                c.ReadAsStreamAsync().GetAwaiter().GetResult().CopyTo(stream);
                Assert.Equal(length, stream.ToArray().Length);

            }).Returns(Task.FromResult(true));

            await proxy.SendAsync(content);

            Callback.Verify();
        }

        [Fact]
        public async Task SendStreamContent_EnsureStreamedOnServer()
        {
            var proxy = CreateProxy();
            bool serverReached = false;
            int reads = 0;

            CancellationTokenSource cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            Mock<IStreamCallback> streamCallback = new Mock<IStreamCallback>();
            streamCallback.Setup(s => s.OnRead()).Returns(() =>
            {
                cancellation.Token.WaitHandle.WaitOne();
                try
                {
                    if (reads == 0)
                    {
                        return 10;
                    }
                    return 0;
                }
                finally
                {
                    reads++;
                }
            });

            Callback.Setup(s => s.SendAsync(It.IsAny<HttpContent>())).Callback<HttpContent>(c =>
            {
                cancellation.Cancel();
                serverReached = true;
                byte[] bytesRead = c.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                Assert.Equal(10, bytesRead.Length);

            }).Returns(Task.FromResult(true));

            HttpContent content = new StreamContent(new ValidatingStream(new byte[100], streamCallback.Object), 5);
            await proxy.SendAsync(content);

            Callback.Verify();

            Assert.True(serverReached, "The content was not streamed.");
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

        private class ValidatingStream : MemoryStream
        {
            private readonly IStreamCallback _streamCallback;

            public ValidatingStream(byte[] content, IStreamCallback streamCallback) : base(content)
            {
                _streamCallback = streamCallback;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _streamCallback.OnRead();
            }
        }

        public interface IStreamCallback
        {
            int OnRead();
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
            services.AddSingleton<ITestContext>(p => this);
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
