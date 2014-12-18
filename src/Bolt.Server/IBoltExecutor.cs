using System.Threading.Tasks;

#if OWIN
using HttpContext = Microsoft.Owin.IOwinContext;
#else
using HttpContext = Microsoft.AspNet.Http.HttpContext;
#endif

namespace Bolt.Server
{
    public interface IBoltExecutor
    {
        ServerConfiguration Configuration { get; }

        void Add(IContractInvoker contractInvoker);

        IContractInvoker Get(ContractDescriptor descriptor);

        Task Execute(HttpContext context);
    }
}