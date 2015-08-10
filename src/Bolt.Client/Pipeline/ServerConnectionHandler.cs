using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Client.Filters;
using Bolt.Core;

namespace Bolt.Client.Pipeline
{
    public class ServerConnectionHandler : IClientContextHandler
    {
        public ServerConnectionHandler(IServerProvider serverProvider, IEndpointProvider endpointProvider)
        {
            if (serverProvider == null) throw new ArgumentNullException(nameof(serverProvider));
            if (endpointProvider == null) throw new ArgumentNullException(nameof(endpointProvider));

            ServerProvider = serverProvider;
            EndpointProvider = endpointProvider;
        }

        public IServerProvider ServerProvider { get; }

        public IEndpointProvider EndpointProvider { get; }

        public HandleContextStage Stage => HandleContextStage.Before;

        public async Task HandleAsync(ClientActionContext context, Func<ClientActionContext, Task> next)
        {
            if (context.Connection == null)
            {
                ConnectionDescriptor serverUri = ServerProvider.GetServer();
                context.Connection =
                    new ConnectionDescriptor(EndpointProvider.GetEndpoint(serverUri.Server, context.Contract,
                        context.Action));
            }

            Uri server = context.Connection.Server;

            try
            {
                await next(context);
            }
            catch (HttpRequestException)
            {
                ServerProvider.OnServerUnavailable(server);
                throw;
            }
        }
    }
}