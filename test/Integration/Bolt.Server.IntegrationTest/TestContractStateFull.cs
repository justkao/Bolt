using System;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Server.IntegrationTest.Core;
using Bolt.Session;

namespace Bolt.Server.IntegrationTest
{
    public class TestContractStateFull : ITestContractStateFull, ISessionCallback
    {
        private readonly ISessionProvider _sessionProvider;
        private readonly ITestState _testState;
        private bool _initialized;
        private string _state;
        private bool _failProxy;

        public TestContractStateFull(ISessionProvider sessionProvider, ITestState testState)
        {
            _sessionProvider = sessionProvider;
            _testState = testState;
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

        public Task<InitSessionResult> InitSessionAsync(InitSessionParameters parameters, ActionContextBase context, CancellationToken cancellation)
        {
            _initialized = true;
            if (_testState.SessionCallback == null)
            {
                return Task.FromResult(new InitSessionResult());
            }

            return _testState.SessionCallback.Object.InitSessionAsync(parameters, context, cancellation);
        }

        public Task<DestroySessionResult> DestroySessionAsync(DestroySessionParameters parameters, ActionContextBase context, CancellationToken cancellation)
        {
            _initialized = false;
            if (_testState.SessionCallback == null)
            {
                return Task.FromResult(new DestroySessionResult());
            }

            return _testState.SessionCallback.Object.DestroySessionAsync(parameters, context, cancellation);
        }
    }
}