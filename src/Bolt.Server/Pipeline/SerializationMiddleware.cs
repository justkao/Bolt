using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public class SerializationMiddleware : MiddlewareBase<ServerActionContext>
    {
        private const int DefaultBuffer = 1024*1024;

        public override async Task InvokeAsync(ServerActionContext context)
        {
            if (context.HasParameters && context.Parameters.Values == null)
            {
                context.Parameters.Values = await DeserializeParameters(context);
            }

            await Next(context);

            if (!context.ResponseHandled)
            {
                await HandleResponse(context);
                context.ResponseHandled = true;
            }
        }

        protected virtual async Task<object[]> DeserializeParameters(ServerActionContext context)
        {
            try
            {
                using (MemoryStream stream = await context.HttpContext.Request.Body.CopyAsync(context.RequestAborted))
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    object[] parameterValues = context.Parameters.Values ?? new object[context.Parameters.Definition.Parameters.Length];
                    context.Configuration.Serializer.Read(stream, context.Action, parameterValues);
                    if (context.Parameters.Definition.CancellationTokenIndex >= 0)
                    {
                        parameterValues[context.Parameters.Definition.CancellationTokenIndex] = context.RequestAborted;
                    }

                    return parameterValues;
                }
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
        }

        protected virtual async Task HandleResponse(ServerActionContext context)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            context.HttpContext.Response.StatusCode = 200;

            if (context.HasSerializableActionResult && context.ActionResult != null)
            {
                MemoryStream stream = new MemoryStream();
                try
                {
                    context.Configuration.Serializer.Write(stream, context.ActionResult);
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

                stream.Seek(0, SeekOrigin.Begin);

                if (stream.Length > 0)
                {
                    context.HttpContext.Response.ContentLength = stream.Length;
                    context.HttpContext.Response.ContentType = context.Configuration.Serializer.ContentType;

                    await stream.CopyToAsync(context.HttpContext.Response.Body, DefaultBuffer, context.RequestAborted);
                }
                else
                {
                    context.HttpContext.Response.ContentLength = 0;
                }
            }
            else
            {
                context.HttpContext.Response.ContentLength = 0;
            }

            context.HttpContext.Response.Body.Dispose();
        }
    }
}
