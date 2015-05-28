using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace Bolt.Server.InstanceProviders
{
    public interface ISessionFactory
    {
        Task<IContractSession> TryGetAsync(HttpContext context);

        Task<IContractSession> GetAsync(HttpContext context);

        Task<IContractSession> CreateAsync(HttpContext context, object instance);
    }
}
