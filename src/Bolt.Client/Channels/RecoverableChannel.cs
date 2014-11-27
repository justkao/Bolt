using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    public class RecoverableChannel<TContract, TContractDescriptor> : ChannelBase
        where TContract : ContractProxy<TContractDescriptor>
        where TContractDescriptor : ContractDescriptor
    {
        public RecoverableChannel(RecoverableChannel<TContract, TContractDescriptor> proxy)
            : base(proxy)
        {
            Retries = proxy.Retries;
            RetryDelay = proxy.RetryDelay;
            ServerProvider = proxy.ServerProvider;
        }

        public RecoverableChannel(TContractDescriptor descriptor, IServerProvider serverProvider, IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
            : base(descriptor, requestForwarder, endpointProvider)
        {
            ServerProvider = serverProvider;
        }

        public int Retries { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public IServerProvider ServerProvider { get; private set; }

        protected virtual void BeforeSending(ClientActionContext context)
        {
        }

        protected virtual void AfterReceived(ClientActionContext context)
        {
        }

        protected virtual bool HandleOpenConnectionError(Exception error)
        {
            return HandleErrorCore(error);
        }

        protected virtual bool HandleError(ClientActionContext context, Exception error)
        {
            if (error is WebException)
            {
                if ((error as WebException).Response == null)
                {
                    ServerProvider.OnServerUnavailable(context.Server);
                }
            }

            return HandleErrorCore(error);
        }

        protected virtual bool HandleErrorCore(Exception error)
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

            if (error is WebException)
            {
                if ((error as WebException).Response == null)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual Task<bool> HandleErrorAsync(ClientActionContext context, Exception error)
        {
            return Task.FromResult(HandleError(context, error));
        }

        public override sealed T SendCore<T, TParameters>(
            TParameters parameters,
            ActionDescriptor descriptor,
            CancellationToken cancellation)
        {
            EnsureNotClosed();

            int tries = 0;

            while (true)
            {
                Exception error = null;
                Uri connection = null;
                try
                {
                    connection = GetRemoteConnection();
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
                            ResponseDescriptor<T> result = RequestForwarder.GetResponse<T, TParameters>(ctxt, parameters);
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

                TaskExtensions.Sleep(RetryDelay, cancellation);
            }
        }

        public sealed override async Task<T> SendCoreAsync<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
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
                            ResponseDescriptor<T> result = await RequestForwarder.GetResponseAsync<T, TParameters>(ctxt, parameters);
                            AfterReceived(ctxt);
                            return result.GetResultOrThrow();
                        }
                        catch (Exception e)
                        {
                            e.EnsureNotCancelled();
                            error = e;
                        }

                        if (!await HandleErrorAsync(ctxt, error))
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

        protected override Uri GetRemoteConnection()
        {
            return ServerProvider.GetServer();
        }

        protected virtual TContract CreateContract(IChannel channel)
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), channel);
        }

        protected TContract CreateContract(Uri server)
        {
            return CreateContract(new DelegatedChannel(server, Descriptor, RequestForwarder, EndpointProvider, BeforeSending));
        }
    }
}