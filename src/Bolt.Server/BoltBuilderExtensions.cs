using System;
using Bolt.Server;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNet.Builder
{
    public static class BoltBuilderExtensions
    {
        /// <summary>
        /// Adds Bolt to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="configure">Register Bolt contracts.</param>
        /// <returns>The <paramref name="app"/>THe builder instance.</returns>
        public static IApplicationBuilder UseBolt(this IApplicationBuilder app, Action<IBoltRouteHandler> configure)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var bolt = app.ApplicationServices.GetRequiredService<IBoltRouteHandler>();
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("Bolt");

            logger.LogInformation(BoltLogId.BoltRegistration, "Registering Bolt middleware. Prefix: {0}", bolt.Configuration.Options.Prefix);
            configure(bolt);

            return app.UseRouter(bolt);
        }
    }
}