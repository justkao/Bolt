using System;

namespace Bolt
{
    /// <summary>
    /// Used by <see cref="DefaultExceptionSerializer"/> to wrap the type of exception that occured on server.
    /// </summary>
    public class BoltWrapperException : Exception
    {
        public BoltWrapperException(string message, string originalExceptionType)
            : base(message)
        {
            OriginalExceptionType = originalExceptionType;
        }

        public string OriginalExceptionType { get; private set; }
    }
}