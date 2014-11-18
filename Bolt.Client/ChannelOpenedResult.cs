using System;

namespace Bolt.Client
{
    public class ChannelOpenedResult
    {
        public ChannelOpenedResult(Uri server, object response)
        {
            Server = server;
            Response = response;
        }

        public Uri Server { get; private set; }

        public object Response { get; private set; }
    }
}