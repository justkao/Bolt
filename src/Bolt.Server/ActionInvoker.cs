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
        private readonly ISessionHandler _sessionHandler;
        private readonly ConcurrentDictionary<MethodInfo, ActionDescriptor> _actions = new ConcurrentDictionary<MethodInfo, ActionDescriptor>();

        public ActionInvoker(ISessionHandler sessionHandler)
        {
            if (sessionHandler == null) throw new ArgumentNullException(nameof(sessionHandler));

            _sessionHandler = sessionHandler;
        }

        public Task InvokeAsync(ServerActionContext context)
        {
            ActionDescriptor result = _actions.GetOrAdd(context.Action, a => new ActionDescriptor(a, _sessionHandler));
            return result.Invoke(context);
        }

        private class ActionDescriptor
        {
            private readonly ISessionHandler _sessionHandler;

            public ActionDescriptor(MethodInfo actionInfo, ISessionHandler sessionHandler)
            {
                _sessionHandler = sessionHandler;
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

                if (context.Action == BoltFramework.InitSessionAction)
                {
                    await _sessionHandler.HandleInitSessionAsync(context);
                }
                else if (context.Action == BoltFramework.DestroySessionAction)
                {
                    await _sessionHandler.HandleDestroySessionAsync(context);
                }
                else
                {
                    object[] arguments = null;
                    if (HasParameters && context.Parameters == null)
                    {
                        await boltFeature.Configuration.ParameterHandler.HandleAsync(context);
                        arguments = BuildParameters(context).ToArray();
                    }

                    var instance = context.GetRequiredInstance();
                    MethodInfo implementedMethod = instance.GetType().GetRuntimeMethod(context.Action.Name, ParametersTypes);

                    object result;
                    try
                    {
                        result = implementedMethod.Invoke(instance, arguments ?? new object[0]);
                    }
                    catch (TargetInvocationException e)
                    {
                        throw e.InnerException;
                    }

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
                }
            }

            private IEnumerable<object> BuildParameters(ServerActionContext actionContext)
            {
                IObjectSerializer rawParameters = actionContext.GetRequiredParameters();

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