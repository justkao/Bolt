using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client;
using Bolt.Serialization;
using Bolt.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.ContentProtection
{
    public class Program
    {
        private static readonly AutoResetEvent TestingFinished = new AutoResetEvent(false);
        private static int Result;

        public static int Main(string[] args)
        {
            var server =
                new WebHostBuilder()
                    .UseKestrel()
                    .UseStartup<Startup>()
                    .Build();

            var serverLifetime = server.Services.GetService<IApplicationLifetime>();
            serverLifetime.ApplicationStarted.Register(() =>
            {
                TestBolt(server.Services.GetRequiredService<ILogger<Program>>(), server.Services.GetRequiredService<ISerializer>(), serverLifetime.ApplicationStopping);
            });

            server.Run();
            Console.WriteLine("Server running ... ");
            TestingFinished.WaitOne();
            server.Dispose();

            return Result;
        }

        public class Startup
        {
            public void ConfigureServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddRouting();
                serviceCollection.AddBolt();
                serviceCollection.Replace(new ServiceDescriptor(typeof (ISerializer), (p) =>
                {
                    IDataProtector protector = p.GetDataProtector("content protection");
                    ILoggerFactory factory = p.GetService<ILoggerFactory>();
                    return new ProtectedSerializer(protector, factory);
                }, ServiceLifetime.Singleton));

                serviceCollection.AddOptions();
                serviceCollection.AddLogging();
                serviceCollection.AddDataProtection();
            }

            public void Configure(IApplicationBuilder builder)
            {
                ILoggerFactory factory = builder.ApplicationServices.GetRequiredService<ILoggerFactory>();
                factory.AddConsole(LogLevel.Debug);

                // we will add IDummyContract endpoint to Bolt
                builder.UseBolt(r => r.Use<IDummyContract, DummyContract>());
            }
        }

        private static async void TestBolt(ILogger<Program> logger, ISerializer serializer, CancellationToken cancellationToken)
        {
            // create Bolt proxy
            ClientConfiguration configuration = new ClientConfiguration() {Serializer = serializer};
            IDummyContract proxy = configuration.CreateProxy<IDummyContract>("http://localhost:5000");

            logger.LogInformation("Testing Bolt Proxy ... ");

            for (int i = 0; i < 2; i++)
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

            Console.WriteLine("Test finished. Press any key to exit the application ... ");
            TestingFinished.Set();
        }
    }
}
