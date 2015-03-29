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
            ServerProvider = new UriServerProvider(server);
        }

        public RecoverableChannel(IServerProvider serverProvider, ClientConfiguration clientConfiguration)
            : base(clientConfiguration)
        {
            ServerProvider = serverProvider;
        }

        public RecoverableChannel(IServerProvider serverProvider, IRequestHandler requestHandler, IEndpointProvider endpointProvider)
            : base(requestHandler, endpointProvider)
        {
            ServerProvider = serverProvider;
        }

        public int Retries { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public IServerProvider ServerProvider { get; private set; }

        public sealed override async Task<T> SendAsync<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            EnsureNotClosed();

            int tries = 0;

            while (true)
            {
                Exception error = null;
                Uri connection = null;
                try
                {
                    connection = await GetRemoteConnectionAsync();
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
                ServerProvider.OnServerUnavailable(context.Server);
            }

            return HandleErrorCore(error);
        }

        protected virtual bool HandleOpenConnectionError(Exception error)
        {
            return HandleErrorCore(error);
        }

        protected override async Task<Uri> GetRemoteConnectionAsync()
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

            if (error is BoltServerException)
            {
                switch (((BoltServerException)error).Error)
                {
                    case ServerErrorCode.ContractNotFound:
                        IsClosed = true;
                        throw error;
                    default:
                        throw error;
                }
            }

            if (error is HttpRequestException)
            {
                return true;
            }

            return false;
        }
    }
}