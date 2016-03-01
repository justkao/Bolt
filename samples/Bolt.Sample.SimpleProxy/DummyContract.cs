using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.SimpleProxy
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
            _logger.LogInformation("Server: {0}", dummyData);

            return Task.FromResult(dummyData);
        }
    }
}