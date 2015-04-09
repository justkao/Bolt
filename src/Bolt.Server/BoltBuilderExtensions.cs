using System;
using Bolt.Server;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Builder
{
    public static class BoltBuilderExtensions
    {
        /// <summary>
        /// Adds Bolt to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="configure">Register Bolt contracts.</param>
        /// <param name="options">The options for this route handler, if specified the default options will be overwritten.</param>
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
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().Create("Bolt");

            logger.WriteInformation(BoltLogId.BoltRegistration, "Registering Bolt middleware. Prefix: {0}", bolt.Options.Prefix);
            configure(bolt);

            return app.UseRouter(bolt);
        }
    }
}