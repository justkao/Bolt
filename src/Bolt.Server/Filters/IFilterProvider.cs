using System.Collections.Generic;

namespace Bolt.Server.Filters
{
    public interface IFilterProvider
    {
        IEnumerable<IActionExecutionFilter> GetFilters(ServerActionContext context);
    }
}