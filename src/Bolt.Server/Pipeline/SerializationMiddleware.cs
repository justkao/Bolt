using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Core;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public class SerializationMiddleware : MiddlewareBase<ServerActionContext>
    {
        public override async Task Invoke(ServerActionContext context)
        {
            if (context.HasSerializableParameters)
            {
                context.Parameters = await DeserializeParameters(context);
            }

            await Next(context);

            await HandleResponse(context);
        }

        protected virtual async Task<object[]> DeserializeParameters(ServerActionContext context)
        {
            IObjectSerializer rawParameters =
                context.Configuration.Serializer.DeserializeParameters(
                    await context.HttpContext.Request.Body.CopyAsync(context.RequestAborted),
                    context.Action);

            ParameterInfo[] parameters = context.Action.GetParameters();
            object[] parameterValues =new object[parameters.Length];


            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                if (parameter.ParameterType == typeof (CancellationToken) ||
                    parameter.ParameterType == typeof (CancellationToken?))
                {
                    parameterValues[i] = context.RequestAborted;
                }

                parameterValues[i] = rawParameters.ReadParameterValue(context.Action, parameter.Name, parameter.ParameterType);
            }

            return parameterValues;
        }


        protected virtual async Task HandleResponse(ServerActionContext context)
        {
            context.RequestAborted.ThrowIfCancellationRequested();
            context.HttpContext.Response.StatusCode = 200;

            if (context.HasSerializableActionResult && context.ActionResult != null)
            {
                byte[] raw = context.Configuration.Serializer.SerializeResponse(context.ActionResult, context.Action).ToArray();
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
