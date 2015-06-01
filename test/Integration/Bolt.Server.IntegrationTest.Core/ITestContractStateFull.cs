using System;
using System.Runtime.Serialization;

namespace Bolt.Server.IntegrationTest.Core
{
#if !DNXCORE50
    [Serializable]
#endif
    public class TestContractProxyFailedException : Exception
    {
        public TestContractProxyFailedException()
        {
        }
#if !DNXCORE50
        protected TestContractProxyFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
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

        [AsyncOperation]
        string GetSessionId();
    }
}
