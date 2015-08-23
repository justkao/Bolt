using System.Threading.Tasks;
using Bolt.Server.IntegrationTest.Core;
using Bolt.Session;

namespace Bolt.Server.IntegrationTest
{
    public class TestContractStateFull : ITestContractStateFull
    {
        private readonly ISessionProvider _sessionProvider;
        private string _state;
        private bool _failProxy;

        public TestContractStateFull(ISessionProvider sessionProvider)
        {
            _sessionProvider = sessionProvider;
        }

        public Task<string> OpenSessionAsync(string arguments)
        {
            return Task.FromResult(arguments);
        }

        public void SetState(string state)
        {
            if (_failProxy)
            {
                _failProxy = false;
                throw new TestContractProxyFailedException();
            }

            _state = state;
        }

        public string GetState()
        {
            if (_failProxy)
            {
                _failProxy = false;
                throw new TestContractProxyFailedException();
            }

            return _state;
        }

        public void NextCallWillFailProxy()
        {
            _failProxy = true;
        }

        public void Destroy()
        {
        }

        public string GetSessionId()
        {
            return _sessionProvider.SessionId;
        }
    }
}