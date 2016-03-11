using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.ContentProtection
{
    public class DummyContract : IDummyContract
    {
        private readonly ILogger<DummyContract> _logger;

        public DummyContract(ILogger<DummyContract> logger)
        {
            _logger = logger;
        }

        public Task<string> ExecuteAsync(string dummyData)
        {
            // just send the data back
            return Task.FromResult(dummyData);
        }
    }
}