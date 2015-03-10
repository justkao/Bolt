using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Bolt.Server;
using TestService.Core;

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
            // services.ConfigureBoltOptions(a => { });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseBolt(b => {
                b.UseTestContract(new TestContractImplementation());
            });
        }
    }
}
