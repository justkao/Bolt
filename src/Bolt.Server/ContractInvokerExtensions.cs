using Owin;
using System;

namespace Bolt.Server
{
    public static class ContractInvokerExtensions
    {
        public static IAppBuilder UseStateLessContractInvoker<TInvoker, TContractImplementation>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ContractDescriptor descriptor)
            where TInvoker : IContractInvoker, new()
            where TContractImplementation : new()
        {
            return builder.UseContractInvoker<TInvoker>(configuration, descriptor, new InstanceProvider<TContractImplementation>());
        }

        public static IAppBuilder UseStateFullContractInvoker<TInvoker, TContractImplementation>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ContractDescriptor descriptor)
            where TInvoker : IContractInvoker, new()
            where TContractImplementation : new()
        {
            return builder.UseContractInvoker<TInvoker>(
                configuration,
                descriptor,
                new StateFullInstanceProvider<TContractImplementation>() { SessionHeader = configuration.SessionHeader });
        }

        public static IAppBuilder UseContractInvoker<TInvoker>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ContractDescriptor descriptor,
            IInstanceProvider instanceProvider) where TInvoker : IContractInvoker, new()
        {
            ContractInvokerFactory<TInvoker> factory = new ContractInvokerFactory<TInvoker>();
            IContractInvoker contractInvoker = factory.Create(configuration, instanceProvider);
            builder.Use<ContractInvokerMiddleware>(new ContractInvokerMiddlewareOptions(contractInvoker, new ActionProvider(descriptor, configuration.EndpointProvider)));
            return builder;
        }

        public static IAppBuilder MapContract(
            this IAppBuilder builder,
            ContractDescriptor descriptor,
            ServerConfiguration configuration,
            string prefix,
            Action<IAppBuilder> configure)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Uri url = configuration.EndpointProvider.GetEndpoint(null, prefix, descriptor, null);
            return builder.Map(url.ToString(), configure);
        }
    }
}