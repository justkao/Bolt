using Bolt;
using Bolt.Helpers;
using Bolt.Server;

using Owin;

using TestService.Core;

namespace TestService.Server
{
    public class Rep : TestContractImplementation
    {
        public override void ThrowsCustom()
        {
            throw new CustomException("test");
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseBolt(new ServerConfiguration(new XmlSerializer(), new JsonExceptionSerializer(new XmlSerializer())));
            app.UseTestContract<TestContractImplementation>();
        }
    }
}
