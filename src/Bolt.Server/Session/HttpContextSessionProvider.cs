﻿using System;

using Microsoft.AspNet.Hosting;
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
            get { return _httpContextAccessor.HttpContext?.GetFeature<IContractSession>()?.SessionId; }
        }

        public ISession Session
        {
            get { return _httpContextAccessor.HttpContext?.GetFeature<ISessionFeature>()?.Session; }
        }
    }
}