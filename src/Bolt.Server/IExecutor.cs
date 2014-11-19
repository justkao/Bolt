using System.Threading.Tasks;

using Microsoft.Owin;

namespace Bolt.Server
{
    public interface IExecutor
    {
        Task Execute(IOwinContext context, ActionDescriptor action);
    }
}