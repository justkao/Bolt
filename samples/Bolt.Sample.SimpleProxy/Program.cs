using System;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client;
using Bolt.Server;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.SimpleProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHostingEngine server =
                new WebHostBuilder()
                    .UseServer("Microsoft.AspNet.Server.Kestrel")
                    .UseStartup<Startup>()
                    .Build();

            IApplication app = server.Start();
            var serverLifetime = app.Services.GetService<IApplicationLifetime>();
            serverLifetime.ApplicationStarted.Register(() =>
            {
                TestBolt(app.Services.GetRequiredService<ILogger<Program>>(), serverLifetime.ApplicationStopping);
            });

            Console.WriteLine("Server running ... ");
            Console.ReadLine();
            app.Dispose();
        }

        public class Startup
        {
            public void ConfigureServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddBolt();
                serviceCollection.AddOptions();
                serviceCollection.AddLogging();
            }

            public void Configure(IApplicationBuilder builder)
            {
                ILoggerFactory factory = builder.ApplicationServices.GetRequiredService<ILoggerFactory>();
                factory.MinimumLevel = LogLevel.Debug;
                factory.AddConsole();

                // we will add IDummyContract endpoint to Bolt
                builder.UseBolt(r => r.Use<IDummyContract, DummyContract>());
            }
        }


        private static async void TestBolt(ILogger<Program> logger, CancellationToken cancellationToken)
        {
            await Task.Delay(2500, cancellationToken);

            // create Bolt proxy
            ClientConfiguration configuration = new ClientConfiguration();
            IDummyContract proxy = configuration.CreateProxy<IDummyContract>("http://localhost:5000");

            logger.LogInformation("Testing Bolt Proxy ... ");

            for (int i = 0; i < 10; i++)
            {
                logger.LogInformation("Client: Sending {0}", i);
                // we can add timeout and CancellationToken to each Bolt call
                using (new RequestScope(TimeSpan.FromSeconds(5), cancellationToken))
                {
                    try
                    {
                        await proxy.ExecuteAsync(i.ToString());
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

                logger.LogInformation("Client: Received {0}", i);
                logger.LogInformation("--------------------------------------------");

                try
                {
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            Console.WriteLine("Test finished. Press any key to exit the application ... ");
        }
    }
}
