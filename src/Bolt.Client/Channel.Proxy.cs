using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace Bolt.Client
{
    public partial class Channel
    {
        public int Retries { get; set; }

        public TimeSpan? RetryDelay { get; set; }

        #region Synchronous Methods

        public virtual void Send<TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            RetrieveResponse<Empty, TRequestParameters>(parameters, descriptor, cancellation);
        }

        public virtual TResult Send<TResult, TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            return RetrieveResponse<TResult, TRequestParameters>(parameters, descriptor, cancellation);
        }

        protected virtual T RetrieveResponse<T, TParameters>(
            TParameters parameters,
            ActionDescriptor descriptor,
            CancellationToken cancellation)
        {
            return RetrieveResponse<T, TParameters>(ConnectionProvider, parameters, descriptor, cancellation);
        }

        protected virtual T RetrieveResponse<T, TParameters>(IConnectionProvider connectionProvider, TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            int tries = 0;
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();

                ConnectionOpenedResult channel = ConnectionOpenedResult.Invalid;
                Exception error = null;

                try
                {
                    channel = OpenConnection(connectionProvider, descriptor, cancellation);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (SerializationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    e.EnsureNotCancelled();
                    error = e;
                }

                using (ClientExecutionContext context = new ClientExecutionContext(descriptor, channel.Request, channel.Server, cancellation, connectionProvider))
                {
                    if (error != null)
                    {
                        if (!HandleCommunicationError(context, error, tries))
                        {
                            connectionProvider.CloseConnection(channel.Server);
                            throw error;
                        }
                    }

                    if (channel.IsValid())
                    {
                        ResponseDescriptor<T> response = RetrieveResponse<T, TParameters>(context, parameters, tries);
                        if (response.IsSuccess)
                        {
                            return response.GetResultOrThrow();
                        }

                        error = response.Error;
                    }
                }

                cancellation.ThrowIfCancellationRequested();

                tries++;
                if (tries > Retries)
                {
                    OnProxyFailed(connectionProvider, channel.Server, error, descriptor);
                    throw error;
                }
                if (RetryDelay != null)
                {
                    TaskExtensions.Sleep(RetryDelay.Value, cancellation);
                }
            }
        }

        #endregion

        #region Asynchornous Methods

        public virtual async Task SendAsync<TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            await RetrieveResponseAsync<Empty, TRequestParameters>(parameters, descriptor, cancellation);
        }

        public virtual async Task<TResult> SendAsync<TResult, TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            return await RetrieveResponseAsync<TResult, TRequestParameters>(parameters, descriptor, cancellation);
        }

        protected virtual Task<T> RetrieveResponseAsync<T, TParameters>(
            TParameters parameters,
            ActionDescriptor descriptor,
            CancellationToken cancellation)
        {
            return RetrieveResponseAsync<T, TParameters>(ConnectionProvider, parameters, descriptor, cancellation);
        }

        protected virtual async Task<T> RetrieveResponseAsync<T, TParameters>(IConnectionProvider connectionProvider, TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            int tries = 0;
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();

                ConnectionOpenedResult channel = ConnectionOpenedResult.Invalid;
                Exception error = null;

                try
                {
                    channel = await OpenConnectionAsync(connectionProvider, descriptor, cancellation);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (SerializationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    e.EnsureNotCancelled();
                    error = e;
                }

                using (ClientExecutionContext context = new ClientExecutionContext(descriptor, channel.Request, channel.Server, cancellation, connectionProvider))
                {
                    if (error != null)
                    {
                        if (!HandleCommunicationError(context, error, tries))
                        {
                            connectionProvider.CloseConnection(channel.Server);
                            throw error;
                        }
                    }

                    if (channel.IsValid())
                    {
                        ResponseDescriptor<T> response = await RetrieveResponseAsync<T, TParameters>(context, parameters, tries);
                        if (response.IsSuccess)
                        {
                            return response.GetResultOrThrow();
                        }

                        error = response.Error;
                    }
                }

                cancellation.ThrowIfCancellationRequested();

                tries++;
                if (tries > Retries)
                {
                    OnProxyFailed(connectionProvider, channel.Server, error, descriptor);
                    throw error;
                }
                if (RetryDelay != null)
                {
                    await Task.Delay(RetryDelay.Value, cancellation);
                }
            }
        }

        #endregion
    }
}
