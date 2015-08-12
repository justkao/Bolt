using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Bolt.Common;
using Bolt.Pipeline;

namespace Bolt.Server.Pipeline
{
    public class ActionInvokerMiddleware : MiddlewareBase<ServerActionContext>
    {
        private readonly ConcurrentDictionary<MethodInfo, ActionDescriptor> _actions = new ConcurrentDictionary<MethodInfo, ActionDescriptor>();

        public override async Task Invoke(ServerActionContext context)
        {
            if (context.HasSerializableParameters)
            {
                try
                {
                    BoltFramework.ValidateParameters(context.Action, context.Parameters);
                }
                catch (Exception e)
                {
                    throw new BoltServerException(ServerErrorCode.ParameterDeserialization, context.Action, context.RequestUrl, e);
                }
            }

            if (context.ContractInstance == null)
            {
                throw new BoltServerException(
                    $"There is no contract instance assigned for action '{context.Action.Name}'.",
                    ServerErrorCode.ContractInstanceNotFound,
                    context.Action,
                    context.RequestUrl);
            }

            if (!(context.ContractInstance.GetType().GetTypeInfo().ImplementedInterfaces.Contains(context.Contract)))
            {
                throw new BoltServerException(
                    $"Contract instance of type {context.Contract.Name} is expected but {context.ContractInstance.GetType().Name} was provided.",
                    ServerErrorCode.ContractInstanceNotFound,
                    context.Action,
                    context.RequestUrl);
            }

            ActionDescriptor result = _actions.GetOrAdd(context.Action, a => new ActionDescriptor(a));
            await result.Execute(context);
            await Next(context);
        }

        private class ActionDescriptor
        {
            public ActionDescriptor(MethodInfo actionInfo)
            {
                Parameters = actionInfo.GetParameters().ToList();
                ParametersTypes = Parameters.Select(p => p.ParameterType).ToArray();
            }

            private IEnumerable<ParameterInfo> Parameters { get; }

            private Type[] ParametersTypes { get; }

            public async Task Execute(ServerActionContext context)
            {
                MethodInfo implementedMethod = context.ContractInstance.GetType().GetRuntimeMethod(context.Action.Name, ParametersTypes);

                if (implementedMethod == null)
                {
                    if (context.Action == BoltFramework.SessionContractDescriptorProvider.InitSessionDummy)
                    {
                        return;
                    }

                    if (context.Action == BoltFramework.SessionContractDescriptorProvider.DestroySessionDummy)
                    {
                        return;
                    }

                    throw new BoltServerException(ServerErrorCode.ActionNotImplemented, context.Action, context.RequestUrl);
                }

                object result;
                try
                {
                    result = implementedMethod.Invoke(context.ContractInstance, context.Parameters);
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
                        context.ActionResult = result.GetType().GetRuntimeProperty("Result").GetValue(result);
                    }
                }
                else
                {
                    context.ActionResult = result;
                }
            }
        }
    }
}