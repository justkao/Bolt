using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Server.Metadata
{
    public interface IBoltMetadataHandler
    {
        Task<bool> HandleBoltMetadataAsync(HttpContext context, IEnumerable<IContractInvoker> contracts);

        Task<bool> HandleContractMetadataAsync(HttpContext context, IContractInvoker descriptor);
    }
}