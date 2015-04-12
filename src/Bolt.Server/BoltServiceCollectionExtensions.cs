using System;
using Bolt;
using Bolt.Server;
using Bolt.Server.Filters;
using Bolt.Server.Metadata;

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
            services.AddTransient<IResponseHandler, ResponseHandler>();
            services.AddTransient<IBoltRouteHandler, BoltRouteHandler>();
            services.AddTransient<IServerErrorHandler, ServerErrorHandler>();
            services.AddTransient<IBoltMetadataHandler, BoltMetadataHandler>();
            services.AddTransient<IFilterProvider, DefaultFilterProvider>();
            services.AddTransient<IActionExecutionFilter, CoreAction>();
            services.AddTransient<IContractInvoker, ContractInvoker>();
            services.AddTransient<IActionPicker, ActionPicker>();

            return services;
        }
    }
}