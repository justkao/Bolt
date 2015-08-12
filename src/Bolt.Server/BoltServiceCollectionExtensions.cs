using System;

using Bolt;
using Bolt.Server;
using Bolt.Server.Metadata;
using Bolt.Server.Pipeline;
using Bolt.Server.Session;
using Bolt.Session;

// ReSharper disable once CheckNamespace
namespace Microsoft.Framework.DependencyInjection
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

            services.AddTransient<ISerializer, JsonSerializer>();
            services.AddTransient<IExceptionWrapper, JsonExceptionWrapper>();
            services.AddTransient<IBoltRouteHandler, BoltRouteHandler>();
            services.AddTransient<IBoltMetadataHandler, BoltMetadataHandler>();
            services.AddTransient<IContractInvoker, ContractInvoker>();
            services.AddTransient<IActionResolver, ActionResolver>();
            services.AddTransient<IContractResolver, ContractResolver>();
            services.AddTransient<IServerSessionHandler, ServerSessionHandler>();
            services.AddTransient<ISessionProvider, HttpContextSessionProvider>();
            services.AddTransient<IHttpSessionProvider, HttpContextSessionProvider>();
            services.AddSingleton<IContractInvokerFactory, ContractInvokerFactory>();
            services.AddSingleton<IServerPipelineBuilder, ServerPipelineBuilder>();
            services.AddSingleton<IServerErrorHandler, ServerErrorHandler>();

            return services;
        }
    }
}