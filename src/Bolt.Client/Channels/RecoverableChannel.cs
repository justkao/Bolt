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
        private Exception _failedReason;

        public RecoverableChannel(RecoverableChannel<TContract, TContractDescriptor> proxy)
            : base(proxy)
        {
            Retries = proxy.Retries;
            RetryDelay = proxy.RetryDelay;
            IsFailed = proxy.IsFailed;
            ServerProvider = proxy.ServerProvider;
            Prefix = proxy.Prefix;
            Descriptor = proxy.Descriptor;
            _failedReason = proxy._failedReason;
        }

        public RecoverableChannel(TContractDescriptor descriptor, string prefix, IServerProvider serverProvider, IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
            : base(requestForwarder, endpointProvider)
        {
            Descriptor = descriptor;
            Prefix = prefix;
            ServerProvider = serverProvider;
        }

        public int Retries { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public TContractDescriptor Descriptor { get; private set; }

        public string Prefix { get; private set; }

        public IServerProvider ServerProvider { get; private set; }

        public bool IsFailed { get; private set; }

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
                if (!(error as WebException).ResponseReceived())
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
                    case ServerErrorCodes.ContractNotFound:
                        FailProxy(error);
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

        public sealed override T SendCore<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            EnsureNotFailed();
            EnsureNotClosed();

            int tries = 0;

            while (true)
            {
                Exception error = null;
                ConnectionDescriptor connection = ConnectionDescriptor.Invalid;
                try
                {
                    connection = GetConnection(descriptor, cancellation, parameters);
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

                if (connection.IsValid())
                {
                    using (connection)
                    {
                        try
                        {
                            BeforeSending(connection.Context);
                            T result = connection.Connection.SendCore<T, TParameters>(parameters, descriptor, cancellation);
                            AfterReceived(connection.Context);
                            return result;
                        }
                        catch (Exception e)
                        {
                            e.EnsureNotCancelled();
                            error = e;
                        }
                    }
                }

                tries++;
                if (tries > Retries)
                {
                    FailProxy(error);
                    throw error;
                }

                if (!HandleError(connection.Context, error))
                {
                    throw error;
                }

                TaskExtensions.Sleep(RetryDelay, connection.Context.Cancellation);
            }
        }

        public sealed override async Task<T> SendCoreAsync<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            EnsureNotFailed();
            EnsureNotClosed();

            int tries = 0;

            while (true)
            {
                Exception error = null;
                ConnectionDescriptor connection = ConnectionDescriptor.Invalid;
                try
                {
                    connection = await GetConnectionAsync(descriptor, cancellation, parameters);
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

                if (connection.IsValid())
                {
                    using (connection)
                    {
                        try
                        {
                            BeforeSending(connection.Context);
                            T result = await connection.Connection.SendCoreAsync<T, TParameters>(parameters, descriptor, cancellation);
                            AfterReceived(connection.Context);
                            return result;
                        }
                        catch (Exception e)
                        {
                            e.EnsureNotCancelled();
                            error = e;
                        }
                    }
                }

                tries++;
                if (tries > Retries)
                {
                    FailProxy(error);
                    throw error;
                }

                if (!await HandleErrorAsync(connection.Context, error))
                {
                    throw error;
                }

                await Task.Delay(RetryDelay, connection.Context.Cancellation);
            }
        }

        protected override ClientActionContext CreateContext(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            Uri server = ServerProvider.GetServer();
            HttpWebRequest webRequest = CreateWebRequest(server, Prefix, Descriptor, actionDescriptor);
            return new ClientActionContext(actionDescriptor, webRequest, server, cancellation);
        }

        protected virtual ConnectionDescriptor GetConnection(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters)
        {
            ClientActionContext ctxt = CreateContext(actionDescriptor, cancellation, parameters);
            return new ConnectionDescriptor(ctxt, new ActionChannel(RequestForwarder, EndpointProvider, ctxt));
        }

        protected virtual Task<ConnectionDescriptor> GetConnectionAsync(ActionDescriptor descriptor, CancellationToken cancellation, object parameters)
        {
            return Task.FromResult(GetConnection(descriptor, cancellation, parameters));
        }

        protected virtual TContract CreateContract(IChannel channel)
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), channel);
        }

        protected TContract CreateContract(Uri server)
        {
            return CreateContract(new DelegatedChannel(RequestForwarder, EndpointProvider, server, Prefix, Descriptor, BeforeSending));
        }

        protected virtual void FailProxy(Exception error)
        {
            _failedReason = error;
            IsFailed = true;
        }

        private void EnsureNotFailed()
        {
            if (IsFailed)
            {
                throw new ProxyFailedException("Proxy failed.", _failedReason);
            }
        }

        protected struct ConnectionDescriptor : IDisposable
        {
            public ConnectionDescriptor(ClientActionContext context, ChannelBase connection)
                : this()
            {
                Context = context;
                Connection = connection;
            }

            public bool IsValid()
            {
                return Context != null;
            }

            public static readonly ConnectionDescriptor Invalid = new ConnectionDescriptor();

            public ClientActionContext Context { get; private set; }

            public ChannelBase Connection { get; private set; }

            public void Dispose()
            {
                if (IsValid())
                {
                    Context.Dispose();
                    Connection.Dispose();
                }
            }
        }
    }
}