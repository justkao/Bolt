using Bolt.Core;
using System.Threading.Tasks;

namespace Bolt.Server.InstanceProviders
{
    public interface IContractSession : ISessionProvider
    {
        object Instance { get; }

        Task CommitAsync();

        Task DestroyAsync();
    }
}
