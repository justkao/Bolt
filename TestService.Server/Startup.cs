
using Bolt.Server;
using Owin;

using TestService.Core;

namespace TestService.Server
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
        }
    }
}
