namespace Bolt.Server
{
    public class ServerConfiguration : Configuration
    {
        public ServerConfiguration(ISerializer serializer)
            : base(serializer)
        {
            ServerDataHandler = new ServerDataHandler(serializer, ExceptionSerializer);
            ResponseHandler = new ResponseHandler(ServerDataHandler);
        }

        public ServerConfiguration()
        {
            ServerDataHandler = new ServerDataHandler(Serializer, ExceptionSerializer);
            ResponseHandler = new ResponseHandler(ServerDataHandler);
        }

        public IResponseHandler ResponseHandler { get; set; }

        public IServerDataHandler ServerDataHandler { get; set; }
    }
}
