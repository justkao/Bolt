using System.Threading.Tasks;

namespace Bolt.Server.InstanceProviders
{
    public interface ISessionStore
    {
        Task<object> GetAsync(string sessionId);

        Task SetAsync(string sessionId, object sessionObject);

        Task UpdateAsync(string sessionId, object sessionObject);

        Task<bool> RemoveAsync(string sessionId);
    }
}
