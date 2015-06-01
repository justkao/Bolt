using System;
using Bolt.Core;
using Microsoft.AspNet.Hosting;

namespace Bolt.Server.InstanceProviders
{
    public class HttpContextSessionProvider : ISessionProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextSessionProvider(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }

            _httpContextAccessor = httpContextAccessor;
        }

        public string SessionId
        {
            get
            {
                IContractSession session = _httpContextAccessor.HttpContext?.GetFeature<IContractSession>();
                return session?.SessionId;
            }
        }
    }
}
