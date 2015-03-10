using System;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;

namespace Bolt.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureBoltOptions(this IServiceCollection services, Action<BoltOptions> configure)
        {
            services.Configure(configure);
            return services;
        }

        public static IServiceCollection AddBolt(this IServiceCollection services, IConfiguration configuration = null)
        {
            services.AddTransient<ISerializer, JsonSerializer>();
            services.AddTransient<IExceptionSerializer, JsonExceptionSerializer>();
            services.AddTransient<IResponseHandler, ResponseHandler>();
            services.AddTransient<IDataHandler, DataHandler>();
            services.AddTransient<IErrorHandler, ErrorHandler>();
            services.AddTransient<IBoltRouteHandler, BoltRouteHandler>();

            return services;
        }
    }
}