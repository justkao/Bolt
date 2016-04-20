using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bolt.Server.Metadata
{
    public interface IBoltMetadataHandler
    {
        Task HandleBoltMetadataAsync(ServerActionContext context, IEnumerable<IContractInvoker> contracts);

        Task HandleContractMetadataAsync(ServerActionContext context);
    }
}