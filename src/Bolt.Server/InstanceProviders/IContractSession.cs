using System.Threading.Tasks;
using Bolt.Session;

namespace Bolt.Server.InstanceProviders
{
    public interface IContractSession : ISessionProvider
    {
        object Instance { get; }

        Task CommitAsync();

        Task DestroyAsync();
    }
}
