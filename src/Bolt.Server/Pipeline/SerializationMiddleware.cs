using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public class SerializationMiddleware : MiddlewareBase<ServerActionContext>
    {
        public override async Task Invoke(ServerActionContext context)
        {
            if (context.HasParameters && context.Parameters == null)
            {
                context.Parameters = await DeserializeParameters(context);
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
            IObjectSerializer rawParameters = null;
            try
            {
                rawParameters =
                    context.Configuration.Serializer.CreateSerializer(
                        await context.HttpContext.Request.Body.CopyAsync(context.RequestAborted));
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

            ParameterInfo[] parameters = context.Action.GetParameters();
            object[] parameterValues = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                if (parameter.IsCancellationToken())
                {
                    parameterValues[i] = context.RequestAborted;
                }

                try
                {
                    object val;
                    if (rawParameters.TryRead(parameter.Name, parameter.ParameterType, out val))
                    {
                        parameterValues[i] = val;
                    }
                }
                catch (Exception e)
                {
                    throw new BoltServerException(
                        $"Failed to deserialize parameter '{parameter.Name}' for action '{context.Action.Name}'.",
                        ServerErrorCode.DeserializeParameters,
                        context.Action,
                        context.RequestUrl,
                        e);
                }
            }

            return parameterValues;
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

                byte[] raw = stream.ToArray();
                if (raw.Length > 0)
                {
                    context.HttpContext.Response.ContentLength = raw.Length;
                    context.HttpContext.Response.ContentType = context.Configuration.Serializer.ContentType;

                    await context.HttpContext.Response.Body.WriteAsync(raw, 0, raw.Length, context.RequestAborted);
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
