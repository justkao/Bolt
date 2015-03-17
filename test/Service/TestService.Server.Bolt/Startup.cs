using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Bolt.Server;
using TestService.Core;
using System;
using System.Linq;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using System.Threading;

namespace TestService.Server.Bolt
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ThreadPool.SetMinThreads(100, 100);
            ThreadPool.SetMinThreads(1000, 1000);

            services.AddLogging();
            services.AddOptions();
            services.AddBolt();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<ILoggerFactory>().AddConsole(LogLevel.Information);

            app.UseBolt(b =>
            {
                b.Use<TestContractInvoker>(new StaticInstanceProvider(new TestContractImplementation()));
            });

            var server = app.Server as Microsoft.AspNet.Server.WebListener.ServerInformation;

            Console.WriteLine("Url: {0}", server.Listener.UrlPrefixes.First());
        }
    }
}
