using System.Collections.Generic;
using Bolt.Server.Filters;

namespace Bolt.Server
{
    public class BoltFeature : IBoltFeature
    {
        public BoltFeature(IBoltRouteHandler root)
        {
            Root = root;
        }

        public IBoltRouteHandler Root { get; private set; }

        public ServerActionContext ActionContext { get; set; }

        public ServerRuntimeConfiguration Configuration { get; set; }

        public IList<IFilterProvider> FilterProviders { get; set; }
    }
}