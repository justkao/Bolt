using System;
using System.Threading.Tasks;

using Microsoft.AspNet.Http;

namespace Bolt.Server.Session
{
    public interface ISessionFactory
    {
        Task<IContractSession> GetExistingAsync(HttpContext context, Func<Task<object>> instanceFactory);

        Task<IContractSession> CreateAsync(HttpContext context, object instance);
    }
}
