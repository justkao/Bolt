using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace Bolt.Server.InstanceProviders
{
    public interface ISessionFactory
    {
        Task<IContractSession> GetExistingAsync(HttpContext context);

        Task<IContractSession> CreateAsync(HttpContext context, object instance);
    }
}
