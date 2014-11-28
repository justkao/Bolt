using Owin;

namespace Bolt.Server
{
    public static class ContractInvokerExtensions
    {
        public static IAppBuilder UseStateLessContractInvoker<TInvoker, TContractImplementation>(
            this IAppBuilder builder,
            ServerConfiguration configuration)
            where TInvoker : ContractInvoker, new()
            where TContractImplementation : new()
        {
            return builder.UseContractInvoker<TInvoker>(configuration, new InstanceProvider<TContractImplementation>());
        }

        public static IAppBuilder UseStateFullContractInvoker<TInvoker, TContractImplementation>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ActionDescriptor initInstanceAction,
            ActionDescriptor releaseInstanceAction)
            where TInvoker : ContractInvoker, new()
            where TContractImplementation : new()
        {
            return builder.UseContractInvoker<TInvoker>(
                configuration,
                new StateFullInstanceProvider<TContractImplementation>(
                    initInstanceAction,
                    releaseInstanceAction,
                    configuration.SessionHeader,
                    configuration.StateFullInstanceLifetime));
        }

        public static IAppBuilder UseContractInvoker<TInvoker>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            IInstanceProvider instanceProvider)
            where TInvoker : ContractInvoker, new()
        {
            TInvoker contractInvoker = new TInvoker();
            contractInvoker.Init(configuration);
            contractInvoker.InstanceProvider = instanceProvider;
            builder.GetBolt().Add(contractInvoker);
            return builder;
        }
    }
}