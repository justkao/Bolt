using Owin;

namespace Bolt.Server
{
    public static class ExecutorMiddlewareExtensions
    {
        public static IAppBuilder RegisterEndpoint<TContract, TContractImplementation, TExecutor>(
            this IAppBuilder builder, ContractDefinition definition, string prefix, ServerConfiguration configuration, IInstanceProvider instanceProvider = null)
            where TExecutor : IExecutor, new()
            where TContractImplementation : TContract
        {
            return builder.Map(
                prefix,
                (b) =>
                    b.RegisterContract<TContract, TContractImplementation, TExecutor>(definition, configuration,
                        instanceProvider));
        }

        public static void RegisterContract<TContract, TContractImplementation, TExecutor>(this IAppBuilder builder, ContractDefinition definition, ServerConfiguration configuration, IInstanceProvider instanceProvider = null)
            where TExecutor : IExecutor, new()
            where TContractImplementation : TContract
        {
            ExecutorFactory<TExecutor> factory = new ExecutorFactory<TExecutor>(definition);

            builder.Use<ExecutorMiddleware>(
                new ExecutorMiddlewareOptions(
                    factory.Create(configuration, instanceProvider ?? new InstanceProvider(definition, typeof(TContractImplementation)))));
        }

        public static void RegisterStatefullContract<TContract, TContractImplementation, TExecutor>(
            this IAppBuilder builder, ContractDefinition definition, ServerConfiguration configuration, IInstanceProvider instanceProvider = null)
            where TExecutor : IExecutor, new()
            where TContractImplementation : TContract
        {
            ExecutorFactory<TExecutor> factory = new ExecutorFactory<TExecutor>(definition);

            if (instanceProvider == null)
            {
                instanceProvider = new StateFullInstanceProvider(definition, typeof(TContractImplementation))
                {
                    SessionHeader = configuration.SessionHeaderName
                };
            }

            builder.Use<ExecutorMiddleware>(new ExecutorMiddlewareOptions(factory.Create(configuration, instanceProvider)));
        }
    }
}