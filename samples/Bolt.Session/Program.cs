using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.SimpleProxy
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((b) => b.AddFilter(ShouldLog))
                .Build();

            CancellationTokenSource cancellation = new CancellationTokenSource();

            var serverLifetime = host.Services.GetService<IApplicationLifetime>();
            serverLifetime.ApplicationStarted.Register(async () =>
            {
                await TestBolt(host.Services.GetRequiredService<ILogger<Program>>(), serverLifetime.ApplicationStopping);
                cancellation.Cancel();
            });

            host.RunAsync(cancellation.Token).GetAwaiter().GetResult();
        }

        private static bool ShouldLog(string name, LogLevel level)
        {
            if (name?.EndsWith(nameof(DummyContract), StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            return false;
        }

        private static async Task TestBolt(ILogger<Program> logger, CancellationToken cancellationToken)
        {
            // create Bolt proxy
            ClientConfiguration configuration = new ClientConfiguration();
            IDummyContract proxy = configuration.CreateSessionProxy<IDummyContract>("http://localhost:5000");
            await proxy.AuthenticateAsync("Dummy User");

            for (int i = 0; i < 10; i++)
            {
                // we can add timeout and CancellationToken to each Bolt call
                using (new RequestScope(TimeSpan.FromSeconds(5), cancellationToken))
                {
                    try
                    {
                        await proxy.ExecuteAsync(i.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (OperationCanceledException)
                    {
                        // ok
                    }
                    catch (Exception e)
                    {
                        logger.LogInformation("Client: Error {0}", e.Message);
                    }
                }

                try
                {
                    await Task.Delay(10, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
