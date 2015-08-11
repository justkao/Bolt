namespace Bolt.Server
{
    public class BoltFeature : IBoltFeature
    {
        public BoltFeature(ServerActionContext actionContext)
        {
            ActionContext = actionContext;
        }

        public ServerActionContext ActionContext { get;  }
    }
}