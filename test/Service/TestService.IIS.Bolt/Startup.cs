using Bolt.Core.Serialization;
using Bolt.Server;

using Owin;

using TestService.Core;

namespace TestService.IIS.Bolt
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ServerConfiguration configuration = new ServerConfiguration(new ProtocolBufferSerializer(), new JsonExceptionSerializer());
            app.MapContract(PersonRepositoryDescriptor.Default, configuration, "api", b => ConfigurePersonRepository(b, configuration));
        }

        private void ConfigurePersonRepository(IAppBuilder obj, ServerConfiguration configuration)
        {
            obj.UseContractInvoker<PersonRepositoryInvoker>(configuration,
                PersonRepositoryDescriptor.Default, new StaticInstanceProvider(new PersonRepository()));
        }
    }
}
