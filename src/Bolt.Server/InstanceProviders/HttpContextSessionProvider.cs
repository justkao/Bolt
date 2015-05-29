using Bolt.Core;
using Microsoft.AspNet.Http;
using System;

namespace Bolt.Server.InstanceProviders
{
    public class HttpContextSessionProvider : ISessionProvider
    {
        private readonly HttpContext _context;

        public HttpContextSessionProvider(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
        }

        public string SessionId
        {
            get
            {
                var session = _context.GetFeature<IContractSession>();
                if ( session == null)
                {
                    return null;
                }

                return session.SessionId;
            }
        }
    }
}
