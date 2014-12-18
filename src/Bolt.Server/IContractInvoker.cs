using System.Threading.Tasks;

using HttpContext = Microsoft.Owin.IOwinContext;

namespace Bolt.Server
{
    public interface IContractInvoker : IContractDescriptorProvider
    {
        Task Execute(HttpContext context, ActionDescriptor action);
    }
}