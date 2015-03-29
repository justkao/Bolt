using Bolt.Server;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using System;

namespace Microsoft.AspNet.Builder
{
    public static class BoltBuilderExtensions
    {
        /// <summary>
        /// Adds Bolt to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="registerContracts">Register Bolt contracts.</param>
        /// <returns>The <paramref name="app"/>.</returns>
        public static IApplicationBuilder UseBolt(this IApplicationBuilder app, Action<IBoltRouteHandler> registerContracts)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (registerContracts == null)
            {
                throw new ArgumentNullException(nameof(registerContracts));
            }

            var bolt = app.ApplicationServices.GetRequiredService<IBoltRouteHandler>();
            app.ApplicationServices.GetRequiredService<ILoggerFactory>().Create("Bolt").WriteInformation("Registering Bolt middleware. Prefix: {0}", bolt.Options.Prefix);
            registerContracts(bolt);

            return app.UseRouter(bolt);
        }
    }
}