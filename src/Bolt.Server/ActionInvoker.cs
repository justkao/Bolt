using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Common;
using Bolt.Core;
using Bolt.Server.Filters;

namespace Bolt.Server
{
    public class ActionInvoker : IActionInvoker
    {
        private readonly ConcurrentDictionary<MethodInfo, ActionDescriptor> _actions = new ConcurrentDictionary<MethodInfo, ActionDescriptor>();

        public Task InvokeAsync(ServerActionContext context)
        {
            ActionDescriptor result = _actions.GetOrAdd(context.Action, a => new ActionDescriptor(a));
            return result.Invoke(context);
        }

        private class ActionDescriptor
        {
            public ActionDescriptor(MethodInfo actionInfo)
            {
                Parameters = actionInfo.GetParameters().ToList();
                ParametersTypes = Parameters.Select(p => p.ParameterType).ToArray();
                HasParameters =
                    Parameters.Select(p => p.ParameterType).Except(new[] { typeof(CancellationToken), typeof(CancellationToken?) }).Any();
            }

            private bool HasParameters { get; }

            private IEnumerable<ParameterInfo> Parameters { get; }

            private Type[] ParametersTypes { get; }

            public Task Invoke(ServerActionContext context)
            {
                return new CoreServerAction().ExecuteAsync(context, InvokeInternal);
            }

            private async Task InvokeInternal(ServerActionContext context)
            {
                IBoltFeature boltFeature = context.HttpContext.GetFeature<IBoltFeature>();

                object[] arguments = null;
                if (HasParameters && context.Parameters == null)
                {
                    await boltFeature.Configuration.ParameterHandler.HandleAsync(context);
                    arguments = BuildParameters(context).ToArray();
                }

                var instance = context.GetRequiredInstance();
                MethodInfo implementedMethod = instance.GetType().GetRuntimeMethod(context.Action.Name, ParametersTypes);
                object result = implementedMethod.Invoke(instance, arguments);

                if (result is Task)
                {
                    await (result as Task);
                    if (result.GetType().IsGenericType())
                    {
                        context.Result = result.GetType().GetRuntimeProperty("Result").GetValue(result);
                    }
                }
                else
                {
                    context.Result = result;
                }

                await boltFeature.Configuration.ResponseHandler.HandleAsync(context);
            }

            private IEnumerable<object> BuildParameters(ServerActionContext actionContext)
            {
                IObjectDeserializer rawParameters = actionContext.GetRequiredParameters();

                foreach (ParameterInfo parameter in Parameters)
                {
                    if (parameter.ParameterType == typeof(CancellationToken) || parameter.ParameterType == typeof(CancellationToken?))
                    {
                        yield return actionContext.RequestAborted;
                    }

                    yield return rawParameters.ReadParameterValue(actionContext.Action, parameter.Name, parameter.ParameterType);
                }
            }
        }
    }
}