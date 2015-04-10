using System.Collections.Generic;

namespace Bolt.Server.Filters
{
    public class DefaultFilterProvider : IFilterProvider
    {
        public IEnumerable<IActionExecutionFilter> GetFilters(ServerActionContext context)
        {
            foreach (var filter in context.ContractInvoker.Filters)
            {
                yield return filter;
            }

            foreach (var filter in context.ContractInvoker.Parent.Filters)
            {
                yield return filter;
            }
        }
    }
}