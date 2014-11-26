using System.Threading.Tasks;
using Microsoft.Owin;

namespace Bolt.Server
{
    public interface IBoltExecutor
    {
        ServerConfiguration Configuration { get; }

        void Add(IContractInvoker contractInvoker);

        Task Execute(IOwinContext context);
    }
}