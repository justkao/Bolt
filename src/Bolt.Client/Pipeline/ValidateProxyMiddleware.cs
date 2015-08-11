using System;
using System.Threading.Tasks;

namespace Bolt.Client.Pipeline
{
    public class ValidateProxyMiddleware : ClientMiddlewareBase
    {
        public override Task Invoke(ClientActionContext context)
        {
            if (context.Proxy == null)
            {
                throw new InvalidOperationException("Proxy object is not assigned.");
            }

            if (context.Proxy.State == ChannelState.Closed)
            {
                throw new ProxyClosedException("Proxy object is already closed.");
            }

            return Next(context);
        }
    }
}
