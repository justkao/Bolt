using System.Threading.Tasks;

using HttpContext = Microsoft.Owin.IOwinContext;

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