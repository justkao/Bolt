using System;
using Bolt;
using Bolt.Server;
using Bolt.Server.Filters;
using Bolt.Server.Metadata;
using Bolt.Server.InstanceProviders;
using Bolt.Session;
using Newtonsoft.Json;

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

            services.AddTransient<ISerializer, Bolt.JsonSerializer>();
            services.AddTransient<IExceptionWrapper, JsonExceptionWrapper>();
            services.AddTransient<IResponseHandler, ResponseHandler>();
            services.AddTransient<IBoltRouteHandler, BoltRouteHandler>();
            services.AddTransient<IServerErrorHandler, ServerErrorHandler>();
            services.AddTransient<IBoltMetadataHandler, BoltMetadataHandler>();
            services.AddTransient<IFilterProvider, DefaultFilterProvider>();
            services.AddTransient<IContractInvoker, ContractInvoker>();
            services.AddTransient<IActionInvoker, ActionInvoker>();
            services.AddTransient<IActionResolver, ActionResolver>();
            services.AddTransient<IContractResolver, ContractResolver>();
            services.AddTransient<IParameterHandler, ParameterHandler>();
            services.AddTransient<ISessionHandler, SessionHandler>();
            services.AddTransient<IServerSessionHandler, ServerSessionHandler>();
            services.AddScoped<ISessionProvider, HttpContextSessionProvider>();
            services.AddSingleton<IContractInvokerFactory, ContractInvokerFactory>();

            return services;
        }
    }
}