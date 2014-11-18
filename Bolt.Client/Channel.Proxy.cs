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

        public virtual void Send<TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor)
        {
            RetrieveResponse<Empty, TRequestParameters>(parameters, descriptor);
        }

        public virtual TResult Send<TResult, TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor)
        {
            return RetrieveResponse<TResult, TRequestParameters>(parameters, descriptor);
        }

        protected virtual T RetrieveResponse<T, TParameters>(TParameters parameters, MethodDescriptor descriptor)
        {
            int tries = 0;
            while (true)
            {
                HttpWebRequest channel = null;
                Exception error = null;

                try
                {
                    channel = GetChannel(descriptor);
                }
                catch (SerializationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    error = e;
                }

                ClientExecutionContext context = new ClientExecutionContext(descriptor, channel, null);

                if (channel != null)
                {
                    ResponseDescriptor<T> response = RetrieveResponse<T, TParameters>(context, parameters, tries);
                    if (response.IsSuccess)
                    {
                        return response.GetResultOrThrow();
                    }

                    error = response.Error;
                }


                tries++;
                if (tries > Retries)
                {
                    OnProxyFailed(error, descriptor);
                    throw error;
                }
                if (RetryDelay != null)
                {
                    Thread.Sleep(RetryDelay.Value);
                }
            }
        }

        #endregion

        #region Asynchornous Methods

        public virtual async Task SendAsync<TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor)
        {
            await RetrieveResponseAsync<Empty, TRequestParameters>(parameters, descriptor);
        }

        public virtual async Task<TResult> SendAsync<TResult, TRequestParameters>(TRequestParameters parameters, MethodDescriptor descriptor)
        {
            return await RetrieveResponseAsync<TResult, TRequestParameters>(parameters, descriptor);
        }

        protected virtual async Task<T> RetrieveResponseAsync<T, TParameters>(TParameters parameters, MethodDescriptor descriptor)
        {
            int tries = 0;
            while (true)
            {
                HttpWebRequest channel = null;
                Exception error = null;

                try
                {
                    channel = await GetChannelAsync(descriptor);
                }
                catch (SerializationException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    error = e;
                }

                ClientExecutionContext context = new ClientExecutionContext(descriptor, channel, null);

                if (channel != null)
                {
                    ResponseDescriptor<T> response = await RetrieveResponseAsync<T, TParameters>(context, parameters, tries);
                    if (response.IsSuccess)
                    {
                        return response.GetResultOrThrow();
                    }

                    error = response.Error;
                }


                tries++;
                if (tries > Retries)
                {
                    OnProxyFailed(error, descriptor);
                    throw error;
                }
                if (RetryDelay != null)
                {
                    Thread.Sleep(RetryDelay.Value);
                }
            }
        }

        #endregion
    }
}
