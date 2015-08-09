using System;
using System.Threading.Tasks;

namespace Bolt.Client.Filters
{
    public interface IClientExecutionFilter
    {
        Task ExecuteAsync(ClientActionContext context, Func<ClientActionContext, Task> next);
    }
}