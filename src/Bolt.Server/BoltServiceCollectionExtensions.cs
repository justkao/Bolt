using System;
using Bolt;
using Bolt.Server;
using Bolt.Server.Metadata;
using Bolt.Server.Pipeline;
using Bolt.Server.Session;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class BoltServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureBolt(this IServiceCollection services, Action<BoltServerOptions> configure)
        {
            services.Configure(configure);

            return services;
        }

        public static IServiceCollection AddBolt(this IServiceCollection services)
        {
            services.ConfigureOptions<ConfigureServerRuntimeConfiguration>();

            services.TryAddTransient<IBoltRouteHandler, BoltRouteHandler>();
            services.TryAddTransient<IExceptionWrapper, JsonExceptionWrapper>();
            services.TryAddTransient<IBoltMetadataHandler, BoltMetadataHandler>();
            services.TryAddTransient<IContractInvoker, ContractInvoker>();
            services.TryAddSingleton<IActionResolver, ActionResolver>();
            services.TryAddSingleton<IContractResolver, ContractResolver>();
            services.TryAddTransient<IServerSessionHandler, ServerSessionHandler>();
            services.TryAddTransient<ISessionProvider, HttpContextSessionProvider>();
            services.TryAddTransient<IHttpSessionProvider, HttpContextSessionProvider>();
            services.TryAddSingleton<IContractInvokerFactory, ContractInvokerFactory>();
            services.TryAddSingleton<IServerPipelineBuilder, ServerPipelineBuilder>();
            services.TryAddSingleton<IServerErrorHandler, ServerErrorHandler>();
            services.TryAddSingleton<ISerializer, JsonSerializer>();

            return services;
        }
    }
}