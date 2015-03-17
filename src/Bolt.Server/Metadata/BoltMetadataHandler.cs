using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Server.Metadata
{
    public class BoltMetadataHandler : IBoltMetadataHandler
    {
        public BoltMetadataHandler(ILoggerFactory factory)
        {
            Logger = factory.Create<BoltMetadataHandler>();
        }

        public ILogger Logger { get; private set; }

        public virtual async Task<bool> HandleBoltMetadataAsync(HttpContext context, IEnumerable<IContractInvoker> contracts)
        {
            try
            {
                var result = JsonConvert.SerializeObject(contracts.Select(c => c.Descriptor.Name).ToList(), Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                await context.Response.SendAsync(result);
                return true;
            }
            catch (Exception e)
            {
                Logger.WriteWarning("Failed to generate Bolt root metadata. Error: {0}", e);
                return false;
            }
        }

        public virtual async Task<bool> HandleContractMetadataAsync(HttpContext context, IContractInvoker descriptor)
        {
            try
            {
                string result = string.Empty;
                string actionName = context.Request.Query["action"]?.Trim();
                ActionDescriptor action = null; ;

                if (!string.IsNullOrEmpty(actionName))
                {
                    action = descriptor.Descriptor.Find(actionName);
                }

                if (action == null)
                {
                    var contractMetadata = CrateContractMetadata(descriptor);
                    result = JsonConvert.SerializeObject(contractMetadata, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                }
                else
                {
                    if (action.HasParameters)
                    {
                        JsonSchemaGenerator generator = new JsonSchemaGenerator();

                        using (var sw = new StringWriter())
                        {
                            using (var jw = new JsonTextWriter(sw))
                            {
                                jw.Formatting = Formatting.Indented;
                                var schema = generator.Generate(action.Parameters);
                                schema.WriteTo(jw);
                            }

                            result = sw.GetStringBuilder().ToString();
                        }
                    }
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;
                await context.Response.SendAsync(result);
                return true;
            }
            catch (Exception e)
            {
                Logger.WriteWarning("Failed to generate Bolt metadata for contract '{0}'. Error: {1}", descriptor, e);
                return false;
            }
        }

        private ContractMetadata CrateContractMetadata(IContractInvoker descriptor)
        {
            var m = new ContractMetadata()
            {
                Actions = descriptor.Descriptor.Select(a => a.Name).ToList(),
            };

            var invoker = descriptor as ContractInvoker;
            if (invoker == null)
            {
                return m;
            }

            m.ErrorHeader = (invoker.Parent as BoltRouteHandler)?.Options.ServerErrorCodesHeader;
            m.ContentType = invoker.DataHandler.Serializer.ContentType;
            var statefullProvder = invoker.InstanceProvider as StateFullInstanceProvider;
            if (statefullProvder != null)
            {
                m.SessionInit = statefullProvder.InitSession.Name;
                m.SessionClose = statefullProvder.CloseSession.Name;
                m.SessionClose = statefullProvder.CloseSession.Name;
                m.SessionTimeout =(int) statefullProvder.SessionTimeout.TotalSeconds;
            }

            return m;
        }

        protected class ContractMetadata
        {
            public string ContentType { get; set; }

            public string ErrorHeader { get; set; }

            public string SessionHeader { get; set; }

            public int? SessionTimeout{ get; set; }

            public string SessionInit { get; set; }

            public string SessionClose { get; set; }

            public List<string> Actions { get; set; }
        }
    }
}