using System.Threading.Tasks;

namespace Bolt.Sample.SimpleProxy
{
    public interface IDummyContract
    {
        Task<string> ExecuteAsync(string dummyData);
    }
}