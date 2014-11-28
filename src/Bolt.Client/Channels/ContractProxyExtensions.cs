using System;

namespace Bolt.Client.Channels
{
    public static class ContractProxyExtensions
    {
        public static RecoverableChannel Recoverable(this ContractProxy proxy)
        {
            return ((RecoverableChannel)proxy.Channel);
        }

        public static ContractProxy WithRetries(this ContractProxy proxy, int retries, TimeSpan retryDelay)
        {
            proxy.Recoverable().Retries = retries;
            proxy.Recoverable().RetryDelay = retryDelay;

            return proxy;
        }
    }
}
