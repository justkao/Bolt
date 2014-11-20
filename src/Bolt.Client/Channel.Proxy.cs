using System;
using System.Net;
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

        protected virtual T RetrieveResponse<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            int tries = 0;
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();

                HttpWebRequest channel = null;
                Exception error = null;

                try
                {
                    channel = GetChannel(descriptor, cancellation);
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
                    error = e;
                }

                using (ClientExecutionContext context = new ClientExecutionContext(descriptor, channel, cancellation, null))
                {
                    if (channel != null)
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
                    OnProxyFailed(error, descriptor);
                    throw error;
                }
                if (RetryDelay != null)
                {
                    TaskExtensions.Sleep(RetryDelay.Value);
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

        protected virtual async Task<T> RetrieveResponseAsync<T, TParameters>(TParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            int tries = 0;
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();

                HttpWebRequest channel = null;
                Exception error = null;

                try
                {
                    channel = await GetChannelAsync(descriptor, cancellation);
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
                    error = e;
                }

                using (ClientExecutionContext context = new ClientExecutionContext(descriptor, channel, cancellation, null))
                {
                    if (channel != null)
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
                    OnProxyFailed(error, descriptor);
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
