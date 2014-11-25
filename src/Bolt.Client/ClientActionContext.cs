using System;
using System.Net;
using System.Threading;

namespace Bolt.Client
{
    public class ClientActionContext : ExecutionContextBase, IDisposable
    {
        public ClientActionContext(ActionDescriptor actionDescriptor, HttpWebRequest request, Uri server, CancellationToken cancellation)
            : base(actionDescriptor)
        {
            Request = request;
            Server = server;
            Cancellation = cancellation;
        }

        public Uri Server { get; private set; }

        public HttpWebRequest Request { get; private set; }

        public CancellationToken Cancellation { get; private set; }

        public HttpWebResponse Response { get; set; }

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