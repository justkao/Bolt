using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IAsyncContractInitializer
    {
        Task InitAsync(ServerExecutionContext context);
    }
}