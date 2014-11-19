namespace Bolt.Server
{
    public class ServerConfiguration : Configuration
    {
        public ServerConfiguration()
        {
            ServerDataHandler = new ServerDataHandler();
            ResponseHandler = new ResponseHandler(ServerDataHandler);
        }

        public IResponseHandler ResponseHandler { get; set; }

        public IServerDataHandler ServerDataHandler { get; set; }
    }
}
