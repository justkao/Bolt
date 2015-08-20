using System;
using System.Diagnostics;
using Bolt.Client;
using Bolt.Client.Pipeline;
using Bolt.Client.Proxy;
using Bolt.Pipeline;
using Bolt.Server.IntegrationTest.Core;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class DynamicProxyStateLessTest : StateLessTest
    {
        public DynamicProxyStateLessTest()
        {
            ClientConfiguration.ProxyFactory = new DynamicProxyFactory(); 
        }

        [Fact]
        public override void CreateProxyPerformance()
        {
            for (int i = 0; i < 10; i++)
            {
                (ClientConfiguration.ProxyBuilder()
                    .Url(ServerUrl)
                    .Recoverable(10, TimeSpan.FromSeconds(1))
                    .Build<ITestContract>() as IDisposable)?.Dispose();
            }

            int cnt = 10000;

            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < cnt; i++)
            {
                (ClientConfiguration.ProxyBuilder()
                    .Url(ServerUrl)
                    .Recoverable(10, TimeSpan.FromSeconds(1))
                    .Build<ITestContract>() as IDisposable)?.Dispose();
            }

            System.Console.WriteLine("Creating {0} dynamic proxies by ProxyBuilder has taken {1}ms", 10000, watch.ElapsedMilliseconds);
        }


        public override ITestContractAsync CreateChannel(IClientPipeline pipeline = null)
        {
            return ClientConfiguration.ProxyFactory.CreateProxy<ITestContractAsync>(pipeline ?? CreatePipeline());
        }
    }
}