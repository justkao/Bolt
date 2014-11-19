namespace Bolt.Server
{
    public class ServerConfiguration : Configuration
    {
        public ServerConfiguration()
        {
            ServerDataHandler = new ServerDataHandler(Serializer);
            ResponseHandler = new ResponseHandler(ServerDataHandler);
        }

        public IResponseHandler ResponseHandler { get; set; }

        public IServerDataHandler ServerDataHandler { get; set; }
    }
}
