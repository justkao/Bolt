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
            app.UseBolt(new ServerConfiguration(new ProtocolBufferSerializer(), new JsonExceptionSerializer(new ProtocolBufferSerializer())));
            app.UseTestContract<TestContractImplementation>();
        }
    }
}
