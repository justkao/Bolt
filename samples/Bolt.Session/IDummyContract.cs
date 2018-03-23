using System.Threading.Tasks;

namespace Bolt.Sample.SimpleProxy
{
    public interface IDummyContract
    {
        Task AuthenticateAsync(string name);

        Task<string> ExecuteAsync(string dummyData);
    }
}