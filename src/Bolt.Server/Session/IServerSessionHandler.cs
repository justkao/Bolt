using Microsoft.AspNetCore.Http;

namespace Bolt.Server.Session
{
    public interface IServerSessionHandler
    {
        string GetIdentifier(HttpContext context);

        string Initialize(HttpContext context);

        void Destroy(HttpContext context);
    }
}