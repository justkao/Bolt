using System;
using System.Collections.Generic;
using Bolt.Client.Channels;
using Bolt.Client.Filters;

namespace Bolt.Client
{
    public static class ClientConfigurationExtensions
    {
        public static TContract CreateSessionProxy<TContract>(this ClientConfiguration clientConfiguration, string uri)
            where TContract : class
        {
            return clientConfiguration.CreateSessionProxy<TContract>(new SingleServerProvider(new Uri(uri)));
        }

        public static TContract CreateSessionProxy<TContract>(this ClientConfiguration clientConfiguration, Uri uri)
            where TContract : class
        {
            return clientConfiguration.CreateSessionProxy<TContract>(new SingleServerProvider(uri));
        }

        public static TContract CreateSessionProxy<TContract>(this ClientConfiguration clientConfiguration,
            IServerProvider serverProvider)
            where TContract : class
        {
            SessionChannel channel = new SessionChannel(typeof(TContract), serverProvider, clientConfiguration);
            TContract result = clientConfiguration.CreateProxy<TContract>(channel);

            if (result is IContractProvider)
            {
                channel.Contract = (result as IContractProvider).Contract;
            }

            return result;
        }

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration, string uri)
            where TContract : class
        {
            return clientConfiguration.CreateProxy<TContract>(new SingleServerProvider(new Uri(uri)));
        }

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration, Uri uri)
            where TContract : class
        {
            return clientConfiguration.CreateProxy<TContract>(new SingleServerProvider(uri));
        }

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration,
            IServerProvider serverProvider)
            where TContract : class
        {
            return clientConfiguration.CreateProxy<TContract>(clientConfiguration.CreateRecoverable(serverProvider));
        }

        public static TContract CreateProxy<TContract>(this ClientConfiguration clientConfiguration, IChannel channel)
            where TContract : class
        {
            if (clientConfiguration.ProxyFactory == null)
            {
                throw new InvalidOperationException(
                    $"Unable to create proxy for contract '{typeof(TContract).Name}' becasue proxy factory is not initialized.");
            }

            return clientConfiguration.ProxyFactory.CreateProxy<TContract>(channel);
        }

        public static ClientConfiguration AddFilter<T>(this ClientConfiguration clientConfiguration) where T: IClientExecutionFilter, new()
        {
            return clientConfiguration.AddFilter(Activator.CreateInstance<T>());
        }

        public static ClientConfiguration AddFilter(this ClientConfiguration clientConfiguration, IClientExecutionFilter executionFilter)
        {
            if (clientConfiguration == null) throw new ArgumentNullException(nameof(clientConfiguration));
            if (executionFilter == null) throw new ArgumentNullException(nameof(executionFilter));

            if (clientConfiguration.Filters == null)
            {
                clientConfiguration.Filters = new List<IClientExecutionFilter>();
            }

            clientConfiguration.Filters.Add(executionFilter);
            return clientConfiguration;
        }
    }
}
