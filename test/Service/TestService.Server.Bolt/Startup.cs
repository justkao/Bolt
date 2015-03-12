using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Bolt.Server;
using TestService.Core;
using System;
using System.Linq;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using Microsoft.AspNet.Security.DataProtection;

namespace TestService.Server.Bolt
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddOptions();
            services.AddBolt();
            services.AddDataProtection();
            services.ConfigureBoltOptions(a =>
            {
                a.Prefix = "bolt";
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<ILoggerFactory>().AddConsole(LogLevel.Verbose);

            app.UseBolt(b => {
                b.UseTestContract(new TestContractImplementation());
            });

            var server = app.Server as Microsoft.AspNet.Server.WebListener.ServerInformation;

            Console.WriteLine("Url: {0}", server.Listener.UrlPrefixes.First());
        }
    }
}
