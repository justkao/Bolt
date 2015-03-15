using System;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server
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
            services.AddTransient<ISerializer, JsonSerializer>();
            services.AddTransient<IExceptionWrapper, JsonExceptionWrapper>();
            services.AddTransient<IResponseHandler, ResponseHandler>();
            services.AddTransient<IServerDataHandler, ServerDataHandler>();
            services.AddTransient<IBoltRouteHandler, BoltRouteHandler>();
            services.AddTransient<IServerErrorHandler, ServerErrorHandler>();

            return services;
        }
    }
}