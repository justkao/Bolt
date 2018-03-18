using System;
using System.Threading.Tasks;

using Bolt.Metadata;
using Bolt.Pipeline;
using System.Linq;
using System.Net.Http.Headers;
using Bolt.Serialization;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Buffers;

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

            var actionMetadata = context.GetActionOrThrow();
            object[] rentedParameters = null;

            if (actionMetadata.HasParameters && context.Parameters == null)
            {
                rentedParameters = ArrayPool<object>.Shared.Rent(actionMetadata.Parameters.Count);
                for (int i = 0; i < actionMetadata.Parameters.Count; i++)
                {
                    // clear array just in case
                    rentedParameters[i] = null;
                }

                await DeserializeParameters(context, actionMetadata, rentedParameters);
                context.Parameters = rentedParameters;
            }

            try
            {
                await Next(context);

                if (!context.ResponseHandled)
                {
                    await HandleResponse(context);
                    context.ResponseHandled = true;
                }
            }
            finally
            {
                if (rentedParameters != null)
                {
                    ArrayPool<object>.Shared.Return(rentedParameters);
                }
            }
        }

        protected virtual async Task DeserializeParameters(ServerActionContext context, ActionMetadata metadata, object[] parameterValues)
        {
            if (metadata.SerializableParameters?.Count > 0)
            {
                ISerializer serializer = context.GetSerializerOrThrow();
                try
                {
                    // TODO: copy body to another stream to prevent blocking in json deserialization
                    await serializer.ReadAsync(context.HttpContext.Request.Body, metadata.SerializableParameters, parameterValues);
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
                        context.Action.Name,
                        context.RequestUrl,
                        e);
                }

                if (metadata.CancellationTokenIndex >= 0)
                {
                    parameterValues[metadata.CancellationTokenIndex] = context.RequestAborted;
                }
            }

            if (metadata.CancellationTokenIndex == 0)
            {
                parameterValues[0] = context.RequestAborted;
            }
        }

        protected virtual async Task HandleResponse(ServerActionContext context)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            context.HttpContext.Response.StatusCode = 200;

            if (context.GetActionOrThrow().HasResult && context.ActionResult != null)
            {
                context.HttpContext.Response.ContentType = context.Configuration.DefaultSerializer.MediaType;
                try
                {
                    await context.GetSerializerOrThrow().WriteAsync(context.HttpContext.Response.Body, context.ActionResult);
                }
                catch (Exception e)
                {
                    throw new BoltServerException(
                        $"Failed to serialize response for action '{context.Action.Name}'.",
                        ServerErrorCode.SerializeResponse,
                        context.Action.Name,
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
