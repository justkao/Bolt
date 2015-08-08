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

        protected TestContractProxyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TestContractProxyFailedException(string message) : base(message)
        {
        }

        public TestContractProxyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}