using Microsoft.AspNet.Http;
using System;

namespace Bolt.Server.InstanceProviders
{
    public class SessionHandler
    {
        public string SessionHeader => _options.SessionHeader;

        private readonly BoltServerOptions _options;

        public SessionHandler(BoltServerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        public virtual string CreateIdentifier()
        {
            return Guid.NewGuid().ToString();
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

        public virtual void Initialize(HttpContext context, string session)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            context.Response.Headers[SessionHeader] = session;
        }

        public virtual void Destroy(HttpContext context)
        {
        }
    }
}
