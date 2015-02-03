using Bolt;
using Bolt.Helpers;
using Bolt.Server;

using Owin;

using TestService.Core;

namespace TestService.IIS.Bolt
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseBolt(new ServerConfiguration(new XmlSerializer(), new JsonExceptionSerializer(new XmlSerializer())));
            app.UseTestContract<TestContractImplementation>();
        }
    }
}
