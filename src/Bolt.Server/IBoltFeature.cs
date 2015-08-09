using Bolt.Server.Filters;
using System.Collections.Generic;

namespace Bolt.Server
{
    public interface IBoltFeature
    {
        IBoltRouteHandler Root { get; }

        ServerActionContext ActionContext { get; set; }

        ServerRuntimeConfiguration Configuration { get; set; }

        IList<IFilterProvider> FilterProviders { get; set; }
    }
}