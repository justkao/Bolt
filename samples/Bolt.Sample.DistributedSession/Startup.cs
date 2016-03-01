using Bolt.Server;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.DistributedSession
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSqlServerCache(
                (o) =>
                    {
                        o.ConnectionString = "Server=localhost;Database=BoltDistributedCachedTestDb;user=sa;password=sa;";
                        o.SchemaName = "dbo";
                        o.TableName = "Entries";
                    });

            services.AddLogging();
            services.AddBolt();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<IDistributedCache>().Connect();
            app.ApplicationServices.GetRequiredService<ILoggerFactory>().AddConsole().MinimumLevel = LogLevel.Debug;
            app.UseBolt(
                b =>
                    {
                        b.UseDistributedSession<IDummyContract, DummyContract>();
                    });
        }
    }
}
