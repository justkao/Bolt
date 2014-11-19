using System;

namespace Bolt.Client
{
    public class ChannelOpenedResult
    {
        public ChannelOpenedResult(Uri server, object response, string sessionId)
        {
            Server = server;
            Response = response;
            SessionId = sessionId;
        }

        public Uri Server { get; private set; }

        public object Response { get; private set; }

        public string SessionId { get; private set; }
    }
}