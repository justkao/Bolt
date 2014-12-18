using System.Threading.Tasks;

#if OWIN
using HttpContext = Microsoft.Owin.IOwinContext;
#else
using HttpContext = Microsoft.AspNet.Http.HttpContext;
#endif

namespace Bolt.Server
{
    public interface IContractInvoker : IContractDescriptorProvider
    {
        Task Execute(HttpContext context, ActionDescriptor action);
    }
}