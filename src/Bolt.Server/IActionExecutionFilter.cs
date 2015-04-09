using System;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IActionExecutionFilter
    {
        Task ExecuteAsync(ServerActionContext context, Func<ServerActionContext, Task> next);
    }
}