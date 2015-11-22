using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace Bolt.Server.Metadata
{
    public class BoltMetadataHandler : IBoltMetadataHandler
    {
        private readonly IActionResolver _actionResolver;

        public BoltMetadataHandler(ILoggerFactory factory, IActionResolver actionResolver)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (actionResolver == null)
            {
                throw new ArgumentNullException(nameof(actionResolver));
            }

            _actionResolver = actionResolver;
            Logger = factory.CreateLogger<BoltMetadataHandler>();
        }

        public ILogger Logger { get; }

        public virtual async Task<bool> HandleBoltMetadataAsync(ServerActionContext context, IEnumerable<IContractInvoker> contracts)
        {
            try
            {
                string result = JsonConvert.SerializeObject(
                    contracts.Select(c => BoltFramework.GetContractName(c.Contract)).ToList(),
                    Formatting.Indented,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                await context.HttpContext.Response.WriteAsync(result);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogWarning(BoltLogId.HandleBoltRootError, "Failed to generate Bolt root metadata. Error: {0}", e);
                return false;
            }
        }

        public virtual async Task<bool> HandleContractMetadataAsync(ServerActionContext context)
        {
            try
            {
                string result = string.Empty;
                string actionName = context.HttpContext.Request.Query["action"][0]?.Trim();
                MethodInfo action = null;

                if (!string.IsNullOrEmpty(actionName))
                {
                    action = _actionResolver.Resolve(context.Contract, actionName);
                }

                if (action == null)
                {
                    var contractMetadata = CrateContractMetadata(context);
                    result = JsonConvert.SerializeObject(
                        contractMetadata,
                        Formatting.Indented,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }
                else
                {
                    context.Action = action;
                    JsonSchema actionSchema = new JsonSchema
                                                  {
                                                      Properties = new Dictionary<string, JsonSchema>(),
                                                      Description = $"Request and response parameters for action '{actionName}'."
                                                  };

                    if (context.ActionMetadata.HasSerializableParameters)
                    {
                        JsonSchemaGenerator generator = new JsonSchemaGenerator();
                        JsonSchema arguments = new JsonSchema
                                                   {
                                                       Properties =
                                                           context.ActionMetadata.SerializableParameters.ToDictionary(
                                                               p => p.Name,
                                                               p => generator.Generate(p.Type)),
                                                       Required = true,
                                                       Type = JsonSchemaType.Object
                                                   };

                        actionSchema.Properties.Add("request", arguments);
                    }

                    if (context.ActionMetadata.HasResult)
                    {
                        JsonSchemaGenerator generator = new JsonSchemaGenerator();
                        actionSchema.Properties.Add("response", generator.Generate(context.ActionMetadata.ResultType));
                    }

                    using (var sw = new StringWriter())
                    {
                        using (JsonTextWriter jw = new JsonTextWriter(sw))
                        {

                            jw.Formatting = Formatting.Indented;
                            actionSchema.WriteTo(jw);
                        }
                        result = sw.GetStringBuilder().ToString();
                    }
                }

                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = 200;
                await context.HttpContext.Response.WriteAsync(result);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogWarning(
                    BoltLogId.HandleContractMetadataError,
                    "Failed to generate Bolt metadata for contract '{0}'. Error: {1}",
                    context.Contract.Name,
                    e);
                return false;
            }
        }

        private ContractMetadata CrateContractMetadata(ServerActionContext context)
        {
            var feature = context.HttpContext.Features.Get<IBoltFeature>();
            var m = new ContractMetadata
                        {
                            Actions = BoltFramework.GetContractActions(context.Contract).Select(a => a.Name).ToList(),
                            ErrorHeader = feature.ActionContext.Configuration.Options.ServerErrorHeader,
                            ContentTypes = feature.ActionContext.Configuration.AvailableSerializers.Select(s => s.MediaType).ToArray()
                        };
            return m;
        }

        protected class ContractMetadata
        {
            public string[] ContentTypes { get; set; }

            public string ErrorHeader { get; set; }

            public string SessionHeader { get; set; }

            public List<string> Actions { get; set; }
        }
    }
}