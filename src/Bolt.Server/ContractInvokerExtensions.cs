using System;

using Owin;

namespace Bolt.Server
{
    public static class ContractInvokerExtensions
    {
        public static IAppBuilder UseStateLessContractInvoker<TInvoker, TContractImplementation, TContractDescriptor>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ContractDescriptor descriptor)
            where TInvoker : IContractInvoker<TContractDescriptor>, new()
            where TContractImplementation : new()
            where TContractDescriptor : ContractDescriptor
        {
            return builder.UseContractInvoker<TInvoker, TContractDescriptor>(configuration, descriptor, new InstanceProvider<TContractImplementation>());
        }

        public static IAppBuilder UseStateFullContractInvoker<TInvoker, TContractImplementation, TContractDescriptor>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ContractDescriptor descriptor)
            where TInvoker : IContractInvoker<TContractDescriptor>, new()
            where TContractImplementation : new()
            where TContractDescriptor : ContractDescriptor
        {
            return builder.UseContractInvoker<TInvoker, TContractDescriptor>(
                configuration,
                descriptor,
                new StateFullInstanceProvider<TContractImplementation>() { SessionHeader = configuration.SessionHeader });
        }

        public static IAppBuilder UseContractInvoker<TInvoker, TDescriptor>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ContractDescriptor descriptor,
            IInstanceProvider instanceProvider)
            where TInvoker : IContractInvoker<TDescriptor>, new()
            where TDescriptor : ContractDescriptor
        {
            ContractInvokerFactory<TInvoker, TDescriptor> factory = new ContractInvokerFactory<TInvoker, TDescriptor>();
            IContractInvoker<TDescriptor> contractInvoker = factory.Create(configuration, instanceProvider);
            contractInvoker.DescriptorCore = descriptor;
            builder.GetBolt().Add(contractInvoker);
            return builder;
        }

        public static IAppBuilder MapContract(
            this IAppBuilder builder,
            ContractDescriptor descriptor,
            ServerConfiguration configuration,
            Action<IAppBuilder> configure)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Uri url = configuration.EndpointProvider.GetEndpoint(null, descriptor, null);
            return builder.Map(url.ToString(), configure);
        }
    }
}