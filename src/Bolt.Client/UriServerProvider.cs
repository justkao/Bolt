using System;

namespace Bolt.Client
{
    /// <summary>
    /// Provides Uri just for single specified server.
    /// </summary>
    public class UriServerProvider : IServerProvider
    {
        private readonly Uri _url;

        public UriServerProvider(Uri url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            _url = url;
        }

        public Uri GetServer()
        {
            return _url;
        }

        public void OnServerUnavailable(Uri server)
        {
        }
    }
}