using System;
using Owin;

namespace Bolt.Server
{
    public static class ExecutorMiddlewareExtensions
    {
        public static IAppBuilder UseStatelessExecutor<TExecutor, TContractImplementation>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ContractDescriptor descriptor) where TExecutor : IContractInvoker, new()
        {
            return builder.UseContract<TExecutor>(configuration, descriptor, new InstanceProvider<TContractImplementation>());
        }

        public static IAppBuilder UseContract<TExecutor, TContractImplementation>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ContractDescriptor descriptor) where TExecutor : IContractInvoker, new()
        {
            return builder.UseContract<TExecutor>(
                configuration,
                descriptor,
                new StateFullInstanceProvider<TContractImplementation>() { SessionHeader = configuration.SessionHeaderName });
        }

        public static IAppBuilder UseContract<TExecutor>(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            ContractDescriptor descriptor,
            IInstanceProvider instanceProvider) where TExecutor : IContractInvoker, new()
        {
            ExecutorFactory<TExecutor> factory = new ExecutorFactory<TExecutor>();
            IContractInvoker contractInvoker = factory.Create(configuration, instanceProvider);
            builder.Use<ExecutorMiddleware>(new ExecutorMiddlewareOptions(contractInvoker, new ActionProvider(descriptor, configuration.EndpointProvider)));
            return builder;
        }

        public static IAppBuilder RegisterEndpoint(
            this IAppBuilder builder,
            ServerConfiguration configuration,
            string prefix,
            Action<IAppBuilder> configure)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Uri url = configuration.EndpointProvider.GetEndpoint(null, prefix, null);
            return builder.Map(url.ToString(), configure);
        }
    }
}