using System;
using System.Runtime.Serialization;

namespace Bolt.Server.IntegrationTest.Core
{
    [Serializable]
    public class TestContractProxyFailedException : Exception
    {
        public TestContractProxyFailedException()
        {
        }

        public TestContractProxyFailedException(string message) : base(message)
        {
        }

        public TestContractProxyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TestContractProxyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}