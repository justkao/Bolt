using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bolt.Sample.SimpleProxy
{
    public class DummyContract : IDummyContract
    {
        private readonly ILogger<DummyContract> _logger;
        private readonly ISessionProvider _session;
        private string _authenticatedUser;

        public DummyContract(ILogger<DummyContract> logger, ISessionProvider sessionProvider)
        {
            _logger = logger;
            _session = sessionProvider;
        }

        public Task AuthenticateAsync(string name)
        {
            _logger.LogInformation("{0}: User '{1}' authenticated", _session.SessionId, name);
            _authenticatedUser = name;

            return Task.CompletedTask;
        }

        public Task<string> ExecuteAsync(string dummyData)
        {
            _logger.LogInformation("{0}: Server received '{1}' data from user '{2}'", _session.SessionId, dummyData, _authenticatedUser);

            return Task.FromResult(dummyData);
        }
    }
}