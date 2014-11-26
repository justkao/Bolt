using Owin;

namespace Bolt.Server
{
    public static class ContractInvokerExtensions
    {
        public static IAppBuilder UseStateLessContractInvoker<TInvoker, TContractImplementation, TContractDescriptor>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            TContractDescriptor descriptor)
            where TInvoker : IContractInvoker<TContractDescriptor>, new()
            where TContractImplementation : new()
            where TContractDescriptor : ContractDescriptor
        {
            return builder.UseContractInvoker<TInvoker, TContractDescriptor>(configuration, descriptor, new InstanceProvider<TContractImplementation>());
        }

        public static IAppBuilder UseStateFullContractInvoker<TInvoker, TContractImplementation, TContractDescriptor>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            TContractDescriptor descriptor)
            where TInvoker : IContractInvoker<TContractDescriptor>, new()
            where TContractImplementation : new()
            where TContractDescriptor : ContractDescriptor
        {
            return builder.UseContractInvoker<TInvoker, TContractDescriptor>(
                configuration,
                descriptor,
                new StateFullInstanceProvider<TContractImplementation> { SessionHeader = configuration.SessionHeader });
        }

        public static IAppBuilder UseContractInvoker<TInvoker, TDescriptor>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            TDescriptor descriptor,
            IInstanceProvider instanceProvider)
            where TInvoker : IContractInvoker<TDescriptor>, new()
            where TDescriptor : ContractDescriptor
        {

            ContractInvoker<TDescriptor> contractInvoker = (ContractInvoker<TDescriptor>)((object)new TInvoker());
            contractInvoker.Init(configuration);
            contractInvoker.Descriptor = descriptor;
            contractInvoker.InstanceProvider = instanceProvider;

            builder.GetBolt().Add(contractInvoker);
            return builder;
        }
    }
}