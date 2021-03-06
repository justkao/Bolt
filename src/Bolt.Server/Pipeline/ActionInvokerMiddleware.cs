using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Metadata;
using Bolt.Pipeline;
using Bolt.Server.Internal;

namespace Bolt.Server.Pipeline
{
    public class ActionInvokerMiddleware : MiddlewareBase<ServerActionContext>
    {
        private readonly ConcurrentDictionary<ActionMetadata, ActionDescriptor> _actions = new ConcurrentDictionary<ActionMetadata, ActionDescriptor>();

        public override async Task InvokeAsync(ServerActionContext context)
        {
            if (context.GetActionOrThrow().HasParameters)
            {
                try
                {
                    context.GetActionOrThrow().ValidateParameters(context.Parameters);
                }
                catch (Exception e)
                {
                    throw new BoltServerException(ServerErrorCode.DeserializeParameters, context.Action.Name, context.RequestUrl, e);
                }
            }

            if (context.ContractInstance == null)
            {
                throw new BoltServerException(
                    $"There is no contract instance assigned for action '{context.Action.Name}'.",
                    ServerErrorCode.NoContractInstance,
                    context.Action.Name,
                    context.RequestUrl);
            }

            /*
            if (!(context.ContractInstance.GetType().GetTypeInfo().ImplementedInterfaces.Contains(context.Contract)))
            {
                throw new BoltServerException(
                    $"Contract instance of type {context.Contract.Name} is expected but {context.ContractInstance.GetType().Name} was provided.",
                    ServerErrorCode.InvalidContractInstance,
                    context.Action,
                    context.RequestUrl);
            }
            */

            if (context.Action.Action == BoltFramework.SessionMetadata.InitSessionDefault ||
                context.Action.Action == BoltFramework.SessionMetadata.DestroySessionDefault)
            {
                await Next(context);
            }
            else
            {
                ActionDescriptor result = _actions.GetOrAdd(context.Action, a => new ActionDescriptor(a));
                await result.ExecuteAsync(context);
                await Next(context);
            }
        }

        private class ActionDescriptor
        {
            private readonly Func<object, object[], object> _compiledLambda;
            private readonly Func<Task, object> _taskResultProvider;
            private readonly bool _isTaskResult;

            public ActionDescriptor(ActionMetadata metadata)
            {
                Parameters = metadata.Action.GetParameters().ToList();
                ParametersTypes = Parameters.Select(p => p.ParameterType).ToArray();
                _compiledLambda = MethodInvokerBuilder.Build(metadata.Action.DeclaringType, metadata.Action);
                _isTaskResult = typeof(Task).IsAssignableFrom(metadata.Action.ReturnType);
                if (_isTaskResult && metadata.Action.ReturnType.GetTypeInfo().IsGenericType)
                {
                    _taskResultProvider = MethodInvokerBuilder.BuildTaskResultProvider(metadata.ResultType);
                }
            }

            private IEnumerable<ParameterInfo> Parameters { get; }

            private Type[] ParametersTypes { get; }

            public async Task ExecuteAsync(ServerActionContext context)
            {
                var result = _compiledLambda(context.ContractInstance, context.Parameters);
                if (_isTaskResult)
                {
                    await (Task)result;
                    if (_taskResultProvider != null)
                    {
                        context.ActionResult = _taskResultProvider((Task)result);
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