using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bolt.Server.Metadata
{
    public interface IBoltMetadataHandler
    {
        Task<bool> HandleBoltMetadataAsync(ServerActionContext context, IEnumerable<IContractInvoker> contracts);

        Task<bool> HandleContractMetadataAsync(ServerActionContext context);
    }
}