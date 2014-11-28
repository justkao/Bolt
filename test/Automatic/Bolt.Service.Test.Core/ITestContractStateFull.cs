using System;

namespace Bolt.Service.Test.Core
{
    public interface ITestContractStateFull
    {
        [InitSession]
        void Init();

        void SetState(string state);

        [AsyncOperation]
        string GetState();

        [CloseSession]
        void Destroy();
    }

    public class TestContractStateFull : ITestContractStateFull
    {
        private bool _initialized;
        private string _state;

        public void Init()
        {
            _initialized = true;
        }

        public void SetState(string state)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Not initialized");
            }

            _state = state;
        }

        public string GetState()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Not initialized");
            }

            return _state;
        }

        public void Destroy()
        {
            _initialized = false;
        }
    }
}
