using System;
using System.Net.Http;

namespace Bolt.Client
{
    public class ClientSessionHandler : IClientSessionHandler
    {
        private readonly BoltOptions _options;

        public ClientSessionHandler(BoltOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public virtual void EnsureSession(HttpRequestMessage request, string session)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!string.IsNullOrEmpty(session))
            {
                if (!request.Headers.Contains(_options.SessionHeader))
                {
                    request.Headers.Add(_options.SessionHeader, session);
                }
                else
                {
                    request.Headers.Remove(_options.SessionHeader);
                    request.Headers.Add(_options.SessionHeader, session);
                }
            }
        }

        public virtual string GetSessionIdentifier(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            return response.Headers.GetHeaderValue(_options.SessionHeader);
        }
    }
}
