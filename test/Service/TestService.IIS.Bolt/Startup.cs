using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Bolt;
using Bolt.Server;

using Owin;

using TestService.Core;
using TestService.Core.Parameters;

namespace TestService.IIS.Bolt
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ServerConfiguration configuration = new ServerConfiguration();

            app.RegisterEndpoint<IPersonRepository, PersonRepository, PersonRepositoryExecutor>(
                Contracts.PersonRepository,
                "/" + Servers.Prefix + "/",
                configuration,
                new SimpleInstanceProvider<PersonRepository>(c => new PersonRepository()));

            app.Run(context =>
            {
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("Hello, world.");
            });
        }
    }
}
