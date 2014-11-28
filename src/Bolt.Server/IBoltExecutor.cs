using Microsoft.Owin;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public interface IBoltExecutor
    {
        ServerConfiguration Configuration { get; }

        void Add(IContractInvoker contractInvoker);

        IContractInvoker Get(ContractDescriptor descriptor);

        Task Execute(IOwinContext context);
    }
}