using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bolt;
using Bolt.Server;
using Bolt.Server.InstanceProviders;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Server.WebListener;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
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

            app.UseBolt(b =>
            {
                b.ActionExecutionFilter = new DiagnosticsActionExecutor();
                b.Use<TestContractInvoker>(new InstanceProvider<TestContractImplementation>(), (c) =>
                {
                    c.Options = new BoltServerOptions() {ServerErrorHeader = "Customized-TestContractImplementation"};
                });
            });

            var server = app.Server as ServerInformation;

            Console.WriteLine("Url: {0}", server.Listener.UrlPrefixes.First());
        }


        private class DiagnosticsActionExecutor : IActionExecutionFilter
        {
            public async Task ExecuteAsync(ServerActionContext context, Func<ServerActionContext, Task> next)
            {
                Console.WriteLine("Executing action: {0}", context.Action);
                await next(context);
            }
        }

        private class TextSerializer : ISerializer
        {
            public void Write(Stream stream, object data)
            {
                var buffer = Encoding.UTF8.GetBytes(data.ToString());
                stream.Write(buffer, 0, buffer.Length);
            }

            public object Read(Type type, Stream stream)
            {
                throw new NotImplementedException();
            }

            public string ContentType => "application/json";
        }
    }
}
