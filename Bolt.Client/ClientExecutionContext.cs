using System;
using System.Net;
using System.Threading;

namespace Bolt.Client
{
    public class ClientExecutionContext : ExecutionContextBase, IDisposable
    {
        public ClientExecutionContext(MethodDescriptor methodDescriptor, HttpWebRequest request, CancellationToken cancellation, HttpWebResponse response = null)
            : base(methodDescriptor)
        {
            Request = request;
            Cancellation = cancellation;
            Response = response;
        }

        public HttpWebRequest Request { get; private set; }

        public CancellationToken Cancellation { get; private set; }

        public HttpWebResponse Response { get; internal set; }

        public void Dispose()
        {
            if (Response != null)
            {
                Response.Dispose();
            }
        }
    }
}