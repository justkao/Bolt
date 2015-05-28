using Microsoft.AspNet.Http;
using System;
using System.Threading.Tasks;

namespace Bolt.Server.InstanceProviders
{
    public interface ISessionFactory
    {
        event EventHandler<SessionTimeoutEventArgs> SessionTimeouted;

        Task<IContractSession> GetExistingAsync(HttpContext context);

        Task<IContractSession> CreateAsync(HttpContext context, object instance);
    }
}
