using System;
using System.Threading.Tasks;

namespace Bolt.Client.Pipeline
{
    public class ValidateProxyMiddleware : ClientMiddlewareBase
    {
        public override async Task InvokeAsync(ClientActionContext context)
        {
            ProxyState proxyState = context.Proxy.State;

            if (context.Proxy == null)
            {
                throw new InvalidOperationException("Proxy object is not assigned.");
            }

            if (context.Proxy.State == ProxyState.Closed)
            {
                throw new ProxyClosedException("Proxy object is already closed.");
            }

            await Next(context).ConfigureAwait(false);

            if (proxyState == ProxyState.Default)
            {
                (context.Proxy as IPipelineCallback)?.ChangeState(ProxyState.Open);
            }
        }
    }
}
