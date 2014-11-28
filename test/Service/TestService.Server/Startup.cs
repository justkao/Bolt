using Bolt.Core.Serialization;
using Bolt.Server;

using Owin;

using TestService.Core;

namespace TestService.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseBolt(new ServerConfiguration(new JsonSerializer(), new JsonExceptionSerializer()));
            app.UsePersonRepository<PersonRepository>();
        }
    }
}
