using Bolt.Serialization;
using Bolt.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.ContentProtection
{
    public partial class Program
    {
        public class Startup
        {
            public void ConfigureServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddRouting();
                serviceCollection.AddBolt();
                serviceCollection.Replace(
                    new ServiceDescriptor(
                        typeof(ISerializer),
                        (p) =>
                        {
                            IDataProtector protector = p.GetDataProtector("content protection");
                            ILoggerFactory factory = p.GetService<ILoggerFactory>();
                            return new ProtectedSerializer(protector, factory);
                        },
                        ServiceLifetime.Singleton));

                serviceCollection.AddOptions();
                serviceCollection.AddLogging();
                serviceCollection.AddDataProtection();
            }

            public void Configure(IApplicationBuilder builder)
            {
                // we will add IDummyContract endpoint to Bolt
                builder.UseBolt(r => r.Use<IDummyContract, DummyContract>());
            }
        }
    }
}
