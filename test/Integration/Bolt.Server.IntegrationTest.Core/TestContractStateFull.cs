using System;
using Bolt.Core;

namespace Bolt.Server.IntegrationTest.Core
{
    public class TestContractStateFull : ITestContractStateFull
    {
        private readonly ISessionProvider _sessionProvider;
        private bool _initialized;
        private string _state;
        private bool _failProxy;

        public TestContractStateFull(ISessionProvider sessionProvider)
        {
            _sessionProvider = sessionProvider;
        }

        public void Init()
        {
            _initialized = true;
        }

        public void InitEx(bool failOperation)
        {
            if (failOperation)
            {
                throw new InvalidOperationException("Forced failure.");
            }
        }

        public void SetState(string state)
        {
            if (_failProxy)
            {
                _failProxy = false;
                throw new TestContractProxyFailedException();
            }

            if (!_initialized)
            {
                throw new InvalidOperationException("Not initialized");
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

            if (!_initialized)
            {
                throw new InvalidOperationException("Not initialized");
            }

            return _state;
        }

        public void NextCallWillFailProxy()
        {
            _failProxy = true;
        }

        public void Destroy()
        {
            _initialized = false;
        }

        public string GetSessionId()
        {
            return _sessionProvider.SessionId;
        }
    }
}