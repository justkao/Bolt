using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace Bolt.Server.Metadata
{
    public interface IBoltMetadataHandler
    {
        Task<bool> HandleBoltMetadataAsync(HttpContext context, IEnumerable<IContractInvoker> contracts);

        Task<bool> HandleContractMetadataAsync(HttpContext context, IContractInvoker descriptor);
    }
}