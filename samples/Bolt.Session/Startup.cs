using Bolt.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.SimpleProxy
{
    public partial class Program
    {
        public class Startup
        {
            public void ConfigureServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddRouting();
                serviceCollection.AddBolt();
                serviceCollection.AddOptions();
                serviceCollection.AddLogging();
                serviceCollection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            }

            public void Configure(IApplicationBuilder builder)
            {
                ILoggerFactory factory = builder.ApplicationServices.GetRequiredService<ILoggerFactory>();

                // we will add IDummyContract endpoint to Bolt
                builder.UseBolt(r => r.UseMemorySession<IDummyContract, DummyContract>());
            }
        }
    }
}
