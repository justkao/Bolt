using System.Reflection;

using Bolt.Core;

namespace Bolt.Client
{
    public class BoltClientException : BoltException
    {
        public BoltClientException(ClientErrorCode error, MethodInfo action, string url)
            : base($"Execution of action '{action.Name}' failed on client with error '{error}'. Url - '{url}'")
        {
            Action = action;
            Url = url;
            Error = error;
        }

        public ClientErrorCode Error { get; private set; }

        public MethodInfo Action { get; private set; }

        public string Url { get; set; }
    }
}