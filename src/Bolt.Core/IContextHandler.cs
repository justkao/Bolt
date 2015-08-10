using System;
using System.Threading.Tasks;
using Bolt.Core;

namespace Bolt
{
    public interface IContextHandler<T> where T:ActionContextBase
    {
        HandleContextStage Stage { get; }

        Task HandleAsync(T context, Func<T, Task> next);
    }
}