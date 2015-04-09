using System;
using Bolt;
using Bolt.Server;
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
            services.AddTransient<ISerializer, JsonSerializer>();
            services.AddTransient<IExceptionWrapper, JsonExceptionWrapper>();
            services.AddTransient<IResponseHandler, ResponseHandler>();
            services.AddTransient<IBoltRouteHandler, BoltRouteHandler>();
            services.AddTransient<IServerErrorHandler, ServerErrorHandler>();
            services.AddTransient<IBoltMetadataHandler, BoltMetadataHandler>();
            services.AddTransient<IParameterBinder, EmptyParameterBinder>();

            return services;
        }
    }
}