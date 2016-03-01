using System.Threading.Tasks;
using Bolt.Server;
using Microsoft.AspNet.Http;

namespace Bolt.Sample.DistributedSession
{
    public class DummyContract : IDummyContract
    {
        private readonly IHttpSessionProvider _sessionProvider;

        public DummyContract(IHttpSessionProvider sessionProvider)
        {
            _sessionProvider = sessionProvider;
        }

        public async Task<int> IncrementRequestCount()
        {
            await _sessionProvider.Session.LoadAsync();
            int? count = _sessionProvider.Session.GetInt32("data");
            if (count == null)
            {
                _sessionProvider.Session.SetInt32("data", 1);
                return 1;
            }

            _sessionProvider.Session.SetInt32("data", count.Value + 1);
            return count.Value + 1;
        }
    }
}