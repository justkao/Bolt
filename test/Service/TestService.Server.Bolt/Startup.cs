using System.Threading;

using Bolt.Server;

using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

using TestService.Core;

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
            app.UseBolt(
                b =>
                    {
                        b.UseStateLess<ITestContract, TestContractImplementation>();
                    });
        }
    }
}
