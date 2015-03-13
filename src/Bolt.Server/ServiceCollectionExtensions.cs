using System;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureBoltOptions(this IServiceCollection services, Action<BoltServerOptions> configure)
        {
            services.Configure(configure);
            return services;
        }

        public static IServiceCollection AddBolt(this IServiceCollection services, IConfiguration configuration = null)
        {
            services.AddTransient<ISerializer, JsonSerializer>();
            services.AddTransient<IExceptionWrapper, JsonExceptionWrapper>();
            services.AddTransient<IResponseHandler, ResponseHandler>();
            services.AddTransient<IServerDataHandler, ServerDataHandler>();
            services.AddTransient<IBoltRouteHandler, BoltRouteHandler>();

            return services;
        }
    }
}