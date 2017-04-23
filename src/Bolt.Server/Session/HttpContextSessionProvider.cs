using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Bolt.Server.Session
{
    public class HttpContextSessionProvider : IHttpSessionProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextSessionProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
