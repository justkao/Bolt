using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;

namespace Bolt.Server.Logging
{
    internal class ServerActionContextLogStructure : LoggerStructureBase
    {
        public ServerActionContextLogStructure(ServerActionContext context)
        {
            Uri = context.Context.Request.Path;
            Action = context.Action;
            Contract = context.Action.Contract;
        }

        public PathString Uri { get; set; }

        public ActionDescriptor Action { get; set; }

        public ContractDescriptor Contract { get; set; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}