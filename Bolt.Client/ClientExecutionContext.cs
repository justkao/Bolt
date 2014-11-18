using System.Net;

namespace Bolt.Client
{
    public class ClientExecutionContext : ExecutionContextBase
    {
        public ClientExecutionContext(MethodDescriptor methodDescriptor, HttpWebRequest request, HttpWebResponse response = null)
            : base(methodDescriptor)
        {
            Request = request;
            Response = response;
        }

        public HttpWebRequest Request { get; private set; }

        public HttpWebResponse Response { get; internal set; }
    }
}