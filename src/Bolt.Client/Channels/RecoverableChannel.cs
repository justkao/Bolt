using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    /// <summary>
    /// Recoverable Bolt channel. 
    /// </summary>
    public class RecoverableChannel : ChannelBase
    {
        public RecoverableChannel(RecoverableChannel proxy)
            : base(proxy)
        {
            Retries = proxy.Retries;
            RetryDelay = proxy.RetryDelay;
            ServerProvider = proxy.ServerProvider;
        }

        public RecoverableChannel(Uri server, ClientConfiguration clientConfiguration)
            : base(clientConfiguration)
        {
            if (server == null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            ServerProvider = new SingleServerProvider(server);
        }

        public RecoverableChannel(IServerProvider serverProvider, ClientConfiguration clientConfiguration)
            : base(clientConfiguration)
        {
            if (serverProvider == null)
            {
                throw new ArgumentNullException(nameof(serverProvider));
            }

            ServerProvider = serverProvider;
        }

        public RecoverableChannel(IServerProvider serverProvider, IRequestHandler requestHandler, IEndpointProvider endpointProvider)
            : base(requestHandler, endpointProvider)
        {
            ServerProvider = serverProvider;
        }

        public int Retries { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public IServerProvider ServerProvider { get; }

        public sealed override async Task<T> SendAsync<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            EnsureNotClosed();

            int tries = 0;

            while (true)
            {
                Exception error = null;
                ConnectionDescriptor connection = null;
                try
                {
                    connection = await GetConnectionAsync();
                }
                catch (Exception e)
                {
                    e.EnsureNotCancelled();

                    if (!HandleOpenConnectionError(e))
                    {
                        throw;
                    }

                    error = e;
                }

                if (connection != null)
                {
                    using (ClientActionContext ctxt = CreateContext(connection, descriptor, cancellation, parameters))
                    {
                        try
                        {
                            BeforeSending(ctxt);
                            ResponseDescriptor<T> result = await RequestHandler.GetResponseAsync<T, TParameters>(ctxt, parameters);
                            AfterReceived(ctxt);
                            return result.GetResultOrThrow();
                        }
                        catch (Exception e)
                        {
                            e.EnsureNotCancelled();
                            error = e;
                        }

                        if (!HandleError(ctxt, error))
                        {
                            throw error;
                        }
                    }
                }

                tries++;
                if (tries > Retries)
                {
                    IsClosed = true;
                    throw error;
                }

                await Task.Delay(RetryDelay, cancellation);
            }
        }

        protected virtual bool HandleError(ClientActionContext context, Exception error)
        {
            if (error is HttpRequestException)
            {
                ServerProvider.OnServerUnavailable(context.Connection.Server);
            }

            return HandleErrorCore(error);
        }

        protected virtual bool HandleOpenConnectionError(Exception error)
        {
            return HandleErrorCore(error);
        }

        protected override async Task<ConnectionDescriptor> GetConnectionAsync()
        {
            await OpenAsync();
            return ServerProvider.GetServer();
        }

        private bool HandleErrorCore(Exception error)
        {
            if (error is NoServersAvailableException)
            {
                return true;
            }

            if (error is BoltSerializationException)
            {
                throw error;
            }

            var exception = error as BoltServerException;
            if (exception == null)
            {
                return error is HttpRequestException;
            }

            switch (exception.Error)
            {
                case ServerErrorCode.ContractNotFound:
                    IsClosed = true;
                    throw error;
                default:
                    throw error;
            }
        }
    }
}