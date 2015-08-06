using System;
using System.Threading.Tasks;

namespace Bolt.Server.Filters
{
    public interface IServerExecutionFilter
    {
        int Order { get; }

        Task ExecuteAsync(ServerActionContext context, Func<ServerActionContext, Task> next);
    }
}