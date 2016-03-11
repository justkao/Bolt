using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Bolt.Client;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.Configuration;

namespace Bolt.Sample.DistributedSession
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                StartClient();
            }
            else
            {
                int port = 5000;
                if (args[0] == "server2")
                {
                    port = 5001;
                    Console.WriteLine("Starting Server 2 ... ");
                }
                else
                {
                    Console.WriteLine("Starting Server 1 ... ");
                }

                StartServer(port);
            }

            Console.WriteLine("Press any key to exit ... ");
            Console.ReadLine();
        }

        private static void StartClient()
        {
            Console.WriteLine("Starting Client ... ");

            IServerProvider serverProvider = new RandomServerProvider(new Uri("http://localhost:5000"), new Uri("http://localhost:5001"));
            ClientConfiguration configuration = new ClientConfiguration();

            IDummyContract proxy1 = CreateProxy(configuration, serverProvider);
            TestProxy(proxy1).GetAwaiter().GetResult();

            IDummyContract proxy2 = CreateProxy(configuration, serverProvider);
            TestProxy(proxy2).GetAwaiter().GetResult();

            Console.WriteLine();
        }

        private static IDummyContract CreateProxy(ClientConfiguration configuration, IServerProvider serverProvider)
        {
            return configuration.ProxyBuilder()
                .UseSession(true)
                .Recoverable(10, TimeSpan.FromSeconds(1))
                .Url(serverProvider).OnSending(
                    async (next, context) =>
                    {
                        Console.WriteLine("Sending request to server: {0}:{1}", context.Request.RequestUri.Host, context.Request.RequestUri.Port);
                        try
                        {
                            await next(context);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Failed to send request to server: {0}:{1}", context.Request.RequestUri.Host, context.Request.RequestUri.Port);
                            throw;
                        }
                    })
                .Build<IDummyContract>();
        }

        public static async Task TestProxy(IDummyContract proxy)
        {
            Console.WriteLine("Sending {0} requests to multiple servers with session", 10);

            int numRequests = 0;
            for (int i = 0; i < 10; i++)
            {
                numRequests = await proxy.IncrementRequestCount();
                Console.WriteLine();

                await Task.Delay(TimeSpan.FromSeconds(0.2));
            }

            if (numRequests != 10)
            {
                throw new InvalidOperationException(
                    $"Distributed session failed. Expected number of requests processed: {10}, Actual: {numRequests}");
            }

            Console.WriteLine("Test finished. Press any key to continue ... ");
        }

        private class RandomServerProvider : IServerProvider
        {
            private readonly Uri[] _servers;

            public RandomServerProvider(params Uri[] servers)
            {
                _servers = servers;
            }

            public ConnectionDescriptor GetServer()
            {
                var server = _servers[new Random().Next(0, _servers.Length)];
                return new ConnectionDescriptor(server);
            }

            public void OnServerUnavailable(Uri server)
            {
            }
        }

        private static void StartServer(int port)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new[]
            {new KeyValuePair<string, string>("server.urls", $"http://localhost:{port}")});

            IHostingEngine server =
                new WebHostBuilder(configurationBuilder.Build())
                    .UseServer("Microsoft.AspNet.Server.Kestrel")
                    .UseStartup<Startup>()
                    .Build();

            server.Start();
        }
    }
}
