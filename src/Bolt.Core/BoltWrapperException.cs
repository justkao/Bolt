using System;

namespace Bolt
{
    public class BoltWrapperException : Exception
    {
        public BoltWrapperException(string message, string originalExceptionType) : base(message)
        {
            OriginalExceptionType = originalExceptionType;
        }

        public string OriginalExceptionType { get; private set; }
    }
}