using System.Threading.Tasks;

namespace Bolt.Server.InstanceProviders
{
    public interface IContractSession
    {
        string Session { get; }

        object Instance { get; }

        Task CommitAsync();

        Task<bool> DestroyAsync();
    }
}
