using Microsoft.AspNet.Http.Features;

namespace Bolt.Server
{
    public interface IHttpSessionProvider : ISessionProvider
    {
        ISession Session { get; }
    }
}
