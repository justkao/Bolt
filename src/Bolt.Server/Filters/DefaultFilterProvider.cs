using System.Collections.Generic;

namespace Bolt.Server.Filters
{
    public class DefaultFilterProvider : IFilterProvider
    {
        public IEnumerable<IServerExecutionFilter> GetFilters(ServerActionContext context)
        {
            foreach (var filter in context.ContractInvoker.Filters)
            {
                yield return filter;
            }
            
            foreach (var filter in context.HttpContext.GetFeature<IBoltFeature>().Root.Filters)
            {
                yield return filter;
            }
        }
    }
}