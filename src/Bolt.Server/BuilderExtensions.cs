using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Bolt.Server
{
    public static class BuliderExtensions
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
            registerContracts(bolt);
            return app.UseRouter(bolt);
        }
    }
}