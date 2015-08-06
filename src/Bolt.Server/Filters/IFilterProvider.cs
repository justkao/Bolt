using System.Collections.Generic;

namespace Bolt.Server.Filters
{
    public interface IFilterProvider
    {
        IEnumerable<IServerExecutionFilter> GetFilters(ServerActionContext context);
    }
}