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
            if (context.HasSerializableParameters && context.Parameters == null)
            {
                throw new InvalidOperationException($"There are no required parameters assigned for action '{context.Action.Name}'.");
            }

            if (context.ContractInstance == null)
            {
                throw new InvalidOperationException($"There is no contract instance assigned for action '{context.Action.Name}'.");
            }

            if (!(context.ContractInstance.GetType().GetTypeInfo().ImplementedInterfaces.Contains(context.Contract)))
            {
                throw new InvalidOperationException($"Contract instance of type {context.Contract.Name} is expected but {context.ContractInstance.GetType().Name} was provided.");
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