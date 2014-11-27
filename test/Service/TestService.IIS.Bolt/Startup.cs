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
            app.UseBolt(new ServerConfiguration(new ProtocolBufferSerializer(), new JsonExceptionSerializer()));
            app.UsePersonRepository<PersonRepository>();
        }
    }
}
