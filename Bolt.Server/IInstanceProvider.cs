using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IInstanceProvider
    {
        Task<T> GetInstanceAsync<T>(ServerExecutionContext context);
    }
}