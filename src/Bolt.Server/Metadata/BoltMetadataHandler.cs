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
        private static readonly Dictionary<Type, string> BuildInClasses = new Dictionary<Type, string>()
        {
            { typeof(int), "int" },
            { typeof(short), "short" },
            { typeof(bool), "bool" },
            { typeof(long), "long" },
            { typeof(double), "double" },
            { typeof(string), "float" },
            { typeof(DateTime), "datetime" },
            { typeof(TimeSpan), "timespan" }
        };

        public BoltMetadataHandler(ILoggerFactory factory)
        {
            Logger = factory.Create<BoltMetadataHandler>();
        }

        public ILogger Logger { get; private set; }

        public virtual async Task<bool> HandleBoltMetadataAsync(HttpContext context, IEnumerable<ContractDescriptor> contracts)
        {
            try
            {
                var result = JsonConvert.SerializeObject(contracts.Select(c => c.Name).ToList(), Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                await context.Response.SendAsync(result);
                return true;
            }
            catch (Exception e)
            {
                Logger.WriteWarning("Failed to generate Bolt root metadata. Error: {0}", e);
                return false;
            }
        }

        public virtual async Task<bool> HandleContractMetadataAsync(HttpContext context, ContractDescriptor descriptor)
        {
            try
            {
                string result = string.Empty;
                string actionName = context.Request.Query["action"]?.Trim();
                ActionDescriptor action = null; ;

                if (!string.IsNullOrEmpty(actionName))
                {
                    action = descriptor.Find(actionName);
                }

                if (action == null)
                { 
                    result = JsonConvert.SerializeObject(descriptor.Select(a => a.Name).ToList(), Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
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
    }
}