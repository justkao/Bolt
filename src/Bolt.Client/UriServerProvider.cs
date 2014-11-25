using System;

namespace Bolt.Client
{
    public class UriServerProvider : IServerProvider
    {
        private readonly Uri _url;

        public UriServerProvider(Uri url)
        {
            _url = url;
        }

        public Uri GetServer()
        {
            return _url;
        }
    }
}