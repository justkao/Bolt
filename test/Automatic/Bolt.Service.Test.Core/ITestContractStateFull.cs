using System;
using System.Runtime.Serialization;

namespace Bolt.Service.Test.Core
{
    public class TestContractProxyFailedException : Exception
    {
        public TestContractProxyFailedException()
        {
        }
    }

    public interface ITestContractStateFull
    {
        [InitSession]
        [AsyncOperation]
        void Init();

        [InitSession]
        [AsyncOperation]
        void InitEx(bool failOperation);

        [AsyncOperation]
        void SetState(string state);

        [AsyncOperation]
        string GetState();

        [AsyncOperation]
        void NextCallWillFailProxy();

        [CloseSession]
        [AsyncOperation]
        void Destroy();
    }

    public class TestContractStateFull : ITestContractStateFull
    {
        private bool _initialized;
        private string _state;
        private bool _failProxy;

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
    }
}
