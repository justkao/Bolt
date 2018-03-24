using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bolt.Server.Session
{
    public interface ISessionFactory
    {
        Task<IContractSession> GetExistingAsync(HttpContext context, Func<object> instanceFactory);

        Task<IContractSession> CreateAsync(HttpContext context, Func<object> instanceFactory);
    }
}
