using System;
using System.Net;
using System.Threading;

namespace Bolt.Client
{
    public class ClientExecutionContext : ExecutionContextBase, IDisposable
    {
        public ClientExecutionContext(ActionDescriptor actionDescriptor, HttpWebRequest request, CancellationToken cancellation, HttpWebResponse response = null)
            : base(actionDescriptor)
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

            GC.SuppressFinalize(this);
        }
    }
}