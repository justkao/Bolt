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
using Bolt.Server.Filters;
using Microsoft.AspNet.Http;

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
                b.Filters.Add(new DiagnosticsActionExecutor());
                b.Filters.Add(new DiagnosticsActionExecutor2());

                b.Use(new TestContractActions(), new InstanceProvider<TestContractImplementation>(), (c) =>
                {
                    c.Configuration.ExceptionWrapper = new TextExceptionWrapper();
                    c.Configuration.Options = new BoltServerOptions() {ServerErrorHeader = "Customized-TestContractImplementation"};
                });
            });

            var server = app.Server as ServerInformation;

            Console.WriteLine("Url: {0}", server.Listener.UrlPrefixes.First());
        }

        private class DiagnosticsActionExecutor : IActionExecutionFilter
        {
            public int Order
            {
                get
                {
                    return 1;
                }
            }

            public async Task ExecuteAsync(ServerActionContext context, Func<ServerActionContext, Task> next)
            {
                Console.WriteLine(GetType().FullName);
                await next(context);
            }
        }

        private class DiagnosticsActionExecutor2 : IActionExecutionFilter
        {
            public int Order => 0;

            public async Task ExecuteAsync(ServerActionContext context, Func<ServerActionContext, Task> next)
            { 
                Console.WriteLine(GetType().FullName);
                context.IsResponseSend = true;
                await next(context);
            }
        }

        private class TextExceptionWrapper : ExceptionWrapper<string>
        {
            protected override Exception UnwrapCore(string wrappedException)
            {
                return new Exception(wrappedException);
            }

            protected override string WrapCore(Exception exception)
            {
                return exception.Message;
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
