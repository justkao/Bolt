using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    public abstract class ProxyBase : ChannelBase
    {
        private Exception _failedReason;

        protected ProxyBase(ProxyBase proxy)
            : base(proxy)
        {
            Retries = proxy.Retries;
            RetryDelay = proxy.RetryDelay;
            IsFailed = proxy.IsFailed;
            _failedReason = proxy._failedReason;
        }

        protected ProxyBase(IRequestForwarder requestForwarder, IEndpointProvider endpointProvider)
            : base(requestForwarder, endpointProvider)
        {
        }

        public int Retries { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public bool IsFailed { get; private set; }

        protected virtual void BeforeSending(ClientActionContext context)
        {
        }

        protected virtual void AfterReceived(ClientActionContext context)
        {
        }

        protected virtual bool HandleError(ClientActionContext context, Exception error)
        {
            if (error is BoltSerializationException)
            {
                throw error;
            }

            if (error is WebException)
            {
                return true;
            }

            return false;
        }

        protected virtual Task<bool> HandleErrorAsync(ClientActionContext context, Exception error)
        {
            return Task.FromResult(HandleError(context, error));
        }

        public override T SendCore<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            EnsureNotFailed();
            EnsureNotClosed();

            int tries = 0;

            while (true)
            {
                using (ConnectionDescriptor connection = GetConnection(descriptor, cancellation, parameters))
                {
                    Exception error;

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

                    tries++;
                    if (tries > Retries)
                    {
                        _failedReason = error;
                        IsFailed = true;
                        throw error;
                    }

                    if (!HandleError(connection.Context, error))
                    {
                        throw error;
                    }

                    TaskExtensions.Sleep(RetryDelay, connection.Context.Cancellation);
                }
            }
        }

        public override async Task<T> SendCoreAsync<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            EnsureNotFailed();
            EnsureNotClosed();

            int tries = 0;

            while (true)
            {
                using (ConnectionDescriptor connection = await GetConnectionAsync(descriptor, cancellation, parameters))
                {
                    Exception error;

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

                    tries++;
                    if (tries > Retries)
                    {
                        _failedReason = error;
                        IsFailed = true;
                        throw error;
                    }

                    if (!await HandleErrorAsync(connection.Context, error))
                    {
                        throw error;
                    }

                    await Task.Delay(RetryDelay, connection.Context.Cancellation);
                }
            }
        }

        protected abstract ConnectionDescriptor GetConnection(ActionDescriptor actionDescriptor, CancellationToken cancellation, object parameters);

        protected abstract Task<ConnectionDescriptor> GetConnectionAsync(ActionDescriptor descriptor, CancellationToken cancellation, object parameters);

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

            public ClientActionContext Context { get; private set; }

            public ChannelBase Connection { get; private set; }

            public void Dispose()
            {
                Context.Dispose();
                Connection.Dispose();
            }
        }
    }
}