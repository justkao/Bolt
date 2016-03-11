using System.Threading.Tasks;

namespace Bolt.Sample.ContentProtection
{
    public interface IDummyContract
    {
        Task<string> ExecuteAsync(string dummyData);
    }
}