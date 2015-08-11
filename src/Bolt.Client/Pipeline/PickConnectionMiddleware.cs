using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Core;

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

        public override async Task Invoke(ClientActionContext context)
        {
            if (context.Connection == null)
            {
                ConnectionDescriptor serverUri = ServerProvider.GetServer();
                context.Connection =
                    new ConnectionDescriptor(EndpointProvider.GetEndpoint(serverUri.Server, context.Contract,
                        context.Action));
            }

            context.Request.RequestUri = context.Connection.Server;

            try
            {
                await Next(context);
            }
            catch (HttpRequestException)
            {
                ServerProvider.OnServerUnavailable(context.Request.RequestUri);
                throw;
            }
        }
    }
}