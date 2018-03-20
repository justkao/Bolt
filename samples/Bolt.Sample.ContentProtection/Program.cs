using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client;
using Bolt.Serialization;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.ContentProtection
{
    public partial class Program
    {
        private static int Result;

        public static int Main(string[] args)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();

            var server = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000")
                .Build();

            var serverLifetime = server.Services.GetService<IApplicationLifetime>();
            serverLifetime.ApplicationStarted.Register(async () =>
            {
                await TestBoltAsync(server.Services.GetRequiredService<ILogger<Program>>(), server.Services.GetRequiredService<ISerializer>(), serverLifetime.ApplicationStopping);
                cancellation.Cancel();
            });

            server.RunAsync(cancellation.Token).GetAwaiter().GetResult();
            return Result;
        }

        private static async Task TestBoltAsync(ILogger<Program> logger, ISerializer serializer, CancellationToken cancellationToken)
        {
            // create Bolt proxy
            ClientConfiguration configuration = new ClientConfiguration() { Serializer = serializer };
            IDummyContract proxy = configuration.CreateProxy<IDummyContract>("http://localhost:5000");

            logger.LogInformation("Testing Bolt Proxy ... ");

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    string payload = Guid.NewGuid().ToString();
                    string result = await proxy.ExecuteAsync(payload);
                    if (result != payload)
                    {
                        throw new InvalidOperationException("Wrong data received.");
                    }
                }
                catch (Exception e)
                {
                    logger.LogInformation("Client: Error {0}", e.Message);
                    Result = 1;
                }

                logger.LogInformation("---------------------------------------------");

                try
                {
                    await Task.Delay(10, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            Console.WriteLine("Test finished. Application will now exit ... ");
        }
    }
}
