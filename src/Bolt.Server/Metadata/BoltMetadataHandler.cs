using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

            _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
            Logger = factory.CreateLogger<BoltMetadataHandler>();
        }

        public ILogger Logger { get; }

        public virtual async Task HandleBoltMetadataAsync(ServerActionContext context, IEnumerable<IContractInvoker> contracts)
        {
            try
            {
                string result = JsonConvert.SerializeObject(
                    contracts.Select(c => c.Contract.NormalizedName).ToList(),
                    Formatting.Indented,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                await context.HttpContext.Response.WriteAsync(result);
            }
            catch (Exception e)
            {
                Logger.LogWarning(BoltLogId.HandleBoltRootError, "Failed to generate Bolt root metadata. Error: {0}", e);
            }
        }

        public virtual async Task HandleContractMetadataAsync(ServerActionContext context)
        {
            try
            {
                string result;
                string actionName = context.HttpContext.Request.Query["action"];

                Bolt.Metadata.ActionMetadata action = null;
                if (!string.IsNullOrEmpty(actionName))
                {
                    action = _actionResolver.Resolve(context.Contract, actionName.AsReadOnlySpan());
                }

                if (action == null)
                {
                    var contractMetadata = CrateContractMetadata(context);
                    result = JsonConvert.SerializeObject(contractMetadata, Formatting.Indented, CreateSettings());
                }
                else
                {
                    context.Action = action;
                    ActionMetadata actionMetadata = new ActionMetadata();

                    if (context.Action.HasSerializableParameters)
                    {
                        actionMetadata.Parameters = context.Action.Parameters.Where(p => p.IsSerializable).Select(p => p.Name).ToArray();
                    }

                    if (context.Action.HasResult)
                    {
                        actionMetadata.Response = context.Action.ResultType.Name;
                    }

                    result = JsonConvert.SerializeObject(actionMetadata, Formatting.Indented, CreateSettings());
                }

                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = 200;
                await context.HttpContext.Response.WriteAsync(result);
            }
            catch (Exception e)
            {
                Logger.LogWarning(
                    BoltLogId.HandleContractMetadataError,
                    "Failed to generate Bolt metadata for contract '{0}'. Error: {1}",
                    context.Contract.Name,
                    e);
            }
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ContractResolver = new CamelCasePropertyNamesContractResolver() };
        }

        private RawContractMetadata CrateContractMetadata(ServerActionContext context)
        {
            var feature = context.HttpContext.Features.Get<IBoltFeature>();
            var m = new RawContractMetadata
                        {
                            Actions = context.Contract.Actions.ToArray().Select(a => a.Name).ToList(),
                            ErrorHeader = feature.ActionContext.Configuration.Options.ServerErrorHeader,
                            ContentTypes = feature.ActionContext.Configuration.AvailableSerializers.Select(s => s.MediaType).ToArray()
                        };
            return m;
        }

        protected class ActionMetadata
        {
            public string[] Parameters { get; set; }

            public string Response { get; set; }
        }

        protected class RawContractMetadata
        {
            public string[] ContentTypes { get; set; }

            public string ErrorHeader { get; set; }

            public string SessionHeader { get; set; }

            public List<string> Actions { get; set; }
        }
    }
}