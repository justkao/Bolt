using Microsoft.AspNetCore.Http;

namespace Bolt.Server
{
    public interface IHttpSessionProvider : ISessionProvider
    {
        ISession Session { get; }
    }
}
