
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
            app.RegisterEndpoint(configuration, Contracts.PersonRepository, "api", b => ConfigurePersonRepository(b, configuration));
        }

        private void ConfigurePersonRepository(IAppBuilder obj, ServerConfiguration configuration)
        {
            obj.UseStatelessExecutor<PersonRepositoryExecutor, PersonRepository>(configuration, PersonRepositoryDescriptor.Instance);
        }
    }
}
