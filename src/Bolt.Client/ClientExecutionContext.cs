using System;
using System.Net;
using System.Threading;

namespace Bolt.Client
{
    public class ClientExecutionContext : ExecutionContextBase, IDisposable
    {
        public ClientExecutionContext(ActionDescriptor actionDescriptor, HttpWebRequest request, Uri server, CancellationToken cancellation, IConnectionProvider connectionProvider)
            : base(actionDescriptor)
        {
            Request = request;
            Server = server;
            Cancellation = cancellation;
            ConnectionProvider = connectionProvider;
        }

        public Uri Server { get; private set; }

        public HttpWebRequest Request { get; private set; }

        public CancellationToken Cancellation { get; private set; }

        public HttpWebResponse Response { get; set; }

        public IConnectionProvider ConnectionProvider { get; private set; }

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