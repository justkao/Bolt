using System;
using System.Collections.Generic;
using System.Linq;
using Bolt;
using Bolt.Serialization;
using Bolt.Server;
using Bolt.Server.Metadata;
using Bolt.Server.Pipeline;
using Bolt.Server.Session;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class BoltServiceCollectionExtensions
    {
        public static IServiceCollection AddBolt(this IServiceCollection services, Action<BoltServerOptions> configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }

            services.TryAddSingleton(s => new ServerRuntimeConfiguration
            {
                Options = s.GetRequiredService<IOptions<BoltServerOptions>>().Value,
                AvailableSerializers = s.GetRequiredService<IEnumerable<ISerializer>>().ToList(),
                ExceptionSerializer = s.GetRequiredService<IExceptionSerializer>(),
                ErrorHandler = s.GetRequiredService<IServerErrorHandler>()
            });

            services.TryAddTransient<IBoltRouteHandler, BoltRouteHandler>();
            services.TryAddTransient<IExceptionSerializer, JsonExceptionSerializer>();
            services.TryAddTransient<IBoltMetadataHandler, BoltMetadataHandler>();
            services.TryAddTransient<IContractInvoker, ContractInvoker>();
            services.TryAddSingleton<IActionResolver, ActionResolver>();
            services.TryAddSingleton<IContractInvokerSelector, ContractInvokerSelector>();
            services.TryAddTransient<IServerSessionHandler, ServerSessionHandler>();
            services.TryAddTransient<ISessionProvider, HttpContextSessionProvider>();
            services.TryAddTransient<IHttpSessionProvider, HttpContextSessionProvider>();
            services.TryAddSingleton<IContractInvokerFactory, ContractInvokerFactory>();
            services.TryAddSingleton<IServerPipelineBuilder, ServerPipelineBuilder>();
            services.TryAddSingleton<IServerErrorHandler, ServerErrorHandler>();
            services.TryAddSingleton<ISerializer, JsonSerializer>();

            services.AddRouting();

            return services;
        }
    }
}