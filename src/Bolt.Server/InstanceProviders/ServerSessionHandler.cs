using Microsoft.AspNet.Http;
using System;

namespace Bolt.Server.InstanceProviders
{
    public class ServerSessionHandler : IServerSessionHandler
    {
        public string SessionHeader => _options.SessionHeader;

        private readonly BoltServerOptions _options;

        public ServerSessionHandler(BoltServerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        public virtual string GetIdentifier(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string sessionId = context.Request.Headers[SessionHeader];
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = context.Response.Headers[SessionHeader];
            }

            return sessionId;
        }

        public virtual string Initialize(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var session = GenerateIdentifier();
            context.Response.Headers[SessionHeader] = session;
            return session;
        }

        public virtual void Destroy(HttpContext context)
        {
        }

        protected virtual string GenerateIdentifier()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
