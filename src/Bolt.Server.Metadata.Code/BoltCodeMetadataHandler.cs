using System.Linq;
using System.Threading.Tasks;
using Bolt.Generators;
using Bolt.Server.Metadata;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;

namespace Bolt.Server.Generator
{
    public class BoltCodeMetadataHandler : BoltMetadataHandler
    {
        public BoltCodeMetadataHandler(ILoggerFactory factory) : base(factory)
        {
        }

        public override async Task<bool> HandleContractMetadataAsync(HttpContext context, IContractInvoker descriptor)
        {
            DocumentGenerator generator = new DocumentGenerator();
            var definition = new ContractDefinition(descriptor.Descriptor.Type);

            var contractGen = new ContractDescriptorGenerator { ContractDefinition = definition };
            var interfaceGen = new InterfaceGenerator { ContractDefinition = definition, ForceAsync=true };
            var clientGen = new ClientGenerator { ContractDefinition = definition, ForceAsync = true };

            generator.Add(contractGen);
            generator.Add(interfaceGen);
            generator.Add(clientGen);

            interfaceGen.Generated += (s, e) =>
            {
                clientGen.BaseInterfaces = interfaceGen.GeneratedAsyncInterfaces.ToList();
            };

            var code = generator.GetResult();

            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            await context.Response.SendAsync(code);

            return true;
        }
    }
}
