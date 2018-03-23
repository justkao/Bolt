using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.ContentProtection
{
    public class DummyContract : IDummyContract
    {
        public Task<string> ExecuteAsync(string dummyData)
        {
            // just send the data back
            return Task.FromResult(dummyData);
        }
    }
}