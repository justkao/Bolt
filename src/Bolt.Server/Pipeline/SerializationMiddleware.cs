using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Bolt.Metadata;
using Bolt.Pipeline;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace Bolt.Server.Pipeline
{
    public class SerializationMiddleware : MiddlewareBase<ServerActionContext>
    {
        public override async Task InvokeAsync(ServerActionContext context)
        {
            if (context.Configuration.DefaultSerializer == null)
            {
                context.Configuration.DefaultSerializer = PickSerializer(context);
            }

            var actionMetadata = context.EnsureActionMetadata();
            if (actionMetadata.HasParameters && context.Parameters == null)
            {
                context.Parameters = await DeserializeParameters(context, actionMetadata);
            }

            await Next(context);

            if (!context.ResponseHandled)
            {
                await HandleResponse(context);
                context.ResponseHandled = true;
            }
        }

        protected virtual async Task<object[]> DeserializeParameters(ServerActionContext context, ActionMetadata metadata)
        {
            ISerializer serializer = context.GetRequiredSerializer();

            DeserializeContext deserializeContext = new DeserializeContext();
            deserializeContext.Stream = await context.HttpContext.Request.Body.CopyAsync(context.RequestAborted);
            deserializeContext.ExpectedValues = metadata.SerializableParameters;

            try
            {
                await serializer.ReadAsync(deserializeContext);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new BoltServerException(
                    $"Failed to deserialize parameters for action '{context.Action.Name}'.",
                    ServerErrorCode.DeserializeParameters,
                    context.Action,
                    context.RequestUrl,
                    e);
            }
            finally
            {
                deserializeContext.Stream.Dispose();
            }

            // now fill the invocation parameters
            object[] parameterValues = new object[metadata.Parameters.Length];
            if (deserializeContext.Values != null)
            {
                for (int i = 0; i < deserializeContext.Values.Count; i++)
                {
                    KeyValuePair<string, object> pair = deserializeContext.Values[i];
                    for (int j = 0; j < metadata.Parameters.Length; j++)
                    {
                        if (metadata.Parameters[j].Name.EqualsNoCase(pair.Key))
                        {
                            parameterValues[j] = pair.Value;
                        }
                    }
                }
            }

            if (metadata.CancellationTokenIndex >= 0)
            {
                parameterValues[metadata.CancellationTokenIndex] = context.RequestAborted;
            }

            return parameterValues;
        }

        protected virtual async Task HandleResponse(ServerActionContext context)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            context.HttpContext.Response.StatusCode = 200;

            if (context.EnsureActionMetadata().HasResult && context.ActionResult != null)
            {
                context.HttpContext.Response.ContentType = context.Configuration.DefaultSerializer.MediaType;
                try
                {
                    await context.GetRequiredSerializer().WriteAsync(context.HttpContext.Response.Body, context.ActionResult);
                }
                catch (Exception e)
                {
                    throw new BoltServerException(
                        $"Failed to serialize response for action '{context.Action.Name}'.",
                        ServerErrorCode.SerializeResponse,
                        context.Action,
                        context.RequestUrl,
                        e);
                }
            }
            else
            {
                context.HttpContext.Response.ContentLength = 0;
            }

            await context.HttpContext.Response.Body.FlushAsync();
            context.HttpContext.Response.Body.Dispose();
        }

        protected virtual ISerializer PickSerializer(ServerActionContext context)
        {
            if (context.Configuration.AvailableSerializers.Count == 1)
            {
                return context.Configuration.AvailableSerializers[0];
            }

            StringValues value;
            if (context.HttpContext.Request.Headers.TryGetValue("Accept", out value))
            {
                MediaTypeHeaderValue header;
                if (MediaTypeHeaderValue.TryParse(value, out header))
                {
                    var found =
                        context.Configuration.AvailableSerializers.FirstOrDefault(f => f.MediaType == header.MediaType);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return context.Configuration.AvailableSerializers[0];
        }
    }
}
