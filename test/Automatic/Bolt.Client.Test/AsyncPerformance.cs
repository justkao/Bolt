using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bolt.Client.Test
{
    public class AsyncPerformance
    {
        [InlineData(true, 1000000)]
        [InlineData(false, 1000000)]
        [Theory]
        public async Task TestProxyPerformance(bool async, int cnt)
        {
            ClientConfiguration configuration = new ClientConfiguration();
            configuration.HttpMessageHandler = new DummyMessageHandler();

            TestContractProxy proxy = new TestContractProxy(configuration.ProxyBuilder().Url(new Uri("http://ldummy.org")).BuildPipeline());
            proxy.DoNothing();
            await proxy.DoNothingAsync();

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < cnt; i++)
            {
                if (async)
                {
                    await proxy.DoNothingAsync();
                }
                else
                {
                    proxy.DoNothing();
                }
            }

            Console.WriteLine("{0} operations executed in {1}ms, Async: {2}", cnt, watch.ElapsedMilliseconds, async);
        }

        private class DummyMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage());
            }
        }
    }
}
