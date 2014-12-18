#if OWIN
using Owin;
using IApplicationBuilder = Owin.IAppBuilder;
#else
using IApplicationBuilder = Microsoft.AspNet.Builder.IApplicationBuilder;
#endif

namespace Bolt.Server
{
    public static class ContractInvokerExtensions
    {
        public static IApplicationBuilder UseStateLessContractInvoker<TInvoker, TContractImplementation>(
            this IApplicationBuilder builder,
            ServerConfiguration configuration)
            where TInvoker : ContractInvoker, new()
            where TContractImplementation : new()
        {
            return builder.UseContractInvoker<TInvoker>(configuration, new InstanceProvider<TContractImplementation>());
        }

        public static IApplicationBuilder UseStateFullContractInvoker<TInvoker, TContractImplementation>(
            this IApplicationBuilder builder,
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

        public static IApplicationBuilder UseContractInvoker<TInvoker>(
            this IApplicationBuilder builder,
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