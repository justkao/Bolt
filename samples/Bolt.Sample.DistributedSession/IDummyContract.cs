using System.Threading.Tasks;

namespace Bolt.Sample.DistributedSession
{
    public interface IDummyContract
    {
        Task<int> IncrementRequestCount();
    }
}
