using System.Collections.Generic;

namespace Bolt.Core
{
    public abstract class PipelineBuilder<T> where T:ActionContextBase
    {
        protected abstract IEnumerable<IContextHandler<T>> Build();
    }
}
