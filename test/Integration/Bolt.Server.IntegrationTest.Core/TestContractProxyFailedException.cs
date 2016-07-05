using System;
using System.Runtime.Serialization;

namespace Bolt.Server.IntegrationTest.Core
{
#if NET451
    [Serializable]
#endif
    public class TestContractProxyFailedException : Exception
    {
        public TestContractProxyFailedException()
        {
        }

#if NET451
        protected TestContractProxyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
        public TestContractProxyFailedException(string message) : base(message)
        {
        }

        public TestContractProxyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}