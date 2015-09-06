using System;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;

namespace Bolt.Server.Session
{
    public class HttpContextSessionProvider : IHttpSessionProvider
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
            get { return _httpContextAccessor.HttpContext?.Features.Get<IContractSession>()?.SessionId; }
        }

        public ISession Session
        {
            get { return _httpContextAccessor.HttpContext?.Features.Get<ISessionFeature>()?.Session; }
        }
    }
}
