using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Bolt.Server;
using TestService.Core;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;

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
            services.ConfigureBoltOptions(a =>
            {
                a.Prefix = "boltex";
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<ILoggerFactory>().AddConsole(LogLevel.Verbose);

            app.UseBolt(b => {
                b.UseTestContract(new TestContractImplementation());
            });
        }
    }
}
