using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bolt.Client.Pipeline
{
    public class PickConnectionMiddleware : ClientMiddlewareBase
    {
        public PickConnectionMiddleware(IServerProvider serverProvider,
            IEndpointProvider endpointProvider)
        {
            if (serverProvider == null) throw new ArgumentNullException(nameof(serverProvider));
            if (endpointProvider == null) throw new ArgumentNullException(nameof(endpointProvider));

            ServerProvider = serverProvider;
            EndpointProvider = endpointProvider;
        }

        public IServerProvider ServerProvider { get; }

        public IEndpointProvider EndpointProvider { get; }

        public override async Task InvokeAsync(ClientActionContext context)
        {
            if (context.ServerConnection == null)
            {
                context.ServerConnection = ServerProvider.GetServer();
            }

            context.GetRequestOrThrow().RequestUri = EndpointProvider.GetEndpoint(context.ServerConnection.Server, context.Contract, context.Action);
            try
            {
                await Next(context).ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                ServerProvider.OnServerUnavailable(context.Request.RequestUri);
                throw;
            }
        }
    }
}